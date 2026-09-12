using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BackHandler : MonoBehaviour
{
    public static BackHandler Instance;
    private bool _dialogOpen = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || _dialogOpen) return;

        string screen = ScreenManager.Instance?.GetCurrentScreenName() ?? "";

        // Layar yang tidak boleh back
        bool isQuizActive = screen == "Quiz"
                         || screen == "TriviaQuiz"
                         || screen == "BattleQuiz";
        if (isQuizActive)
        {
            NetworkManager.Instance?.ShowToast(
                "Tidak bisa keluar saat kuis berlangsung!", true);
            HapticManager.Instance?.Vibrate(HapticType.Error);
            return;
        }

        // Layar utama → konfirmasi keluar
        bool isMainScreen = screen == "MainMenu"
                         || screen == "Welcome"
                         || screen == "Splash";
        if (isMainScreen)
        {
            ShowExitDialog();
            return;
        }

        // Layar lain → kembali
        ScreenManager.Instance?.GoBack();
    }

    void ShowExitDialog()
    {
        if (_dialogOpen) return;
        _dialogOpen = true;

        Transform canvas = ScreenManager.Instance?.canvasTransform;
        if (canvas == null) { _dialogOpen = false; return; }

        // Background gelap
        GameObject overlay = new GameObject("ExitDialog");
        overlay.transform.SetParent(canvas, false);
        overlay.transform.SetAsLastSibling();
        RectTransform rtO = overlay.AddComponent<RectTransform>();
        rtO.anchorMin = Vector2.zero;
        rtO.anchorMax = Vector2.one;
        rtO.offsetMin = rtO.offsetMax = Vector2.zero;
        Image bgImg = overlay.AddComponent<Image>();
        bgImg.sprite = UIBuilder.GetDefaultSprite();
        bgImg.color  = new Color(0, 0, 0, 0.72f);
        bgImg.raycastTarget = true;

        // Panel
        Sprite wp = TOBATheme.LoadSprite("panel_wood");
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);
        RectTransform rtP = panel.AddComponent<RectTransform>();
        rtP.anchorMin = rtP.anchorMax = new Vector2(0.5f, 0.5f);
        rtP.sizeDelta = new Vector2(840, 500);
        rtP.anchoredPosition = Vector2.zero;
        panel.transform.localScale = Vector3.zero;

        Image pImg = panel.AddComponent<Image>();
        pImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        pImg.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        pImg.color  = new Color(0.24f, 0.11f, 0.03f, 0.99f);
        pImg.raycastTarget = false;
        Outline ol = panel.AddComponent<Outline>();
        ol.effectColor    = TOBATheme.BatakGold;
        ol.effectDistance = new Vector2(5, 5);

        // Animasi scale in
        ScreenManager.Instance.StartCoroutine(
            UIBuilder.AnimateScaleIn(panel.transform, 0.22f));

        // Ikon
        UIBuilder.CreateTextSized(panel.transform, "🏠", 95,
            Color.white, TextAlignmentOptions.Center,
            new Vector2(200, 120), new Vector2(0, 135f))
            .GetComponent<TextMeshProUGUI>().raycastTarget = false;

        // Judul
        TextMeshProUGUI titleT = UIBuilder.CreateTextSized(panel.transform,
            "Keluar dari TOBA?", 50, TOBATheme.BatakGold,
            TextAlignmentOptions.Center, new Vector2(760, 72),
            new Vector2(0, 28f)).GetComponent<TextMeshProUGUI>();
        titleT.fontStyle = FontStyles.Bold;
        titleT.raycastTarget = false;

        // Pesan
        TextMeshProUGUI msgT = UIBuilder.CreateTextSized(panel.transform,
            "Progress kamu sudah tersimpan.\nYakin ingin keluar?",
            32, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(740, 90), new Vector2(0, -52f))
            .GetComponent<TextMeshProUGUI>();
        msgT.fontStyle        = FontStyles.Normal;
        msgT.textWrappingMode = TextWrappingModes.Normal;
        msgT.raycastTarget    = false;

        // Tombol YA
        GameObject btnYa = UIBuilder.CreateButton(panel.transform,
            "BtnYa", "YA, KELUAR",
            new Vector2(330, 112), TOBATheme.Error, () => {
                Object.Destroy(overlay);
                _dialogOpen = false;
                HapticManager.Instance?.Vibrate(HapticType.Medium);
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
        btnYa.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(-192f, -168f);

        // Tombol TIDAK
        GameObject btnTdak = UIBuilder.CreateButton(panel.transform,
            "BtnTidak", "LANJUT MAIN",
            new Vector2(330, 112), TOBATheme.BtnPrimary, () => {
                Object.Destroy(overlay);
                _dialogOpen = false;
            });
        btnTdak.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(192f, -168f);
    }
}