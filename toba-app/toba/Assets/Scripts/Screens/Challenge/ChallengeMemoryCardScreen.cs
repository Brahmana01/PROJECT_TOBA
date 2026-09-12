using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MemoryCardItem { public string id; public string label; public string emoji; }
[System.Serializable]
public class MemoryCardData  { public List<MemoryCardItem> cards; }

public class ChallengeMemoryCardScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<string> cardValues = new List<string>();
    private List<string> cardEmojis = new List<string>();
    private List<GameObject> cards  = new List<GameObject>();
    private List<bool> revealed     = new List<bool>();
    private List<bool> matched      = new List<bool>();
    private int firstIdx = -1, secondIdx = -1;
    private bool isChecking = false;
    private int matchedPairs = 0;
    private int totalPairs   = 0;
    private TextMeshProUGUI pairsTxt;

    public ChallengeMemoryCardScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeMemoryCard") { challenge = ch; }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        UIBuilder.CreateBackground(screenObj.transform, TOBATheme.LoadSprite("bg_toba"));
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0,0.04f,0,0.5f), Vector2.zero, Vector2.one);
        HeaderBar.Create(screenObj.transform, "MEMORY CARD BUDAYA", () =>
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)));

        MemoryCardData data = JsonUtility.FromJson<MemoryCardData>(challenge.data_json);
        if (data?.cards == null || data.cards.Count == 0)
        {
            EmptyState.Show(screenObj.transform, "🃏", "Data kosong", "Hubungi admin");
            return;
        }

        // Buat pasangan kartu (nama + emoji/gambar)
        foreach (var c in data.cards)
        {
            cardValues.Add(c.label); cardEmojis.Add(c.label);   // kartu teks
            cardValues.Add(c.id);   cardEmojis.Add(c.emoji);    // kartu emoji/id
        }
        totalPairs = data.cards.Count;

        // Shuffle
        for (int i = cardValues.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i+1);
            (cardValues[i], cardValues[j]) = (cardValues[j], cardValues[i]);
            (cardEmojis[i], cardEmojis[j]) = (cardEmojis[j], cardEmojis[i]);
        }

        // Status
        pairsTxt = UIBuilder.CreateTextSized(screenObj.transform,
            "Pasangan: 0/" + totalPairs, 34, TOBATheme.BatakGold,
            TextAlignmentOptions.Center, new Vector2(500, 50), new Vector2(0, 760f))
            .GetComponent<TextMeshProUGUI>();
        pairsTxt.fontStyle = FontStyles.Normal; pairsTxt.raycastTarget = false;

        UIBuilder.CreateTextSized(screenObj.transform,
            "Temukan semua pasangan gambar budaya!", 28,
            new Color(1f,1f,1f,0.7f), TextAlignmentOptions.Center,
            new Vector2(800, 45), new Vector2(0, 700f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        // Grid kartu 4x4 (max 8 pasang)
        int cols = 4, rows = Mathf.CeilToInt(cardValues.Count / (float)cols);
        float cW = 195f, cH = 220f, gX = 15f, gY = 15f;
        float totalGridW = cols*(cW+gX)-gX;
        float startY = 350f;

        for (int i = 0; i < cardValues.Count; i++)
        {
            revealed.Add(false); matched.Add(false);
            int col = i % cols, row = i / cols;
            float x = -totalGridW/2f + col*(cW+gX) + cW/2f;
            float y = startY - row*(cH+gY);
            cards.Add(CreateCard(i, cardEmojis[i], x, y, cW, cH));
        }
    }

    GameObject CreateCard(int idx, string label, float x, float y, float w, float h)
    {
        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        GameObject card = new GameObject("Card_" + idx);
        card.transform.SetParent(screenObj.transform, false);
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);

        // Sisi depan (tersembunyi)
        Image backImg = card.AddComponent<Image>();
        backImg.sprite = wp ?? UIBuilder.GetDefaultSprite();
        backImg.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        backImg.color  = new Color(0.3f, 0.15f, 0.05f, 1f);
        backImg.raycastTarget = true;

        TextMeshProUGUI questionMark = UIBuilder.CreateTextSized(card.transform,
            "?", 72, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(w, h), Vector2.zero).GetComponent<TextMeshProUGUI>();
        questionMark.fontStyle = FontStyles.Bold;
        questionMark.name = "QuestionMark";
        questionMark.raycastTarget = false;

        // Sisi belakang (konten)
        GameObject frontGO = new GameObject("Front");
        frontGO.transform.SetParent(card.transform, false);
        frontGO.SetActive(false);
        frontGO.name = "Front";
        RectTransform rtF = frontGO.AddComponent<RectTransform>();
        rtF.anchorMin = Vector2.zero; rtF.anchorMax = Vector2.one;
        rtF.offsetMin = rtF.offsetMax = Vector2.zero;
        Image frontImg = frontGO.AddComponent<Image>();
        frontImg.sprite = UIBuilder.GetDefaultSprite();
        frontImg.color  = new Color(0.92f, 0.82f, 0.65f, 1f);
        frontImg.raycastTarget = false;
        TextMeshProUGUI frontTxt = UIBuilder.CreateTextSized(frontGO.transform,
            label, 36, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(w-20, h-20), Vector2.zero).GetComponent<TextMeshProUGUI>();
        frontTxt.fontStyle        = FontStyles.Bold;
        frontTxt.textWrappingMode = TextWrappingModes.Normal;
        frontTxt.raycastTarget    = false;

        Button btn = card.AddComponent<Button>();
        btn.targetGraphic = backImg;
        int capturedIdx = idx;
        btn.onClick.AddListener(() => OnCardClick(capturedIdx));

        return card;
    }

    void OnCardClick(int idx)
    {
        if (isChecking || revealed[idx] || matched[idx]) return;
        SoundManager.Instance?.PlaySFX("sfx_pop");
        RevealCard(idx);

        if (firstIdx == -1) { firstIdx = idx; return; }
        secondIdx = idx;
        isChecking = true;
        ScreenManager.Instance.StartCoroutine(CheckMatch());
    }

    void RevealCard(int idx)
    {
        revealed[idx] = true;
        Transform card = cards[idx].transform;
        card.Find("QuestionMark")?.gameObject.SetActive(false);
        card.Find("Front")?.gameObject.SetActive(true);
    }

    void HideCard(int idx)
    {
        revealed[idx] = false;
        Transform card = cards[idx].transform;
        card.Find("QuestionMark")?.gameObject.SetActive(true);
        card.Find("Front")?.gameObject.SetActive(false);
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSecondsRealtime(1f);
        if (screenObj == null) yield break;

        bool isMatch = cardValues[firstIdx] == cardValues[secondIdx]
                    && firstIdx != secondIdx;

        if (isMatch)
        {
            matched[firstIdx] = matched[secondIdx] = true;
            SoundManager.Instance?.PlaySFX("sfx_correct");
            HapticManager.Instance?.Vibrate(HapticType.Success);
            // Warna hijau untuk match
            cards[firstIdx].GetComponent<Image>().color = TOBATheme.OptionCorrect;
            cards[secondIdx].GetComponent<Image>().color = TOBATheme.OptionCorrect;
            matchedPairs++;
            if (pairsTxt) pairsTxt.text = "Pasangan: " + matchedPairs + "/" + totalPairs;

            if (matchedPairs >= totalPairs) { SubmitResult(); yield break; }
        }
        else
        {
            SoundManager.Instance?.PlaySFX("sfx_wrong");
            HapticManager.Instance?.Vibrate(HapticType.Error);
            HideCard(firstIdx); HideCard(secondIdx);
        }

        firstIdx = secondIdx = -1;
        isChecking = false;
    }

    void SubmitResult()
    {
        APIManager.Instance.PostRequest("/challenge/submit",
            JsonUtility.ToJson(new SubmitChallengeRequest {
                player_id = PlayerPrefs.GetInt("PlayerID"),
                challenge_id = challenge.id, score = matchedPairs }),
            (r) => ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)),
            (e) => ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container)));
    }
}