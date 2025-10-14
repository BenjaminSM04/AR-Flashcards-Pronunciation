using UnityEngine;
using Vuforia;

public class OverlayTrackableEventHandler : DefaultTrackableEventHandler
{
    [Header("Datos de esta tarjeta")]
    [SerializeField] private string englishName = "Scissors";
    [SerializeField] private string phonetic = "/ˈsɪz.ɚz/";

    [Header("Audio (opcional)")]
    [SerializeField] private AudioClip voiceClip;   // ← arrastra aquí el audio de esta tarjeta

    [Header("Refs")]
    [SerializeField] private AROverlayController overlay;

    protected override void Start()
    {
        base.Start();
        if (!overlay) overlay = FindObjectOfType<AROverlayController>();
        overlay?.HideOverlay(); // aseguramos oculto al inicio
    }

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        // SOLO actualiza datos (incluye el clip). NO mostrar el panel aquí.
        if (overlay)
        {
            if (voiceClip)
                overlay.SetDataWithClip(englishName, phonetic, voiceClip);
            else
                overlay.SetData(englishName, phonetic); // fallback: usará Resources/tts/<englishName>
        }
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        overlay?.HideOverlay();
    }
}
