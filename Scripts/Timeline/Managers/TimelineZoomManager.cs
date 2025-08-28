using UnityEngine;
using UnityEngine.UI;

public class TimelineZoomManager : MonoBehaviour
{
    private TimelineGrid timeline;
    private ScrollRect parentScrollRect;
    private Canvas cachedCanvas;

    public void Initialize(TimelineGrid grid, Canvas canvas)
    {
        timeline = grid;
        cachedCanvas = canvas;
        parentScrollRect = grid.GetComponentInParent<ScrollRect>();
    }

    public void HandleZoomInput()
    {
        if (Input.mouseScrollDelta.y != 0f)
        {
            Vector2 mousePos = Input.mousePosition;
            if (IsMouseOverTimeline(mousePos))
            {
                ZoomTimeline(Input.mouseScrollDelta.y, mousePos);
            }
        }
    }

    private bool IsMouseOverTimeline(Vector2 mousePos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            timeline.scrollViewContent, mousePos, cachedCanvas?.worldCamera);
    }

    private void ZoomTimeline(float zoomDelta, Vector2 mousePos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            timeline.scrollViewContent, mousePos, cachedCanvas?.worldCamera, out Vector2 localPoint);

        float mouseTimeBeforeZoom = timeline.XToTime(localPoint.x);

        var config = timeline.GetConfig();
        float zoomFactor = zoomDelta > 0 ? config.zoomSpeed : 1f / config.zoomSpeed;
        float newZoom = Mathf.Clamp(config.gridCellHorizontalPixelCount * zoomFactor, config.minZoom, config.maxZoom);

        if (Mathf.Abs(newZoom - config.gridCellHorizontalPixelCount) < 0.1f) return;

        config.gridCellHorizontalPixelCount = (int)newZoom;

        RefreshAllBarsAfterZoom();

        MaintainMousePosition(mouseTimeBeforeZoom, localPoint);
    }

    private void RefreshAllBarsAfterZoom()
    {
        var barManager = timeline.GetBarManager();
        var contentToBar = barManager.GetContentToBar();

        foreach (var controller in timeline.GetVideoControllers())
        {
            var content = controller.getvc();
            RectTransform bar = null;
            if (contentToBar.TryGetValue(content, out bar) && bar != null)
                UpdateBarPosition(bar, content);
        }

        foreach (var controller in timeline.GetImageControllers())
        {
            var content = controller.getic();
            RectTransform bar = null;
            if (contentToBar.TryGetValue(content, out bar) && bar != null)
                UpdateBarPosition(bar, content);
        }

        foreach (var controller in timeline.GetAudioControllers())
        {
            var content = controller.getac();
            RectTransform bar = null;
            if (contentToBar.TryGetValue(content, out bar) && bar != null)
                UpdateBarPosition(bar, content);
        }

        foreach (var controller in timeline.modelControllerList)
        {
            var content = controller.getmc();
            RectTransform bar = null;
            if (contentToBar.TryGetValue(content, out bar) && bar != null)
                UpdateBarPosition(bar, content);
        }

        UpdateContentWidthAfterZoom();
        timeline.UpdateTimeRuler();
    }

    private void UpdateBarPosition(RectTransform bar, Content content)
    {
        if (!bar || content == null) return;

        float startX = timeline.TimeToX(content.getStart());
        float duration = timeline.GetContentDuration(content);
        float width = timeline.TimeToX(duration) - timeline.TimeToX(0f);

        bar.anchoredPosition = new Vector2(startX, bar.anchoredPosition.y);
        bar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        timeline.GetPreviewManager().RefreshBarPreviews(bar, content);
    }

    private void UpdateContentWidthAfterZoom()
    {
        float maxEndTime = 0f;

        foreach (var controller in timeline.GetVideoControllers())
            maxEndTime = Mathf.Max(maxEndTime, controller.getvc().getEnd());

        foreach (var controller in timeline.GetImageControllers())
            maxEndTime = Mathf.Max(maxEndTime, controller.getic().getEnd());

        foreach (var controller in timeline.GetAudioControllers())
            maxEndTime = Mathf.Max(maxEndTime, controller.getac().getEnd());

        foreach (var controller in timeline.modelControllerList)
            maxEndTime = Mathf.Max(maxEndTime, controller.getmc().getEnd());

        if (maxEndTime > 0f)
            timeline.EnsureWidthForTime(maxEndTime);
    }

    private void MaintainMousePosition(float mouseTime, Vector2 originalLocalPoint)
    {
        if (!parentScrollRect) return;

        float oldMouseX = originalLocalPoint.x;
        float newMouseX = timeline.TimeToX(mouseTime);
        float deltaX = newMouseX - oldMouseX;

        float contentWidth = timeline.scrollViewContent.rect.width;
        float viewportWidth = parentScrollRect.viewport.rect.width;

        if (contentWidth > viewportWidth)
        {
            float maxScroll = contentWidth - viewportWidth;
            float currentScroll = parentScrollRect.horizontalNormalizedPosition * maxScroll;
            float newScroll = Mathf.Clamp(currentScroll + deltaX, 0f, maxScroll);

            parentScrollRect.horizontalNormalizedPosition = newScroll / maxScroll;
        }
    }
}