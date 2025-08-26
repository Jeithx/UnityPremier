using System;
using UnityEngine;

public class TimelineInputManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Fare tekerleği girdisinin algılanacağı alan (ScrollView Content)")]
    public RectTransform timelineArea; // TimelineGrid'deki scrollViewContent'i buraya atayacaksınız

    // Dışarıya yayınlanacak olaylar (Events)
    public event Action<float, Vector2> OnZoomRequested;

    private Canvas cachedCanvas;

    void Awake()
    {
        cachedCanvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        // Sadece Zoom girdisini dinle
        HandleZoomInput();
    }

    private void HandleZoomInput()
    {
        // Fare tekerleği hareket ettiyse
        if (Input.mouseScrollDelta.y != 0f)
        {
            Vector2 mousePos = Input.mousePosition;
            // Eğer fare imleci timeline alanı üzerindeyse
            if (IsMouseOverTimeline(mousePos))
            {
                // Zoom yapılması gerektiğini ilgili sistemlere bildir (event yayınla)
                OnZoomRequested?.Invoke(Input.mouseScrollDelta.y, mousePos);
            }
        }
    }

    private bool IsMouseOverTimeline(Vector2 mousePos)
    {
        if (timelineArea == null) return false;

        // RectTransformUtility kullanarak farenin belirtilen alan üzerinde olup olmadığını kontrol et
        return RectTransformUtility.RectangleContainsScreenPoint(
            timelineArea, mousePos, cachedCanvas?.worldCamera);
    }
}