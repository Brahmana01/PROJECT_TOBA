using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StarRating
{
    public static void Create(Transform parent, int earned, int maxStars,
        Vector2 centerPos, float starSize = 140f)
    {
        float spacing = starSize + 15f;
        float totalWidth = (maxStars - 1) * spacing;

        for (int i = 0; i < maxStars; i++)
        {
            bool active = i < earned;
            float xPos = centerPos.x - (totalWidth / 2f) + (i * spacing);

            GameObject starGO = new GameObject("Star_" + i);
            starGO.transform.SetParent(parent, false);
            RectTransform rt = starGO.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(starSize, starSize);
            rt.anchoredPosition = new Vector2(xPos, centerPos.y);

            TextMeshProUGUI tmp = starGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "★";
            tmp.fontSize = (int)(starSize * 0.88f);
            tmp.color = active ? TOBATheme.StarActive : TOBATheme.StarInactive;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            // Shadow bintang aktif
            if (active)
            {
                Shadow sh = starGO.AddComponent<Shadow>();
                sh.effectColor = new Color(1f, 0.8f, 0f, 0.5f);
                sh.effectDistance = new Vector2(2, -2);
            }
        }
    }

    public static IEnumerator CreateAnimated(Transform parent, int earned,
        int maxStars, Vector2 centerPos, float starSize = 140f)
    {
        float spacing = starSize + 15f;
        float totalWidth = (maxStars - 1) * spacing;

        for (int i = 0; i < maxStars; i++)
        {
            bool active = i < earned;
            float xPos = centerPos.x - (totalWidth / 2f) + (i * spacing);

            GameObject starGO = new GameObject("Star_" + i);
            starGO.transform.SetParent(parent, false);
            RectTransform rt = starGO.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(starSize, starSize);
            rt.anchoredPosition = new Vector2(xPos, centerPos.y);
            starGO.transform.localScale = Vector3.zero;

            TextMeshProUGUI tmp = starGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "★";
            tmp.fontSize = (int)(starSize * 0.88f);
            tmp.color = active ? TOBATheme.StarActive : TOBATheme.StarInactive;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            if (active)
            {
                Shadow sh = starGO.AddComponent<Shadow>();
                sh.effectColor = new Color(1f, 0.8f, 0f, 0.5f);
                sh.effectDistance = new Vector2(2, -2);
                SoundManager.Instance?.PlaySFX("sfx_star");
            }

            if (ScreenManager.Instance != null)
                ScreenManager.Instance.StartCoroutine(PopStar(starGO.transform));

            yield return new WaitForSecondsRealtime(0.22f);
        }
    }

    private static IEnumerator PopStar(Transform t)
    {
        if (t == null) yield break;
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1.25f, elapsed / 0.25f);
            if (t != null) t.localScale = Vector3.one * s;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(1.25f, 1f, elapsed / 0.1f);
            if (t != null) t.localScale = Vector3.one * s;
            yield return null;
        }
        if (t != null) t.localScale = Vector3.one;
    }
}