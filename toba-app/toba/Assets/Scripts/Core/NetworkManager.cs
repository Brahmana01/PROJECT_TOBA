using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    public bool IsOnline { get; private set; } = true;

    private GameObject _banner;
    private Transform  _canvas;
    private const float CHECK_INTERVAL = 15f;
    private string _pingUrl = "http://43.173.13.187/api/";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public void Init(Transform canvas, string backendUrl)
    {
        _canvas  = canvas;
        _pingUrl = backendUrl.Replace("/api", "/api/");
        StartCoroutine(CheckLoop());
    }

    IEnumerator CheckLoop()
    {
        while (true)
        {
            yield return StartCoroutine(Ping());
            yield return new WaitForSecondsRealtime(CHECK_INTERVAL);
        }
    }

    public IEnumerator Ping(System.Action<bool> cb = null)
    {
        using var req = UnityWebRequest.Get(_pingUrl);
        req.timeout = 5;
        yield return req.SendWebRequest();
        bool ok = req.result == UnityWebRequest.Result.Success;
        if (ok != IsOnline) { IsOnline = ok; OnStatusChanged(ok); }
        cb?.Invoke(ok);
    }

    void OnStatusChanged(bool online)
    {
        if (online) HideBanner();
        else ShowBanner();
    }

    // ── BANNER OFFLINE ────────────────────────────────────
    public void ShowBanner()
    {
        if (_banner != null || _canvas == null) return;

        _banner = new GameObject("OfflineBanner");
        _banner.transform.SetParent(_canvas, false);
        _banner.transform.SetAsLastSibling();

        RectTransform rt = _banner.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.92f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Image img = _banner.AddComponent<Image>();
        img.sprite = UIBuilder.GetDefaultSprite();
        img.color  = new Color(0.75f, 0.1f, 0.1f, 0.97f);
        img.raycastTarget = false;

        GameObject txt = new GameObject("T");
        txt.transform.SetParent(_banner.transform, false);
        RectTransform rtT = txt.AddComponent<RectTransform>();
        rtT.anchorMin = Vector2.zero;
        rtT.anchorMax = Vector2.one;
        rtT.offsetMin = rtT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text      = "⚠  Tidak ada koneksi internet. Beberapa fitur tidak tersedia.";
        tmp.fontSize  = 26;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Normal;
        tmp.raycastTarget = false;
    }

    public void HideBanner()
    {
        if (_banner != null) { Object.Destroy(_banner); _banner = null; }
    }

    // ── TOAST ─────────────────────────────────────────────
    public void ShowToast(string msg, bool isError = true)
    {
        if (_canvas == null) return;
        StartCoroutine(ToastCoroutine(msg, isError));
    }

    IEnumerator ToastCoroutine(string msg, bool isError)
    {
        GameObject toast = new GameObject("Toast");
        toast.transform.SetParent(_canvas, false);
        toast.transform.SetAsLastSibling();

        RectTransform rt = toast.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900, 95);
        rt.anchoredPosition = new Vector2(0, -720f);

        Image img = toast.AddComponent<Image>();
        img.sprite = UIBuilder.GetDefaultSprite();
        img.color  = isError ? new Color(0.1f, 0.1f, 0.1f, 0.95f) : new Color(0.1f, 0.4f, 0.1f, 0.95f);
        img.raycastTarget = false;

        Outline ol = toast.AddComponent<Outline>();
        ol.effectColor    = isError ? TOBATheme.Error : TOBATheme.Success;
        ol.effectDistance = new Vector2(2, 2);

        GameObject txtGO = new GameObject("T");
        txtGO.transform.SetParent(toast.transform, false);
        RectTransform rtT = txtGO.AddComponent<RectTransform>();
        rtT.anchorMin = Vector2.zero; rtT.anchorMax = Vector2.one;
        rtT.offsetMin = rtT.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = msg; tmp.fontSize = 32; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Normal; tmp.raycastTarget = false;

        CanvasGroup cg = toast.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float t = 0f;
        while (t < 0.25f) { t += Time.deltaTime; cg.alpha = t / 0.25f; yield return null; }
        yield return new WaitForSecondsRealtime(2.5f);
        t = 0f;
        while (t < 0.25f) { t += Time.deltaTime; cg.alpha = 1f - t / 0.25f; yield return null; }
        Object.Destroy(toast);
    }
}