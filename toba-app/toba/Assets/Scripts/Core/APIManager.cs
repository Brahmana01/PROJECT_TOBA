using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using NativeWebSocket;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    [Header("URLs")]
    private string BASE_URL = "http://43.173.13.187/api";
    private string WS_URL   = "ws://43.173.13.187/ws";

    // WebSocket
    private WebSocket battleSocket;
    private bool wsConnected = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[API] URL: " + BASE_URL);
        }
        else Destroy(gameObject);
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        battleSocket?.DispatchMessageQueue();
#endif
    }

    // ── URL CONFIG ────────────────────────────────────────
    public string GetBaseUrl() => BASE_URL;
    public string GetWsUrl()   => WS_URL;

    // ── GET REQUEST ───────────────────────────────────────
    public void GetRequest(string endpoint,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        StartCoroutine(SendGet(BASE_URL + endpoint, onSuccess, onError));
    }

    // GET dengan cache offline
    public void GetRequestWithCache(string endpoint, string cachedData,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        StartCoroutine(SendGetWithCache(BASE_URL + endpoint, cachedData,
            onSuccess, onError));
    }

    // Tambahkan di APIManager.cs
    public void GetRequestWithRetry(string endpoint, Transform canvas,
        System.Action<string> onSuccess,
        System.Action<string> onError,
        int maxRetry = 3)
    {
        StartCoroutine(RetryCoroutine(endpoint, canvas, onSuccess, onError, maxRetry));
    }

    IEnumerator RetryCoroutine(string endpoint, Transform canvas,
        System.Action<string> onSuccess,
        System.Action<string> onError, int maxRetry)
    {
        int attempt = 0;
        while (attempt < maxRetry)
        {
            bool done = false;
            string result = null;
            bool success = false;

            GetRequest(endpoint,
                (r) => { result = r; success = true; done = true; },
                (e) => { result = e; done = true; });

            yield return new WaitUntil(() => done);

            if (success)
            {
                onSuccess?.Invoke(result);
                yield break;
            }

            attempt++;
            if (attempt < maxRetry)
            {
                NetworkManager.Instance?.ShowToast(
                    $"Gagal, mencoba ulang... ({attempt}/{maxRetry})", true);

                yield return new WaitForSecondsRealtime(2f);
            }
        }

        onError?.Invoke("Gagal setelah " + maxRetry + " percobaan.");
    }

    IEnumerator SendGet(string url,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        using var req = UnityWebRequest.Get(url);
        req.timeout = 10;

        // ── AUTH TOKEN ──────────────────────────────────
        string token = PlayerPrefs.GetString("UserToken", "");

        if (!string.IsNullOrEmpty(token))
        {
            req.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
        {
            string body = req.downloadHandler != null
                ? req.downloadHandler.text
                : "";

            Debug.LogError(
                $"[API] POST GAGAL | URL={url} | " +
                $"HTTP={req.responseCode} | " +
                $"Result={req.result} | " +
                $"Error={req.error} | Body={body}"
            );

            string errMsg = GetFriendlyError(req.result, req.error);
            onError?.Invoke(errMsg);
        }
    }

    IEnumerator SendGetWithCache(string url, string cachedData,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        using var req = UnityWebRequest.Get(url);
        req.timeout = 8;

        // ── AUTH TOKEN ──────────────────────────────────
        string token = PlayerPrefs.GetString("UserToken", "");

        if (!string.IsNullOrEmpty(token))
        {
            req.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(req.downloadHandler.text);
        }
        else if (!string.IsNullOrEmpty(cachedData))
        {
            Debug.Log("[API] Offline — menggunakan data cache.");

            NetworkManager.Instance?.ShowToast(
                "Mode offline: menampilkan data terakhir tersimpan.", false);

            onSuccess?.Invoke(cachedData);
        }
        else
        {
            string errMsg = GetFriendlyError(req.result, req.error);
            onError?.Invoke(errMsg);
        }
    }

    // ── POST REQUEST ──────────────────────────────────────
    public void PostRequest(string endpoint, string jsonData,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        StartCoroutine(SendPost(BASE_URL + endpoint, jsonData, onSuccess, onError));
    }

    IEnumerator SendPost(string url, string jsonData,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        using var req = new UnityWebRequest(url, "POST");

        byte[] body = System.Text.Encoding.UTF8.GetBytes(jsonData);

        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/json");

        // ── AUTH TOKEN ──────────────────────────────────
        string token = PlayerPrefs.GetString("UserToken", "");

        if (!string.IsNullOrEmpty(token))
        {
            req.SetRequestHeader("Authorization", "Bearer " + token);
        }

        req.timeout = 10;

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
        {
            string errMsg = GetFriendlyError(req.result, req.error);
            onError?.Invoke(errMsg);
        }
    }

    // ── ERROR MESSAGE RAMAH ───────────────────────────────
    string GetFriendlyError(UnityWebRequest.Result result, string raw)
    {
        return result switch
        {
            UnityWebRequest.Result.ConnectionError =>
                "Tidak dapat terhubung ke server. Periksa koneksi internet kamu.",

            UnityWebRequest.Result.ProtocolError =>
                "Server merespons dengan error. Coba lagi nanti.",

            UnityWebRequest.Result.DataProcessingError =>
                "Terjadi kesalahan memproses data.",

            _ => "Terjadi kesalahan: " + raw
        };
    }

    // ── WEBSOCKET BATTLE ──────────────────────────────────
    public async void ConnectBattleWS(string roomCode, int playerID,
        string playerName, bool isHost,
        System.Action<string> onMessage,
        System.Action onConnected,
        System.Action onDisconnected)
    {
        if (battleSocket != null)
            await battleSocket.Close();

        string isHostStr = isHost ? "true" : "false";

        string url =
            $"{WS_URL}/battle/{roomCode}/{playerID}/{playerName}/{isHostStr}";

        battleSocket = new WebSocket(url);

        battleSocket.OnOpen += () =>
        {
            wsConnected = true;

            Debug.Log("[WS] Connected: " + url);

            onConnected?.Invoke();
        };

        battleSocket.OnMessage += (bytes) =>
        {
            string msg = System.Text.Encoding.UTF8.GetString(bytes);
            onMessage?.Invoke(msg);
        };

        battleSocket.OnError += (err) =>
            Debug.LogWarning("[WS] Error: " + err);

        battleSocket.OnClose += (code) =>
        {
            wsConnected = false;

            Debug.Log("[WS] Closed. Code: " + code);

            onDisconnected?.Invoke();
        };

        await battleSocket.Connect();
    }

    public async void SendBattleWSMessage(string json)
    {
        if (battleSocket != null && wsConnected)
            await battleSocket.SendText(json);
    }

    public async void DisconnectBattleWS()
    {
        if (battleSocket == null)
            return;

        wsConnected = false;

        await battleSocket.Close();

        battleSocket = null;
    }

    public bool IsWSConnected() => wsConnected;

    async void OnApplicationQuit()
    {
        if (battleSocket != null)
            await battleSocket.Close();
    }
}