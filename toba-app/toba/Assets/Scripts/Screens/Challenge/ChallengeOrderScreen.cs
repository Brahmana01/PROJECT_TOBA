using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class OrderData
{
    public string question;
    public List<string> correct_order;
}

public class ChallengeOrderScreen : BaseScreen
{
    private ChallengeData challenge;
    private List<string> currentOrder = new List<string>();
    private List<string> correctOrder = new List<string>();
    private string question;
    private List<GameObject> itemCards = new List<GameObject>();
    private int selectedIdx = -1;

    public ChallengeOrderScreen(Transform parent, ChallengeData ch)
        : base(parent, "ChallengeOrder") { challenge = ch; }

    public override void Show()
    {
        SetBackground(TOBATheme.BGDark);
        UIBuilder.CreateBackground(screenObj.transform, TOBATheme.LoadSprite("bg_toba"));
        UIBuilder.CreatePanel(screenObj.transform, "Overlay",
            new Color(0,0.04f,0,0.5f), Vector2.zero, Vector2.one);
        HeaderBar.Create(screenObj.transform, "SUSUN URUTAN", () =>
            ScreenManager.Instance?.ChangeScreen(new MainMenuScreen(container)));

        OrderData data = null;

        try
        {
            if (challenge == null ||
                string.IsNullOrWhiteSpace(challenge.data_json))
            {
                throw new System.FormatException("Data challenge kosong.");
            }

            data = JsonUtility.FromJson<OrderData>(challenge.data_json);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[ChallengeOrder] JSON tidak valid: " + ex.Message);
        }
        if (data == null || data.correct_order == null)

        data.correct_order.RemoveAll(
            item => string.IsNullOrWhiteSpace(item)
        );

        if (data.correct_order.Count < 2)
        {
            EmptyState.Show(
                screenObj.transform,
                "📋",
                "Data urutan tidak lengkap",
                "Hubungi admin"
            );

            return;
        }
        {
            EmptyState.Show(screenObj.transform, "📋", "Data kosong", "Hubungi admin");
            return;
        }

        question     = data.question;
        correctOrder = new List<string>(data.correct_order);
        currentOrder = new List<string>(data.correct_order);

        // Shuffle urutan
        for (int i = currentOrder.Count-1; i > 0; i--)
        {
            int j = Random.Range(0, i+1);
            (currentOrder[i], currentOrder[j]) = (currentOrder[j], currentOrder[i]);
        }

        // Pertanyaan
        Sprite wp = TOBATheme.LoadSprite("panel_wood");
        GameObject qCard = UIBuilder.CreateSpritePanel(screenObj.transform,
            "QCard", wp, new Vector2(940, 130), new Vector2(0, 680f));
        if (wp != null) qCard.GetComponent<Image>().color =
            new Color(0.22f, 0.1f, 0.03f, 0.97f);
        TextMeshProUGUI qTxt = UIBuilder.CreateTextSized(qCard.transform,
            question, 32, TOBATheme.TextLight, TextAlignmentOptions.Center,
            new Vector2(900, 110), Vector2.zero).GetComponent<TextMeshProUGUI>();
        qTxt.fontStyle = FontStyles.Normal; qTxt.raycastTarget = false;

        UIBuilder.CreateTextSized(screenObj.transform,
            "Tap item lalu tap posisi yang diinginkan", 26,
            new Color(1f,1f,1f,0.55f), TextAlignmentOptions.Center,
            new Vector2(800, 45), new Vector2(0, 570f))
            .GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

        BuildItemList();

        // Tombol Cek
        UIBuilder.CreateButton(screenObj.transform, "BtnCek", "✅ CEK URUTAN",
            new Vector2(700, 120), TOBATheme.BtnPrimary, CheckOrder)
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -750f);
    }

    void BuildItemList()
    {
        foreach (var c in itemCards) if (c) Object.Destroy(c);
        itemCards.Clear();

        float itemH = 115f, gap = 14f;
        float startY = 350f;
        Sprite wp = TOBATheme.LoadSprite("panel_wood");

        for (int i = 0; i < currentOrder.Count; i++)
        {
            int capturedI = i;
            float y = startY - i * (itemH + gap);

            GameObject item = new GameObject("Item_" + i);
            item.transform.SetParent(screenObj.transform, false);
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900, itemH);
            rt.anchoredPosition = new Vector2(0, y);

            Image img = item.AddComponent<Image>();
            img.sprite = wp ?? UIBuilder.GetDefaultSprite();
            img.type   = wp != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color  = new Color(0.88f, 0.78f, 0.58f, 1f);
            img.raycastTarget = true;

            // Nomor urut
            UIBuilder.CreateTextSized(item.transform,
                (i+1).ToString(), 38, TOBATheme.WoodDark,
                TextAlignmentOptions.Left, new Vector2(60, itemH),
                new Vector2(-390f, 0)).GetComponent<TextMeshProUGUI>()
                .fontStyle = FontStyles.Bold;

            // Teks item
            TextMeshProUGUI itemTxt = UIBuilder.CreateTextSized(item.transform,
                currentOrder[i], 32, TOBATheme.TextDark, TextAlignmentOptions.Left,
                new Vector2(760, itemH), new Vector2(40f, 0))
                .GetComponent<TextMeshProUGUI>();
            itemTxt.fontStyle = FontStyles.Normal; itemTxt.raycastTarget = false;

            Button btn = item.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => OnItemClick(capturedI, img));

            itemCards.Add(item);
        }
    }

    void OnItemClick(int idx, Image img)
    {
        if (screenObj == null ||
            img == null ||
            currentOrder == null ||
            idx < 0 ||
            idx >= currentOrder.Count)
            return;
        SoundManager.Instance?.PlaySFX("sfx_click");
        if (selectedIdx == -1)
        {
            selectedIdx = idx;
            img.color = TOBATheme.BatakGold;
        }
        else if (selectedIdx == idx)
        {
            selectedIdx = -1;
            img.color = new Color(0.88f, 0.78f, 0.58f, 1f);
        }
        else
        {
            // Tukar posisi
            (currentOrder[selectedIdx], currentOrder[idx]) =
                (currentOrder[idx], currentOrder[selectedIdx]);
            selectedIdx = -1;
            BuildItemList();
        }
    }

    void CheckOrder()
    {
        if (screenObj == null ||
            currentOrder == null ||
            correctOrder == null ||
            currentOrder.Count != correctOrder.Count ||
            currentOrder.Count == 0)
            return;
        bool correct = true;
        for (int i = 0; i < correctOrder.Count; i++)
            if (currentOrder[i] != correctOrder[i]) { correct = false; break; }

        SoundManager.Instance?.PlaySFX(correct ? "sfx_correct" : "sfx_wrong");
        HapticManager.Instance?.Vibrate(correct ? HapticType.Success : HapticType.Error);

        NetworkManager.Instance?.ShowToast(
            correct ? "✅ Urutan benar! Hebat!" : "❌ Belum tepat, coba lagi!",
            !correct);

        if (correct && ScreenManager.Instance != null)
            ScreenManager.Instance?.StartCoroutine(FinishDelay());
    }

    IEnumerator FinishDelay()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        if (screenObj == null)
            yield break;

        TOBAChallengeSubmission.Submit(
            screenObj,
            container,
            challenge,
            1
        );
    }
}