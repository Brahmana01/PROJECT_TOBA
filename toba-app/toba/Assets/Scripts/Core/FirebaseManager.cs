#define FIREBASE_ENABLED

using UnityEngine;
using System.Collections;

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Messaging;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
#endif

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    public static string FCMToken { get; private set; } = "";
    public static bool IsReady { get; private set; } = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize()
    {
#if FIREBASE_ENABLED
        Debug.Log("[Firebase] Initialize() dipanggil.");
        StartCoroutine(InitFirebase());
#else
        Debug.Log("[Firebase] SDK belum terinstall.");
        IsReady = false;
#endif
    }

#if FIREBASE_ENABLED

    IEnumerator InitFirebase()
    {
        Debug.Log("[Firebase] Mengecek dependency...");

        var task = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCanceled)
        {
            Debug.LogWarning("[Firebase] CheckAndFixDependenciesAsync dibatalkan.");
            IsReady = false;
            yield break;
        }

        if (task.IsFaulted)
        {
            Debug.LogWarning(
                "[Firebase] CheckAndFixDependenciesAsync gagal: " +
                task.Exception
            );

            IsReady = false;
            yield break;
        }

        Debug.Log(
            "[Firebase] Dependency status: " +
            task.Result
        );

        if (task.Result == DependencyStatus.Available)
        {
            Debug.Log("[Firebase] Dependency tersedia.");

#if UNITY_ANDROID && !UNITY_EDITOR

            // ==========================================
            // REQUEST PERMISSION NOTIFIKASI ANDROID
            // ==========================================
            RequestAndroidNotificationPermission();

#else

            Debug.Log(
                "[Firebase] Permission notifikasi dilewati " +
                "(bukan Android device)."
            );

#endif

            // Subscribe ke topic umum
            FirebaseMessaging.SubscribeAsync("all_players")
                .ContinueWithOnMainThread(t =>
                {
                    if (t.IsCanceled)
                    {
                        Debug.LogWarning(
                            "[Firebase] Subscribe all_players dibatalkan."
                        );
                    }
                    else if (t.IsFaulted)
                    {
                        Debug.LogWarning(
                            "[Firebase] Subscribe all_players gagal: " +
                            t.Exception
                        );
                    }
                    else
                    {
                        Debug.Log(
                            "[Firebase] Subscribe all_players berhasil."
                        );
                    }
                });

            FirebaseMessaging.SubscribeAsync("toba_updates")
                .ContinueWithOnMainThread(t =>
                {
                    if (t.IsCanceled)
                    {
                        Debug.LogWarning(
                            "[Firebase] Subscribe toba_updates dibatalkan."
                        );
                    }
                    else if (t.IsFaulted)
                    {
                        Debug.LogWarning(
                            "[Firebase] Subscribe toba_updates gagal: " +
                            t.Exception
                        );
                    }
                    else
                    {
                        Debug.Log(
                            "[Firebase] Subscribe toba_updates berhasil."
                        );
                    }
                });

            // Handler token
            FirebaseMessaging.TokenReceived += OnToken;
            FirebaseMessaging.MessageReceived += OnMessage;

            Debug.Log("[Firebase] Event handler TokenReceived dipasang.");
            Debug.Log("[Firebase] Event handler MessageReceived dipasang.");

            // Request token
            Debug.Log("[Firebase] Meminta FCM token...");

            FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(
                t =>
                {
                    Debug.Log("[Firebase] GetTokenAsync selesai.");

                    if (t.IsCanceled)
                    {
                        Debug.LogWarning(
                            "[Firebase] GetTokenAsync DIBATALKAN."
                        );

                        return;
                    }

                    if (t.IsFaulted)
                    {
                        Debug.LogWarning(
                            "[Firebase] GetTokenAsync GAGAL."
                        );

                        if (t.Exception != null)
                        {
                            Debug.LogWarning(
                                "[Firebase] Detail error: " +
                                t.Exception
                            );

                            Debug.LogWarning(
                                "[Firebase] InnerException: " +
                                t.Exception.GetBaseException()
                            );
                        }

                        return;
                    }

                    if (string.IsNullOrEmpty(t.Result))
                    {
                        Debug.LogWarning(
                            "[Firebase] GetTokenAsync berhasil tetapi token kosong."
                        );

                        return;
                    }

                    FCMToken = t.Result;

                    PlayerPrefs.SetString(
                        "FCMToken",
                        FCMToken
                    );

                    PlayerPrefs.Save();

                    Debug.Log("[Firebase] Token FCM berhasil didapat.");

                    // Kirim token ke backend
                    int pid =
                        PlayerPrefs.GetInt(
                            "PlayerID"
                        );

                    if (pid > 0)
                    {
                        Debug.Log(
                            "[Firebase] PlayerID ditemukan: " +
                            pid +
                            ". Mengirim token ke backend..."
                        );

                        SendTokenToBackend(pid);
                    }
                    else
                    {
                        Debug.LogWarning(
                            "[Firebase] PlayerID belum tersedia. " +
                            "Token belum dikirim ke backend."
                        );
                    }
                }
            );

            IsReady = true;

            Debug.Log("[Firebase] ✅ Siap!");
        }
        else
        {
            IsReady = false;

            Debug.LogWarning(
                "[Firebase] Dependency tidak tersedia: " +
                task.Result
            );
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR

    void RequestAndroidNotificationPermission()
    {
        const string notificationPermission =
            "android.permission.POST_NOTIFICATIONS";

        Debug.Log(
            "[Firebase] Mengecek permission notifikasi Android..."
        );

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
            "[Firebase] Permission notifikasi belum diberikan."
        );

        Debug.Log(
            "[Firebase] Meminta permission POST_NOTIFICATIONS..."
        );

        var callbacks =
            new UnityEngine.Android.PermissionCallbacks();

        callbacks.PermissionGranted +=
            PermissionGrantedCallback;

        callbacks.PermissionDenied +=
            PermissionDeniedCallback;

        callbacks.PermissionDeniedAndDontAskAgain +=
            PermissionDeniedAndDontAskAgainCallback;

        UnityEngine.Android.Permission.RequestUserPermission(
            notificationPermission,
            callbacks
        );

        Debug.Log(
            "[Firebase] Dialog permission notifikasi Android diminta."
        );
    }

    void PermissionGrantedCallback(string permissionName)
    {
        Debug.Log(
            "[Firebase] ✅ Permission diberikan: " +
            permissionName
        );
    }

    void PermissionDeniedCallback(string permissionName)
    {
        Debug.LogWarning(
            "[Firebase] ❌ Permission ditolak: " +
            permissionName
        );
    }

    void PermissionDeniedAndDontAskAgainCallback(
        string permissionName)
    {
        Debug.LogWarning(
            "[Firebase] ❌ Permission ditolak dan " +
            "jangan tanyakan lagi: " +
            permissionName
        );

        Debug.LogWarning(
            "[Firebase] User harus mengaktifkan permission " +
            "secara manual dari Settings Android."
        );
    }

#endif

    void OnToken(
        object sender,
        TokenReceivedEventArgs e)
    {
        Debug.Log(
            "[Firebase] TokenReceived event dipanggil."
        );

        if (e == null)
        {
            Debug.LogWarning(
                "[Firebase] TokenReceivedEventArgs null."
            );

            return;
        }

        FCMToken = e.Token;

        if (string.IsNullOrEmpty(FCMToken))
        {
            Debug.LogWarning(
                "[Firebase] TokenReceived event memberikan token kosong."
            );

            return;
        }

        PlayerPrefs.SetString(
            "FCMToken",
            FCMToken
        );

        PlayerPrefs.Save();

        Debug.Log("[Firebase] Event FCM TokenReceived berhasil.");

        int pid =
            PlayerPrefs.GetInt(
                "PlayerID"
            );

        if (pid > 0)
        {
            Debug.Log(
                "[Firebase] Mengirim token hasil TokenReceived ke backend. " +
                "PlayerID: " +
                pid
            );

            SendTokenToBackend(pid);
        }
        else
        {
            Debug.LogWarning(
                "[Firebase] PlayerID belum tersedia pada TokenReceived."
            );
        }
    }

    void OnMessage(
        object sender,
        MessageReceivedEventArgs e)
    {
        if (e == null || e.Message == null)
            return;

        string title =
            e.Message.Notification?.Title
            ?? "Notifikasi TOBA";

        string body =
            e.Message.Notification?.Body
            ?? "";

        Debug.Log(
            "[Firebase] Notif: " +
            title +
            " | " +
            body
        );

        // Tampilkan toast di game
        if (
            NetworkManager.Instance != null &&
            !string.IsNullOrEmpty(body)
        )
        {
            NetworkManager.Instance.ShowToast(
                title + ": " + body,
                false
            );
        }
    }

#else

    IEnumerator InitFirebase()
    {
        yield break;
    }

#endif

    public void SendTokenToBackend(int playerId)
    {
        if (string.IsNullOrEmpty(FCMToken))
        {
            Debug.LogWarning(
                "[Firebase] SendTokenToBackend dibatalkan: FCMToken kosong."
            );

            return;
        }

        string json =
            JsonUtility.ToJson(
                new FCMTokenRequest
                {
                    player_id = playerId,
                    fcm_token = FCMToken
                }
            );

        Debug.Log(
            "[Firebase] Mengirim FCM token ke backend. " +
            "PlayerID: " +
            playerId
        );

        APIManager.Instance?.PostRequest(
            "/player/fcm-token",
            json,
            (r) =>
            {
                Debug.Log(
                    "[Firebase] Token terkirim ke backend ✅"
                );
            },
            (e) =>
            {
                Debug.LogWarning(
                    "[Firebase] Gagal kirim token ke backend: " +
                    e
                );
            }
        );
    }
}