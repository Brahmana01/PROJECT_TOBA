using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CategorizeCategory
{
    public string name;
    public List<string> items;
}

[System.Serializable]
public class CategorizeData
{
    public List<CategorizeCategory> categories;
}

public class ChallengeCategorizeScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<CategorizeCategory> categories;

    private readonly Dictionary<string, string> correctMap =
        new Dictionary<string, string>();

    private readonly Dictionary<string, string> playerMap =
        new Dictionary<string, string>();

    private string selectedItem = null;
    private GameObject selectedCard = null;
    private bool isSubmitting = false;

    public ChallengeCategorizeScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeCategorize")
    {
        challenge = ch;
    }

    public override void Show()
    {
        if (!IsReady)
            return;

        SetBackground(TOBATheme.BGDark);

        UIBuilder.CreateBackground(
            screenObj.transform,
            TOBATheme.LoadSprite("bg_toba")
        );

        UIBuilder.CreatePanel(
            screenObj.transform,
            "Overlay",
            new Color(0, 0.04f, 0, 0.5f),
            Vector2.zero,
            Vector2.one
        );

        HeaderBar.Create(
            screenObj.transform,
            "KELOMPOKKAN BUDAYA",
            () =>
            {
                if (ScreenManager.Instance != null && container != null)
                {
                    ScreenManager.Instance.ChangeScreen(
                        new MainMenuScreen(container)
                    );
                }
            }
        );

        // Reset state agar aman jika Show dipanggil kembali.
        correctMap.Clear();
        playerMap.Clear();
        selectedItem = null;
        selectedCard = null;
        isSubmitting = false;
        categories = null;

        CategorizeData data = null;

        try
        {
            if (challenge == null ||
                challenge.id <= 0 ||
                string.IsNullOrWhiteSpace(challenge.data_json))
            {
                throw new System.FormatException(
                    "Data challenge kosong atau tidak valid."
                );
            }

            data = JsonUtility.FromJson<CategorizeData>(
                challenge.data_json
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning(
                "[Categorize] JSON tidak valid: " + ex.Message
            );
        }

        if (data == null || data.categories == null)
        {
            ShowInvalidData(
                "Data kategori tidak tersedia",
                "Hubungi admin"
            );
            return;
        }

        // Hapus kategori yang tidak memiliki struktur data valid.
        data.categories.RemoveAll(category =>
            category == null ||
            string.IsNullOrWhiteSpace(category.name) ||
            category.items == null
        );

        // Rapikan spasi pada nama kategori dan item.
        foreach (CategorizeCategory category in data.categories)
        {
            category.name = category.name.Trim();

            for (int i = category.items.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(category.items[i]))
                {
                    category.items.RemoveAt(i);
                }
                else
                {
                    category.items[i] = category.items[i].Trim();
                }
            }
        }

        // Kategori tanpa item tidak dapat dimainkan.
        data.categories.RemoveAll(category =>
            category.items == null || category.items.Count == 0
        );

        if (data.categories.Count == 0)
        {
            ShowInvalidData(
                "Kategori masih kosong",
                "Hubungi admin"
            );
            return;
        }

        // Setiap nama kategori harus unik agar pilihan tidak ambigu.
        HashSet<string> categoryNames = new HashSet<string>(
            System.StringComparer.OrdinalIgnoreCase
        );

        // Item harus unik karena item menjadi key pada correctMap.
        // Item ganda sebelumnya dapat menimpa kategori yang benar.
        HashSet<string> itemNames = new HashSet<string>(
            System.StringComparer.OrdinalIgnoreCase
        );

        bool hasDuplicateCategory = false;
        bool hasDuplicateItem = false;

        foreach (CategorizeCategory category in data.categories)
        {
            if (!categoryNames.Add(category.name))
            {
                hasDuplicateCategory = true;
                break;
            }

            foreach (string item in category.items)
            {
                if (!itemNames.Add(item))
                {
                    hasDuplicateItem = true;
                    break;
                }

                // correctMap dibangun satu kali saja.
                correctMap.Add(item, category.name);
            }

            if (hasDuplicateItem)
                break;
        }

        if (hasDuplicateCategory)
        {
            correctMap.Clear();

            ShowInvalidData(
                "Nama kategori ganda",
                "Periksa kembali data kategori."
            );
            return;
        }

        if (hasDuplicateItem)
        {
            correctMap.Clear();

            ShowInvalidData(
                "Item budaya terduplikasi",
                "Setiap item harus berada pada satu kategori saja."
            );
            return;
        }

        if (correctMap.Count == 0)
        {
            ShowInvalidData(
                "Data item tidak tersedia",
                "Hubungi admin"
            );
            return;
        }

        categories = data.categories;

        // ========================================================
        // TAMPILAN ASLI DIPERTAHANKAN
        // ========================================================

        UIBuilder.CreateTextSized(
            screenObj.transform,
            "Pilih item lalu pilih kategorinya!",
            28,
            new Color(1f, 1f, 1f, 0.65f),
            TextAlignmentOptions.Center,
            new Vector2(800, 45),
            new Vector2(0, 760f)
        )
        .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        BuildUI();
    }

    private void ShowInvalidData(string title, string message)
    {
        if (!IsAlive)
            return;

        EmptyState.Show(
            screenObj.transform,
            "📂",
            title,
            message
        );
    }

    private void BuildUI()
    {
        if (!IsAlive || categories == null || categories.Count == 0)
            return;

        // Kumpulkan semua item (shuffle).
        List<string> allItems = new List<string>();

        foreach (CategorizeCategory category in categories)
        {
            if (category == null || category.items == null)
                continue;

            allItems.AddRange(category.items);
        }

        if (allItems.Count == 0 || allItems.Count != correctMap.Count)
        {
            Debug.LogWarning(
                "[Categorize] Jumlah item tidak konsisten dengan kunci jawaban."
            );
            return;
        }

        for (int i = allItems.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = allItems[i];
            allItems[i] = allItems[j];
            allItems[j] = temp;
        }

        // Area item (kiri) - ukuran dan posisi asli dipertahankan.
        float itemW = 360f;
        float itemH = 80f;
        float gap = 10f;
        float startY = 480f;

        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        UIBuilder.CreateTextSized(
            screenObj.transform,
            "ITEM",
            28,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(380, 40),
            new Vector2(-260f, 560f)
        )
        .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        for (int i = 0; i < allItems.Count; i++)
        {
            if (!IsAlive)
                return;

            string item = allItems[i];
            float y = startY - i * (itemH + gap);

            GameObject card = new GameObject("Item_" + item);
            card.transform.SetParent(screenObj.transform, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(itemW, itemH);
            rt.anchoredPosition = new Vector2(-270f, y);

            Image img = card.AddComponent<Image>();
            img.sprite = wp ?? UIBuilder.GetDefaultSprite();
            img.type = wp != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = new Color(0.88f, 0.78f, 0.58f, 1f);
            img.raycastTarget = true;

            TextMeshProUGUI t = UIBuilder.CreateTextSized(
                card.transform,
                item,
                26,
                TOBATheme.TextDark,
                TextAlignmentOptions.Center,
                new Vector2(itemW - 16, itemH),
                Vector2.zero
            ).GetComponent<TextMeshProUGUI>();

            if (t != null)
            {
                t.fontStyle = FontStyles.Normal;
                t.raycastTarget = false;
            }

            Button b = card.AddComponent<Button>();
            b.targetGraphic = img;

            string capturedItem = item;
            GameObject capturedCard = card;
            Image capturedImage = img;

            b.onClick.AddListener(() =>
            {
                if (!IsAlive || isSubmitting ||
                    capturedCard == null || capturedImage == null)
                {
                    return;
                }

                SoundManager.Instance?.PlaySFX("sfx_click");

                if (selectedCard != null && selectedCard != capturedCard)
                {
                    Image previousImage =
                        selectedCard.GetComponent<Image>();

                    if (previousImage != null)
                    {
                        previousImage.color =
                            new Color(0.88f, 0.78f, 0.58f, 1f);
                    }
                }

                selectedItem = capturedItem;
                selectedCard = capturedCard;
                capturedImage.color = TOBATheme.BatakGold;
            });
        }

        // Area kategori (kanan) - ukuran dan posisi asli dipertahankan.
        float catW = 350f;
        float catH = 100f;
        float catGap = 20f;
        float catStartY = 300f;

        UIBuilder.CreateTextSized(
            screenObj.transform,
            "KATEGORI",
            28,
            TOBATheme.BatakGold,
            TextAlignmentOptions.Center,
            new Vector2(360, 40),
            new Vector2(260f, 560f)
        )
        .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        for (int i = 0; i < categories.Count; i++)
        {
            if (!IsAlive)
                return;

            string catName = categories[i].name;
            float y = catStartY - i * (catH + catGap);

            GameObject catCard = new GameObject("Cat_" + catName);
            catCard.transform.SetParent(screenObj.transform, false);

            RectTransform rt = catCard.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(catW, catH);
            rt.anchoredPosition = new Vector2(260f, y);

            Image img = catCard.AddComponent<Image>();
            img.sprite = wp ?? UIBuilder.GetDefaultSprite();
            img.type = wp != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = new Color(0.2f, 0.45f, 0.15f, 0.9f);
            img.raycastTarget = true;

            Outline ol = catCard.AddComponent<Outline>();
            ol.effectColor = TOBATheme.BatakGold;
            ol.effectDistance = new Vector2(3, 3);

            TextMeshProUGUI ct = UIBuilder.CreateTextSized(
                catCard.transform,
                catName,
                28,
                Color.white,
                TextAlignmentOptions.Center,
                new Vector2(catW - 16, catH),
                Vector2.zero
            ).GetComponent<TextMeshProUGUI>();

            if (ct != null)
            {
                ct.fontStyle = FontStyles.Bold;
                ct.raycastTarget = false;
            }

            Button b = catCard.AddComponent<Button>();
            b.targetGraphic = img;

            string capturedCat = catName;

            b.onClick.AddListener(() =>
            {
                if (!IsAlive || isSubmitting ||
                    string.IsNullOrEmpty(selectedItem))
                {
                    return;
                }

                // Cegah item yang sama masuk ke playerMap lebih dari sekali.
                if (!correctMap.ContainsKey(selectedItem) ||
                    playerMap.ContainsKey(selectedItem))
                {
                    selectedItem = null;
                    selectedCard = null;
                    return;
                }

                SoundManager.Instance?.PlaySFX("sfx_click");

                playerMap.Add(selectedItem, capturedCat);

                // Update warna item - tampilan asli dipertahankan.
                if (selectedCard != null)
                {
                    bool right =
                        correctMap.ContainsKey(selectedItem) &&
                        correctMap[selectedItem] == capturedCat;

                    Image selectedImage =
                        selectedCard.GetComponent<Image>();

                    Button selectedButton =
                        selectedCard.GetComponent<Button>();

                    if (selectedImage != null)
                    {
                        selectedImage.color = right
                            ? TOBATheme.OptionCorrect
                            : TOBATheme.OptionWrong;
                    }

                    if (selectedButton != null)
                        selectedButton.interactable = false;
                }

                selectedItem = null;
                selectedCard = null;

                if (playerMap.Count >= correctMap.Count)
                {
                    BeginFinish();
                }
            });
        }
    }

    private void BeginFinish()
    {
        if (!IsAlive || isSubmitting)
            return;

        isSubmitting = true;

        ScreenManager manager = ScreenManager.Instance;

        if (manager == null)
        {
            isSubmitting = false;
            Debug.LogWarning(
                "[Categorize] ScreenManager tidak tersedia saat menyelesaikan challenge."
            );
            return;
        }

        manager.StartCoroutine(FinishDelay());
    }

    private IEnumerator FinishDelay()
    {
        yield return new WaitForSecondsRealtime(1.2f);

        // Screen bisa saja sudah ditutup selama jeda animasi.
        if (!IsCurrent || !isSubmitting)
            yield break;

        int score = 0;

        foreach (KeyValuePair<string, string> answer in correctMap)
        {
            string playerAnswer;

            if (playerMap.TryGetValue(answer.Key, out playerAnswer) &&
                answer.Value == playerAnswer)
            {
                score++;
            }
        }

        if (screenObj == null)
            yield break;

        TOBAChallengeSubmission.Submit(
            screenObj,
            container,
            challenge,
            score
        );
    }
}
