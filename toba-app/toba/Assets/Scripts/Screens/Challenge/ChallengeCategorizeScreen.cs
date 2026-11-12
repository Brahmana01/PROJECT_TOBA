using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CategorizeCategory { public string name; public List<string> items; }
[System.Serializable]
public class CategorizeData     { public List<CategorizeCategory> categories; }

public class ChallengeCategorizeScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<CategorizeCategory> categories;
    private Dictionary<string, string> correctMap = new Dictionary<string,string>();
    private Dictionary<string, string> playerMap  = new Dictionary<string,string>();
    private string selectedItem = null;
    private GameObject selectedCard = null;

    public ChallengeCategorizeScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeCategorize") { challenge = ch; }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        UIBuilder.CreateBackground(screenObj.transform, TOBATheme.LoadSprite("bg_toba"));
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0,0.04f,0,0.5f), Vector2.zero, Vector2.one);
        HeaderBar.Create(screenObj.transform, "KELOMPOKKAN BUDAYA", () =>
            ScreenManager.Instance?.ChangeScreen(new MainMenuScreen(container)));

        CategorizeData data = null;

        try
        {
            if (challenge == null ||
                string.IsNullOrWhiteSpace(challenge.data_json))
            {
                throw new System.FormatException("Data challenge kosong.");
            }

            data = JsonUtility.FromJson<CategorizeData>(challenge.data_json);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[Categorize] JSON tidak valid: " + ex.Message);
        }
        if (data == null || data.categories == null)
        {
            EmptyState.Show(
                screenObj.transform,
                "📂",
                "Data kategori tidak tersedia",
                "Hubungi admin"
            );
            return;
        }

        data.categories.RemoveAll(category =>
            category == null ||
            string.IsNullOrWhiteSpace(category.name) ||
            category.items == null
        );

        foreach (CategorizeCategory category in data.categories)
        {
            category.items.RemoveAll(
                item => string.IsNullOrWhiteSpace(item)
            );
        }

        data.categories.RemoveAll(
            category => category.items.Count == 0
        );

        categories = data.categories;

        if (categories.Count == 0)
        {
            EmptyState.Show(
                screenObj.transform,
                "📂",
                "Kategori masih kosong",
                "Hubungi admin"
            );
            return;
        }

        correctMap.Clear();

        foreach (CategorizeCategory category in categories)
        {
            foreach (string item in category.items)
            {
                if (!correctMap.ContainsKey(item))
                    correctMap[item] = category.name;
            }
        }

        // Build correct map
        foreach (var cat in categories)
            foreach (var item in cat.items)
                correctMap[item] = cat.name;

        UIBuilder.CreateTextSized(screenObj.transform,
            "Pilih item lalu pilih kategorinya!", 28,
            new Color(1f,1f,1f,0.65f), TextAlignmentOptions.Center,
            new Vector2(800,45), new Vector2(0, 760f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        BuildUI();
    }

    void BuildUI()
    {
        // Kumpulkan semua item (shuffle)
        List<string> allItems = new List<string>();
        foreach (var c in categories) allItems.AddRange(c.items);
        for (int i = allItems.Count-1; i > 0; i--)
        {
            int j = Random.Range(0, i+1);
            (allItems[i], allItems[j]) = (allItems[j], allItems[i]);
        }

        // Area item (kiri)
        float itemW = 360f, itemH = 80f, gap = 10f;
        float startY = 480f;
        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        UIBuilder.CreateTextSized(screenObj.transform,
            "ITEM", 28, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(380, 40), new Vector2(-260f, 560f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        for (int i = 0; i < allItems.Count; i++)
        {
            string item = allItems[i];
            float y = startY - i*(itemH+gap);

            GameObject card = new GameObject("Item_" + item);
            card.transform.SetParent(screenObj.transform, false);
            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(itemW, itemH);
            rt.anchoredPosition = new Vector2(-270f, y);
            Image img = card.AddComponent<Image>();
            img.sprite = wp ?? UIBuilder.GetDefaultSprite();
            img.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color  = new Color(0.88f, 0.78f, 0.58f, 1f);
            img.raycastTarget = true;
            TextMeshProUGUI t = UIBuilder.CreateTextSized(card.transform,
                item, 26, TOBATheme.TextDark, TextAlignmentOptions.Center,
                new Vector2(itemW-16, itemH), Vector2.zero)
                .GetComponent<TextMeshProUGUI>();
            t.fontStyle = FontStyles.Normal; t.raycastTarget = false;
            Button b = card.AddComponent<Button>();
            b.targetGraphic = img;
            string capturedItem = item;
            b.onClick.AddListener(() => {
                SoundManager.Instance?.PlaySFX("sfx_click");
                if (selectedCard != null)
                {
                    Image previousImage = selectedCard.GetComponent<Image>();

                    if (previousImage != null)
                        previousImage.color = new Color(0.88f, 0.78f, 0.58f, 1f);
                }
                selectedItem = capturedItem;
                selectedCard = card;
                img.color = TOBATheme.BatakGold;
            });
        }

        // Area kategori (kanan)
        float catW = 350f, catH = 100f, catGap = 20f;
        float catStartY = 300f;

        UIBuilder.CreateTextSized(screenObj.transform,
            "KATEGORI", 28, TOBATheme.BatakGold, TextAlignmentOptions.Center,
            new Vector2(360, 40), new Vector2(260f, 560f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        for (int i = 0; i < categories.Count; i++)
        {
            string catName = categories[i].name;
            float y = catStartY - i*(catH+catGap);

            GameObject catCard = new GameObject("Cat_" + catName);
            catCard.transform.SetParent(screenObj.transform, false);
            RectTransform rt = catCard.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(catW, catH);
            rt.anchoredPosition = new Vector2(260f, y);
            Image img = catCard.AddComponent<Image>();
            img.sprite = wp ?? UIBuilder.GetDefaultSprite();
            img.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color  = new Color(0.2f, 0.45f, 0.15f, 0.9f);
            img.raycastTarget = true;
            Outline ol = catCard.AddComponent<Outline>();
            ol.effectColor = TOBATheme.BatakGold; ol.effectDistance = new Vector2(3,3);
            TextMeshProUGUI ct = UIBuilder.CreateTextSized(catCard.transform,
                catName, 28, Color.white, TextAlignmentOptions.Center,
                new Vector2(catW-16, catH), Vector2.zero)
                .GetComponent<TextMeshProUGUI>();
            ct.fontStyle = FontStyles.Bold; ct.raycastTarget = false;
            Button b = catCard.AddComponent<Button>();
            b.targetGraphic = img;
            string capturedCat = catName;
            b.onClick.AddListener(() => {
                if (string.IsNullOrEmpty(selectedItem)) return;
                SoundManager.Instance?.PlaySFX("sfx_click");
                playerMap[selectedItem] = capturedCat;
                // Update warna item
                if (selectedCard != null)
                {
                    bool right = correctMap.ContainsKey(selectedItem)
                              && correctMap[selectedItem] == capturedCat;
                    Image selectedImage = selectedCard.GetComponent<Image>();
                    Button selectedButton = selectedCard.GetComponent<Button>();

                    if (selectedImage != null)
                    {
                        selectedImage.color =
                            right ? TOBATheme.OptionCorrect : TOBATheme.OptionWrong;
                    }

                    if (selectedButton != null)
                        selectedButton.interactable = false;
                }
                selectedItem = null; selectedCard = null;
                if (playerMap.Count >= correctMap.Count &&
                    ScreenManager.Instance != null)
                {
                    ScreenManager.Instance?.StartCoroutine(FinishDelay());
                }
            });
        }
    }

    IEnumerator FinishDelay()
    {
        yield return new WaitForSecondsRealtime(1.2f);

        if (screenObj == null)
            yield break;

        int score = 0;

        foreach (var kv in playerMap)
        {
            if (correctMap.ContainsKey(kv.Key) &&
                correctMap[kv.Key] == kv.Value)
            {
                score++;
            }
        }

        TOBAChallengeSubmission.Submit(
            screenObj,
            container,
            challenge,
            score
        );
    }

}