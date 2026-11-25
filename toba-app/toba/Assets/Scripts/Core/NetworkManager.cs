using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    public bool IsOnline { get; private set; } = true;

    private GameObject _banner;
    private GameObject _offlineOverlay;
    private Transform _offlineSpinner;
    private Coroutine _offlineSpinnerCoroutine;
    private Transform _canvas;
    private Coroutine _checkLoopCoroutine;

    private readonly List<GameObject> _activeToasts =
        new List<GameObject>();

    private const float CHECK_INTERVAL = 5f;
    private const int PING_TIMEOUT = 5;

    private string _pingUrl = "http://43.173.13.187/api/";
    private bool _isShuttingDown;

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
    }

    private void OnEnable()
    {
        if (Instance == this &&
            _canvas != null &&
            !_isShuttingDown &&
            _checkLoopCoroutine == null)
        {
            _checkLoopCoroutine = StartCoroutine(CheckLoop());
        }
    }

    private void OnDisable()
    {
        if (_checkLoopCoroutine != null)
        {
            StopCoroutine(_checkLoopCoroutine);
            _checkLoopCoroutine = null;
        }
    }

    private void OnApplicationQuit()
    {
        _isShuttingDown = true;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused || Instance != this || _canvas == null ||
            _isShuttingDown || !isActiveAndEnabled)
            return;

        if (_checkLoopCoroutine != null)
        {
            StopCoroutine(_checkLoopCoroutine);
            _checkLoopCoroutine = null;
        }

        _checkLoopCoroutine = StartCoroutine(CheckLoop());
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        _isShuttingDown = true;

        if (_checkLoopCoroutine != null)
        {
            StopCoroutine(_checkLoopCoroutine);
            _checkLoopCoroutine = null;
        }

        StopAllCoroutines();

        DestroyBanner();
        HideOfflineOverlay();
        DestroyAllToasts();

        _canvas = null;
        Instance = null;
    }

    // ============================================================
    // INITIALIZATION
    // ============================================================

    public void Init(Transform canvas, string backendUrl)
    {
        if (_isShuttingDown)
            return;

        if (canvas != null)
            _canvas = canvas;

        if (!string.IsNullOrWhiteSpace(backendUrl))
        {
            string normalizedUrl;
            string error;

            if (TryBuildPingUrl(backendUrl, out normalizedUrl, out error))
            {
                _pingUrl = normalizedUrl;
            }
            else
            {
                Debug.LogWarning(
                    "[Network] URL pemeriksaan koneksi tidak valid: " + error
                );
            }
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
            SetOnlineState(false);

        // Hanya satu loop pemeriksaan koneksi yang boleh berjalan.
        if (_checkLoopCoroutine != null)
        {
            StopCoroutine(_checkLoopCoroutine);
            _checkLoopCoroutine = null;
        }

        if (IsOnline)
        {
            HideBanner();
            HideOfflineOverlay();
        }
        else
        {
            DestroyBanner();
            ShowOfflineOverlay();
        }

        if (isActiveAndEnabled && _checkLoopCoroutine == null)
            _checkLoopCoroutine = StartCoroutine(CheckLoop());
    }

    private bool TryBuildPingUrl(
        string backendUrl,
        out string result,
        out string error)
    {
        result = null;
        error = null;

        if (string.IsNullOrWhiteSpace(backendUrl))
        {
            error = "URL server kosong.";
            return false;
        }

        string value = backendUrl.Trim();
        Uri uri;

        if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
        {
            error = "Format URL server tidak valid.";
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps)
        {
            error = "URL harus menggunakan HTTP atau HTTPS.";
            return false;
        }

        result = value.TrimEnd('/') + "/";
        return true;
    }

    // ============================================================
    // CONNECTION CHECK LOOP
    // ============================================================

    private IEnumerator CheckLoop()
    {
        while (!_isShuttingDown && isActiveAndEnabled)
        {
            yield return Ping();

            if (_isShuttingDown || !isActiveAndEnabled)
                yield break;

            yield return new WaitForSecondsRealtime(CHECK_INTERVAL);
        }

        _checkLoopCoroutine = null;
    }

    // ============================================================
    // PING
    // ============================================================

    public IEnumerator Ping(Action<bool> callback = null)
    {
        if (_isShuttingDown ||
            !isActiveAndEnabled ||
            string.IsNullOrWhiteSpace(_pingUrl))
        {
            if (!_isShuttingDown)
                SetOnlineState(false);

            callback?.Invoke(false);
            yield break;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            SetOnlineState(false);
            callback?.Invoke(false);
            yield break;
        }

        UnityWebRequest request = null;
        UnityWebRequestAsyncOperation operation = null;
        bool requestStartFailed = false;
        string startError = null;

        // Jangan meletakkan yield di dalam blok try-catch.
        try
        {
            request = UnityWebRequest.Get(_pingUrl);
            request.timeout = PING_TIMEOUT;
            operation = request.SendWebRequest();
        }
        catch (Exception ex)
        {
            if (request != null)
                request.Dispose();

            request = null;
            requestStartFailed = true;
            startError = ex.Message;
        }

        if (requestStartFailed)
        {
            Debug.LogWarning(
                "[Network] Tidak dapat memulai ping: " + startError
            );

            SetOnlineState(false);
            callback?.Invoke(false);
            yield break;
        }

        if (operation == null)
        {
            if (request != null)
                request.Dispose();

            SetOnlineState(false);
            callback?.Invoke(false);
            yield break;
        }

        try
        {
            yield return operation;

            if (_isShuttingDown || request == null)
                yield break;

            bool online =
                request.result == UnityWebRequest.Result.Success;

            SetOnlineState(online);
            callback?.Invoke(online);
        }
        finally
        {
            if (request != null)
                request.Dispose();
        }
    }

    // ============================================================
    // CONNECTION STATE
    // ============================================================

    private void SetOnlineState(bool online)
    {
        if (IsOnline == online)
            return;

        IsOnline = online;
        OnStatusChanged(online);
    }

    private void OnStatusChanged(bool online)
    {
        if (online)
        {
            HideOfflineOverlay();
            HideBanner();
        }
        else
        {
            DestroyBanner();
            ShowOfflineOverlay();
        }
    }

    // Overlay koneksi berada di atas seluruh layar dan tidak bergantung
    // pada screen aktif. Ping tetap berjalan di belakang overlay.
    private void ShowOfflineOverlay()
    {
        if (_canvas == null)
            return;

        if (_offlineOverlay != null && _offlineOverlay.transform.parent == _canvas)
            return;

        HideOfflineOverlay();

        _offlineOverlay = new GameObject("OfflineConnectionOverlay");
        _offlineOverlay.transform.SetParent(_canvas, false);
        _offlineOverlay.transform.SetAsLastSibling();

        RectTransform overlayRect = _offlineOverlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image shade = _offlineOverlay.AddComponent<Image>();
        shade.sprite = UIBuilder.GetDefaultSprite();
        shade.color = new Color(0.015f, 0.025f, 0.02f, 0.78f);
        shade.raycastTarget = true;

        GameObject panel = new GameObject("ConnectionPanel");
        panel.transform.SetParent(_offlineOverlay.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(850f, 430f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        Sprite wood = TOBATheme.LoadSprite("panel_wood");
        panelImage.sprite = wood != null ? wood : UIBuilder.GetDefaultSprite();
        panelImage.type = wood != null ? Image.Type.Sliced : Image.Type.Simple;
        panelImage.color = new Color(0.20f, 0.10f, 0.045f, 0.98f);
        panelImage.raycastTarget = false;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = TOBATheme.BatakGold;
        outline.effectDistance = new Vector2(5f, 5f);

        GameObject spinner = UIBuilder.CreateTextSized(
            panel.transform,
            "◌",
            100,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(105f, 105f),
            new Vector2(0f, 115f)
        );
        spinner.name = "ConnectionSpinner";
        TextMeshProUGUI spinnerText = spinner.GetComponent<TextMeshProUGUI>();
        if (spinnerText != null)
            spinnerText.raycastTarget = false;
        _offlineSpinner = spinner.transform;

        UIBuilder.CreateTextSized(
            panel.transform,
            "PERIKSA KONEKSI PERANGKAT ANDA",
            32,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(790f, 70f),
            new Vector2(0f, 15f)
        );

        UIBuilder.CreateTextSized(
            panel.transform,
            "Menunggu koneksi internet dan server TOBA...",
            25,
            TOBATheme.TextLight,
            TextAlignmentOptions.Center,
            new Vector2(790f, 65f),
            new Vector2(0f, -65f)
        );

        _offlineSpinnerCoroutine = StartCoroutine(AnimateOfflineSpinner());
    }

    private IEnumerator AnimateOfflineSpinner()
    {
        while (_offlineSpinner != null && _offlineOverlay != null)
        {
            _offlineSpinner.localEulerAngles += new Vector3(
                0f, 0f, -250f * Time.unscaledDeltaTime
            );
            yield return null;
        }
        _offlineSpinnerCoroutine = null;
    }

    private void HideOfflineOverlay()
    {
        if (_offlineSpinnerCoroutine != null)
        {
            StopCoroutine(_offlineSpinnerCoroutine);
            _offlineSpinnerCoroutine = null;
        }

        if (_offlineOverlay != null)
            Destroy(_offlineOverlay);

        _offlineOverlay = null;
        _offlineSpinner = null;
    }

    // ============================================================
    // OFFLINE BANNER
    // Desain asli dipertahankan.
    // ============================================================

    public void ShowBanner()
    {
        if (_banner != null || _canvas == null)
            return;

        _banner = new GameObject("OfflineBanner");
        _banner.transform.SetParent(_canvas, false);
        _banner.transform.SetAsLastSibling();

        RectTransform rt =
            _banner.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0f, 0.92f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = _banner.AddComponent<Image>();
        img.sprite = UIBuilder.GetDefaultSprite();
        img.color = new Color(0.75f, 0.1f, 0.1f, 0.97f);
        img.raycastTarget = false;

        GameObject txt = new GameObject("T");
        txt.transform.SetParent(_banner.transform, false);

        RectTransform rtT =
            txt.AddComponent<RectTransform>();

        rtT.anchorMin = Vector2.zero;
        rtT.anchorMax = Vector2.one;
        rtT.offsetMin = Vector2.zero;
        rtT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp =
            txt.AddComponent<TextMeshProUGUI>();

        tmp.text =
            "⚠  Server TOBA tidak dapat dijangkau. Fitur online sementara tidak tersedia.";

        tmp.fontSize = 26;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Normal;
        tmp.raycastTarget = false;
    }

    public void HideBanner()
    {
        DestroyBanner();
    }

    private void DestroyBanner()
    {
        if (_banner == null)
        {
            _banner = null;
            return;
        }

        GameObject oldBanner = _banner;
        _banner = null;
        Destroy(oldBanner);
    }

    // ============================================================
    // TOAST
    // Desain asli dipertahankan.
    // ============================================================

    public void ShowToast(string msg, bool isError = true)
    {
        if (_isShuttingDown ||
            !isActiveAndEnabled ||
            _canvas == null ||
            string.IsNullOrWhiteSpace(msg))
        {
            return;
        }

        StartCoroutine(ToastCoroutine(msg, isError));
    }

    private IEnumerator ToastCoroutine(string msg, bool isError)
    {
        if (_canvas == null || _isShuttingDown)
            yield break;

        GameObject toast = new GameObject("Toast");
        toast.transform.SetParent(_canvas, false);
        toast.transform.SetAsLastSibling();

        _activeToasts.Add(toast);

        RectTransform rt =
            toast.AddComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 95f);
        rt.anchoredPosition = new Vector2(0f, -720f);

        Image img = toast.AddComponent<Image>();
        img.sprite = UIBuilder.GetDefaultSprite();

        img.color = isError
            ? new Color(0.1f, 0.1f, 0.1f, 0.95f)
            : new Color(0.1f, 0.4f, 0.1f, 0.95f);

        img.raycastTarget = false;

        Outline ol = toast.AddComponent<Outline>();
        ol.effectColor = isError ? TOBATheme.Error : TOBATheme.Success;
        ol.effectDistance = new Vector2(2f, 2f);

        GameObject txtGO = new GameObject("T");
        txtGO.transform.SetParent(toast.transform, false);

        RectTransform rtT =
            txtGO.AddComponent<RectTransform>();

        rtT.anchorMin = Vector2.zero;
        rtT.anchorMax = Vector2.one;
        rtT.offsetMin = Vector2.zero;
        rtT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp =
            txtGO.AddComponent<TextMeshProUGUI>();

        tmp.text = msg;
        tmp.fontSize = 32;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Normal;
        tmp.raycastTarget = false;

        CanvasGroup cg = toast.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Fade in memakai unscaled time agar tetap berjalan
        // ketika Time.timeScale bernilai 0.
        float elapsed = 0f;

        while (elapsed < 0.25f &&
               toast != null &&
               cg != null &&
               !_isShuttingDown)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / 0.25f);
            yield return null;
        }

        if (toast == null || cg == null || _isShuttingDown)
        {
            RemoveToastReference(toast);
            yield break;
        }

        cg.alpha = 1f;

        yield return new WaitForSecondsRealtime(2.5f);

        if (toast == null || cg == null || _isShuttingDown)
        {
            RemoveToastReference(toast);
            yield break;
        }

        // Fade out.
        elapsed = 0f;

        while (elapsed < 0.25f &&
               toast != null &&
               cg != null &&
               !_isShuttingDown)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = 1f - Mathf.Clamp01(elapsed / 0.25f);
            yield return null;
        }

        if (toast != null)
            Destroy(toast);

        RemoveToastReference(toast);
    }

    private void RemoveToastReference(GameObject toast)
    {
        for (int i = _activeToasts.Count - 1; i >= 0; i--)
        {
            GameObject item = _activeToasts[i];

            if (item == null || object.ReferenceEquals(item, toast))
                _activeToasts.RemoveAt(i);
        }
    }

    private void DestroyAllToasts()
    {
        for (int i = _activeToasts.Count - 1; i >= 0; i--)
        {
            GameObject toast = _activeToasts[i];

            if (toast != null)
                Destroy(toast);
        }

        _activeToasts.Clear();
    }
}
