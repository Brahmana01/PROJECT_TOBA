using UnityEngine;
using UnityEngine.UI;

public abstract class BaseScreen
{
    protected GameObject screenObj;
    protected Transform container;

    public BaseScreen(Transform parent, string name)
    {
        container = parent;
        screenObj = new GameObject(name);
        screenObj.transform.SetParent(parent, false);

        RectTransform rt = screenObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Tambah Image default agar semua screen tidak error
        SetBackground(TOBATheme.ForestGreen);
    }

    // Helper: set background color
    protected void SetBackground(Color color)
    {
        Image img = screenObj.GetComponent<Image>();
        if (img == null)
        {
            img = screenObj.AddComponent<Image>();
            img.sprite        = UIBuilder.GetDefaultSprite();
            img.raycastTarget = false;
        }
        img.color = color;
    }

    public abstract void Show();

    public virtual void Hide()
    {
        if (screenObj != null) Object.Destroy(screenObj);
    }
}