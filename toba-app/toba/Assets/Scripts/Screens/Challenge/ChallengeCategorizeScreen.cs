using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CategorizeCategory
{
    public string name;
}

[System.Serializable]
public class CategorizeData
{
    public List<CategorizeCategory> categories;
    public List<string> items;
}

public class ChallengeCategorizeScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<CategorizeCategory> categories;

    private readonly Dictionary<string, string> playerMap =
        new Dictionary<string, string>();

    private List<string> items = new List<string>();

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
        playerMap.Clear();
        items.Clear();
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

        if (data == null || data.categories == null || data.items == null)
        {
            ShowInvalidData("Data kategori tidak tersedia", "Hubungi admin");
            return;
        }

        data.categories.RemoveAll(category =>
            category == null || string.IsNullOrWhiteSpace(category.name));
        data.items.RemoveAll(item => string.IsNullOrWhiteSpace(item));

        foreach (CategorizeCategory category in data.categories)
            category.name = category.name.Trim();
        for (int i = 0; i < data.items.Count; i++)
            data.items[i] = data.items[i].Trim();

        HashSet<string> categoryNames = new HashSet<string>(
            System.StringComparer.OrdinalIgnoreCase);
        foreach (CategorizeCategory category in data.categories)
        {
            if (!categoryNames.Add(category.name))
            {
                ShowInvalidData("Nama kategori ganda", "Periksa kembali data kategori.");
                return;
            }
        }

        HashSet<string> itemNames = new HashSet<string>(
            System.StringComparer.OrdinalIgnoreCase);
        foreach (string item in data.items)
        {
            if (!itemNames.Add(item))
            {
                ShowInvalidData("Item budaya terduplikasi", "Setiap item harus unik.");
                return;
            }
        }

        if (data.categories.Count < 2 || data.items.Count < 2)
        {
            ShowInvalidData("Konten belum lengkap", "Hubungi admin.");
            return;
        }

        categories = data.categories;
        items = data.items;

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

        // Item yang diterima client sudah dipisahkan dari kunci jawaban server.
        List<string> allItems = new List<string>(items);
        if (allItems.Count == 0)
            return;

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
                if (!items.Contains(selectedItem) ||
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
                    Image selectedImage = selectedCard.GetComponent<Image>();
                    Button selectedButton = selectedCard.GetComponent<Button>();

                    if (selectedImage != null)
                        selectedImage.color = TOBATheme.BatakGold;

                    if (selectedButton != null)
                        selectedButton.interactable = false;
                }

                selectedItem = null;
                selectedCard = null;

                if (playerMap.Count >= items.Count)
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

        if (screenObj == null)
            yield break;

        ChallengeCategorizeSubmission submission = new ChallengeCategorizeSubmission
        {
            assignments = new List<ChallengeCategorizeAssignment>()
        };
        foreach (KeyValuePair<string, string> assignment in playerMap)
        {
            submission.assignments.Add(new ChallengeCategorizeAssignment
            {
                item = assignment.Key,
                category = assignment.Value
            });
        }

        TOBAChallengeSubmission.Submit(
            screenObj,
            container,
            challenge,
            JsonUtility.ToJson(submission)
        );
    }
}
