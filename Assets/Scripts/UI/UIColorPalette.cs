using UnityEngine;

[CreateAssetMenu(fileName = "UIColorPalette", menuName = "UI/Color Palette")]
public class UIColorPalette : ScriptableObject
{
    [Header("Glimmer Palette")]
    public Color Glimmer1 = new Color32(166, 88, 109, 255); // #A6586D
    public Color Glimmer2 = new Color32(242, 206, 219, 255); // #F2CEDB
    public Color Glimmer3 = new Color32(191, 101, 143, 255); // #BF658F
    public Color Glimmer4 = new Color32(189, 242, 212, 255); // #BDF2D4
    public Color Glimmer5 = new Color32(217, 200, 169, 255); // #D9C8A9

    [Header("Neutrals (recomendados)")]
    public Color TextPrimary = Color.white;
    public Color TextDark = new Color32(32, 32, 32, 255);    // para fondo claro
    public Color PanelOverlay = new Color(0, 0, 0, 0.66f);      // negro 66% para fondo del modal
}
