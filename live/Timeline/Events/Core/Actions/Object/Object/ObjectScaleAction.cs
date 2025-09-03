using UnityEngine;
using System.Collections;

/// <summary>
/// GameObject'in scale'ini değiştiren action (smooth transition ile)
/// </summary>
public class ObjectScaleAction : BaseEventAction
{
    public override string ActionType => "ObjectScale";
    public override bool RequiresContinuousUpdate => true; // Animation için gerekli

    private struct ScaleState
    {
        public GameObject target;
        public Vector3 previousScale;
        public Coroutine activeCoroutine;
    }

    // Undo için önceki state'leri sakla
    private static System.Collections.Generic.Dictionary<string, ScaleState> previousStates =
        new System.Collections.Generic.Dictionary<string, ScaleState>();

    public override void Execute(EventActionData actionData)
    {
        GameObject target = FindTargetObject(actionData);
        if (target == null) return;

        Vector3 targetScale = actionData.GetParameter<Vector3>("scale", Vector3.one);
        float duration = actionData.GetParameter<float>("duration", 1f);
        string easingType = actionData.GetParameter<string>("easing", "linear");

        // Undo için önceki state'i sakla
        string key = actionData.actionId;
        previousStates[key] = new ScaleState
        {
            target = target,
            previousScale = target.transform.localScale,
            activeCoroutine = null
        };

        // Scale animation'ı başlat
        if (duration > 0.01f)
        {
            // Smooth transition
            var state = previousStates[key];
            state.activeCoroutine = target.GetComponent<MonoBehaviour>()?.StartCoroutine(
                AnimateScale(target, target.transform.localScale, targetScale, duration, easingType));
            previousStates[key] = state;
        }
        else
        {
            // Instant change
            target.transform.localScale = targetScale;
        }

        LogExecution(actionData, $"Scale to {targetScale} over {duration:F2}s");
    }

    public override void Undo(EventActionData actionData)
    {
        string key = actionData.actionId;

        if (previousStates.TryGetValue(key, out ScaleState state))
        {
            if (state.target != null)
            {
                // Aktif coroutine'i durdur
                if (state.activeCoroutine != null)
                {
                    state.target.GetComponent<MonoBehaviour>()?.StopCoroutine(state.activeCoroutine);
                }

                // Önceki scale'e geri dön
                state.target.transform.localScale = state.previousScale;
                LogExecution(actionData, $"Restored scale to {state.previousScale}");
            }

            // State'i temizle
            previousStates.Remove(key);
        }
    }

    public override bool IsValid(EventActionData actionData)
    {
        if (!base.IsValid(actionData)) return false;

        // scale parametresi mevcut mu kontrol et
        bool hasScaleParam = actionData.parameters.Exists(p => p.name == "scale");
        if (!hasScaleParam)
        {
            Debug.LogWarning($"[ObjectScaleAction] Missing 'scale' parameter for target: {actionData.targetObjectName}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Scale animation coroutine'i
    /// </summary>
    private IEnumerator AnimateScale(GameObject target, Vector3 fromScale, Vector3 toScale, float duration, string easingType)
    {
        float elapsed = 0f;

        while (elapsed < duration && target != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Easing function uygula
            float easedT = ApplyEasing(t, easingType);

            // Scale'i interpolate et
            Vector3 currentScale = Vector3.Lerp(fromScale, toScale, easedT);
            target.transform.localScale = currentScale;

            yield return null;
        }

        // Final scale'i garantile
        if (target != null)
        {
            target.transform.localScale = toScale;
        }
    }

    /// <summary>
    /// Easing function'ları uygula
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