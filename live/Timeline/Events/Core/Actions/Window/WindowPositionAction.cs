using UnityEngine;
using System.Collections;

/// <summary>
/// Timeline content'lerin window pozisyonunu değiştiren action
/// Mevcut WindowManager API'sini kullanır
/// </summary>
public class WindowPositionAction : BaseEventAction
{
    public override string ActionType => "WindowPosition";
    public override bool RequiresContinuousUpdate => true;

    private struct WindowState
    {
        public Content content;
        public Vector2 previousPosition;
        public Vector2 previousSize;
        public Coroutine activeCoroutine;
        public TimelineGrid timeline;
    }

    // Undo için önceki state'leri sakla
    private static System.Collections.Generic.Dictionary<string, WindowState> previousStates =
        new System.Collections.Generic.Dictionary<string, WindowState>();

    public override void Execute(EventActionData actionData)
    {
        Debug.Log($"[WindowPositionAction] *** EXECUTE START ***");
        Debug.Log($"[WindowPositionAction] Target name: '{actionData.targetObjectName}'");
        Debug.Log($"[WindowPositionAction] Target path: '{actionData.targetObjectPath}'");

        // Timeline referansını al
        TimelineGrid timeline = GameObject.FindObjectOfType<TimelineGrid>();
        if (timeline == null)
        {
            Debug.LogError("[WindowPositionAction] TimelineGrid not found!");
            return;
        }

        Debug.Log($"[WindowPositionAction] Timeline found, video controllers: {timeline.GetVideoControllers().Count}");

        // Target content'i bul
        Content targetContent = FindTimelineContent(actionData, timeline);
        Debug.Log($"[WindowPositionAction] FindTimelineContent result: {(targetContent != null ? "FOUND" : "NULL")}");

        if (targetContent == null)
        {
            Debug.LogError($"[WindowPositionAction] Timeline content not found: {actionData.targetObjectName}");
            return;
        }

    // Parametreleri al
    Vector2 targetPosition = actionData.GetParameter<Vector2>("position", Vector2.zero);
        float duration = actionData.GetParameter<float>("duration", 1f);
        string easingType = actionData.GetParameter<string>("easing", "linear");
        bool maintainSize = actionData.GetParameter<bool>("maintainSize", true);
        Vector2 targetSize = actionData.GetParameter<Vector2>("size", new Vector2(400, 300));

        // Mevcut window pozisyonu ve boyutunu al
        var windowManager = timeline.GetWindowManager();
        Vector2 currentPosition = windowManager.GetWindowPosition(targetContent);
        Vector2 currentSize = windowManager.GetWindowSize(targetContent);

        // Undo için state'i sakla
        string key = actionData.actionId;
        previousStates[key] = new WindowState
        {
            content = targetContent,
            previousPosition = currentPosition,
            previousSize = currentSize,
            activeCoroutine = null,
            timeline = timeline
        };

        // Animation başlat
        if (duration > 0.01f)
        {
            var state = previousStates[key];
            state.activeCoroutine = timeline.StartCoroutine(AnimateWindow(
                targetContent,
                currentPosition, targetPosition,
                maintainSize ? currentSize : targetSize,
                maintainSize ? currentSize : targetSize,
                duration, easingType, windowManager));
            previousStates[key] = state;
        }
        else
        {
            // Instant change - mevcut WindowManager API kullan
            windowManager.SetWindowProperties(targetContent,
                targetPosition.x, targetPosition.y,
                maintainSize ? currentSize.x : targetSize.x,
                maintainSize ? currentSize.y : targetSize.y);
        }

        LogExecution(actionData, $"Window move to {targetPosition} over {duration:F2}s");
    }

    public override void Undo(EventActionData actionData)
    {
        string key = actionData.actionId;

        if (previousStates.TryGetValue(key, out WindowState state))
        {
            if (state.content != null && state.timeline != null)
            {
                // Aktif coroutine'i durdur
                if (state.activeCoroutine != null)
                {
                    state.timeline.StopCoroutine(state.activeCoroutine);
                }

                // Önceki pozisyon ve boyuta geri dön
                var windowManager = state.timeline.GetWindowManager();
                windowManager.SetWindowProperties(state.content,
                    state.previousPosition.x, state.previousPosition.y,
                    state.previousSize.x, state.previousSize.y);

                LogExecution(actionData, $"Restored window to {state.previousPosition}");
            }

            // State'i temizle
            previousStates.Remove(key);
        }
    }

    public override bool IsValid(EventActionData actionData)
    {
        if (!base.IsValid(actionData)) return false;

        // position parametresi mevcut mu kontrol et
        bool hasPositionParam = actionData.parameters.Exists(p => p.name == "position");
        if (!hasPositionParam)
        {
            Debug.LogWarning($"[WindowPositionAction] Missing 'position' parameter for target: {actionData.targetObjectName}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Window position/size animation coroutine
    /// </summary>
    private IEnumerator AnimateWindow(Content content,
        Vector2 fromPos, Vector2 toPos,
        Vector2 fromSize, Vector2 toSize,
        float duration, string easingType,
        TimelineWindowManager windowManager)
    {
        float elapsed = 0f;

        while (elapsed < duration && content != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Easing uygula
            float easedT = ApplyEasing(t, easingType);

            // Position ve size interpolate et
            Vector2 currentPos = Vector2.Lerp(fromPos, toPos, easedT);
            Vector2 currentSize = Vector2.Lerp(fromSize, toSize, easedT);

            // Mevcut WindowManager API kullan
            windowManager.SetWindowProperties(content,
                currentPos.x, currentPos.y,
                currentSize.x, currentSize.y);

            yield return null;
        }

        // Final values'ları garantile
        if (content != null)
        {
            windowManager.SetWindowProperties(content,
                toPos.x, toPos.y,
                toSize.x, toSize.y);
        }
    }

    /// <summary>
    /// Easing function'ları (ObjectScaleAction'dan kopyalandı)
    /// </summary>
    private float ApplyEasing(float t, string easingType)
    {
        switch (easingType.ToLower())
        {
            case "ease-in":
                return t * t;

            case "ease-out":
                return t * (2 - t);

            case "ease-in-out":
                return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

            case "bounce":
                if (t < 1f / 2.75f)
                    return 7.5625f * t * t;
                else if (t < 2f / 2.75f)
                    return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
                else if (t < 2.5f / 2.75f)
                    return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
                else
                    return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;

            default: // linear
                return t;
        }
    }
}