using UnityEngine;
using System.Collections;

/// <summary>
/// Model'lerin transform'unu (position, rotation, scale) değiştiren action
/// Mevcut ModelContent.GetModelInstance() API'sini kullanır
/// </summary>
public class ModelTransformAction : BaseEventAction
{
    public override string ActionType => "ModelTransform";
    public override bool RequiresContinuousUpdate => true; // Animation için gerekli

    private struct TransformState
    {
        public GameObject modelInstance;
        public Vector3 previousPosition;
        public Vector3 previousRotation;
        public Vector3 previousScale;
        public Coroutine activeCoroutine;
        public TimelineGrid timeline;
    }

    // Undo için önceki state'leri sakla
    private static System.Collections.Generic.Dictionary<string, TransformState> previousStates =
        new System.Collections.Generic.Dictionary<string, TransformState>();

    public override void Execute(EventActionData actionData)
    {
        // Timeline referansını al
        TimelineGrid timeline = GameObject.FindObjectOfType<TimelineGrid>();
        if (timeline == null)
        {
            Debug.LogError("[ModelTransformAction] TimelineGrid not found!");
            return;
        }

        // Target model content'i bul
        ModelContent targetModel = FindTimelineContent(actionData, timeline) as ModelContent;
        if (targetModel == null)
        {
            Debug.LogError($"[ModelTransformAction] Model content not found: {actionData.targetObjectName}");
            return;
        }

        GameObject modelInstance = targetModel.GetModelInstance();
        if (modelInstance == null)
        {
            Debug.LogError($"[ModelTransformAction] Model instance not found for: {actionData.targetObjectName}");
            return;
        }

        // Parametreleri al
        Vector3 targetPosition = actionData.GetParameter<Vector3>("position", modelInstance.transform.position);
        Vector3 targetRotation = actionData.GetParameter<Vector3>("rotation", modelInstance.transform.eulerAngles);
        Vector3 targetScale = actionData.GetParameter<Vector3>("scale", modelInstance.transform.localScale);

        float duration = actionData.GetParameter<float>("duration", 1f);
        string easingType = actionData.GetParameter<string>("easing", "linear");
        bool useLocalSpace = actionData.GetParameter<bool>("local", true);

        string transformType = actionData.GetParameter<string>("transformType", "all"); // all, position, rotation, scale

        // Mevcut transform değerlerini al
        Vector3 currentPos = useLocalSpace ? modelInstance.transform.localPosition : modelInstance.transform.position;
        Vector3 currentRot = modelInstance.transform.eulerAngles;
        Vector3 currentScale = modelInstance.transform.localScale;

        // Undo için state'i sakla
        string key = actionData.actionId;
        previousStates[key] = new TransformState
        {
            modelInstance = modelInstance,
            previousPosition = currentPos,
            previousRotation = currentRot,
            previousScale = currentScale,
            activeCoroutine = null,
            timeline = timeline
        };

        // Animation başlat
        if (duration > 0.01f)
        {
            var state = previousStates[key];
            state.activeCoroutine = timeline.StartCoroutine(AnimateTransform(
                modelInstance,
                currentPos, targetPosition,
                currentRot, targetRotation,
                currentScale, targetScale,
                duration, easingType, useLocalSpace, transformType));
            previousStates[key] = state;
        }
        else
        {
            // Instant change
            ApplyTransform(modelInstance, targetPosition, targetRotation, targetScale, useLocalSpace, transformType);
        }

        LogExecution(actionData, $"Transform model over {duration:F2}s (type: {transformType})");
    }

    public override void Undo(EventActionData actionData)
    {
        string key = actionData.actionId;

        if (previousStates.TryGetValue(key, out TransformState state))
        {
            if (state.modelInstance != null && state.timeline != null)
            {
                // Aktif coroutine'i durdur
                if (state.activeCoroutine != null)
                {
                    state.timeline.StopCoroutine(state.activeCoroutine);
                }

                // Önceki transform'a geri dön
                bool useLocal = actionData.GetParameter<bool>("local", true);
                ApplyTransform(state.modelInstance,
                    state.previousPosition, state.previousRotation, state.previousScale,
                    useLocal, "all");

                LogExecution(actionData, "Restored model transform");
            }

            // State'i temizle
            previousStates.Remove(key);
        }
    }

    public override bool IsValid(EventActionData actionData)
    {
        if (!base.IsValid(actionData)) return false;

        // En az bir transform parametresi olmalı
        bool hasTransformParam =
            actionData.parameters.Exists(p => p.name == "position") ||
            actionData.parameters.Exists(p => p.name == "rotation") ||
            actionData.parameters.Exists(p => p.name == "scale");

        if (!hasTransformParam)
        {
            Debug.LogWarning($"[ModelTransformAction] No transform parameters found for: {actionData.targetObjectName}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Transform'u direkt uygula
    /// </summary>
    private void ApplyTransform(GameObject modelInstance, Vector3 position, Vector3 rotation, Vector3 scale,
        bool useLocal, string transformType)
    {
        switch (transformType.ToLower())
        {
            case "position":
                if (useLocal)
                    modelInstance.transform.localPosition = position;
                else
                    modelInstance.transform.position = position;
                break;

            case "rotation":
                modelInstance.transform.eulerAngles = rotation;
                break;

            case "scale":
                modelInstance.transform.localScale = scale;
                break;

            default: // "all"
                if (useLocal)
                    modelInstance.transform.localPosition = position;
                else
                    modelInstance.transform.position = position;
                modelInstance.transform.eulerAngles = rotation;
                modelInstance.transform.localScale = scale;
                break;
        }
    }

    /// <summary>
    /// Transform animation coroutine
    /// </summary>
    private IEnumerator AnimateTransform(GameObject modelInstance,
        Vector3 fromPos, Vector3 toPos,
        Vector3 fromRot, Vector3 toRot,
        Vector3 fromScale, Vector3 toScale,
        float duration, string easingType, bool useLocal, string transformType)
    {
        float elapsed = 0f;

        while (elapsed < duration && modelInstance != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Easing uygula
            float easedT = ApplyEasing(t, easingType);

            // Transform interpolate et
            Vector3 currentPos = Vector3.Lerp(fromPos, toPos, easedT);
            Vector3 currentRot = Vector3.Lerp(fromRot, toRot, easedT);
            Vector3 currentScale = Vector3.Lerp(fromScale, toScale, easedT);

            // Transform'u uygula
            ApplyTransform(modelInstance, currentPos, currentRot, currentScale, useLocal, transformType);

            yield return null;
        }

        // Final values'ları garantile
        if (modelInstance != null)
        {
            ApplyTransform(modelInstance, toPos, toRot, toScale, useLocal, transformType);
        }
    }

    /// <summary>
    /// Easing function'ları
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