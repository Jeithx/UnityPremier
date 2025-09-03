//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections.Generic;

//public class TimelineEventEditor : MonoBehaviour
//{
//    [Header("UI References")]
//    public GameObject editorPanel;
//    public TMP_InputField eventNameInput;
//    public TMP_InputField eventTimeInput;
//    public TMP_Dropdown actionTypeDropdown;

//    [Header("Action Parameter Panels")]
//    public GameObject cameraActionPanel;
//    public GameObject modelActionPanel;
//    public GameObject audioActionPanel;
//    public GameObject uiActionPanel;
//    public GameObject sceneActionPanel;
//    public GameObject timelineActionPanel;

//    [Header("Camera Action Controls")]
//    public TMP_Dropdown cameraActionDropdown;
//    public TMP_InputField intensityInput;
//    public TMP_InputField durationInput;
//    public TMP_InputField fovInput;
//    public TMP_InputField posXInput;
//    public TMP_InputField posYInput;
//    public TMP_InputField posZInput;

//    [Header("Model Action Controls")]
//    public TMP_InputField targetModelInput;
//    public TMP_InputField animationNameInput;

//    [Header("Audio Action Controls")]
//    public TMP_Dropdown audioActionDropdown;
//    public TMP_InputField volumeInput;
//    public TMP_InputField audioPathInput;

//    [Header("UI Action Controls")]
//    public TMP_InputField targetUIInput;
//    public TMP_Dropdown uiActionDropdown;
//    public TMP_InputField uiTextInput;
//    public Toggle uiToggle;

//    [Header("Scene Action Controls")]
//    public TMP_Dropdown sceneActionDropdown;
//    public Image colorPicker;
//    public TMP_InputField lightIntensityInput;

//    [Header("Timeline Action Controls")]
//    public TMP_Dropdown timelineActionDropdown;
//    public TMP_InputField jumpTimeInput;
//    public TMP_InputField speedInput;

//    [Header("Buttons")]
//    public Button saveButton;
//    public Button cancelButton;
//    public Button deleteButton;

//    private ActionBasedTimelineEvent currentEvent;
//    private System.Action<ActionBasedTimelineEvent> onEventSaved;
//    private System.Action<ActionBasedTimelineEvent> onEventDeleted;

//    void Start()
//    {
//        SetupDropdowns();
//        SetupButtons();
//        HideAllActionPanels();
//        editorPanel.SetActive(false);
//    }

//    private void SetupDropdowns()
//    {
//        // Action Type Dropdown
//        actionTypeDropdown.ClearOptions();
//        var actionTypes = new List<string>
//        {
//            "Custom", "Camera Effect", "Model Animation",
//            "Audio Control", "UI Control", "Scene Effect", "Timeline Control"
//        };
//        actionTypeDropdown.AddOptions(actionTypes);
//        actionTypeDropdown.onValueChanged.AddListener(OnActionTypeChanged);

//        // Camera Action Dropdown
//        cameraActionDropdown.ClearOptions();
//        cameraActionDropdown.AddOptions(new List<string> { "Shake", "Zoom", "Move" });

//        // Audio Action Dropdown
//        audioActionDropdown.ClearOptions();
//        audioActionDropdown.AddOptions(new List<string> { "Play Sound", "Set Master Volume" });

//        // UI Action Dropdown
//        uiActionDropdown.ClearOptions();
//        uiActionDropdown.AddOptions(new List<string> { "Show/Hide", "Set Text" });

//        // Scene Action Dropdown
//        sceneActionDropdown.ClearOptions();
//        sceneActionDropdown.AddOptions(new List<string> { "Change Background", "Set Lighting" });

//        // Timeline Action Dropdown
//        timelineActionDropdown.ClearOptions();
//        timelineActionDropdown.AddOptions(new List<string> { "Pause", "Jump To Time", "Change Speed" });
//    }

//    private void SetupButtons()
//    {
//        saveButton.onClick.AddListener(SaveEvent);
//        cancelButton.onClick.AddListener(CancelEdit);
//        deleteButton.onClick.AddListener(DeleteEvent);
//    }

//    public void EditEvent(ActionBasedTimelineEvent eventToEdit, System.Action<ActionBasedTimelineEvent> saveCallback, System.Action<ActionBasedTimelineEvent> deleteCallback = null)
//    {
//        currentEvent = eventToEdit;
//        onEventSaved = saveCallback;
//        onEventDeleted = deleteCallback;

//        LoadEventData();
//        editorPanel.SetActive(true);
//    }

//    private void LoadEventData()
//    {
//        if (currentEvent == null) return;

//        eventNameInput.text = currentEvent.eventName;
//        eventTimeInput.text = currentEvent.time.ToString("F2");
//        actionTypeDropdown.value = (int)currentEvent.actionType;

//        OnActionTypeChanged((int)currentEvent.actionType);
//        LoadActionData();
//    }

//    private void LoadActionData()
//    {
//        var data = currentEvent.actionData;

//        // Camera
//        cameraActionDropdown.value = (int)data.cameraAction;
//        intensityInput.text = data.intensity.ToString("F2");
//        durationInput.text = data.duration.ToString("F2");
//        fovInput.text = data.targetValue.ToString("F2");
//        posXInput.text = data.targetPosition.x.ToString("F2");
//        posYInput.text = data.targetPosition.y.ToString("F2");
//        posZInput.text = data.targetPosition.z.ToString("F2");

//        // Model
//        targetModelInput.text = data.targetObjectName;
//        animationNameInput.text = data.animationName;

//        // Audio
//        audioActionDropdown.value = (int)data.audioAction;
//        volumeInput.text = data.volume.ToString("F2");
//        audioPathInput.text = data.audioClipPath;

//        // UI
//        targetUIInput.text = data.targetObjectName;
//        uiActionDropdown.value = (int)data.uiAction;
//        uiTextInput.text = data.stringValue;
//        uiToggle.isOn = data.boolValue;

//        // Scene
//        sceneActionDropdown.value = (int)data.sceneAction;
//        colorPicker.color = data.colorValue;
//        lightIntensityInput.text = data.intensity.ToString("F2");

//        // Timeline
//        timelineActionDropdown.value = (int)data.timelineAction;
//        jumpTimeInput.text = data.targetValue.ToString("F2");
//        speedInput.text = data.targetValue.ToString("F2");
//    }

//    private void OnActionTypeChanged(int typeIndex)
//    {
//        HideAllActionPanels();

//        switch ((EventActionType)typeIndex)
//        {
//            case EventActionType.CameraEffect:
//                cameraActionPanel.SetActive(true);
//                break;
//            case EventActionType.ModelAnimation:
//                modelActionPanel.SetActive(true);
//                break;
//            case EventActionType.AudioControl:
//                audioActionPanel.SetActive(true);
//                break;
//            case EventActionType.UIControl:
//                uiActionPanel.SetActive(true);
//                break;
//            case EventActionType.SceneEffect:
//                sceneActionPanel.SetActive(true);
//                break;
//            case EventActionType.TimelineControl:
//                timelineActionPanel.SetActive(true);
//                break;
//        }
//    }

//    private void HideAllActionPanels()
//    {
//        cameraActionPanel.SetActive(false);
//        modelActionPanel.SetActive(false);
//        audioActionPanel.SetActive(false);
//        uiActionPanel.SetActive(false);
//        sceneActionPanel.SetActive(false);
//        timelineActionPanel.SetActive(false);
//    }

//    private void SaveEvent()
//    {
//        if (currentEvent == null) return;

//        // Basic properties
//        currentEvent.eventName = eventNameInput.text;
//        if (float.TryParse(eventTimeInput.text, out float time))
//        {
//            currentEvent.time = Mathf.Max(0f, time);
//        }

//        currentEvent.actionType = (EventActionType)actionTypeDropdown.value;

//        // Action data
//        SaveActionData();

//        onEventSaved?.Invoke(currentEvent);
//        CloseEditor();
//    }

//    private void SaveActionData()
//    {
//        var data = currentEvent.actionData;

//        switch (currentEvent.actionType)
//        {
//            case EventActionType.CameraEffect:
//                data.cameraAction = (CameraAction)cameraActionDropdown.value;
//                float.TryParse(intensityInput.text, out data.intensity);
//                float.TryParse(durationInput.text, out data.duration);
//                float.TryParse(fovInput.text, out data.targetValue);

//                if (float.TryParse(posXInput.text, out float x) &&
//                    float.TryParse(posYInput.text, out float y) &&
//                    float.TryParse(posZInput.text, out float z))
//                {
//                    data.targetPosition = new Vector3(x, y, z);
//                }
//                break;

//            case EventActionType.ModelAnimation:
//                data.targetObjectName = targetModelInput.text;
//                data.animationName = animationNameInput.text;
//                break;

//            case EventActionType.AudioControl:
//                data.audioAction = (AudioAction)audioActionDropdown.value;
//                float.TryParse(volumeInput.text, out data.volume);
//                data.audioClipPath = audioPathInput.text;
//                break;

//            case EventActionType.UIControl:
//                data.targetObjectName = targetUIInput.text;
//                data.uiAction = (UIAction)uiActionDropdown.value;
//                data.stringValue = uiTextInput.text;
//                data.boolValue = uiToggle.isOn;
//                break;

//            case EventActionType.SceneEffect:
//                data.sceneAction = (SceneAction)sceneActionDropdown.value;
//                data.colorValue = colorPicker.color;
//                float.TryParse(lightIntensityInput.text, out data.intensity);
//                break;

//            case EventActionType.TimelineControl:
//                data.timelineAction = (TimelineAction)timelineActionDropdown.value;

//                if (data.timelineAction == TimelineAction.JumpToTime)
//                {
//                    float.TryParse(jumpTimeInput.text, out data.targetValue);
//                }
//                else if (data.timelineAction == TimelineAction.ChangeSpeed)
//                {
//                    float.TryParse(speedInput.text, out data.targetValue);
//                }
//                break;
//        }
//    }

//    private void CancelEdit()
//    {
//        CloseEditor();
//    }

//    private void DeleteEvent()
//    {
//        if (currentEvent != null)
//        {
//            onEventDeleted?.Invoke(currentEvent);
//        }
//        CloseEditor();
//    }

//    private void CloseEditor()
//    {
//        editorPanel.SetActive(false);
//        currentEvent = null;
//        onEventSaved = null;
//        onEventDeleted = null;
//    }

//    // Helper method to create a new event
//    public static ActionBasedTimelineEvent CreateNewEvent(float time)
//    {
//        return new ActionBasedTimelineEvent(time, $"Event @ {time:F2}s", EventActionType.Custom);
//    }
//}