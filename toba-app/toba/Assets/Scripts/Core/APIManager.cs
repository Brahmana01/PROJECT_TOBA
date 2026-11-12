using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    [Header("URLs")]
    private string BASE_URL = "http://43.173.13.187/api";
    private string WS_URL = "ws://43.173.13.187/ws";

    private const int DEFAULT_TIMEOUT = 10;

    // WebSocket Battle
    private WebSocket battleSocket;
    private bool wsConnected;
    private int wsConnectionVersion;

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

        Debug.Log("[API] APIManager siap digunakan.");
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (battleSocket == null)
            return;

        try
        {
            battleSocket.DispatchMessageQueue();
        }
        catch (Exception ex)
        {
            // Kegagalan dispatch socket tidak boleh menghentikan Update Unity.
            Debug.LogWarning("[WS] Gagal memproses message queue: " + ex.Message);
        }
#endif
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;
        wsConnectionVersion++;
        wsConnected = false;

        WebSocket socket = battleSocket;
        battleSocket = null;

        if (socket != null)
            CloseSocketAfterDestroy(socket);
    }

    private async void CloseSocketAfterDestroy(WebSocket socket)
    {
        if (socket == null)
            return;

        try
        {
            await socket.Close();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[WS] Gagal menutup socket saat cleanup: " + ex.Message);
        }
    }

    private async void OnApplicationQuit()
    {
        wsConnectionVersion++;
        wsConnected = false;

        WebSocket socket = battleSocket;
        battleSocket = null;

        if (socket == null)
            return;

        try
        {
            await socket.Close();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[WS] Gagal menutup koneksi saat aplikasi ditutup: " + ex.Message);
        }
    }

    // ============================================================
    // URL CONFIGURATION
    // ============================================================

    public string GetBaseUrl()
    {
        return string.IsNullOrWhiteSpace(BASE_URL)
            ? string.Empty
            : BASE_URL.TrimEnd('/');
    }

    public string GetWsUrl()
    {
        return string.IsNullOrWhiteSpace(WS_URL)
            ? string.Empty
            : WS_URL.TrimEnd('/');
    }

    private bool TryBuildApiUrl(
        string endpoint,
        out string url,
        out string error)
    {
        url = null;
        error = null;

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            error = "Alamat permintaan tidak valid.";
            return false;
        }

        string value = endpoint.Trim();
        Uri absoluteUri;

        if (Uri.TryCreate(value, UriKind.Absolute, out absoluteUri))
        {
            if (absoluteUri.Scheme != Uri.UriSchemeHttp &&
                absoluteUri.Scheme != Uri.UriSchemeHttps)
            {
                error = "Protokol alamat server tidak didukung.";
                return false;
            }

            url = absoluteUri.ToString();
            return true;
        }

        if (string.IsNullOrWhiteSpace(BASE_URL))
        {
            error = "Alamat server belum dikonfigurasi.";
            return false;
        }

        url = BASE_URL.TrimEnd('/') + "/" + value.TrimStart('/');

        Uri finalUri;
        if (!Uri.TryCreate(url, UriKind.Absolute, out finalUri))
        {
            url = null;
            error = "Alamat permintaan tidak dapat dibaca.";
            return false;
        }

        if (finalUri.Scheme != Uri.UriSchemeHttp &&
            finalUri.Scheme != Uri.UriSchemeHttps)
        {
            url = null;
            error = "Protokol alamat server tidak didukung.";
            return false;
        }

        return true;
    }

    private bool CanStartRequest(Action<string> onError)
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            onError?.Invoke("Layanan API sedang tidak aktif. Silakan coba lagi.");
            return false;
        }

        return true;
    }

    // ============================================================
    // GET REQUEST
    // ============================================================

    public void GetRequest(
        string endpoint,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string url;
        string error;

        if (!TryBuildApiUrl(endpoint, out url, out error))
        {
            onError?.Invoke(error);
            return;
        }

        if (!CanStartRequest(onError))
            return;

        StartCoroutine(SendGet(url, onSuccess, onError));
    }

    // ============================================================
    // GET REQUEST WITH OFFLINE CACHE
    // ============================================================

    public void GetRequestWithCache(
        string endpoint,
        string cachedData,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string url;
        string error;

        if (!TryBuildApiUrl(endpoint, out url, out error))
        {
            if (!string.IsNullOrEmpty(cachedData))
            {
                onSuccess?.Invoke(cachedData);
                return;
            }

            onError?.Invoke(error);
            return;
        }

        if (!CanStartRequest(onError))
            return;

        StartCoroutine(SendGetWithCache(
            url,
            cachedData,
            onSuccess,
            onError
        ));
    }

    private IEnumerator SendGetWithCache(
        string url,
        string cachedData,
        Action<string> onSuccess,
        Action<string> onError)
    {
        bool succeeded = false;
        string responseData = null;
        string failureMessage = null;

        yield return StartCoroutine(SendGet(
            url,
            response =>
            {
                succeeded = true;
                responseData = response;
            },
            error =>
            {
                failureMessage = error;
            }
        ));

        if (succeeded)
        {
            onSuccess?.Invoke(responseData);
            yield break;
        }

        if (!string.IsNullOrEmpty(cachedData))
        {
            Debug.LogWarning("[API] Server tidak tersedia. Menggunakan data cache.");

            NetworkManager.Instance?.ShowToast(
                "Mode offline: menampilkan data terakhir tersimpan.",
                false
            );

            onSuccess?.Invoke(cachedData);
            yield break;
        }

        onError?.Invoke(
            string.IsNullOrEmpty(failureMessage)
                ? "Data tidak dapat dimuat. Periksa koneksi internet."
                : failureMessage
        );
    }

    // ============================================================
    // GET REQUEST WITH RETRY
    // ============================================================

    public void GetRequestWithRetry(
        string endpoint,
        Transform canvas,
        Action<string> onSuccess,
        Action<string> onError,
        int maxRetry = 3)
    {
        string url;
        string error;

        if (!TryBuildApiUrl(endpoint, out url, out error))
        {
            onError?.Invoke(error);
            return;
        }

        if (!CanStartRequest(onError))
            return;

        maxRetry = Mathf.Max(1, maxRetry);

        StartCoroutine(RetryCoroutine(
            url,
            canvas,
            onSuccess,
            onError,
            maxRetry
        ));
    }

    private IEnumerator RetryCoroutine(
        string url,
        Transform canvas,
        Action<string> onSuccess,
        Action<string> onError,
        int maxRetry)
    {
        for (int attempt = 1; attempt <= maxRetry; attempt++)
        {
            bool succeeded = false;
            string responseData = null;
            string failureMessage = null;

            yield return StartCoroutine(SendGet(
                url,
                response =>
                {
                    succeeded = true;
                    responseData = response;
                },
                error =>
                {
                    failureMessage = error;
                }
            ));

            if (succeeded)
            {
                onSuccess?.Invoke(responseData);
                yield break;
            }

            if (attempt < maxRetry)
            {
                if (canvas != null)
                {
                    NetworkManager.Instance?.ShowToast(
                        "Gagal, mencoba ulang... (" + attempt + "/" + maxRetry + ")",
                        true
                    );
                }

                yield return new WaitForSecondsRealtime(2f);
            }
            else
            {
                onError?.Invoke(
                    string.IsNullOrEmpty(failureMessage)
                        ? "Gagal setelah " + maxRetry + " percobaan."
                        : failureMessage
                );
            }
        }
    }

    // ============================================================
    // GET REQUEST COROUTINE
    // ============================================================

    private IEnumerator SendGet(
        string url,
        Action<string> onSuccess,
        Action<string> onError)
    {
        UnityWebRequest request = null;
        string setupError = null;

        try
        {
            request = UnityWebRequest.Get(url);
        }
        catch (Exception ex)
        {
            setupError = ex.Message;
        }

        if (request == null)
        {
            Debug.LogWarning("[API] GET tidak dapat disiapkan: " + setupError);
            onError?.Invoke("Permintaan ke server tidak dapat disiapkan.");
            yield break;
        }

        using (request)
        {
            if (!TryPrepareRequest(request, DEFAULT_TIMEOUT, out setupError))
            {
                Debug.LogWarning("[API] GET konfigurasi gagal: " + setupError);
                onError?.Invoke("Permintaan ke server tidak dapat disiapkan.");
                yield break;
            }

            UnityWebRequestAsyncOperation operation = null;

            try
            {
                operation = request.SendWebRequest();
            }
            catch (Exception ex)
            {
                setupError = ex.Message;
            }

            if (operation == null)
            {
                Debug.LogWarning("[API] GET gagal dimulai: " + setupError);
                onError?.Invoke("Koneksi ke server gagal dimulai. Silakan coba lagi.");
                yield break;
            }

            yield return operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler != null
                    ? request.downloadHandler.text
                    : string.Empty;

                onSuccess?.Invoke(response);
                yield break;
            }

            LogRequestFailure("GET", request);
            onError?.Invoke(GetFriendlyError(
                request.result,
                request.responseCode,
                request.error
            ));
        }
    }

    // ============================================================
    // POST REQUEST
    // ============================================================

    public void PostRequest(
        string endpoint,
        string jsonData,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string url;
        string error;

        if (!TryBuildApiUrl(endpoint, out url, out error))
        {
            onError?.Invoke(error);
            return;
        }

        if (string.IsNullOrWhiteSpace(jsonData))
        {
            onError?.Invoke("Data permintaan kosong atau tidak valid.");
            return;
        }

        if (!CanStartRequest(onError))
            return;

        StartCoroutine(SendPost(url, jsonData, onSuccess, onError));
    }

    private IEnumerator SendPost(
        string url,
        string jsonData,
        Action<string> onSuccess,
        Action<string> onError)
    {
        UnityWebRequest request = null;
        string setupError = null;

        try
        {
            request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }
        catch (Exception ex)
        {
            setupError = ex.Message;
        }

        if (request == null || !string.IsNullOrEmpty(setupError))
        {
            if (request != null)
                request.Dispose();

            Debug.LogWarning("[API] POST tidak dapat disiapkan: " + setupError);
            onError?.Invoke("Permintaan ke server tidak dapat disiapkan.");
            yield break;
        }

        using (request)
        {
            if (!TryPrepareRequest(request, DEFAULT_TIMEOUT, out setupError))
            {
                Debug.LogWarning("[API] POST konfigurasi gagal: " + setupError);
                onError?.Invoke("Permintaan ke server tidak dapat disiapkan.");
                yield break;
            }

            UnityWebRequestAsyncOperation operation = null;

            try
            {
                operation = request.SendWebRequest();
            }
            catch (Exception ex)
            {
                setupError = ex.Message;
            }

            if (operation == null)
            {
                Debug.LogWarning("[API] POST gagal dimulai: " + setupError);
                onError?.Invoke("Koneksi ke server gagal dimulai. Silakan coba lagi.");
                yield break;
            }

            yield return operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler != null
                    ? request.downloadHandler.text
                    : string.Empty;

                onSuccess?.Invoke(response);
                yield break;
            }

            LogRequestFailure("POST", request);
            onError?.Invoke(GetFriendlyError(
                request.result,
                request.responseCode,
                request.error
            ));
        }
    }

    // ============================================================
    // REQUEST CONFIGURATION AND ERROR HANDLING
    // ============================================================

    private bool TryPrepareRequest(
        UnityWebRequest request,
        int timeoutSeconds,
        out string error)
    {
        error = null;

        if (request == null)
        {
            error = "Request object null.";
            return false;
        }

        try
        {
            request.timeout = Mathf.Max(1, timeoutSeconds);

            string token = PlayerPrefs.GetString("UserToken", string.Empty);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.SetRequestHeader(
                    "Authorization",
                    "Bearer " + token.Trim()
                );
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private void LogRequestFailure(string method, UnityWebRequest request)
    {
        if (request == null)
            return;

        // HTTP/network failure adalah kondisi runtime yang harus ditangani,
        // bukan exception Unity. Gunakan Warning agar tidak menjadi error merah.
        Debug.LogWarning(
            "[API] " + method + " gagal. HTTP=" + request.responseCode +
            ", Result=" + request.result +
            ", Error=" + (string.IsNullOrEmpty(request.error) ? "Tidak diketahui" : request.error)
        );
    }

    private string GetFriendlyError(
        UnityWebRequest.Result result,
        long responseCode,
        string raw)
    {
        if (result == UnityWebRequest.Result.ConnectionError)
        {
            return "Tidak dapat terhubung ke server. Periksa koneksi internet kamu.";
        }

        if (result == UnityWebRequest.Result.ProtocolError)
        {
            if (responseCode == 401)
                return "Sesi login tidak valid. Silakan login kembali.";

            if (responseCode == 403)
                return "Kamu tidak memiliki izin untuk melakukan tindakan ini.";

            if (responseCode == 404)
                return "Data atau layanan yang diminta tidak ditemukan.";

            if (responseCode == 429)
                return "Terlalu banyak permintaan. Tunggu sebentar lalu coba lagi.";

            if (responseCode >= 500)
                return "Server sedang mengalami gangguan. Coba lagi nanti.";

            return "Server merespons dengan error. Coba lagi nanti.";
        }

        if (result == UnityWebRequest.Result.DataProcessingError)
        {
            return "Terjadi kesalahan saat memproses respons server.";
        }

        return string.IsNullOrWhiteSpace(raw)
            ? "Terjadi kesalahan saat menghubungi server."
            : raw;
    }

    // ============================================================
    // WEBSOCKET BATTLE
    // ============================================================

    public async void ConnectBattleWS(
        string roomCode,
        int playerID,
        string playerName,
        bool isHost,
        Action<string> onMessage,
        Action onConnected,
        Action onDisconnected)
    {
        int connectionId = ++wsConnectionVersion;

        WebSocket previousSocket = battleSocket;
        battleSocket = null;
        wsConnected = false;

        if (previousSocket != null)
        {
            try
            {
                await previousSocket.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[WS] Koneksi sebelumnya gagal ditutup: " + ex.Message);
            }
        }

        if (connectionId != wsConnectionVersion || this == null)
            return;

        if (string.IsNullOrWhiteSpace(WS_URL) ||
            string.IsNullOrWhiteSpace(roomCode) ||
            playerID <= 0)
        {
            Debug.LogWarning("[WS] Data koneksi battle tidak lengkap.");
            InvokeDisconnected(onDisconnected);
            return;
        }

        string safeRoomCode = Uri.EscapeDataString(roomCode.Trim());
        string safePlayerName = Uri.EscapeDataString(
            string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim()
        );
        string hostValue = isHost ? "true" : "false";

        string url = GetWsUrl() + "/battle/" + safeRoomCode + "/" +
                     playerID + "/" + safePlayerName + "/" + hostValue;

        WebSocket socket = null;
        string creationError = null;

        try
        {
            socket = new WebSocket(url);
        }
        catch (Exception ex)
        {
            creationError = ex.Message;
        }

        if (socket == null)
        {
            Debug.LogWarning("[WS] Socket tidak dapat dibuat: " + creationError);
            InvokeDisconnected(onDisconnected);
            return;
        }

        if (connectionId != wsConnectionVersion)
        {
            try
            {
                await socket.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[WS] Socket lama gagal ditutup: " + ex.Message);
            }

            return;
        }

        battleSocket = socket;
        bool disconnectNotified = false;

        socket.OnOpen += () =>
        {
            if (!IsCurrentSocket(socket, connectionId))
                return;

            wsConnected = true;
            Debug.Log("[WS] Battle berhasil terhubung.");
            InvokeConnected(onConnected);
        };

        socket.OnMessage += bytes =>
        {
            if (!IsCurrentSocket(socket, connectionId) || bytes == null)
                return;

            string message = Encoding.UTF8.GetString(bytes);
            InvokeMessage(onMessage, message);
        };

        socket.OnError += error =>
        {
            if (!IsCurrentSocket(socket, connectionId))
                return;

            wsConnected = false;
            Debug.LogWarning("[WS] Battle mengalami gangguan: " + error);
        };

        socket.OnClose += code =>
        {
            if (!IsCurrentSocket(socket, connectionId))
                return;

            wsConnected = false;
            battleSocket = null;

            Debug.Log("[WS] Battle terputus. Code: " + code);

            if (!disconnectNotified)
            {
                disconnectNotified = true;
                InvokeDisconnected(onDisconnected);
            }
        };

        try
        {
            await socket.Connect();
        }
        catch (Exception ex)
        {
            bool isCurrentConnection = IsCurrentSocket(socket, connectionId);

            if (isCurrentConnection)
            {
                wsConnected = false;
                battleSocket = null;
            }

            // Jangan biarkan koneksi lama mengubah status screen baru.
            if (connectionId == wsConnectionVersion)
            {
                Debug.LogWarning("[WS] Gagal menghubungkan battle: " + ex.Message);

                if (!disconnectNotified)
                {
                    disconnectNotified = true;
                    InvokeDisconnected(onDisconnected);
                }
            }

            try
            {
                await socket.Close();
            }
            catch (Exception closeEx)
            {
                Debug.LogWarning("[WS] Cleanup socket gagal: " + closeEx.Message);
            }
        }
    }

    private bool IsCurrentSocket(WebSocket socket, int connectionId)
    {
        return socket != null &&
               battleSocket == socket &&
               wsConnectionVersion == connectionId;
    }

    public async void SendBattleWSMessage(string json)
    {
        WebSocket socket = battleSocket;

        if (socket == null || !wsConnected || string.IsNullOrWhiteSpace(json))
            return;

        try
        {
            await socket.SendText(json);
        }
        catch (Exception ex)
        {
            if (battleSocket == socket)
                wsConnected = false;

            Debug.LogWarning("[WS] Gagal mengirim pesan battle: " + ex.Message);
        }
    }

    public async void DisconnectBattleWS()
    {
        WebSocket socket = battleSocket;

        if (socket == null)
        {
            wsConnected = false;
            return;
        }

        wsConnected = false;

        try
        {
            await socket.Close();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[WS] Gagal memutus koneksi battle: " + ex.Message);
        }
        finally
        {
            if (battleSocket == socket)
                battleSocket = null;
        }
    }

    public bool IsWSConnected()
    {
        return wsConnected && battleSocket != null;
    }

    // ============================================================
    // SAFE WEBSOCKET CALLBACKS
    // ============================================================

    private void InvokeMessage(Action<string> callback, string message)
    {
        if (callback == null)
            return;

        try
        {
            callback(message);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[WS] Callback message gagal: " + ex.Message);
        }
    }

    private void InvokeConnected(Action callback)
    {
        if (callback == null)
            return;

        try
        {
            callback();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[WS] Callback connected gagal: " + ex.Message);
        }
    }

    private void InvokeDisconnected(Action callback)
    {
        if (callback == null)
            return;

        try
        {
            callback();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[WS] Callback disconnected gagal: " + ex.Message);
        }
    }
}
