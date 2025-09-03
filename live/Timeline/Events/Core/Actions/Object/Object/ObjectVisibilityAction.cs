using UnityEngine;

/// <summary>
/// GameObject'in visibility'sini (SetActive) kontrol eden action
/// </summary>
public class ObjectVisibilityAction : BaseEventAction
{
    public override string ActionType => "ObjectVisibility";

    private struct VisibilityState
    {
        public GameObject target;
        public bool previousState;
    }

    private static System.Collections.Generic.Dictionary<string, VisibilityState> previousStates =
        new System.Collections.Generic.Dictionary<string, VisibilityState>();

    public override void Execute(EventActionData actionData)
    {
        GameObject target = FindTargetObject(actionData);
        if (target == null) return;

        bool visible = actionData.GetParameter<bool>("visible", true);

        string key = actionData.actionId;
        previousStates[key] = new VisibilityState
        {
            target = target,
            previousState = target.activeInHierarchy
        };

        //set the visibility alpha to 0 for target

        SetVisibility(target, visible);


        LogExecution(actionData, $"Set visibility to {visible}");
    
    }

    public void SetVisibility(GameObject target, bool visible)
    {
        foreach (var r in target.GetComponentsInChildren<Renderer>())
        {
            r.enabled = visible;
        }
    }

    public override void Undo(EventActionData actionData)
    {
        string key = actionData.actionId;

        if (previousStates.TryGetValue(key, out VisibilityState state))
        {
            if (state.target != null)
            {
                SetVisibility(state.target, state.previousState);
                LogExecution(actionData, $"Restored visibility to {state.previousState}");
            }

            // State'i temizle
            previousStates.Remove(key);
        }
    }

    public override bool IsValid(EventActionData actionData)
    {
        if (!base.IsValid(actionData)) return false;

        bool hasVisibleParam = actionData.parameters.Exists(p => p.name == "visible");
        if (!hasVisibleParam)
        {
            Debug.LogWarning($"[ObjectVisibilityAction] Missing 'visible' parameter for target: {actionData.targetObjectName}");
            return false;
        }

        return true;
    }
}