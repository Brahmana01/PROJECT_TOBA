
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseScreen
{
    protected GameObject screenObj;
    protected Transform container;

    private bool isHidden = false;

    // ============================================================
    // SCREEN STATE
    // ============================================================

    public bool IsAlive
    {
        get
        {
            return !isHidden && screenObj != null;
        }
    }

    public bool IsReady
    {
        get
        {
            return IsAlive && container != null;
        }
    }

    public bool IsCurrent
    {
        get
        {
            return IsReady &&
                   ScreenManager.Instance != null &&
                   ScreenManager.Instance.IsCurrentScreen(this);
        }
    }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public BaseScreen(Transform parent, string name)
    {
        container = parent;

        if (parent == null)
        {
            isHidden = true;

            Debug.LogWarning(
                "[BaseScreen] Parent canvas tidak tersedia: " + name
            );

            return;
        }

        screenObj = new GameObject(name);

        screenObj.transform.SetParent(parent, false);

        RectTransform rt = screenObj.AddComponent<RectTransform>();

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        SetBackground(TOBATheme.ForestGreen);
    }

    // ============================================================
    // BACKGROUND
    // ============================================================

    protected void SetBackground(Color color)
    {
        if (!IsAlive)
            return;

        Image img = screenObj.GetComponent<Image>();

        if (img == null)
        {
            img = screenObj.AddComponent<Image>();

            img.sprite = UIBuilder.GetDefaultSprite();
            img.raycastTarget = false;
        }

        img.color = color;
    }

    // ============================================================
    // SHOW
    // ============================================================

    public abstract void Show();

    // ============================================================
    // HIDE / CLEANUP
    // ============================================================

    public virtual void Hide()
    {
        if (isHidden)
            return;

        isHidden = true;

        GameObject oldScreen = screenObj;

        // Putuskan referensi sebelum GameObject dihancurkan.
        screenObj = null;

        if (oldScreen != null)
        {
            Object.Destroy(oldScreen);
        }
    }
}
