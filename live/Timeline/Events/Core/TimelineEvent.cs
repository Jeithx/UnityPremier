using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class TimelineEvent
{

    public List<EventActionData> actions = new List<EventActionData>();
    public TimelineEventTriggerType triggerType = TimelineEventTriggerType.OnTime;
    public float time; public bool isRepeatable = true;


    public string eventName;
    public bool triggered;

    public TimelineEvent(float time, string eventName)
    {
        this.time = time;
        this.eventName = eventName;
        this.triggered = false;
        this.actions = new List<EventActionData>();
        this.triggerType = TimelineEventTriggerType.OnTime;
        this.isRepeatable = true;
    }
    public void Reset()
    {
        triggered = false;
    }

    /// <summary>
    /// Event'e yeni action ekle
    /// </summary>
    public void AddAction(EventActionData actionData)
    {
        if (actionData != null && !actions.Contains(actionData))
        {
            actions.Add(actionData);
        }
    }

    /// <summary>
    /// Event'ten action kaldır
    /// </summary>
    public void RemoveAction(EventActionData actionData)
    {
        actions.Remove(actionData);
    }

    /// <summary>
    /// Event'te action var mı kontrol et
    /// </summary>
    public bool HasActions()
    {
        return actions != null && actions.Count > 0;
    }
}

/// <summary>
/// Şimdilik hepsi OnTime olacak fakat ileride kullanılmak istenirse eklenebilir
/// </summary>

[System.Serializable]
public enum TimelineEventTriggerType
{
    OnTime,     
    OnClick,
    OnHover
}