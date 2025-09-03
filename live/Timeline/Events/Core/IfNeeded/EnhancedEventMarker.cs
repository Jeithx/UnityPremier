//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using TMPro;
//using System.Collections.Generic;

//public class EnhancedEventMarker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
//{
//    [Header("Visual References")]
//    public Image markerIcon;
//    public TextMeshProUGUI eventNameText;
//    public GameObject tooltipPanel;
//    public TextMeshProUGUI tooltipText;

//    [Header("Visual Settings")]
//    public Color defaultColor = Color.yellow;
//    public Color hoverColor = Color.white;
//    public Color triggeredColor = Color.green;
//    public Color selectedColor = Color.cyan;

//    [Header("Selection")]
//    public GameObject selectionHighlight;

//    private ActionBasedTimelineEvent timelineEvent;
//    private TimelineEventEditor eventEditor;
//    private System.Action<EnhancedEventMarker> onMarkerDeleted;
//    private bool isSelected = false;
//    private bool isHovered = false;

//    void Start()
//    {
//        if (tooltipPanel != null)
//            tooltipPanel.SetActive(false);

//        if (selectionHighlight != null)
//            selectionHighlight.SetActive(false);

//        UpdateVisuals();

//        // Make selectable for keyboard input
//        var selectable = GetComponent<Selectable>();
//        if (selectable == null)
//        {
//            selectable = gameObject.AddComponent<Button>();
//            var button = selectable as Button;
//            button.transition = Selectable.Transition.None;
//        }
//    }

//    void Update()
//    {
//        // Handle keyboard input when selected
//        if (isSelected)
//        {
//            if (Input.GetKeyDown(KeyCode.Delete))
//            {
//                DeleteMarker();
//            }
//            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
//            {
//                // Enter key opens editor
//                OpenEditor();
//            }
//            else if (Input.GetKeyDown(KeyCode.Space))
//            {
//                // Space key triggers the event manually (for testing)
//                TriggerEvent();
//            }
//        }
//    }

//    public void Initialize(ActionBasedTimelineEvent newEvent, TimelineEventEditor editor, System.Action<EnhancedEventMarker> deleteCallback = null)
//    {
//        timelineEvent = newEvent;
//        eventEditor = editor;
//        onMarkerDeleted = deleteCallback;

//        UpdateVisuals();
//        UpdateTooltip();
//    }

//    public void OnPointerClick(PointerEventData eventData)
//    {
//        if (eventData.button == PointerEventData.InputButton.Left)
//        {
//            // Single click - select
//            SelectMarker(true);

//            // Deselect other markers
//            var allMarkers = FindObjectsOfType<EnhancedEventMarker>();
//            foreach (var marker in allMarkers)
//            {
//                if (marker != this)
//                {
//                    marker.SelectMarker(false);
//                }
//            }

//            // Double-click detection for editing
//            if (eventData.clickCount == 2)
//            {
//                OpenEditor();
//            }
//        }
//        else if (eventData.button == PointerEventData.InputButton.Right)
//        {
//            // Right click - context menu or direct delete
//            ShowContextMenu(eventData.position);
//        }
//    }

//    public void OnSelect(BaseEventData eventData)
//    {
//        SelectMarker(true);
//    }

//    public void SelectMarker(bool selected)
//    {
//        isSelected = selected;

//        if (selectionHighlight != null)
//        {
//            selectionHighlight.SetActive(selected);
//        }

//        UpdateVisuals();

//        if (selected)
//        {
//            EventSystem.current.SetSelectedGameObject(gameObject);
//        }
//    }

//    private void ShowContextMenu(Vector2 screenPosition)
//    {
//        // Simple context menu - in production you might want a proper context menu UI
//        Debug.Log("Event Marker Context Menu:");
//        Debug.Log("- Left Click: Select");
//        Debug.Log("- Double Click: Edit");
//        Debug.Log("- Right Click: Delete");
//        Debug.Log("- Delete Key: Delete (when selected)");
//        Debug.Log("- Enter Key: Edit (when selected)");
//        Debug.Log("- Space Key: Trigger manually (when selected)");

//        // For immediate deletion
//        DeleteMarker();
//    }

//    private void OpenEditor()
//    {
//        if (eventEditor != null && timelineEvent != null)
//        {
//            eventEditor.EditEvent(timelineEvent, OnEventUpdated, OnEventDeleted);
//        }
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        isHovered = true;
//        UpdateVisuals();

//        if (tooltipPanel != null)
//        {
//            tooltipPanel.SetActive(true);
//            UpdateTooltip();
//        }
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        isHovered = false;
//        UpdateVisuals();

//        if (tooltipPanel != null)
//        {
//            tooltipPanel.SetActive(false);
//        }
//    }

//    private void UpdateVisuals()
//    {
//        if (timelineEvent == null || markerIcon == null) return;

//        Color targetColor;

//        if (isSelected)
//        {
//            targetColor = selectedColor;
//        }
//        else if (isHovered)
//        {
//            targetColor = hoverColor;
//        }
//        else if (timelineEvent.triggered)
//        {
//            targetColor = triggeredColor;
//        }
//        else
//        {
//            targetColor = GetActionTypeColor();
//        }

//        markerIcon.color = targetColor;

//        if (eventNameText != null)
//        {
//            eventNameText.text = timelineEvent.eventName;
//            eventNameText.color = isSelected ? selectedColor : Color.white;
//        }
//    }

//    private Color GetActionTypeColor()
//    {
//        switch (timelineEvent.actionType)
//        {
//            case EventActionType.Custom: return Color.yellow;
//            case EventActionType.CameraEffect: return Color.cyan;
//            case EventActionType.ModelAnimation: return Color.magenta;
//            case EventActionType.AudioControl: return Color.green;
//            case EventActionType.UIControl: return Color.blue;
//            case EventActionType.SceneEffect: return Color.red;
//            case EventActionType.TimelineControl: return Color.white;
//            default: return defaultColor;
//        }
//    }

//    private void UpdateTooltip()
//    {
//        if (tooltipText == null || timelineEvent == null) return;

//        string tooltip = $"<b>{timelineEvent.eventName}</b>\n";
//        tooltip += $"Time: {timelineEvent.time:F2}s\n";
//        tooltip += $"Type: {timelineEvent.actionType}\n";
//        tooltip += $"Status: {(timelineEvent.triggered ? "Triggered" : "Waiting")}\n";

//        // Add action-specific info
//        switch (timelineEvent.actionType)
//        {
//            case EventActionType.CameraEffect:
//                tooltip += $"Action: {timelineEvent.actionData.cameraAction}\n";
//                if (timelineEvent.actionData.cameraAction == CameraAction.Shake)
//                    tooltip += $"Intensity: {timelineEvent.actionData.intensity:F2}";
//                else if (timelineEvent.actionData.cameraAction == CameraAction.Zoom)
//                    tooltip += $"FOV: {timelineEvent.actionData.targetValue:F2}";
//                break;

//            case EventActionType.ModelAnimation:
//                tooltip += $"Target: {timelineEvent.actionData.targetObjectName}\n";
//                tooltip += $"Animation: {timelineEvent.actionData.animationName}";
//                break;

//            case EventActionType.AudioControl:
//                tooltip += $"Action: {timelineEvent.actionData.audioAction}\n";
//                if (timelineEvent.actionData.audioAction == AudioAction.PlaySound)
//                    tooltip += $"Audio: {System.IO.Path.GetFileName(timelineEvent.actionData.audioClipPath)}";
//                else
//                    tooltip += $"Volume: {timelineEvent.actionData.volume:F2}";
//                break;

//            case EventActionType.UIControl:
//                tooltip += $"Target: {timelineEvent.actionData.targetObjectName}\n";
//                tooltip += $"Action: {timelineEvent.actionData.uiAction}";
//                if (timelineEvent.actionData.uiAction == UIAction.SetText)
//                    tooltip += $"\nText: \"{timelineEvent.actionData.stringValue}\"";
//                break;

//            case EventActionType.SceneEffect:
//                tooltip += $"Action: {timelineEvent.actionData.sceneAction}";
//                break;

//            case EventActionType.TimelineControl:
//                tooltip += $"Action: {timelineEvent.actionData.timelineAction}";
//                if (timelineEvent.actionData.timelineAction == TimelineAction.JumpToTime)
//                    tooltip += $"\nTarget Time: {timelineEvent.actionData.targetValue:F2}s";
//                else if (timelineEvent.actionData.timelineAction == TimelineAction.ChangeSpeed)
//                    tooltip += $"\nSpeed: {timelineEvent.actionData.targetValue:F2}x";
//                break;
//        }

//        // Add keyboard shortcuts info
//        tooltip += "\n\n<size=10><color=#888888>";
//        tooltip += "• Double-click or Enter: Edit\n";
//        tooltip += "• Delete key: Remove\n";
//        tooltip += "• Right-click: Quick delete\n";
//        tooltip += "• Space: Trigger manually</color></size>";

//        tooltipText.text = tooltip;
//    }

//    public void TriggerEvent()
//    {
//        if (timelineEvent != null)
//        {
//            timelineEvent.TriggerEvent();
//            UpdateVisuals();
//            UpdateTooltip();
//        }
//    }

//    public void ResetEvent()
//    {
//        if (timelineEvent != null)
//        {
//            timelineEvent.Reset();
//            UpdateVisuals();
//            UpdateTooltip();
//        }
//    }

//    private void OnEventUpdated(ActionBasedTimelineEvent updatedEvent)
//    {
//        timelineEvent = updatedEvent;
//        UpdateVisuals();
//        UpdateTooltip();
//        UpdateMarkerPosition();
//    }

//    private void OnEventDeleted(ActionBasedTimelineEvent deletedEvent)
//    {
//        DeleteMarker();
//    }

//    private void DeleteMarker()
//    {
//        Debug.Log($"Deleting event marker: {timelineEvent?.eventName}");
//        onMarkerDeleted?.Invoke(this);
//        Destroy(gameObject);
//    }

//    private void UpdateMarkerPosition()
//    {
//        if (timelineEvent == null) return;

//        // Find timeline grid to get position
//        var timelineGrid = FindObjectOfType<TimelineGrid>();
//        if (timelineGrid != null)
//        {
//            float xPos = timelineGrid.TimeToX(timelineEvent.time);
//            GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
//        }
//    }

//    // Getters
//    public ActionBasedTimelineEvent GetEvent() => timelineEvent;
//    public float GetEventTime() => timelineEvent?.time ?? 0f;
//    public bool IsSelected() => isSelected;

//    // Static helper methods for bulk operations
//    public static void DeselectAllMarkers()
//    {
//        var allMarkers = FindObjectsOfType<EnhancedEventMarker>();
//        foreach (var marker in allMarkers)
//        {
//            marker.SelectMarker(false);
//        }
//    }

//    public static List<EnhancedEventMarker> GetSelectedMarkers()
//    {
//        var selectedMarkers = new List<EnhancedEventMarker>();
//        var allMarkers = FindObjectsOfType<EnhancedEventMarker>();

//        foreach (var marker in allMarkers)
//        {
//            if (marker.IsSelected())
//            {
//                selectedMarkers.Add(marker);
//            }
//        }

//        return selectedMarkers;
//    }

//    public static void DeleteSelectedMarkers()
//    {
//        var selectedMarkers = GetSelectedMarkers();
//        foreach (var marker in selectedMarkers)
//        {
//            marker.DeleteMarker();
//        }

//        Debug.Log($"Deleted {selectedMarkers.Count} selected event markers");
//    }

//    // Input validation helpers
//    public static bool IsValidEventName(string name)
//    {
//        return !string.IsNullOrWhiteSpace(name) && name.Length <= 50;
//    }

//    public static bool IsValidTime(float time)
//    {
//        return time >= 0f && time <= 3600f; // Max 1 hour
//    }
//}