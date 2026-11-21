using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChallengeMatchingScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<string> batakWords = new List<string>();
    private List<string> indoWords = new List<string>();
    private readonly List<MatchPair> submittedPairs = new List<MatchPair>();
    private readonly HashSet<int> usedBatak = new HashSet<int>();
    private readonly HashSet<int> usedIndo = new HashSet<int>();
    private Button[] batakButtons;
    private Button[] indoButtons;
    private Image[] batakImages;
    private Image[] indoImages;
    private int selectedBatakIndex = -1;
    private bool isSubmitting;

    public ChallengeMatchingScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeMatching")
    {
        challenge = ch;
    }

    public override void Show()
    {
        if (!IsReady || challenge == null ||
            challenge.id <= 0 || string.IsNullOrWhiteSpace(challenge.session_id))
        {
            ReturnToMainMenu();
            return;
        }

        MatchingPublicData data = null;
        try { data = JsonUtility.FromJson<MatchingPublicData>(challenge.data_json); }
        catch (System.Exception ex) { Debug.LogWarning("[ChallengeMatching] JSON: " + ex.Message); }

        if (data == null || data.batak_words == null || data.indo_words == null ||
            data.batak_words.Count < 2 || data.batak_words.Count != data.indo_words.Count)
        {
            ShowDataError();
            return;
        }

        batakWords = new List<string>(data.batak_words);
        indoWords = new List<string>(data.indo_words);
        for (int i = 0; i < batakWords.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(batakWords[i]) || string.IsNullOrWhiteSpace(indoWords[i]))
            {
                ShowDataError();
                return;
            }
            batakWords[i] = batakWords[i].Trim();
            indoWords[i] = indoWords[i].Trim();
        }

        SetBackground(TOBATheme.BGDark);
        UIBuilder.CreateBackground(screenObj.transform, TOBATheme.LoadSprite("bg_toba"));
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0, 0.04f, 0, 0.5f), Vector2.zero, Vector2.one);
        HeaderBar.Create(screenObj.transform,
            string.IsNullOrWhiteSpace(challenge.title) ? "COCOKKAN KATA" : challenge.title.ToUpperInvariant(),
            ReturnToMainMenu);

        UIBuilder.CreateTextSized(screenObj.transform,
            "Cocokkan kata Batak dengan artinya!", 30, TOBATheme.TextLight,
            TextAlignmentOptions.Center, new Vector2(850, 55), new Vector2(0, 650));
        UIBuilder.CreateTextSized(screenObj.transform, "BAHASA BATAK", 25,
            TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(400, 45), new Vector2(-235, 570));
        UIBuilder.CreateTextSized(screenObj.transform, "BAHASA INDONESIA", 25,
            TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(400, 45), new Vector2(235, 570));

        int count = batakWords.Count;
        batakButtons = new Button[count];
        indoButtons = new Button[count];
        batakImages = new Image[count];
        indoImages = new Image[count];
        float rowGap = Mathf.Min(105f, 480f / count);
        float startY = ((count - 1) * rowGap) / 2f + 250f;

        for (int i = 0; i < count; i++)
        {
            int index = i;
            CreateWordButton(batakWords[i], new Vector2(-235f, startY - i * rowGap),
                out batakButtons[i], out batakImages[i], () => SelectBatak(index));
            CreateWordButton(indoWords[i], new Vector2(235f, startY - i * rowGap),
                out indoButtons[i], out indoImages[i], () => SelectIndo(index));
        }
    }

    private void CreateWordButton(string text, Vector2 position,
        out Button button, out Image image, UnityEngine.Events.UnityAction onClick)
    {
        GameObject card = new GameObject("WordCard_" + text);
        card.transform.SetParent(screenObj.transform, false);
        RectTransform rect = card.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 82f);
        rect.anchoredPosition = position;

        image = card.AddComponent<Image>();
        image.sprite = TOBATheme.LoadSprite("panel_wood") ?? UIBuilder.GetDefaultSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(0.88f, 0.78f, 0.58f, 1f);
        image.raycastTarget = true;

        GameObject label = UIBuilder.CreateTextSized(card.transform, text, 27,
            TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(400f, 76f), Vector2.zero);
        TextMeshProUGUI labelText = label != null ? label.GetComponent<TextMeshProUGUI>() : null;
        if (labelText != null)
        {
            labelText.fontStyle = FontStyles.Normal;
            labelText.textWrappingMode = TextWrappingModes.Normal;
            labelText.raycastTarget = false;
        }

        button = card.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
    }

    private void SelectBatak(int index)
    {
        if (isSubmitting || usedBatak.Contains(index)) return;
        if (selectedBatakIndex >= 0 && selectedBatakIndex < batakImages.Length &&
            !usedBatak.Contains(selectedBatakIndex))
            batakImages[selectedBatakIndex].color = new Color(0.88f, 0.78f, 0.58f, 1f);

        selectedBatakIndex = index;
        batakImages[index].color = TOBATheme.BatakGold;
        SoundManager.Instance?.PlaySFX("sfx_click");
    }

    private void SelectIndo(int index)
    {
        if (isSubmitting || selectedBatakIndex < 0 || usedIndo.Contains(index)) return;

        submittedPairs.Add(new MatchPair
        {
            batak = batakWords[selectedBatakIndex],
            indo = indoWords[index]
        });
        usedBatak.Add(selectedBatakIndex);
        usedIndo.Add(index);
        batakButtons[selectedBatakIndex].interactable = false;
        indoButtons[index].interactable = false;
        batakImages[selectedBatakIndex].color = TOBATheme.BatakGold;
        indoImages[index].color = TOBATheme.BatakGold;
        selectedBatakIndex = -1;
        SoundManager.Instance?.PlaySFX("sfx_click");

        if (submittedPairs.Count == batakWords.Count)
            SubmitResult();
    }

    private void SubmitResult()
    {
        if (isSubmitting || !IsAlive) return;
        isSubmitting = true;
        MatchingSubmissionData submission = new MatchingSubmissionData
        {
            pairs = submittedPairs
        };
        TOBAChallengeSubmission.Submit(screenObj, container, challenge,
            JsonUtility.ToJson(submission));
    }

    private void ShowDataError()
    {
        if (screenObj != null)
            EmptyState.Show(screenObj.transform, "🔤", "Data tantangan tidak tersedia", "Hubungi admin");
    }

    private void ReturnToMainMenu()
    {
        if (ScreenManager.Instance != null && container != null)
            ScreenManager.Instance.ChangeScreen(new MainMenuScreen(container));
    }
}
