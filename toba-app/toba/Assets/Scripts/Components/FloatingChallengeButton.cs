using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class FloatingChallengeButton
{
    private static GameObject _btn;
    private static Transform _buttonCanvas;
    private static ChallengeData _cachedChallenge;

    private static Coroutine _pulseCoroutine;
    private static Coroutine _fadeCoroutine;

    private static int _requestVersion;
    private static bool _isOpening;

    private static readonly string[] TEXTS =
    {
        "Tantangan baru! <color=#D6A84B><sprite name=\"trivia\" tint=1></color>",
        "Ayo main! ✨",
        "Ada hadiah nih! 🎁",
        "Coba sekarang! 🔥",
        "Uji kemampuanmu! 💪"
    };

    // ============================================================
    // CHECK ACTIVE CHALLENGE
    // ============================================================

    public static void CheckAndShow(Transform canvas)
    {
        // Setiap pemanggilan baru membatalkan callback request sebelumnya.
        int requestId = ++_requestVersion;

        if (canvas == null)
        {
            Hide();
            return;
        }

        ScreenManager manager = ScreenManager.Instance;

        if (manager == null ||
            manager.GetCurrentScreenName() != "MainMenu")
        {
            Hide();
            return;
        }

        int playerID = PlayerPrefs.GetInt("PlayerID");

        if (playerID <= 0)
        {
            Hide();
            return;
        }

        APIManager api = APIManager.Instance;

        if (api == null)
        {
            Hide();
            return;
        }

        api.GetRequest(
            "/challenge/active?player_id=" + playerID,

            response =>
            {
                // Abaikan response dari request lama.
                if (requestId != _requestVersion)
                    return;

                // Jangan membuat tombol pada screen yang sudah ditutup.
                if (!IsMainMenuActive(canvas))
                {
                    Hide();
                    return;
                }

                ActiveChallengeResponse res = null;

                try
                {
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        res = JsonUtility.FromJson<ActiveChallengeResponse>(
                            response
                        );
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(
                        "[FloatingChallenge] JSON tidak valid: " +
                        ex.Message
                    );

                    Hide();
                    return;
                }

                if (res == null ||
                    !res.has_challenge ||
                    res.is_completed ||
                    res.challenge == null ||
                    res.challenge.id <= 0 ||
                    string.IsNullOrWhiteSpace(res.challenge.game_type))
                {
                    Hide();
                    return;
                }

                _cachedChallenge = res.challenge;
                Show(canvas);
            },

            error =>
            {
                // Abaikan error dari request lama.
                if (requestId != _requestVersion)
                    return;

                // Jangan mengubah tombol pada screen yang sudah berganti.
                if (!IsMainMenuActive(canvas))
                {
                    Hide();
                    return;
                }

                // Jika request gagal, hilangkan tombol lama agar tidak
                // membuka challenge dari data yang sudah kedaluwarsa.
                RemoveButton();
                _cachedChallenge = null;
            }
        );
    }

    // ============================================================
    // CURRENT SCREEN VALIDATION
    // ============================================================

    private static bool IsMainMenuActive(Transform canvas)
    {
        if (canvas == null || ScreenManager.Instance == null)
            return false;

        return ScreenManager.Instance.GetCurrentScreenName() == "MainMenu";
    }

    // ============================================================
    // SHOW BUTTON
    // ============================================================

    private static void Show(Transform canvas)
    {
        if (canvas == null ||
            _cachedChallenge == null ||
            !IsMainMenuActive(canvas))
        {
            return;
        }

        // Jika tombol sudah ada pada canvas yang sama, cukup pertahankan.
        if (_btn != null && _buttonCanvas == canvas)
            return;

        // Bersihkan tombol lama jika berasal dari screen/canvas berbeda.
        if (_btn != null)
            RemoveButton();

        bool onLeft = Random.value > 0.5f;

        _btn = new GameObject("FloatingChallengeBtn");
        _buttonCanvas = canvas;

        _btn.transform.SetParent(canvas, false);
        _btn.transform.SetAsLastSibling();

        RectTransform rt = _btn.AddComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax =
            new Vector2(onLeft ? 0f : 1f, 0.55f);

        rt.pivot = new Vector2(onLeft ? 0f : 1f, 0.5f);
        rt.sizeDelta = new Vector2(230, 100);
        rt.anchoredPosition = new Vector2(
            onLeft ? 15f : -15f,
            0f
        );

        // Pertahankan sprite dan warna desain sebelumnya.
        Sprite wp = TOBATheme.LoadSprite("btn_gold");

        Image img = _btn.AddComponent<Image>();
        img.sprite = wp ?? UIBuilder.GetDefaultSprite();

        img.type = wp != null
            ? Image.Type.Sliced
            : Image.Type.Simple;

        img.color = wp != null
            ? Color.white
            : new Color(0.85f, 0.2f, 0.2f, 0.95f);

        img.raycastTarget = true;

        Outline ol = _btn.AddComponent<Outline>();
        ol.effectColor = new Color(1f, 1f, 1f, 0.6f);
        ol.effectDistance = new Vector2(2, 2);

        string label = TEXTS[Random.Range(0, TEXTS.Length)];

        GameObject textObj = UIBuilder.CreateTextSized(
            _btn.transform,
            label,
            24,
            TOBATheme.TextDark,
            TextAlignmentOptions.Center,
            new Vector2(210, 90),
            Vector2.zero
        );

        TextMeshProUGUI txt = textObj != null
            ? textObj.GetComponent<TextMeshProUGUI>()
            : null;

        if (txt != null)
        {
            txt.fontStyle = FontStyles.Bold;
            txt.textWrappingMode = TextWrappingModes.Normal;
            txt.raycastTarget = false;
        }

        Button btn = _btn.AddComponent<Button>();
        btn.targetGraphic = img;

        btn.onClick.AddListener(() =>
        {
            if (_isOpening)
                return;

            SoundManager.Instance?.PlaySFX("sfx_click");
            HapticManager.Instance?.Vibrate(HapticType.Light);

            OpenChallenge(canvas);
        });

        ScreenManager manager = ScreenManager.Instance;

        if (manager != null)
        {
            _pulseCoroutine = manager.StartCoroutine(
                PulseAnim(_btn.transform)
            );

            _fadeCoroutine = manager.StartCoroutine(
                FadeIn(_btn)
            );
        }
    }

    // ============================================================
    // OPEN CHALLENGE
    // ============================================================

    private static void OpenChallenge(Transform canvas)
    {
        if (_isOpening)
            return;

        ScreenManager manager = ScreenManager.Instance;

        if (_cachedChallenge == null ||
            canvas == null ||
            manager == null ||
            !IsMainMenuActive(canvas))
        {
            Hide();
            return;
        }

        ChallengeData selectedChallenge = _cachedChallenge;

        // Screen challenge harus menjadi child dari SafeArea/root canvas,
        // bukan child dari MainMenu screen yang akan dihancurkan.
        Transform parent = manager.canvasTransform;

        if (parent == null || parent == canvas)
            parent = canvas.parent;

        if (parent == null || parent == canvas)
        {
            Debug.LogWarning(
                "[FloatingChallenge] Parent screen tujuan tidak tersedia."
            );

            Hide();
            return;
        }

        _isOpening = true;

        // Hapus tombol dan batalkan callback request yang masih tertunda.
        Hide();

        // Hide mereset flag; aktifkan kembali selama navigasi berlangsung.
        _isOpening = true;

        manager.ChangeScreen(
            new ChallengeRouterScreen(parent, selectedChallenge)
        );

        _isOpening = false;
    }

    // ============================================================
    // HIDE / CLEANUP
    // ============================================================

    public static void Hide()
    {
        // Membatalkan semua callback request yang belum selesai.
        _requestVersion++;

        RemoveButton();

        _cachedChallenge = null;
        _isOpening = false;
    }

    private static void RemoveButton()
    {
        ScreenManager manager = ScreenManager.Instance;

        if (manager != null)
        {
            if (_pulseCoroutine != null)
                manager.StopCoroutine(_pulseCoroutine);

            if (_fadeCoroutine != null)
                manager.StopCoroutine(_fadeCoroutine);
        }

        _pulseCoroutine = null;
        _fadeCoroutine = null;

        if (_btn != null)
            Object.Destroy(_btn);

        _btn = null;
        _buttonCanvas = null;
    }

    // ============================================================
    // FADE ANIMATION
    // ============================================================

    private static IEnumerator FadeIn(GameObject go)
    {
        if (go == null)
            yield break;

        CanvasGroup cg = go.GetComponent<CanvasGroup>();

        if (cg == null)
            cg = go.AddComponent<CanvasGroup>();

        if (cg == null)
            yield break;

        cg.alpha = 0f;

        float t = 0f;

        while (t < 0.4f &&
               go != null &&
               cg != null &&
               _btn == go)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / 0.4f);

            yield return null;
        }

        if (go != null && cg != null && _btn == go)
            cg.alpha = 1f;

        _fadeCoroutine = null;
    }

    // ============================================================
    // PULSE ANIMATION
    // ============================================================

    private static IEnumerator PulseAnim(Transform target)
    {
        while (target != null &&
               _btn != null &&
               target.gameObject == _btn)
        {
            float scale =
                1f + Mathf.Sin(Time.time * 2.5f) * 0.06f;

            target.localScale = Vector3.one * scale;

            yield return null;
        }

        _pulseCoroutine = null;
    }
}
