using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

// Пространство имён для FilterFunction может быть разным в зависимости от версии Unity.
// В Unity 6 оно находится в UnityEngine.UIElements.
// Если потребуется, замените на using UnityEngine.UIElements.UIR.Experimental;

[UxmlElement]
public partial class BoxShadowElement : VisualElement
{
    private struct ParsedShadow
    {
        public float offsetX, offsetY, blurRadius, spreadRadius;
        public Color color;
        public bool inset;
    }

    private List<ParsedShadow> m_Shadows = new List<ParsedShadow>();

    [UxmlAttribute]
    public string boxShadow
    {
        get => m_BoxShadowString;
        set
        {
            m_BoxShadowString = value;
            ParseBoxShadow(value);
            RefreshShadows();
        }
    }
    private string m_BoxShadowString = "";

    private VisualElement m_ShadowLayer;   // абсолютный слой для тени
    private VisualElement m_MaskLayer;     // обычный дочерний элемент, задаёт обрезку
    private VisualElement m_ContentHost;   // внутри маски

    public BoxShadowElement()
    {
        // 1. Слой тени – абсолютный, чтобы не влиять на размеры родителя
        m_ShadowLayer = new VisualElement
        {
            name = "shadow-layer",
            pickingMode = PickingMode.Ignore
        };
        m_ShadowLayer.style.position = Position.Absolute;
        m_ShadowLayer.style.overflow = Overflow.Visible;
        m_ShadowLayer.generateVisualContent += DrawShadowContent;
        hierarchy.Add(m_ShadowLayer);

        // 2. Маска – обычный элемент, растягивается на весь BoxShadowElement
        m_MaskLayer = new VisualElement
        {
            name = "mask-layer",
            pickingMode = PickingMode.Ignore
        };
        m_MaskLayer.style.flexGrow = 1f;
        m_MaskLayer.style.overflow = Overflow.Hidden; // обрезает по border-radius
        hierarchy.Add(m_MaskLayer);

        // 3. Контент внутри маски
        m_ContentHost = new VisualElement
        {
            name = "content-host"
        };
        m_ContentHost.style.flexGrow = 1f;
        m_MaskLayer.hierarchy.Add(m_ContentHost);

        contentContainer = m_ContentHost;

        RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnAttachToPanel(AttachToPanelEvent evt) => ApplyBlurAndUpdateSize();
    private bool m_Updating;
    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        if (m_Updating) return;
        m_Updating = true;
        try { ApplyBlurAndUpdateSize(); }
        finally { m_Updating = false; }
    }

    public override VisualElement contentContainer { get; }

    // ----- Парсинг CSS-строки -----
    private void ParseBoxShadow(string cssString)
    {
        m_Shadows.Clear();
        if (string.IsNullOrWhiteSpace(cssString)) return;

        string[] parts = cssString.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            if (trimmed.Length == 0) continue;

            bool inset = false;
            if (trimmed.StartsWith("inset", StringComparison.OrdinalIgnoreCase))
            {
                inset = true;
                trimmed = trimmed.Substring(5).Trim();
            }
            else if (trimmed.EndsWith("inset", StringComparison.OrdinalIgnoreCase))
            {
                inset = true;
                trimmed = trimmed.Substring(0, trimmed.Length - 5).Trim();
            }

            string[] tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2) continue;

            Color color = Color.black;
            int colorIndex = -1;
            for (int i = tokens.Length - 1; i >= 0; i--)
            {
                if (TryParseColor(tokens[i], out color))
                {
                    colorIndex = i;
                    break;
                }
            }

            List<float> numbers = new List<float>();
            int end = (colorIndex >= 0) ? colorIndex : tokens.Length;
            for (int i = 0; i < end; i++)
            {
                string numStr = tokens[i].Replace("px", "");
                if (float.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                    numbers.Add(val);
            }

            m_Shadows.Add(new ParsedShadow
            {
                offsetX = numbers.Count > 0 ? numbers[0] : 0f,
                offsetY = numbers.Count > 1 ? numbers[1] : 0f,
                blurRadius = numbers.Count > 2 ? numbers[2] : 0f,
                spreadRadius = numbers.Count > 3 ? numbers[3] : 0f,
                color = color,
                inset = inset
            });
        }
    }

    private bool TryParseColor(string token, out Color color)
    {
        if (token.StartsWith("#"))
            return ColorUtility.TryParseHtmlString(token, out color);
        if (token.StartsWith("rgba", StringComparison.OrdinalIgnoreCase) ||
            token.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            string inner = token.Substring(token.IndexOf('(') + 1).TrimEnd(')');
            string[] rgba = inner.Split(',');
            if (rgba.Length >= 3)
            {
                float r = float.Parse(rgba[0], CultureInfo.InvariantCulture) / 255f;
                float g = float.Parse(rgba[1], CultureInfo.InvariantCulture) / 255f;
                float b = float.Parse(rgba[2], CultureInfo.InvariantCulture) / 255f;
                float a = rgba.Length > 3 ? float.Parse(rgba[3], CultureInfo.InvariantCulture) : 1f;
                color = new Color(r, g, b, a);
                return true;
            }
        }
        color = Color.black;
        return false;
    }

    // ----- Отрисовка тени -----
    private void DrawShadowContent(MeshGenerationContext mgc)
    {
        Rect maskRect = m_MaskLayer.layout;
        if (maskRect.width < 0.1f || maskRect.height < 0.1f) return;

        float shadowLayerX = m_ShadowLayer.layout.x;
        float shadowLayerY = m_ShadowLayer.layout.y;
    
        maskRect.x -= shadowLayerX;
        maskRect.y -= shadowLayerY;

        var boxStyle = resolvedStyle;
        Vector4 borderRadius = new Vector4(
            boxStyle.borderTopLeftRadius,
            boxStyle.borderTopRightRadius,
            boxStyle.borderBottomRightRadius,
            boxStyle.borderBottomLeftRadius
        );

        Painter2D painter = mgc.painter2D;

        foreach (var shadow in m_Shadows)
        {
            if (shadow.color.a <= 0f) continue;

            Rect shadowRect = maskRect;
            shadowRect.x += shadow.offsetX;
            shadowRect.y += shadow.offsetY;
            shadowRect = new Rect(
                shadowRect.x - shadow.spreadRadius,
                shadowRect.y - shadow.spreadRadius,
                shadowRect.width + shadow.spreadRadius * 2f,
                shadowRect.height + shadow.spreadRadius * 2f
            );

            painter.fillColor = shadow.color;
            painter.BeginPath();
            AddRoundedRect(painter, shadowRect, borderRadius);
            painter.Fill();
        
            // Проверяем, был ли нарисован хотя бы один путь
            Debug.Log("Fill() called");
        }
    }

    private void AddRoundedRect(Painter2D painter, Rect rect, Vector4 radius)
    {
        float x = rect.x, y = rect.y, w = rect.width, h = rect.height;
        float r0 = Mathf.Min(radius.x, w * 0.5f, h * 0.5f);
        float r1 = Mathf.Min(radius.y, w * 0.5f, h * 0.5f);
        float r2 = Mathf.Min(radius.z, w * 0.5f, h * 0.5f);
        float r3 = Mathf.Min(radius.w, w * 0.5f, h * 0.5f);

        painter.MoveTo(new Vector2(x + r0, y));
        painter.LineTo(new Vector2(x + w - r1, y));
        painter.ArcTo(new Vector2(x + w, y), new Vector2(x + w, y + r1), r1);
        painter.LineTo(new Vector2(x + w, y + h - r2));
        painter.ArcTo(new Vector2(x + w, y + h), new Vector2(x + w - r2, y + h), r2);
        painter.LineTo(new Vector2(x + r3, y + h));
        painter.ArcTo(new Vector2(x, y + h), new Vector2(x, y + h - r3), r3);
        painter.LineTo(new Vector2(x, y + r0));
        painter.ArcTo(new Vector2(x, y), new Vector2(x + r0, y), r0);
        painter.ClosePath();
    }

    // ----- Размеры и размытие -----
    private void ApplyBlurAndUpdateSize()
    {
        if (m_ShadowLayer == null || m_MaskLayer == null) return;
        
        m_MaskLayer.style.borderTopLeftRadius = resolvedStyle.borderTopLeftRadius;
        m_MaskLayer.style.borderTopRightRadius = resolvedStyle.borderTopRightRadius;
        m_MaskLayer.style.borderBottomRightRadius = resolvedStyle.borderBottomRightRadius;
        m_MaskLayer.style.borderBottomLeftRadius = resolvedStyle.borderBottomLeftRadius;

        Rect maskBounds = m_MaskLayer.layout;

        // Максимальный радиус размытия
        float maxBlur = 0f;
        foreach (var shadow in m_Shadows)
            if (shadow.blurRadius > maxBlur) maxBlur = shadow.blurRadius;

        // Задаём слою тени размеры с учётом расширения под размытие
        float expand = maxBlur * 1.5f;
        m_ShadowLayer.style.left = maskBounds.x - expand;
        m_ShadowLayer.style.top = maskBounds.y - expand;
        m_ShadowLayer.style.width = maskBounds.width + expand * 2f;
        m_ShadowLayer.style.height = maskBounds.height + expand * 2f;

        // Применяем фильтр размытия к слою тени
        if (maxBlur > 0f)
        {
            // Создаём FilterFunction с типом BuiltinBlur
            var blurFilter = new FilterFunction(FilterFunctionType.Blur);
            blurFilter.AddParameter(new FilterParameter(maxBlur));
            m_ShadowLayer.style.filter = new List<FilterFunction> { blurFilter };
        }
        else
        {
            // Сбрасываем фильтр, если размытия нет
            m_ShadowLayer.style.filter = new List<FilterFunction>();
        }

        m_ShadowLayer.MarkDirtyRepaint();
    }

    public void RefreshShadows()
    {
        ParseBoxShadow(m_BoxShadowString);
        ApplyBlurAndUpdateSize();
    }
}