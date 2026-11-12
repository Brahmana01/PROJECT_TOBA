
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public static class FloatingChallengeButton
{
    private static GameObject _btn;
    private static ChallengeData _cachedChallenge;

    private static readonly string[] TEXTS = {
        "Tantangan baru! <color=#D6A84B><sprite name=\"trivia\" tint=1></color>",
        "Ayo main! ✨",
        "Ada hadiah nih! 🎁",
        "Coba sekarang! 🔥",
        "Uji kemampuanmu! 💪"
    };

    public static void CheckAndShow(Transform canvas)
    {
        if (canvas == null)
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

        if (APIManager.Instance == null)
        {
            Hide();
            return;
        }

        APIManager.Instance.GetRequest(
            "/challenge/active?player_id=" + playerID,

            (response) =>
            {
                if (canvas == null)
                {
                    Hide();
                    return;
                }

                ActiveChallengeResponse res = null;

                try
                {
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        res = JsonUtility.FromJson<ActiveChallengeResponse>(response);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(
                        "[FloatingChallenge] JSON tidak valid: " + ex.Message
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

            (err) =>
            {
                // Kegagalan jaringan tidak menghentikan aplikasi.
            }
        );
    }

    static void Show(Transform canvas)
    {
        if (canvas == null || _cachedChallenge == null)
            return;

        if (_btn != null)
            return;

        bool onLeft = Random.value > 0.5f;

        _btn = new GameObject("FloatingChallengeBtn");
        _btn.transform.SetParent(canvas, false);
        _btn.transform.SetAsLastSibling();

        RectTransform rt = _btn.AddComponent<RectTransform>();

        rt.anchorMin = rt.anchorMax =
            new Vector2(onLeft ? 0f : 1f, 0.55f);

        rt.pivot = new Vector2(onLeft ? 0f : 1f, 0.5f);
        rt.sizeDelta = new Vector2(230, 100);
        rt.anchoredPosition = new Vector2(onLeft ? 15f : -15f, 0f);

        Sprite wp = TOBATheme.LoadSprite("btn_gold");

        Image img = _btn.AddComponent<Image>();
        img.sprite = wp ?? UIBuilder.GetDefaultSprite();
        img.type = wp != null ? Image.Type.Sliced : Image.Type.Simple;
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
            SoundManager.Instance?.PlaySFX("sfx_click");
            HapticManager.Instance?.Vibrate(HapticType.Light);

            OpenChallenge(canvas);
        });

        if (ScreenManager.Instance != null)
        {
            ScreenManager.Instance.StartCoroutine(PulseAnim(_btn.transform));
            ScreenManager.Instance.StartCoroutine(FadeIn(_btn));
        }
    }

    static void OpenChallenge(Transform canvas)
    {
        if (_cachedChallenge == null ||
            canvas == null ||
            ScreenManager.Instance == null)
        {
            Hide();
            return;
        }

        ChallengeData selectedChallenge = _cachedChallenge;

        ScreenManager.Instance.ChangeScreen(
            new ChallengeRouterScreen(canvas, selectedChallenge)
        );

        Hide();
    }

    public static void Hide()
    {
        if (_btn != null)
        {
            Object.Destroy(_btn);
            _btn = null;
        }

        _cachedChallenge = null;
    }

    static IEnumerator FadeIn(GameObject go)
    {
        if (go == null)
            yield break;

        CanvasGroup cg = go.AddComponent<CanvasGroup>();

        if (cg == null)
            yield break;

        cg.alpha = 0f;

        float t = 0f;

        while (t < 0.4f && go != null && cg != null)
        {
            t += Time.deltaTime;
            cg.alpha = t / 0.4f;

            yield return null;
        }
    }

    static IEnumerator PulseAnim(Transform t)
    {
        while (t != null)
        {
            float s = 1f + Mathf.Sin(Time.time * 2.5f) * 0.06f;

            t.localScale = Vector3.one * s;

            yield return null;
        }
    }
}
