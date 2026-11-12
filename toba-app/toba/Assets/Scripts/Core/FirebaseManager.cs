#define FIREBASE_ENABLED

using UnityEngine;
using System;
using System.Collections;

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Messaging;
using Firebase.Extensions;
using System.Threading.Tasks;
#endif

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public static string FCMToken { get; private set; } = "";
    public static bool IsReady { get; private set; } = false;

    private bool _initializationStarted;
    private bool _eventsSubscribed;
    private bool _isShuttingDown;

    private Coroutine _initializationCoroutine;

    private bool _tokenSendInProgress;
    private string _lastSentToken = "";
    private int _lastSentPlayerId;

    private int _queuedPlayerId;

#if UNITY_ANDROID && !UNITY_EDITOR
    private UnityEngine.Android.PermissionCallbacks _permissionCallbacks;
#endif

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        FCMToken = PlayerPrefs.GetString("FCMToken", "");
    }

    private void OnApplicationQuit()
    {
        _isShuttingDown = true;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        _isShuttingDown = true;
        IsReady = false;

        if (_initializationCoroutine != null)
        {
            StopCoroutine(_initializationCoroutine);
            _initializationCoroutine = null;
        }

#if FIREBASE_ENABLED
        UnsubscribeMessagingEvents();
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        if (_permissionCallbacks != null)
        {
            _permissionCallbacks.PermissionGranted -= PermissionGrantedCallback;
            _permissionCallbacks.PermissionDenied -= PermissionDeniedCallback;
            _permissionCallbacks.PermissionDeniedAndDontAskAgain -=
                PermissionDeniedAndDontAskAgainCallback;

            _permissionCallbacks = null;
        }
#endif

        Instance = null;
    }

    // ============================================================
    // INITIALIZATION
    // ============================================================

    public void Initialize()
    {
        if (_isShuttingDown)
            return;

        // MainApp dapat memanggil Initialize lebih dari sekali.
        // Firebase hanya boleh diinisialisasi satu kali per instance.
        if (_initializationStarted)
            return;

        _initializationStarted = true;
        IsReady = false;

#if FIREBASE_ENABLED
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning(
                "[Firebase] Initialize dibatalkan karena komponen tidak aktif."
            );

            _initializationStarted = false;
            return;
        }

        Debug.Log("[Firebase] Initialize() dipanggil.");

        _initializationCoroutine = StartCoroutine(InitFirebase());
#else
        Debug.LogWarning("[Firebase] Firebase SDK belum diaktifkan.");
        IsReady = false;
        _initializationStarted = false;
#endif
    }

#if FIREBASE_ENABLED

    // ============================================================
    // FIREBASE DEPENDENCY
    // ============================================================

    private IEnumerator InitFirebase()
    {
        Debug.Log("[Firebase] Mengecek dependency...");

        Task<DependencyStatus> dependencyTask = null;
        string startError = null;

        // Pisahkan pemanggilan task dari yield agar try-catch
        // tidak membungkus yield di dalam iterator.
        try
        {
            dependencyTask =
                FirebaseApp.CheckAndFixDependenciesAsync();
        }
        catch (Exception ex)
        {
            startError = ex.Message;
        }

        if (dependencyTask == null)
        {
            Debug.LogWarning(
                "[Firebase] Tidak dapat memulai pemeriksaan dependency: " +
                (startError ?? "Task kosong.")
            );

            IsReady = false;
            _initializationStarted = false;
            _initializationCoroutine = null;
            yield break;
        }

        float dependencyWaitStarted = Time.realtimeSinceStartup;
        const float dependencyTimeoutSeconds = 30f;

        while (!dependencyTask.IsCompleted)
        {
            if (_isShuttingDown || this == null)
                yield break;

            if (Time.realtimeSinceStartup - dependencyWaitStarted >
                dependencyTimeoutSeconds)
            {
                Debug.LogWarning(
                    "[Firebase] Pemeriksaan dependency melewati batas waktu."
                );

                IsReady = false;
                _initializationStarted = false;
                _initializationCoroutine = null;
                yield break;
            }

            yield return null;
        }

        if (_isShuttingDown || this == null)
            yield break;

        if (dependencyTask.IsCanceled)
        {
            Debug.LogWarning(
                "[Firebase] CheckAndFixDependenciesAsync dibatalkan."
            );

            IsReady = false;
            _initializationStarted = false;
            _initializationCoroutine = null;
            yield break;
        }

        if (dependencyTask.IsFaulted)
        {
            Debug.LogWarning(
                "[Firebase] CheckAndFixDependenciesAsync gagal: " +
                GetTaskError(dependencyTask)
            );

            IsReady = false;
            _initializationStarted = false;
            _initializationCoroutine = null;
            yield break;
        }

        DependencyStatus dependencyStatus = default(DependencyStatus);
        bool dependencyResultFailed = false;
        string dependencyResultError = null;

        try
        {
            dependencyStatus = dependencyTask.Result;
        }
        catch (Exception ex)
        {
            dependencyResultFailed = true;
            dependencyResultError = ex.Message;
        }

        if (dependencyResultFailed)
        {
            Debug.LogWarning(
                "[Firebase] Gagal membaca status dependency: " +
                dependencyResultError
            );

            IsReady = false;
            _initializationStarted = false;
            _initializationCoroutine = null;
            yield break;
        }

        Debug.Log(
            "[Firebase] Dependency status: " +
            dependencyStatus
        );

        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogWarning(
                "[Firebase] Dependency tidak tersedia: " +
                dependencyStatus
            );

            IsReady = false;
            _initializationStarted = false;
            _initializationCoroutine = null;
            yield break;
        }

        Debug.Log("[Firebase] Dependency tersedia.");

#if UNITY_ANDROID && !UNITY_EDITOR
        RequestAndroidNotificationPermission();
#else
        Debug.Log(
            "[Firebase] Permission notifikasi dilewati (bukan Android device)."
        );
#endif

        SubscribeMessagingEvents();

        // Subscription dan permintaan token ditangani terpisah.
        // Kegagalan subscription tidak menghentikan aplikasi.
        SubscribeTopic("all_players");
        SubscribeTopic("toba_updates");

        IsReady = true;

        Debug.Log("[Firebase] Siap!");

        RequestFcmToken();

        _initializationCoroutine = null;
    }

    private string GetTaskError(Task task)
    {
        if (task == null || task.Exception == null)
            return "Detail exception tidak tersedia.";

        return task.Exception.GetBaseException().Message;
    }

    // ============================================================
    // MESSAGING EVENT HANDLERS
    // ============================================================

    private void SubscribeMessagingEvents()
    {
        if (_eventsSubscribed || _isShuttingDown)
            return;

        try
        {
            FirebaseMessaging.TokenReceived += OnToken;
            FirebaseMessaging.MessageReceived += OnMessage;

            _eventsSubscribed = true;

            Debug.Log("[Firebase] Event handler messaging dipasang.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[Firebase] Gagal memasang event handler: " +
                ex.Message
            );
        }
    }

    private void UnsubscribeMessagingEvents()
    {
        if (!_eventsSubscribed)
            return;

        try
        {
            FirebaseMessaging.TokenReceived -= OnToken;
            FirebaseMessaging.MessageReceived -= OnMessage;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[Firebase] Gagal melepas event handler: " +
                ex.Message
            );
        }

        _eventsSubscribed = false;
    }

    // ============================================================
    // TOPIC SUBSCRIPTION
    // ============================================================

    private void SubscribeTopic(string topic)
    {
        if (_isShuttingDown || string.IsNullOrWhiteSpace(topic))
            return;

        Task subscriptionTask = null;
        string startError = null;

        try
        {
            subscriptionTask = FirebaseMessaging.SubscribeAsync(topic);
        }
        catch (Exception ex)
        {
            startError = ex.Message;
        }

        if (subscriptionTask == null)
        {
            Debug.LogWarning(
                "[Firebase] Subscription " + topic +
                " tidak dapat dimulai: " +
                (startError ?? "Task kosong.")
            );

            return;
        }

        subscriptionTask.ContinueWithOnMainThread(task =>
        {
            if (_isShuttingDown || this == null)
                return;

            if (task.IsCanceled)
            {
                Debug.LogWarning(
                    "[Firebase] Subscribe " + topic + " dibatalkan."
                );
            }
            else if (task.IsFaulted)
            {
                Debug.LogWarning(
                    "[Firebase] Subscribe " + topic + " gagal: " +
                    GetTaskError(task)
                );
            }
            else
            {
                Debug.Log(
                    "[Firebase] Subscribe " + topic + " berhasil."
                );
            }
        });
    }

    // ============================================================
    // FCM TOKEN REQUEST
    // ============================================================

    private void RequestFcmToken()
    {
        if (_isShuttingDown || !IsReady)
            return;

        Task<string> tokenTask = null;
        string startError = null;

        try
        {
            tokenTask = FirebaseMessaging.GetTokenAsync();
        }
        catch (Exception ex)
        {
            startError = ex.Message;
        }

        if (tokenTask == null)
        {
            Debug.LogWarning(
                "[Firebase] GetTokenAsync tidak dapat dimulai: " +
                (startError ?? "Task kosong.")
            );

            return;
        }

        tokenTask.ContinueWithOnMainThread(task =>
        {
            if (_isShuttingDown || this == null)
                return;

            if (task.IsCanceled)
            {
                Debug.LogWarning(
                    "[Firebase] GetTokenAsync dibatalkan."
                );
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogWarning(
                    "[Firebase] GetTokenAsync gagal: " +
                    GetTaskError(task)
                );
                return;
            }

            HandleToken(task.Result);
        });
    }

    private void HandleToken(string token)
    {
        if (_isShuttingDown || string.IsNullOrWhiteSpace(token))
        {
            Debug.LogWarning(
                "[Firebase] Token kosong atau Firebase sedang dihentikan."
            );
            return;
        }

        FCMToken = token.Trim();

        PlayerPrefs.SetString("FCMToken", FCMToken);
        PlayerPrefs.Save();

        Debug.Log("[Firebase] Token FCM berhasil didapat.");

        int playerId = PlayerPrefs.GetInt("PlayerID", 0);

        if (playerId > 0)
        {
            SendTokenToBackend(playerId);
        }
        else
        {
            Debug.Log(
                "[Firebase] PlayerID belum tersedia. " +
                "Token disimpan dan akan dikirim setelah login."
            );
        }
    }

    // ============================================================
    // ANDROID NOTIFICATION PERMISSION
    // ============================================================

#if UNITY_ANDROID && !UNITY_EDITOR

    private void RequestAndroidNotificationPermission()
    {
        if (_isShuttingDown)
            return;

        const string notificationPermission =
            "android.permission.POST_NOTIFICATIONS";

        try
        {
            bool alreadyGranted =
                UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                    notificationPermission
                );

            if (alreadyGranted)
            {
                Debug.Log(
                    "[Firebase] Permission notifikasi sudah diberikan."
                );
                return;
            }

            Debug.Log(
                "[Firebase] Meminta permission POST_NOTIFICATIONS..."
            );

            _permissionCallbacks =
                new UnityEngine.Android.PermissionCallbacks();

            _permissionCallbacks.PermissionGranted +=
                PermissionGrantedCallback;

            _permissionCallbacks.PermissionDenied +=
                PermissionDeniedCallback;

            _permissionCallbacks.PermissionDeniedAndDontAskAgain +=
                PermissionDeniedAndDontAskAgainCallback;

            UnityEngine.Android.Permission.RequestUserPermission(
                notificationPermission,
                _permissionCallbacks
            );
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[Firebase] Gagal meminta permission notifikasi: " +
                ex.Message
            );
        }
    }

    private void PermissionGrantedCallback(string permissionName)
    {
        if (_isShuttingDown)
            return;

        Debug.Log(
            "[Firebase] Permission diberikan: " +
            permissionName
        );
    }

    private void PermissionDeniedCallback(string permissionName)
    {
        if (_isShuttingDown)
            return;

        Debug.LogWarning(
            "[Firebase] Permission ditolak: " +
            permissionName
        );
    }

    private void PermissionDeniedAndDontAskAgainCallback(
        string permissionName)
    {
        if (_isShuttingDown)
            return;

        Debug.LogWarning(
            "[Firebase] Permission ditolak dan jangan tanyakan lagi: " +
            permissionName
        );

        Debug.LogWarning(
            "[Firebase] Aktifkan permission notifikasi melalui Settings Android."
        );
    }

#endif

    // ============================================================
    // TOKEN RECEIVED EVENT
    // ============================================================

    private void OnToken(
        object sender,
        TokenReceivedEventArgs eventArgs)
    {
        if (_isShuttingDown)
            return;

        if (eventArgs == null)
        {
            Debug.LogWarning(
                "[Firebase] TokenReceivedEventArgs null."
            );
            return;
        }

        HandleToken(eventArgs.Token);
    }

    // ============================================================
    // MESSAGE RECEIVED EVENT
    // ============================================================

    private void OnMessage(
        object sender,
        MessageReceivedEventArgs eventArgs)
    {
        if (_isShuttingDown ||
            eventArgs == null ||
            eventArgs.Message == null)
        {
            return;
        }

        string title =
            eventArgs.Message.Notification != null
                ? eventArgs.Message.Notification.Title
                : "Notifikasi TOBA";

        string body =
            eventArgs.Message.Notification != null
                ? eventArgs.Message.Notification.Body
                : "";

        if (string.IsNullOrWhiteSpace(title))
            title = "Notifikasi TOBA";

        Debug.Log(
            "[Firebase] Notifikasi diterima: " +
            title + " | " + body
        );

        if (NetworkManager.Instance != null &&
            !string.IsNullOrWhiteSpace(body))
        {
            NetworkManager.Instance.ShowToast(
                title + ": " + body,
                false
            );
        }
    }

#else

    private IEnumerator InitFirebase()
    {
        yield break;
    }

#endif

    // ============================================================
    // SEND TOKEN TO BACKEND
    // ============================================================

    public void SendTokenToBackend(int playerId)
    {
        if (_isShuttingDown)
            return;

        if (playerId <= 0)
        {
            Debug.LogWarning(
                "[Firebase] Pengiriman token dibatalkan: PlayerID tidak valid."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(FCMToken))
        {
            // Coba ambil token yang pernah tersimpan.
            FCMToken = PlayerPrefs.GetString("FCMToken", "");
        }

        if (string.IsNullOrWhiteSpace(FCMToken))
        {
            Debug.Log(
                "[Firebase] Token belum tersedia. Pengiriman ditunda."
            );
            return;
        }

        string tokenToSend = FCMToken;

        // Hindari mengirim token yang sama berulang kali
        // untuk pemain yang sama.
        if (_lastSentPlayerId == playerId &&
            _lastSentToken == tokenToSend)
        {
            return;
        }

        // Jika request sedang berjalan, simpan pemain yang
        // terakhir meminta pengiriman untuk diproses sesudahnya.
        if (_tokenSendInProgress)
        {
            _queuedPlayerId = playerId;
            return;
        }

        APIManager api = APIManager.Instance;

        if (api == null)
        {
            Debug.LogWarning(
                "[Firebase] APIManager belum tersedia. Token belum dikirim."
            );
            return;
        }

        string json;

        try
        {
            json = JsonUtility.ToJson(
                new FCMTokenRequest
                {
                    player_id = playerId,
                    fcm_token = tokenToSend
                }
            );
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[Firebase] Gagal membuat JSON token: " +
                ex.Message
            );
            return;
        }

        _tokenSendInProgress = true;

        Debug.Log(
            "[Firebase] Mengirim token ke backend. PlayerID: " +
            playerId
        );

        try
        {
            api.PostRequest(
                "/player/fcm-token",
                json,
                response =>
                {
                    if (_isShuttingDown || this == null)
                        return;

                    _lastSentToken = tokenToSend;
                    _lastSentPlayerId = playerId;

                    FinishTokenSend();

                    Debug.Log(
                        "[Firebase] Token berhasil dikirim ke backend."
                    );
                },
                error =>
                {
                    if (_isShuttingDown || this == null)
                        return;

                    FinishTokenSend();

                    Debug.LogWarning(
                        "[Firebase] Gagal mengirim token ke backend: " +
                        error
                    );
                }
            );
        }
        catch (Exception ex)
        {
            FinishTokenSend();

            Debug.LogWarning(
                "[Firebase] Pengiriman token gagal dimulai: " +
                ex.Message
            );
        }
    }

    private void FinishTokenSend()
    {
        _tokenSendInProgress = false;

        int queuedPlayerId = _queuedPlayerId;
        _queuedPlayerId = 0;

        if (queuedPlayerId > 0 &&
            !_isShuttingDown)
        {
            SendTokenToBackend(queuedPlayerId);
        }
    }
}
