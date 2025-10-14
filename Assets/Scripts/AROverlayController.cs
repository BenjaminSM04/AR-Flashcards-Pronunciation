using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AROverlayController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;         
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text phoneticText;
    [SerializeField] private Button pronounceBtn;
    [SerializeField] private Button closeBtn;

    [Header("Objeto / Datos")]
    [SerializeField] private string englishName = "Scissors";
    [SerializeField] private string phonetic = "/ˈsɪz.əz/";

    [Header("Audio (opcional por objeto)")]
    [SerializeField] private AudioClip audioClipOverride; 

    [Header("Texto 3D (opcional)")]
    [SerializeField] private Transform anchor;
    [SerializeField] private GameObject worldLabelPrefab;
    private GameObject worldLabelInstance;

    private bool visible = false;

    void Start()
    {
        if (panel) panel.SetActive(false);
        if (pronounceBtn) pronounceBtn.onClick.AddListener(Pronounce);
        if (closeBtn) closeBtn.onClick.AddListener(HideOverlay);

        ApplyTexts();

        if (worldLabelPrefab && anchor)
        {
            worldLabelInstance = Instantiate(worldLabelPrefab, anchor.position, anchor.rotation);
            worldLabelInstance.SetActive(false);
            var tmp = worldLabelInstance.GetComponentInChildren<TMP_Text>();
            if (tmp) tmp.text = englishName;
        }
    }

    private void ApplyTexts()
    {
        if (nameText) nameText.text = englishName;
        if (phoneticText) phoneticText.text = phonetic;
    }

    public void ShowOverlay()
    {
        visible = true;
        if (panel) panel.SetActive(true);
        if (worldLabelInstance) worldLabelInstance.SetActive(true);

        Pronounce();
    }

    public void HideOverlay()
    {
        visible = false;
        if (panel) panel.SetActive(false);
        if (worldLabelInstance) worldLabelInstance.SetActive(false);
        AudioPronouncer.I?.Stop();
    }

    public void ToggleOverlay()
    {
        if (visible) HideOverlay(); else ShowOverlay();
    }

    public void Pronounce()
    {
        if (audioClipOverride)
            AudioPronouncer.I?.SayClip(audioClipOverride);
        else
            AudioPronouncer.I?.Say(englishName); 
    }

    // Para cambiar palabra/fonética sin clip
    public void SetData(string newName, string newPhonetic)
    {
        englishName = newName;
        phonetic = newPhonetic;
        ApplyTexts();

        if (worldLabelInstance)
        {
            var tmp = worldLabelInstance.GetComponentInChildren<TMP_Text>();
            if (tmp) tmp.text = englishName;
        }
    }

    public void SetDataWithClip(string newName, string newPhonetic, AudioClip clip)
    {
        audioClipOverride = clip;
        SetData(newName, newPhonetic);
    }
}
