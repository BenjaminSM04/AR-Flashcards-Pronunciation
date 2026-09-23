using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScanStatusController : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image statusDot;
    [SerializeField] private TMP_Text instructionText;

    [Header("Colores")]
    [SerializeField]
    private Color searchingColor =
        new Color32(217, 200, 169, 255);

    [SerializeField]
    private Color foundColor =
        new Color32(83, 200, 139, 255);

    [SerializeField]
    private Color lostColor =
        new Color32(217, 107, 114, 255);

    private void Start()
    {
        SetSearching();
    }

    public void SetSearching()
    {
        SetState(
            "Buscando tarjeta…",
            "Apunta a la tarjeta",
            searchingColor
        );
    }

    public void SetFound(string word)
    {
        SetState(
            "Tarjeta detectada",
            string.IsNullOrWhiteSpace(word)
                ? "Tarjeta detectada"
                : $"¡{word} detectado!",
            foundColor
        );
    }

    public void SetLost()
    {
        SetState(
            "Seguimiento perdido",
            "Mantén la tarjeta frente a la cámara",
            lostColor
        );
    }

    private void SetState(
        string status,
        string instruction,
        Color dotColor
    )
    {
        if (statusText != null)
            statusText.text = status;

        if (instructionText != null)
            instructionText.text = instruction;

        if (statusDot != null)
            statusDot.color = dotColor;
    }
}