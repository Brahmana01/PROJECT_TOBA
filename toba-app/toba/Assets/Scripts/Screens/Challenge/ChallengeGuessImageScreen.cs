using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GuessImageQuestion
{
    public string image;
    public string answer;
    public List<string> options;
}

[System.Serializable]
public class GuessImageData
{
    public List<GuessImageQuestion> questions;
}

public class ChallengeGuessImageScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<GuessImageQuestion> questions;
    private int currentIndex = 0;
    private int correctCount = 0;

    private TextMeshProUGUI progressTxt;
    private GameObject imageArea;
    private GameObject optionsContainer;

    public ChallengeGuessImageScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeGuessImage") { challenge = ch; }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        UIBuilder.CreateBackground(screenObj.transform,
            TOBATheme.LoadSprite("bg_toba"));
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0, 0.04f, 0, 0.5f), Vector2.zero, Vector2.one);

        HeaderBar.Create(screenObj.transform, "TEBAK GAMBAR BUDAYA", () =>
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)));

        GuessImageData data = JsonUtility.FromJson<GuessImageData>(challenge.data_json);
        questions = data?.questions ?? new List<GuessImageQuestion>();

        if (questions.Count == 0)
        {
            EmptyState.Show(screenObj.transform, "<color=#F2E4C7><sprite name=\"gallery\" tint=1></color>",
                "Data tantangan kosong", "Hubungi admin");
            return;
        }

        // Progress
        progressTxt = UIBuilder.CreateTextSized(screenObj.transform,
            "1/" + questions.Count, 38, TOBATheme.BatakGold,
            TextAlignmentOptions.Center, new Vector2(300, 55),
            new Vector2(0, 740f)).GetComponent<TextMeshProUGUI>();
        progressTxt.fontStyle = FontStyles.Bold;
        progressTxt.raycastTarget = false;

        // Area gambar
        imageArea = new GameObject("ImageArea");
        imageArea.transform.SetParent(screenObj.transform, false);
        RectTransform rtI = imageArea.AddComponent<RectTransform>();
        rtI.anchorMin = rtI.anchorMax = new Vector2(0.5f, 0.5f);
        rtI.sizeDelta = new Vector2(700, 700);
        rtI.anchoredPosition = new Vector2(0, 250f);
        Image iImg = imageArea.AddComponent<Image>();
        iImg.sprite = UIBuilder.GetDefaultSprite();
        iImg.color  = new Color(0.15f, 0.3f, 0.5f, 0.8f);
        iImg.raycastTarget = false;
        Outline iOl = imageArea.AddComponent<Outline>();
        iOl.effectColor = TOBATheme.BatakGold;
        iOl.effectDistance = new Vector2(5, 5);

        // Opsi container
        optionsContainer = new GameObject("Options");
        optionsContainer.transform.SetParent(screenObj.transform, false);
        RectTransform rtO = optionsContainer.AddComponent<RectTransform>();
        rtO.anchorMin = new Vector2(0.03f, 0.08f);
        rtO.anchorMax = new Vector2(0.97f, 0.38f);
        rtO.offsetMin = rtO.offsetMax = Vector2.zero;

        ShowQuestion(0);
    }

    void ShowQuestion(int index)
    {
        if (screenObj == null || index >= questions.Count) return;
        currentIndex = index;
        var q = questions[index];

        if (progressTxt) progressTxt.text = (index + 1) + "/" + questions.Count;

        // Load gambar dari Resources
        Sprite imgSprite = TOBATheme.LoadSprite("gambar/" + q.image.Replace(".png",""));
        Image iImg = imageArea?.GetComponent<Image>();
        if (iImg != null)
            iImg.sprite = imgSprite ?? UIBuilder.GetDefaultSprite();

        // Teks placeholder kalau gambar tidak ada
        Transform existing = imageArea?.transform.Find("ImgLabel");
        if (existing != null) Object.Destroy(existing.gameObject);
        if (imgSprite == null)
        {
            GameObject lbl = UIBuilder.CreateTextSized(imageArea.transform,
                "[ " + q.answer + " ]", 38, new Color(1f,1f,1f,0.4f),
                TextAlignmentOptions.Center, new Vector2(660, 80), Vector2.zero);
            lbl.name = "ImgLabel";
            lbl.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        }

        // Bersihkan opsi lama
        if (optionsContainer)
            foreach (Transform c in optionsContainer.transform)
                Object.Destroy(c.gameObject);

        // Shuffle opsi
        var opts = new List<string>(q.options);
        for (int i = opts.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (opts[i], opts[j]) = (opts[j], opts[i]);
        }

        // Buat tombol opsi 2x2
        float btnW = 440f, btnH = 110f, gapX = 20f, gapY = 14f;
        for (int i = 0; i < Mathf.Min(opts.Count, 4); i++)
        {
            int col = i % 2, row = i / 2;
            float x = col == 0 ? -(btnW/2f + gapX/2f) : (btnW/2f + gapX/2f);
            float y = row == 0 ? (btnH/2f + gapY/2f) : -(btnH/2f + gapY/2f);
            string opt = opts[i];
            CreateOptionBtn(opt, new Vector2(x, y), btnW, btnH, q.answer);
        }
    }

    void CreateOptionBtn(string text, Vector2 pos, float w, float h, string correct)
    {
        Sprite wp = TOBATheme.LoadSprite("panel_wood");
        GameObject btn = new GameObject("Opt_" + text);
        btn.transform.SetParent(optionsContainer.transform, false);
        RectTransform rt = btn.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = pos;
        Image img = btn.AddComponent<Image>();
        img.sprite = wp ?? UIBuilder.GetDefaultSprite();
        img.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        img.color  = new Color(0.92f, 0.82f, 0.65f, 1f);
        img.raycastTarget = true;
        TextMeshProUGUI tmp = UIBuilder.CreateTextSized(btn.transform,
            text, 32, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(w-20, h), Vector2.zero).GetComponent<TextMeshProUGUI>();
        tmp.fontStyle = FontStyles.Normal; tmp.raycastTarget = false;
        Button b = btn.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(() => {
            bool right = text == correct;
            img.color = right ? TOBATheme.OptionCorrect : TOBATheme.OptionWrong;
            SoundManager.Instance?.PlaySFX(right ? "sfx_correct" : "sfx_wrong");
            HapticManager.Instance?.Vibrate(right ? HapticType.Success : HapticType.Error);
            if (right) correctCount++;
            // Disable semua tombol
            foreach (Transform c in optionsContainer.transform)
                c.GetComponent<Button>().interactable = false;
            ScreenManager.Instance.StartCoroutine(NextOrFinish());
        });
    }

    IEnumerator NextOrFinish()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        if (screenObj == null) yield break;
        int next = currentIndex + 1;
        if (next < questions.Count) ShowQuestion(next);
        else SubmitResult();
    }

    void SubmitResult()
    {
        APIManager.Instance.PostRequest("/challenge/submit",
            JsonUtility.ToJson(new SubmitChallengeRequest {
                player_id = PlayerPrefs.GetInt("PlayerID"),
                challenge_id = challenge.id, score = correctCount }),
            (r) => ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)),
            (e) => ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)));
    }
}