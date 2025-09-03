using UnityEngine;
using System.Collections;

/// <summary>
/// GameObject'in pozisyonunu değiştiren action (smooth transition ile)
/// </summary>
public class ObjectPositionAction : BaseEventAction
{
    public override string ActionType => "ObjectPosition";
    public override bool RequiresContinuousUpdate => true; // Animation için gerekli

    private struct PositionState
    {
        public GameObject target;
        public Vector3 previousPosition;
        public Coroutine activeCoroutine;
        public bool wasLocal; // Local pozisyon mu, world pozisyon mu
    }

    // Undo için önceki state'leri sakla
    private static System.Collections.Generic.Dictionary<string, PositionState> previousStates =
        new System.Collections.Generic.Dictionary<string, PositionState>();

    public override void Execute(EventActionData actionData)
    {
        GameObject target = FindTargetObject(actionData);
        if (target == null) return;

        Vector3 targetPosition = actionData.GetParameter<Vector3>("position", Vector3.zero);
        float duration = actionData.GetParameter<float>("duration", 1f);
        string easingType = actionData.GetParameter<string>("easing", "linear");
        bool useLocalPosition = actionData.GetParameter<bool>("local", true);
        string movementType = actionData.GetParameter<string>("movementType", "absolute"); // absolute, relative, offset

        // Current position'ı al
        Vector3 currentPos = useLocalPosition ? target.transform.localPosition : target.transform.position;

        // Target position'ı hesapla
        Vector3 finalTargetPos = targetPosition;
        if (movementType == "relative" || movementType == "offset")
        {
            finalTargetPos = currentPos + targetPosition;
        }

        // Undo için önceki state'i sakla
        string key = actionData.actionId;
        previousStates[key] = new PositionState
        {
            target = target,
            previousPosition = currentPos,
            activeCoroutine = null,
            wasLocal = useLocalPosition
        };

        // Position animation'ı başlat
        if (duration > 0.01f)
        {
            // Smooth transition
            var state = previousStates[key];
            state.activeCoroutine = target.GetComponent<MonoBehaviour>()?.StartCoroutine(
                AnimatePosition(target, currentPos, finalTargetPos, duration, easingType, useLocalPosition));
            previousStates[key] = state;
        }
        else
        {
            // Instant change
            if (useLocalPosition)
                target.transform.localPosition = finalTargetPos;
            else
                target.transform.position = finalTargetPos;
        }

        LogExecution(actionData, $"Move to {finalTargetPos} ({(useLocalPosition ? "local" : "world")}) over {duration:F2}s");
    }

    public override void Undo(EventActionData actionData)
    {
        string key = actionData.actionId;

        if (previousStates.TryGetValue(key, out PositionState state))
        {
            if (state.target != null)
            {
                // Aktif coroutine'i durdur
                if (state.activeCoroutine != null)
                {
                    state.target.GetComponent<MonoBehaviour>()?.StopCoroutine(state.activeCoroutine);
                }

                // Önceki position'a geri dön
                if (state.wasLocal)
                    state.target.transform.localPosition = state.previousPosition;
                else
                    state.target.transform.position = state.previousPosition;

                LogExecution(actionData, $"Restored position to {state.previousPosition}");
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
            Debug.LogWarning($"[ObjectPositionAction] Missing 'position' parameter for target: {actionData.targetObjectName}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Position animation coroutine'i
    /// </summary>
    private IEnumerator AnimatePosition(GameObject target, Vector3 fromPosition, Vector3 toPosition,
        float duration, string easingType, bool useLocal)
    {
        float elapsed = 0f;

        while (elapsed < duration && target != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Easing function uygula (ObjectScaleAction'dan kopyalandı)
            float easedT = ApplyEasing(t, easingType);

            // Position'ı interpolate et
            Vector3 currentPosition = Vector3.Lerp(fromPosition, toPosition, easedT);

            if (useLocal)
                target.transform.localPosition = currentPosition;
            else
                target.transform.position = currentPosition;

            yield return null;
        }

        // Final position'ı garantile
        if (target != null)
        {
            if (useLocal)
                target.transform.localPosition = toPosition;
            else
                target.transform.position = toPosition;
        }
    }

    /// <summary>
    /// Easing function'ları uygula (ObjectScaleAction'dan kopyalandı)
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

            case "elastic":
                if (t == 0) return 0;
                if (t == 1) return 1;
                float p = 0.3f;
                return -(Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p));

            default: // linear
                return t;
        }
    }
}