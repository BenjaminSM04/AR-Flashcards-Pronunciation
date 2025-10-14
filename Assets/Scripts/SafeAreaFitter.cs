using UnityEngine;

[ExecuteAlways]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rectTransform;
    Rect lastSafeArea;

    void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
#if UNITY_EDITOR
        ApplySafeArea();
#endif
    }

    void ApplySafeArea()
    {
        if (Screen.safeArea == lastSafeArea && rectTransform.hasChanged == false) return;

        lastSafeArea = Screen.safeArea;

        var anchorMin = lastSafeArea.position;
        var anchorMax = lastSafeArea.position + lastSafeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        rectTransform.hasChanged = false;
    }
}
