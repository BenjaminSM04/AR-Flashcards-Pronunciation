using System.Collections;
using UnityEngine;
using Vuforia;

public class OverlayTrackableEventHandler
    : DefaultTrackableEventHandler
{
    [Header("Datos de esta tarjeta")]
    [SerializeField] private string englishName = "Scissors";
    [SerializeField] private string phonetic = "/ˈsɪz.ɚz/";
    [SerializeField] private AudioClip voiceClip;

    [Header("Referencias")]
    [SerializeField] private AROverlayController overlay;
    [SerializeField] private ScanStatusController scanStatus;

    [Header("Seguimiento")]
    [SerializeField, Min(0f)]
    private float trackingLostDelay = 1f;

    private Coroutine closeRoutine;
    private static OverlayTrackableEventHandler currentTarget;

    protected override void Start()
    {
        base.Start();

        if (overlay == null)
            overlay = FindObjectOfType<AROverlayController>();

        if (scanStatus == null)
            scanStatus = FindObjectOfType<ScanStatusController>();
    }

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();

        CancelPendingClose();
        currentTarget = this;

        // Pasamos el clip incluso si es nulo. Así el controlador elimina
        // cualquier audio que haya quedado de la tarjeta anterior.
        if (overlay != null)
            overlay.SetDataWithClip(englishName, phonetic, voiceClip);

        if (scanStatus != null)
            scanStatus.SetFound(englishName);

        // El panel sigue abriéndose mediante el Virtual Button.
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();

        // Si otra tarjeta pasó a ser la activa, esta no debe cerrar su panel.
        if (currentTarget != this)
            return;

        if (scanStatus != null)
            scanStatus.SetLost();

        CancelPendingClose();
        closeRoutine = StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(trackingLostDelay);

        closeRoutine = null;

        if (currentTarget != this)
            yield break;

        if (overlay != null)
            overlay.HideOverlay();

        if (scanStatus != null)
            scanStatus.SetSearching();

        currentTarget = null;
    }

    private void CancelPendingClose()
    {
        if (closeRoutine == null)
            return;

        StopCoroutine(closeRoutine);
        closeRoutine = null;
    }
}