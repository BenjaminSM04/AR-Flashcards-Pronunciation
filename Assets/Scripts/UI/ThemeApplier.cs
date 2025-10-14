using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThemeApplier : MonoBehaviour
{
    [SerializeField] private UIColorPalette palette;

    [Header("Estructura")]
    [SerializeField] private Image panelBackground;   
    [SerializeField] private Image cardBackground;   
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text phoneticText;
    [SerializeField] private Button pronounceBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Image previewImage;      
    [SerializeField] private Image replayIcon;    

    void Awake()
    {
        if (!palette) return;

       // if (panelBackground) panelBackground.color = palette.PanelOverlay;

       // if (cardBackground) cardBackground.color = Color.white;

        // Textos
        if (nameText) nameText.color = palette.TextPrimary;
        if (phoneticText) phoneticText.color = new Color(palette.TextPrimary.r, palette.TextPrimary.g, palette.TextPrimary.b, 0.75f);

        // Botón principal (Pronounce) – normal/hover/pressed
        ApplyButtonColors(pronounceBtn,
            normal: palette.Glimmer3,   // base
            highlighted: palette.Glimmer1, // hover
            pressed: Darken(palette.Glimmer3, 0.85f),
            textColor: Color.white
        );

        // Botón secundario (Close) – tono suave
        ApplyButtonColors(closeBtn,
            normal: palette.Glimmer2,
            highlighted: Lighten(palette.Glimmer2, 1.08f),
            pressed: Darken(palette.Glimmer2, 0.9f),
            textColor: palette.TextDark
        );

        // Previsualización/íconos (opcional)
        if (previewImage) previewImage.color = Color.white;
        if (replayIcon) replayIcon.color = palette.Glimmer3;
    }

    private void ApplyButtonColors(Button btn, Color normal, Color highlighted, Color pressed, Color textColor)
    {
        if (!btn) return;

        var cb = btn.colors;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = .08f;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        cb.pressedColor = pressed;
        cb.selectedColor = normal;
        cb.disabledColor = new Color(normal.r, normal.g, normal.b, 0.4f);
        btn.colors = cb;

        var img = btn.GetComponent<Image>();
        if (img) img.color = normal;

        var tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp) tmp.color = textColor;
    }

    private Color Darken(Color c, float factor) => new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
    private Color Lighten(Color c, float factor) => new Color(
        Mathf.Clamp01(c.r * factor),
        Mathf.Clamp01(c.g * factor),
        Mathf.Clamp01(c.b * factor),
        c.a);
}
