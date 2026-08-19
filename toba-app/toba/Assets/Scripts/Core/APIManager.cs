using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;

    private string BASE_URL = "http://localhost:8080/api";
    private string WS_URL   = "ws://localhost:8080/ws";

    // WebSocket
    private WebSocket battleSocket;
    private bool wsConnected = false;

    private List<System.Action<string>> wsMessageHandlers =
        new List<System.Action<string>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[API] URL: " + BASE_URL);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (battleSocket != null)
            battleSocket.DispatchMessageQueue();
#endif
    }

    // ============================================================
    // GET
    // ============================================================

    public void GetRequest(
        string endpoint,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        StartCoroutine(
            SendGet(
                BASE_URL + endpoint,
                onSuccess,
                onError
            )
        );
    }

    IEnumerator SendGet(
        string url,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 10;

            req.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            yield return req.SendWebRequest();

            string response =
                req.downloadHandler != null
                    ? req.downloadHandler.text
                    : "";

            // Berhasil
            if (
                req.result ==
                UnityWebRequest.Result.Success
            )
            {
                onSuccess?.Invoke(response);
                yield break;
            }

            // HTTP error dari server:
            // 400, 401, 403, 404, 409, 422, 500, dst.
            // Kirim BODY response ke caller.
            if (
                req.responseCode >= 400 &&
                req.responseCode <= 599
            )
            {
                Debug.LogWarning(
                    "[API] GET HTTP " +
                    req.responseCode +
                    " -> " +
                    response
                );

                onError?.Invoke(response);
                yield break;
            }

            // Benar-benar error koneksi.
            Debug.LogWarning(
                "[API] GET Network Error -> " +
                req.error
            );

            onError?.Invoke(
                "NETWORK_ERROR"
            );
        }
    }

    // ============================================================
    // POST
    // ============================================================

    public void PostRequest(
        string endpoint,
        string jsonData,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        StartCoroutine(
            SendPost(
                BASE_URL + endpoint,
                jsonData,
                onSuccess,
                onError
            )
        );
    }

    IEnumerator SendPost(
        string url,
        string jsonData,
        System.Action<string> onSuccess,
        System.Action<string> onError)
    {
        using (
            UnityWebRequest req =
            new UnityWebRequest(
                url,
                "POST"
            )
        )
        {
            byte[] body =
                System.Text.Encoding.UTF8.GetBytes(
                    jsonData
                );

            req.uploadHandler =
                new UploadHandlerRaw(body);

            req.downloadHandler =
                new DownloadHandlerBuffer();

            req.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            req.timeout = 10;

            yield return req.SendWebRequest();

            string response =
                req.downloadHandler != null
                    ? req.downloadHandler.text
                    : "";

            // ====================================================
            // SUCCESS
            // ====================================================

            if (
                req.result ==
                UnityWebRequest.Result.Success
            )
            {
                Debug.Log(
                    "[API] POST SUCCESS: " +
                    url
                );

                onSuccess?.Invoke(response);
                yield break;
            }

            // ====================================================
            // HTTP ERROR DARI BACKEND
            // ====================================================

            if (
                req.responseCode >= 400 &&
                req.responseCode <= 599
            )
            {
                Debug.LogWarning(
                    "[API] POST HTTP " +
                    req.responseCode +
                    " -> " +
                    response
                );

                // PENTING:
                // Jangan kirim req.error.
                // Kirim JSON/body dari backend.
                onError?.Invoke(response);

                yield break;
            }

            // ====================================================
            // TRUE NETWORK ERROR
            // ====================================================

            Debug.LogWarning(
                "[API] POST NETWORK ERROR -> " +
                req.error
            );

            onError?.Invoke(
                "NETWORK_ERROR"
            );
        }
    }

    // ============================================================
    // WEBSOCKET BATTLE
    // TIDAK DIUBAH
    // ============================================================

    public async void ConnectBattleWS(
        string roomCode,
        int playerID,
        string playerName,
        bool isHost,
        System.Action<string> onMessage,
        System.Action onConnected,
        System.Action onDisconnected)
    {
        if (battleSocket != null)
            await battleSocket.Close();

        string isHostStr =
            isHost ? "true" : "false";

        string url =
            $"{WS_URL}/battle/{roomCode}/{playerID}/{playerName}/{isHostStr}";

        battleSocket =
            new WebSocket(url);

        battleSocket.OnOpen += () =>
        {
            wsConnected = true;

            Debug.Log(
                "[WS] Battle connected: " +
                url
            );

            onConnected?.Invoke();
        };

        battleSocket.OnMessage += (bytes) =>
        {
            string msg =
                System.Text.Encoding.UTF8
                    .GetString(bytes);

            Debug.Log(
                "[WS] Received: " +
                msg
            );

            onMessage?.Invoke(msg);
        };

        battleSocket.OnError += (err) =>
        {
            Debug.LogWarning(
                "[WS] Error: " +
                err
            );
        };

        battleSocket.OnClose += (code) =>
        {
            wsConnected = false;

            Debug.Log(
                "[WS] Disconnected. Code: " +
                code
            );

            onDisconnected?.Invoke();
        };

        await battleSocket.Connect();
    }

    public async void SendBattleWSMessage(
        string jsonMsg)
    {
        if (
            battleSocket != null &&
            wsConnected
        )
        {
            await battleSocket.SendText(
                jsonMsg
            );
        }
    }

    public async void DisconnectBattleWS()
    {
        if (battleSocket != null)
        {
            wsConnected = false;

            await battleSocket.Close();

            battleSocket = null;
        }
    }

    public bool IsWSConnected()
    {
        return wsConnected;
    }

    async void OnApplicationQuit()
    {
        if (battleSocket != null)
            await battleSocket.Close();
    }
}