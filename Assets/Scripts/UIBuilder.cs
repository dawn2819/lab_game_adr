using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hàm dựng UI bằng code dùng chung cho HUD (bảng thắng/thua) và các màn hình menu.
/// </summary>
public static class UIBuilder
{
    private static Font font;
    public static Font DefaultFont
    {
        get
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    /// <summary>Canvas + EventSystem chuẩn 1920x1080 (dùng cho scene menu không có sẵn UI).</summary>
    public static Canvas CreateCanvas(string name)
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static Image Panel(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        Stretch(obj.GetComponent<RectTransform>());
        return img;
    }

    public static Image Box(Transform parent, string name, Color color, Vector2 pos, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return img;
    }

    public static Text Label(Transform parent, string text, int fontSize, Color color, Vector2 pos, Vector2 size, TextAnchor align = TextAnchor.MiddleCenter)
    {
        GameObject obj = new GameObject("Label");
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = align;
        txt.text = text;
        txt.raycastTarget = false;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2, -2);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return txt;
    }

    public static Button Button(Transform parent, string text, Vector2 pos, Vector2 size, Color color, UnityAction onClick)
    {
        GameObject obj = new GameObject("Btn_" + text);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        cb.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        btn.colors = cb;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text label = Label(obj.transform, text, 30, Color.white, Vector2.zero, Vector2.zero);
        Stretch(label.rectTransform);

        btn.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            onClick.Invoke();
        });
        return btn;
    }

    /// <summary>Xếp hàng ngang các nút cách đều nhau, tâm hàng tại centerY.</summary>
    public static void ButtonRow(Transform parent, float centerY, Vector2 size, float gap, Color color, params (string text, UnityAction action)[] buttons)
    {
        float totalW = buttons.Length * size.x + (buttons.Length - 1) * gap;
        float x = -totalW * 0.5f + size.x * 0.5f;
        foreach (var b in buttons)
        {
            Button(parent, b.text, new Vector2(x, centerY), size, color, b.action);
            x += size.x + gap;
        }
    }

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
