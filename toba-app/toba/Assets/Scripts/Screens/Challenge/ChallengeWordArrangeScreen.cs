using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WordArrangeItem
{
    public string scrambled;
    public string answer;
    public string hint;
}

[System.Serializable]
public class WordArrangeData
{
    public List<WordArrangeItem> words;
}

public class ChallengeWordArrangeScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<WordArrangeItem> words;
    private int currentIndex = 0;
    private int correctCount = 0;

    private TextMeshProUGUI progressTxt;
    private TextMeshProUGUI hintTxt;
    private TextMeshProUGUI answerDisplayTxt;
    private string currentAnswer = "";
    private List<GameObject> letterBtns = new List<GameObject>();

    public ChallengeWordArrangeScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeWordArrange") { challenge = ch; }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        UIBuilder.CreateBackground(screenObj.transform, TOBATheme.LoadSprite("bg_toba"));
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0, 0.04f, 0, 0.5f), Vector2.zero, Vector2.one);
        HeaderBar.Create(screenObj.transform, "SUSUN KATA BATAK", () =>
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)));

        WordArrangeData data = JsonUtility.FromJson<WordArrangeData>(challenge.data_json);
        words = data?.words ?? new List<WordArrangeItem>();
        if (words.Count == 0)
        {
            EmptyState.Show(screenObj.transform, "🔤", "Data kosong", "Hubungi admin");
            return;
        }

        progressTxt = UIBuilder.CreateTextSized(screenObj.transform,
            "1/" + words.Count, 38, TOBATheme.BatakGold,
            TextAlignmentOptions.Center, new Vector2(300, 55), new Vector2(0, 740f))
            .GetComponent<TextMeshProUGUI>();
        progressTxt.raycastTarget = false;

        // Area jawaban (kotak blank)
        Sprite wp = TOBATheme.LoadSprite("panel_wood");
        GameObject ansArea = new GameObject("AnswerArea");
        ansArea.transform.SetParent(screenObj.transform, false);
        RectTransform rtA = ansArea.AddComponent<RectTransform>();
        rtA.anchorMin = rtA.anchorMax = new Vector2(0.5f, 0.5f);
        rtA.sizeDelta = new Vector2(900, 130);
        rtA.anchoredPosition = new Vector2(0, 420f);
        Image aImg = ansArea.AddComponent<Image>();
        aImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        aImg.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        aImg.color  = new Color(0.15f, 0.08f, 0.02f, 0.9f);
        aImg.raycastTarget = false;
        Outline aOl = ansArea.AddComponent<Outline>();
        aOl.effectColor = TOBATheme.BatakGold; aOl.effectDistance = new Vector2(4,4);

        answerDisplayTxt = UIBuilder.CreateTextSized(ansArea.transform,
            "_ _ _ _", 56, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(860, 110), Vector2.zero).GetComponent<TextMeshProUGUI>();
        answerDisplayTxt.fontStyle = FontStyles.Bold;
        answerDisplayTxt.raycastTarget = false;

        // Hint
        hintTxt = UIBuilder.CreateTextSized(screenObj.transform,
            "", 30, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(800, 55), new Vector2(0, 260f))
            .GetComponent<TextMeshProUGUI>();
        hintTxt.fontStyle = FontStyles.Normal; hintTxt.raycastTarget = false;

        // Tombol hapus
        UIBuilder.CreateSmallButton(screenObj.transform, "BtnHapus", "⌫ HAPUS",
            new Vector2(240, 80), TOBATheme.WoodDark, TOBATheme.BatakGold, () => {
                if (currentAnswer.Length > 0)
                {
                    string removed = currentAnswer[currentAnswer.Length - 1].ToString();
                    currentAnswer = currentAnswer.Substring(0, currentAnswer.Length - 1);
                    UpdateAnswerDisplay();
                    // Aktifkan kembali tombol huruf yang sesuai
                    ReactivateLetter(removed);
                }
            }).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100f);

        ShowWord(0);
    }

    void ShowWord(int index)
    {
        if (screenObj == null || index >= words.Count) return;
        currentIndex = index;
        currentAnswer = "";
        var w = words[index];

        if (progressTxt) progressTxt.text = (index+1) + "/" + words.Count;
        if (hintTxt) hintTxt.text = "<color=#D6A84B><sprite name=\"fact\" tint=1></color> " + w.hint;
        UpdateAnswerDisplay();

        // Hapus tombol huruf lama
        foreach (var b in letterBtns) if (b) Object.Destroy(b);
        letterBtns.Clear();

        // Shuffle huruf
        char[] chars = w.scrambled.ToCharArray();
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i+1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        // Buat tombol per huruf
        float btnSize = 120f, gap = 14f;
        float totalW  = chars.Length * (btnSize + gap) - gap;
        Sprite btnSp  = TOBATheme.LoadSprite("btn_gold");

        for (int i = 0; i < chars.Length; i++)
        {
            string letter = chars[i].ToString();
            float x = -totalW/2f + i*(btnSize+gap) + btnSize/2f;

            GameObject lb = new GameObject("L_" + i);
            lb.transform.SetParent(screenObj.transform, false);
            RectTransform rt = lb.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(btnSize, btnSize);
            rt.anchoredPosition = new Vector2(x, -180f);
            Image img = lb.AddComponent<Image>();
            img.sprite = btnSp ?? UIBuilder.GetDefaultSprite();
            img.type   = btnSp != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color  = Color.white;
            img.raycastTarget = true;
            TextMeshProUGUI tmp = UIBuilder.CreateTextSized(lb.transform,
                letter, 52, TOBATheme.TextDark, TextAlignmentOptions.Center,
                new Vector2(btnSize, btnSize), Vector2.zero)
                .GetComponent<TextMeshProUGUI>();
            tmp.fontStyle = FontStyles.Bold; tmp.raycastTarget = false;
            Button b = lb.AddComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(() => {
                if (!b.interactable) return;
                currentAnswer += letter;
                b.interactable = false;
                img.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
                UpdateAnswerDisplay();
                CheckAnswer(words[currentIndex].answer);
            });
            letterBtns.Add(lb);
        }
    }

    void UpdateAnswerDisplay()
    {
        if (answerDisplayTxt == null) return;
        if (currentAnswer.Length == 0)
        {
            int len = words.Count > 0 ? words[currentIndex].answer.Length : 4;
            answerDisplayTxt.text = string.Join(" ", new string('_', len).ToCharArray());
            return;
        }
        string display = "";
        foreach (char c in currentAnswer) display += c + " ";
        answerDisplayTxt.text = display.Trim();
    }

    void ReactivateLetter(string letter)
    {
        foreach (var lb in letterBtns)
        {
            if (lb == null) continue;
            TextMeshProUGUI t = lb.GetComponentInChildren<TextMeshProUGUI>();
            Button b = lb.GetComponent<Button>();
            if (t != null && t.text == letter && !b.interactable)
            {
                b.interactable = true;
                lb.GetComponent<Image>().color = Color.white;
                break;
            }
        }
    }

    void CheckAnswer(string correct)
    {
        if (currentAnswer.Length < correct.Length) return;

        bool right = currentAnswer.ToUpper() == correct.ToUpper();
        SoundManager.Instance?.PlaySFX(right ? "sfx_correct" : "sfx_wrong");
        HapticManager.Instance?.Vibrate(right ? HapticType.Success : HapticType.Error);

        if (answerDisplayTxt)
            answerDisplayTxt.color = right ? TOBATheme.OptionCorrect : TOBATheme.OptionWrong;

        if (right) correctCount++;

        ScreenManager.Instance.StartCoroutine(NextWordDelay());
    }

    IEnumerator NextWordDelay()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (screenObj == null) yield break;
        if (answerDisplayTxt) answerDisplayTxt.color = TOBATheme.BatakGold;
        int next = currentIndex + 1;
        if (next < words.Count) ShowWord(next);
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