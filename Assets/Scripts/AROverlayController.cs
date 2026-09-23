using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AROverlayController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RectTransform panelRect;

    [Header("Textos")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text phoneticText;

    [Header("Botones")]
    [SerializeField] private Button pronounceBtn;
    [SerializeField] private Button closeBtn;

    [Header("Datos actuales")]
    [SerializeField] private string englishName = "Scissors";
    [SerializeField] private string phonetic = "/ˈsɪz.ərz/";

    [Header("Audio")]
    [SerializeField] private AudioClip audioClipOverride;
    [SerializeField] private bool autoPlayOnOpen = false;

    [Header("Animación")]
    [SerializeField, Min(0.05f)]
    private float showDuration = 0.25f;

    [SerializeField, Min(0.05f)]
    private float hideDuration = 0.18f;

    [SerializeField]
    private float hiddenOffsetY = -90f;

    [SerializeField, Range(0.8f, 1f)]
    private float hiddenScale = 0.97f;

    [Header("Prueba en el Editor")]
    [SerializeField] private KeyCode debugToggleKey = KeyCode.Space;

    [Header("Texto 3D opcional")]
    [SerializeField] private Transform anchor;
    [SerializeField] private GameObject worldLabelPrefab;

    private GameObject worldLabelInstance;
    private Coroutine animationRoutine;
    private Vector2 shownPosition;
    private bool visible;

    public bool IsVisible => visible;

    private void Awake()
    {
        CachePanelComponents();

        if (panelRect != null)
            shownPosition = panelRect.anchoredPosition;
    }

    private void OnEnable()
    {
        if (pronounceBtn != null)
            pronounceBtn.onClick.AddListener(Pronounce);

        if (closeBtn != null)
            closeBtn.onClick.AddListener(HideOverlay);
    }

    private void Start()
    {
        ApplyTexts();
        CreateWorldLabel();
        SetPanelImmediate(false);
    }

    private void OnDisable()
    {
        if (pronounceBtn != null)
            pronounceBtn.onClick.RemoveListener(Pronounce);

        if (closeBtn != null)
            closeBtn.onClick.RemoveListener(HideOverlay);

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }

#if UNITY_EDITOR && ENABLE_LEGACY_INPUT_MANAGER
    private void Update()
    {
        if (Input.GetKeyDown(debugToggleKey))
            ToggleOverlay();
    }
#endif

    private void CachePanelComponents()
    {
        if (panel == null)
            return;

        if (panelRect == null)
            panelRect = panel.GetComponent<RectTransform>();

        if (panelCanvasGroup == null)
            panelCanvasGroup = panel.GetComponent<CanvasGroup>();

        if (panelCanvasGroup == null)
            panelCanvasGroup = panel.AddComponent<CanvasGroup>();
    }

    private void ApplyTexts()
    {
        if (nameText != null)
            nameText.text = englishName;

        if (phoneticText != null)
            phoneticText.text = phonetic;
    }

    private void CreateWorldLabel()
    {
        if (worldLabelPrefab == null || anchor == null)
            return;

        worldLabelInstance = Instantiate(
            worldLabelPrefab,
            anchor.position,
            anchor.rotation
        );

        worldLabelInstance.SetActive(false);

        TMP_Text worldText =
            worldLabelInstance.GetComponentInChildren<TMP_Text>();

        if (worldText != null)
            worldText.text = englishName;
    }

    public void ShowOverlay()
    {
        if (panel == null)
        {
            Debug.LogWarning(
                "AROverlayController: no se asignó WordBottomSheet.",
                this
            );
            return;
        }

        CachePanelComponents();

        if (visible && animationRoutine == null)
            return;

        visible = true;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimatePanel(true));

        if (worldLabelInstance != null)
            worldLabelInstance.SetActive(true);

        if (autoPlayOnOpen)
            Pronounce();
    }

    public void HideOverlay()
    {
        visible = false;

        if (worldLabelInstance != null)
            worldLabelInstance.SetActive(false);

        AudioPronouncer.I?.Stop();

        if (panel == null || !panel.activeSelf)
            return;

        CachePanelComponents();

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimatePanel(false));
    }

    public void ToggleOverlay()
    {
        if (visible)
            HideOverlay();
        else
            ShowOverlay();
    }

    private IEnumerator AnimatePanel(bool showing)
    {
        if (panelRect == null || panelCanvasGroup == null)
        {
            SetPanelImmediate(showing);
            animationRoutine = null;
            yield break;
        }

        if (showing)
            panel.SetActive(true);

        Vector2 hiddenPosition =
            shownPosition + new Vector2(0f, hiddenOffsetY);

        Vector2 startPosition = panelRect.anchoredPosition;
        Vector2 targetPosition =
            showing ? shownPosition : hiddenPosition;

        Vector3 startScale = panelRect.localScale;
        Vector3 targetScale =
            showing
                ? Vector3.one
                : Vector3.one * hiddenScale;

        float startAlpha = panelCanvasGroup.alpha;
        float targetAlpha = showing ? 1f : 0f;

        float duration = showing ? showDuration : hideDuration;
        float elapsed = 0f;

        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = showing;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = EaseOutCubic(progress);

            panelRect.anchoredPosition = Vector2.LerpUnclamped(
                startPosition,
                targetPosition,
                easedProgress
            );

            panelRect.localScale = Vector3.LerpUnclamped(
                startScale,
                targetScale,
                easedProgress
            );

            panelCanvasGroup.alpha = Mathf.LerpUnclamped(
                startAlpha,
                targetAlpha,
                easedProgress
            );

            yield return null;
        }

        panelRect.anchoredPosition = targetPosition;
        panelRect.localScale = targetScale;
        panelCanvasGroup.alpha = targetAlpha;

        panelCanvasGroup.interactable = showing;
        panelCanvasGroup.blocksRaycasts = showing;

        if (!showing)
            panel.SetActive(false);

        animationRoutine = null;
    }

    private void SetPanelImmediate(bool show)
    {
        if (panel == null)
            return;

        CachePanelComponents();

        visible = show;

        Vector2 hiddenPosition =
            shownPosition + new Vector2(0f, hiddenOffsetY);

        if (panelRect != null)
        {
            panelRect.anchoredPosition =
                show ? shownPosition : hiddenPosition;

            panelRect.localScale =
                show ? Vector3.one : Vector3.one * hiddenScale;
        }

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = show ? 1f : 0f;
            panelCanvasGroup.interactable = show;
            panelCanvasGroup.blocksRaycasts = show;
        }

        panel.SetActive(show);
    }

    private static float EaseOutCubic(float value)
    {
        float inverse = 1f - value;
        return 1f - inverse * inverse * inverse;
    }

    public void Pronounce()
    {
        if (audioClipOverride != null)
            AudioPronouncer.I?.SayClip(audioClipOverride);
        else
            AudioPronouncer.I?.Say(englishName);
    }

    public void SetData(string newName, string newPhonetic)
    {
        englishName = newName;
        phonetic = newPhonetic;
        audioClipOverride = null;

        ApplyTexts();
        UpdateWorldLabel();
    }

    public void SetDataWithClip(
        string newName,
        string newPhonetic,
        AudioClip clip
    )
    {
        englishName = newName;
        phonetic = newPhonetic;
        audioClipOverride = clip;

        ApplyTexts();
        UpdateWorldLabel();
    }

    private void UpdateWorldLabel()
    {
        if (worldLabelInstance == null)
            return;

        TMP_Text worldText =
            worldLabelInstance.GetComponentInChildren<TMP_Text>();

        if (worldText != null)
            worldText.text = englishName;
    }

    [ContextMenu("Debug/Mostrar panel")]
    private void DebugShowPanel()
    {
        ShowOverlay();
    }

    [ContextMenu("Debug/Ocultar panel")]
    private void DebugHidePanel()
    {
        HideOverlay();
    }

    [ContextMenu("Debug/Alternar panel")]
    private void DebugTogglePanel()
    {
        ToggleOverlay();
    }
}