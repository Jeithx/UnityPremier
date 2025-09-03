//using UnityEngine;
//using UnityEngine.Events;
//using System;

//[System.Serializable]
//public class ActionBasedTimelineEvent
//{
//    [Header("Basic Properties")]
//    public float time;
//    public string eventName;
//    public bool triggered;

//    [Header("Action Type")]
//    public EventActionType actionType;

//    [Header("Action Parameters")]
//    public EventActionData actionData;

//    // UnityEvent for inspector-assignable actions
//    [Header("Custom Actions")]
//    public UnityEvent onTrigger;

//    public ActionBasedTimelineEvent(float time, string eventName, EventActionType actionType = EventActionType.Custom)
//    {
//        this.time = time;
//        this.eventName = eventName;
//        this.actionType = actionType;
//        this.triggered = false;
//        this.actionData = new EventActionData();
//        this.onTrigger = new UnityEvent();
//    }

//    public void Reset()
//    {
//        triggered = false;
//    }

//    public bool ShouldTrigger(float currentTime)
//    {
//        return !triggered && currentTime >= time;
//    }

//    public void TriggerEvent()
//    {
//        if (triggered) return;

//        triggered = true;
//        Debug.Log($"Event Triggered: '{eventName}' at {time:F2}s - Action: {actionType}");

//        // Execute the specific action based on type
//        ExecuteAction();

//        // Also trigger any custom UnityEvent actions
//        onTrigger?.Invoke();
//    }

//    private void ExecuteAction()
//    {
//        switch (actionType)
//        {
//            case EventActionType.Custom:
//                // Only UnityEvent actions
//                break;

//            case EventActionType.CameraEffect:
//                ExecuteCameraEffect();
//                break;

//            case EventActionType.ModelAnimation:
//                ExecuteModelAnimation();
//                break;

//            case EventActionType.AudioControl:
//                ExecuteAudioControl();
//                break;

//            case EventActionType.UIControl:
//                ExecuteUIControl();
//                break;

//            case EventActionType.SceneEffect:
//                ExecuteSceneEffect();
//                break;

//            case EventActionType.TimelineControl:
//                ExecuteTimelineControl();
//                break;
//        }
//    }

//    private void ExecuteCameraEffect()
//    {
//        var cameraController = Camera.main?.GetComponent<CameraEventController>();
//        if (cameraController == null)
//        {
//            Debug.LogWarning("No CameraEventController found on main camera for camera effect event");
//            return;
//        }

//        switch (actionData.cameraAction)
//        {
//            case CameraAction.Shake:
//                cameraController.Shake(actionData.intensity, actionData.duration);
//                break;
//            case CameraAction.Zoom:
//                cameraController.ZoomTo(actionData.targetValue, actionData.duration);
//                break;
//            case CameraAction.Move:
//                cameraController.MoveTo(actionData.targetPosition, actionData.duration);
//                break;
//        }
//    }

//    private void ExecuteModelAnimation()
//    {
//        if (string.IsNullOrEmpty(actionData.targetObjectName))
//        {
//            Debug.LogWarning("Model animation event: target object name is empty");
//            return;
//        }

//        GameObject targetModel = GameObject.Find(actionData.targetObjectName);
//        if (targetModel == null)
//        {
//            Debug.LogWarning($"Model animation event: target object '{actionData.targetObjectName}' not found");
//            return;
//        }

//        Animator animator = targetModel.GetComponent<Animator>();
//        if (animator != null && !string.IsNullOrEmpty(actionData.animationName))
//        {
//            animator.Play(actionData.animationName);
//            Debug.Log($"Playing animation '{actionData.animationName}' on '{actionData.targetObjectName}'");
//        }
//    }

//    private void ExecuteAudioControl()
//    {
//        switch (actionData.audioAction)
//        {
//            case AudioAction.PlaySound:
//                if (!string.IsNullOrEmpty(actionData.audioClipPath))
//                {
//                    AudioSource.PlayClipAtPoint(
//                        Resources.Load<AudioClip>(actionData.audioClipPath),
//                        Camera.main?.transform.position ?? Vector3.zero,
//                        actionData.volume
//                    );
//                }
//                break;

//            case AudioAction.SetMasterVolume:
//                AudioListener.volume = actionData.volume;
//                break;
//        }
//    }

//    private void ExecuteUIControl()
//    {
//        if (string.IsNullOrEmpty(actionData.targetObjectName)) return;

//        GameObject targetUI = GameObject.Find(actionData.targetObjectName);
//        if (targetUI == null) return;

//        switch (actionData.uiAction)
//        {
//            case UIAction.ShowHide:
//                targetUI.SetActive(actionData.boolValue);
//                break;

//            case UIAction.SetText:
//                var textComp = targetUI.GetComponent<TMPro.TextMeshProUGUI>();
//                if (textComp != null)
//                {
//                    textComp.text = actionData.stringValue;
//                }
//                break;
//        }
//    }

//    private void ExecuteSceneEffect()
//    {
//        switch (actionData.sceneAction)
//        {
//            case SceneAction.ChangeBackground:
//                Camera.main.backgroundColor = actionData.colorValue;
//                break;

//            case SceneAction.SetLighting:
//                RenderSettings.ambientIntensity = actionData.intensity;
//                break;
//        }
//    }

//    private void ExecuteTimelineControl()
//    {
//        var timelineGrid = UnityEngine.Object.FindObjectOfType<TimelineGrid>();
//        if (timelineGrid == null) return;

//        switch (actionData.timelineAction)
//        {
//            case TimelineAction.Pause:
//                timelineGrid.stopVideo();
//                break;

//            case TimelineAction.JumpToTime:
//                timelineGrid.SetTime(actionData.targetValue);
//                break;

//            case TimelineAction.ChangeSpeed:
//                Time.timeScale = actionData.targetValue;
//                break;
//        }
//    }
//}

//// Event action types
//public enum EventActionType
//{
//    Custom,           // Only UnityEvent actions
//    CameraEffect,     // Camera shake, zoom, movement
//    ModelAnimation,   // Change model animations
//    AudioControl,     // Audio effects, volume changes
//    UIControl,        // Show/hide UI, change text
//    SceneEffect,      // Lighting, background changes
//    TimelineControl   // Pause, jump, speed changes
//}

//// Specific action enums
//public enum CameraAction { Shake, Zoom, Move }
//public enum AudioAction { PlaySound, SetMasterVolume }
//public enum UIAction { ShowHide, SetText }
//public enum SceneAction { ChangeBackground, SetLighting }
//public enum TimelineAction { Pause, JumpToTime, ChangeSpeed }

//[System.Serializable]
//public class EventActionData
//{
//    [Header("Target")]
//    public string targetObjectName;

//    [Header("Common Parameters")]
//    public float duration = 1f;
//    public float intensity = 1f;
//    public float targetValue = 1f;
//    public float volume = 1f;

//    [Header("Specific Parameters")]
//    public Vector3 targetPosition;
//    public Color colorValue = Color.white;
//    public string animationName;
//    public string audioClipPath;
//    public string stringValue;
//    public bool boolValue = true;

//    [Header("Action Specifics")]
//    public CameraAction cameraAction;
//    public AudioAction audioAction;
//    public UIAction uiAction;
//    public SceneAction sceneAction;
//    public TimelineAction timelineAction;
//}