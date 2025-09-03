using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Timeline event'lerini yöneten controller
/// VideoController, ModelController pattern'ini takip eder
/// </summary>
public class EventController : MonoBehaviour
{
    private TimelineEvent timelineEvent;
    private Timer timer;
    private TimelineGrid timeline;

    [SerializeField] private float epsilon = 0.05f;
    private bool triggered;
    private List<IEventAction> activeActions = new List<IEventAction>();

    /// <summary>
    /// EventController oluştur (Factory pattern)
    /// </summary>
    public static EventController Create(
        GameObject host,
        Timer time,
        TimelineEvent eventData,
        TimelineGrid tlg)
    {
        var ctrl = host.AddComponent<EventController>();
        ctrl.Init(time, eventData, tlg);
        return ctrl;
    }

    public void Init(Timer time, TimelineEvent eventData, TimelineGrid tlg)
    {
        timer = time;
        timelineEvent = eventData;
        timeline = tlg;
        triggered = false;

        InitializeActions();
    }

    /// <summary>
    /// Event action'larını başlat
    /// </summary>
    private void InitializeActions()
    {
        Debug.Log($"[EventController] *** InitializeActions called for event '{timelineEvent.eventName}' ***");

        activeActions.Clear();

        if (timelineEvent?.actions == null)
        {
            Debug.LogWarning($"[EventController] timelineEvent.actions is null!");
            return;
        }

        Debug.Log($"[EventController] Event has {timelineEvent.actions.Count} actions");

        foreach (var actionData in timelineEvent.actions)
        {
            Debug.Log($"[EventController] Processing action type: '{actionData.actionType}'");

            var action = CreateActionFromData(actionData);
            Debug.Log($"[EventController] CreateActionFromData result: {(action != null ? action.GetType().Name : "NULL")}");

            if (action != null && action.IsValid(actionData))
            {
                activeActions.Add(action);
                Debug.Log($"[EventController] Action added successfully: {action.ActionType}");
            }
            else
            {
                Debug.LogWarning($"[EventController] Action creation failed or invalid for type: {actionData.actionType}");
            }
        }

        Debug.Log($"[EventController] Initialized {activeActions.Count} actions for event '{timelineEvent.eventName}'");
    }

    /// <summary>
    /// ActionData'dan IEventAction instance'ı oluştur
    /// </summary>
    private IEventAction CreateActionFromData(EventActionData actionData)
    {
        switch (actionData.actionType)
        {
            case "Debug":
                return new DebugAction();

            // other actions will be added here
            case "ObjectVisibility":
                return new ObjectVisibilityAction();

            case "ObjectScale":
                return new ObjectScaleAction();

            case "ObjectPosition":
                return new ObjectPositionAction();

            case "WindowPosition":
                return new WindowPositionAction();

            case "ModelTransform":
                return new ModelTransformAction();

            default:
                Debug.LogWarning($"[EventController] Unknown action type: {actionData.actionType}");
                return null;
        }
    }

    public TimelineEvent GetTimelineEvent() => timelineEvent;

    private void Update()
    {
        if (!timer || !timeline)
        {
            Debug.LogWarning($"[EventController] Missing references: timer={timer != null}, timeline={timeline != null}");
            return;
        }

        if (!timeline.getFlag())
        {
            // Debug.Log($"[EventController] Timeline not playing (flag: {timeline.getFlag()})");
            return; // Timeline playing değilse çalışma
        }

        float t = timer.getCurrentTime();
        float eventTime = timelineEvent.time;

        // Event tetiklenmesi gereken zamana geldi mi?
        if (!triggered && t >= eventTime - epsilon && t <= eventTime + epsilon + 1f)
        {
            Debug.Log($"[EventController] *** TRIGGERING EVENT '{timelineEvent.eventName}' at {t:F2}s ***");
            triggered = true;
            ExecuteEvent();
        }
    }

    /// <summary>
    /// Event'i çalıştır (tüm action'ları tetikle)
    /// </summary>
    private void ExecuteEvent()
    {
        Debug.Log($"[EventController] Executing event '{timelineEvent.eventName}' at time {timelineEvent.time:F2}s");

        if (timelineEvent.actions == null || timelineEvent.actions.Count == 0)
        {
            Debug.Log($"[EventController] No actions to execute for event '{timelineEvent.eventName}'");
            return;
        }

        // Tüm action'ları çalıştır
        for (int i = 0; i < activeActions.Count && i < timelineEvent.actions.Count; i++)
        {
            var action = activeActions[i];
            var actionData = timelineEvent.actions[i];

            try
            {
                if (actionData.delay > 0)
                {
                    // Delay varsa coroutine ile çalıştır
                    StartCoroutine(ExecuteActionWithDelay(action, actionData, actionData.delay));
                }
                else
                {
                    // Delay yoksa direkt çalıştır
                    action.Execute(actionData);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EventController] Error executing action {actionData.actionType}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Action'ı delay ile çalıştır
    /// </summary>
    private IEnumerator ExecuteActionWithDelay(IEventAction action, EventActionData actionData, float delay)
    {
        yield return new WaitForSeconds(delay);

        try
        {
            action.Execute(actionData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[EventController] Error executing delayed action {actionData.actionType}: {e.Message}");
        }
    }

    /// <summary>
    /// Scrubbing desteği (VideoController'dan kopyalandı)
    /// </summary>
    public void ScrubTo(float globalTime, bool shouldPlay)
    {
        float eventTime = timelineEvent.time;

        if (globalTime < eventTime - epsilon)
        {
            // Event zamanından önceye gittik, reset et
            if (triggered)
            {
                triggered = false;
                UndoEvent();
            }
        }
        else if (globalTime >= eventTime - epsilon && globalTime <= eventTime + epsilon + 1f)
        {
            // Event zamanına geldik, eğer henüz tetiklenmemişse tetikle
            if (!triggered)
            {
                triggered = true;
                ExecuteEvent();
            }
        }
    }

    /// <summary>
    /// Event'i geri al (scrubbing için)
    /// </summary>
    private void UndoEvent()
    {
        Debug.Log($"[EventController] Undoing event '{timelineEvent.eventName}'");

        for (int i = 0; i < activeActions.Count && i < timelineEvent.actions.Count; i++)
        {
            var action = activeActions[i];
            var actionData = timelineEvent.actions[i];

            try
            {
                action.Undo(actionData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[EventController] Error undoing action {actionData.actionType}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// State'i reset et
    /// </summary>
    public void ResetState()
    {
        triggered = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Debug için geçici action sınıfı
    /// </summary>
    private class DebugAction : IEventAction
    {
        public string ActionType => "Debug";
        public bool RequiresContinuousUpdate => false;

        public void Execute(EventActionData actionData)
        {
            string message = actionData.GetParameter<string>("message", "Event triggered!");
            Debug.Log($"[DebugAction] {message}");
        }

        public void Undo(EventActionData actionData)
        {
            Debug.Log($"[DebugAction] Undo - Event undone");
        }

        public bool IsValid(EventActionData actionData)
        {
            return true;
        }
    }
}