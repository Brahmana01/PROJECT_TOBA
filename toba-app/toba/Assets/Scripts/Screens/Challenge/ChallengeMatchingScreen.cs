using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ChallengeMatchingScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<MatchPair> pairs;
    private int correctCount = 0;
    private int currentIndex = 0;
    private string selectedBatak = null;
    private GameObject batakContainer, indoContainer;

    public ChallengeMatchingScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeMatching")
    {
        challenge = ch;
    }

    public override void Show()
    {
        if (challenge == null)
        {
            Debug.LogWarning("[ChallengeMatching] Data challenge kosong.");
            ScreenManager.Instance?.ChangeScreen(new MainMenuScreen(container));
            return;
        }

        SetBackground(TOBATheme.BGDark);
        Sprite bg = TOBATheme.LoadSprite("bg_toba");
        UIBuilder.CreateBackground(screenObj.transform, bg);
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0, 0.04f, 0, 0.5f), Vector2.zero, Vector2.one);

        string challengeTitle = string.IsNullOrWhiteSpace(challenge.title)
            ? "TANTANGAN"
            : challenge.title.ToUpperInvariant();

        HeaderBar.Create(screenObj.transform, challengeTitle, () => {
            ScreenManager.Instance?.ChangeScreen(new MainMenuScreen(container));
        });

        MatchingData data = null;

        if (!string.IsNullOrWhiteSpace(challenge.data_json))
        {
            try
            {
                data = JsonUtility.FromJson<MatchingData>(challenge.data_json);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[ChallengeMatching] JSON tidak valid: " + ex.Message);
            }
        }

        pairs = data?.pairs?.FindAll(pair =>
            pair != null &&
            !string.IsNullOrWhiteSpace(pair.batak) &&
            !string.IsNullOrWhiteSpace(pair.indo)
        );

        if (pairs == null || pairs.Count == 0)
        {
            Debug.LogWarning("[ChallengeMatching] Data pasangan kata kosong.");
            NetworkManager.Instance?.ShowToast("Data tantangan tidak tersedia.", true);
            ScreenManager.Instance?.ChangeScreen(new MainMenuScreen(container));
            return;
        }

        // Instruksi
        UIBuilder.CreateTextSized(screenObj.transform,
            "Cocokkan kata Batak dengan artinya!", 34,
            TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(800, 60), new Vector2(0, 700f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        batakContainer = new GameObject("BatakCol");
        batakContainer.transform.SetParent(screenObj.transform, false);
        RectTransform rtB = batakContainer.AddComponent<RectTransform>();
        rtB.anchorMin = new Vector2(0.05f, 0.15f);
        rtB.anchorMax = new Vector2(0.48f, 0.75f);
        rtB.offsetMin = rtB.offsetMax = Vector2.zero;

        indoContainer = new GameObject("IndoCol");
        indoContainer.transform.SetParent(screenObj.transform, false);
        RectTransform rtI = indoContainer.AddComponent<RectTransform>();
        rtI.anchorMin = new Vector2(0.52f, 0.15f);
        rtI.anchorMax = new Vector2(0.95f, 0.75f);
        rtI.offsetMin = rtI.offsetMax = Vector2.zero;

        var shuffledIndo = new List<MatchPair>(pairs);
        Shuffle(shuffledIndo);

        for (int i = 0; i < pairs.Count; i++)
            CreateWordCard(batakContainer.transform, pairs[i].batak, i, true);
        for (int i = 0; i < shuffledIndo.Count; i++)
            CreateWordCard(indoContainer.transform, shuffledIndo[i].indo, i, false);
    }

    void CreateWordCard(Transform parent, string text, int index, bool isBatak)
    {
        Sprite wp = TOBATheme.LoadSprite("panel_wood");
        float h = 110f, gap = 15f;
        float y = -(index * (h + gap)) - h / 2f;

        GameObject card = new GameObject("Card_" + text);
        card.transform.SetParent(parent, false);
        RectTransform rt = card.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0, h);
        rt.anchoredPosition = new Vector2(0, y);

        Image img = card.AddComponent<Image>();
        img.sprite = wp ?? UIBuilder.GetDefaultSprite();
        img.type = wp != null ? Image.Type.Sliced : Image.Type.Simple;
        img.color = new Color(0.9f, 0.8f, 0.6f, 1f);
        img.raycastTarget = true;

        TextMeshProUGUI txt = UIBuilder.CreateTextSized(card.transform,
            text, 30, TOBATheme.TextDark, TextAlignmentOptions.Center,
            new Vector2(0, h), Vector2.zero).GetComponent<TextMeshProUGUI>();
        txt.raycastTarget = false;

        Button btn = card.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => OnWordClicked(text, isBatak, img));
    }

    void OnWordClicked(string text, bool isBatak, Image img)
    {
        if (img == null || pairs == null || pairs.Count == 0)
            return;

        SoundManager.Instance?.PlaySFX("sfx_click");
        if (isBatak)
        {
            selectedBatak = text;
            img.color = TOBATheme.BatakGold;
        }
        else if (selectedBatak != null)
        {
            bool correct = false;
            foreach (var p in pairs)
                if (p.batak == selectedBatak && p.indo == text) correct = true;

            if (correct)
            {
                correctCount++;
                SoundManager.Instance?.PlaySFX("sfx_correct");
                HapticManager.Instance?.Vibrate(HapticType.Success);
                img.color = TOBATheme.Success;
            }
            else
            {
                SoundManager.Instance?.PlaySFX("sfx_wrong");
                HapticManager.Instance?.Vibrate(HapticType.Error);
                img.color = TOBATheme.Error;
            }

            selectedBatak = null;
            currentIndex++;
            if (currentIndex >= pairs.Count)
                ScreenManager.Instance?.StartCoroutine(FinishAfterDelay());
        }
    }

    IEnumerator FinishAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        SubmitResult();
    }

    void SubmitResult()
    {
        TOBAChallengeSubmission.Submit(
            screenObj,
            container,
            challenge,
            correctCount
        );
    }
    void Shuffle(List<MatchPair> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}