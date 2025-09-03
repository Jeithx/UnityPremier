//using UnityEngine;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;

//public class GlobalTimelineKeyboard : MonoBehaviour
//{
//    [Header("References")]
//    public TimelineGrid timeline;

//    [Header("Keyboard Settings")]
//    public bool enableGlobalShortcuts = true;
//    public bool enableContentDeletion = true;
//    public bool enableEventDeletion = true;

//    void Start()
//    {
//        if (timeline == null)
//            timeline = GetComponent<TimelineGrid>();
//    }

//    void Update()
//    {
//        if (!enableGlobalShortcuts) return;

//        HandleGlobalShortcuts();
//        HandleDeletionKeys();
//        HandleSelectionKeys();
//        HandlePlaybackKeys();
//    }

//    private void HandleGlobalShortcuts()
//    {
//        // Play/Pause
//        if (Input.GetKeyDown(KeyCode.Space) && !IsTypingInInputField())
//        {
//            if (timeline.getFlag())
//                timeline.stopVideo();
//            else
//                timeline.playVideo();
//        }

//        // Timeline navigation
//        if (Input.GetKeyDown(KeyCode.Home))
//        {
//            timeline.SetTime(0f);
//        }

//        if (Input.GetKeyDown(KeyCode.End))
//        {
//            float maxTime = CalculateTimelineEnd();
//            timeline.SetTime(maxTime);
//        }

//        // Frame stepping
//        if (Input.GetKeyDown(KeyCode.LeftArrow))
//        {
//            timeline.StepSeconds(-0.25f);
//        }
//        else if (Input.GetKeyDown(KeyCode.RightArrow))
//        {
//            timeline.StepSeconds(0.25f);
//        }

//        // Skip by larger amounts
//        if (Input.GetKeyDown(KeyCode.PageUp))
//        {
//            timeline.StepSeconds(-5f);
//        }
//        else if (Input.GetKeyDown(KeyCode.PageDown))
//        {
//            timeline.StepSeconds(5f);
//        }
//    }

//    private void HandleDeletionKeys()
//    {
//        if (!Input.GetKeyDown(KeyCode.Delete)) return;

//        // Check what's currently selected/focused
//        GameObject selected = EventSystem.current.currentSelectedGameObject;

//        // Priority 1: Track headers (handles both empty and content deletion)
//        var trackHeader = selected?.GetComponent<TrackHeader>();
//        if (trackHeader != null)
//        {
//            return; // Let track header handle its own deletion
//        }

//        // Priority 2: Event markers
//        var eventMarker = selected?.GetComponent<EnhancedEventMarker>();
//        if (eventMarker != null && enableEventDeletion)
//        {
//            return; // Let event marker handle its own deletion
//        }

//        // Priority 3: Selected content bars
//        if (enableContentDeletion)
//        {
//            var barManager = timeline.GetBarManager();
//            var selectedBars = barManager.GetSelectedBars();

//            if (selectedBars.Count > 0)
//            {
//                bool forceDelete = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

//                if (forceDelete || ShowContentDeletionConfirmation(selectedBars.Count))
//                {
//                    timeline.DeleteSelectedContent();
//                }
//                return;
//            }
//        }

//        // Priority 4: Selected event markers (if none specifically focused)
//        if (enableEventDeletion)
//        {
//            var selectedEventMarkers = EnhancedEventMarker.GetSelectedMarkers();
//            if (selectedEventMarkers.Count > 0)
//            {
//                bool forceDelete = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

//                if (forceDelete || ShowEventDeletionConfirmation(selectedEventMarkers.Count))
//                {
//                    EnhancedEventMarker.DeleteSelectedMarkers();
//                }
//                return;
//            }
//        }

//        // If nothing specific is selected, show help
//        if (selected == null)
//        {
//            ShowDeletionHelp();
//        }
//    }

//    private void HandleSelectionKeys()
//    {
//        // Ctrl+A - Select all content
//        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
//        {
//            SelectAllContent();
//        }

//        // Ctrl+D - Deselect all
//        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
//        {
//            DeselectAll();
//        }

//        // Escape - Deselect all
//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            DeselectAll();
//        }
//    }

//    private void HandlePlaybackKeys()
//    {
//        // J, K, L keys (common in video editors)
//        if (Input.GetKeyDown(KeyCode.J))
//        {
//            // Reverse play or step back
//            timeline.StepSeconds(-1f);
//        }
//        else if (Input.GetKeyDown(KeyCode.K))
//        {
//            // Pause
//            timeline.stopVideo();
//        }
//        else if (Input.GetKeyDown(KeyCode.L))
//        {
//            // Play forward
//            timeline.playVideo();
//        }
//    }

//    private bool ShowContentDeletionConfirmation(int contentCount)
//    {
//        // In a real implementation, you'd show a proper dialog
//        // For now, we'll use a simple log-based confirmation
//        Debug.Log($"Delete {contentCount} content item(s)? Press Shift+Delete to confirm or any other key to cancel.");

//        // Simple confirmation: if Shift is held, consider it confirmed
//        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
//    }

//    private bool ShowEventDeletionConfirmation(int eventCount)
//    {
//        Debug.Log($"Delete {eventCount} event marker(s)? Press Shift+Delete to confirm or any other key to cancel.");
//        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
//    }

//    private void ShowDeletionHelp()
//    {
//        Debug.Log("DELETION HELP:");
//        Debug.Log("• Click on track header, then Delete = Delete track");
//        Debug.Log("• Click on content bar, then Delete = Delete content");
//        Debug.Log("• Click on event marker, then Delete = Delete event");
//        Debug.Log("• Shift+Delete = Force delete without confirmation");
//        Debug.Log("• Ctrl+A = Select all content");
//        Debug.Log("• Escape = Deselect all");
//    }

//    private void SelectAllContent()
//    {
//        var barManager = timeline.GetBarManager();
//        barManager.ClearSelection();

//        foreach (var kvp in barManager.GetBarMap())
//        {
//            if (kvp.Key != null)
//            {
//                barManager.SelectBar(kvp.Key, true);
//            }
//        }

//        Debug.Log($"Selected all content ({barManager.GetSelectedBars().Count} items)");
//    }

//    private void DeselectAll()
//    {
//        // Deselect content
//        timeline.GetBarManager().ClearSelection();

//        // Deselect tracks
//        TrackHeader.DeselectAllTracks();

//        // Deselect event markers
//        EnhancedEventMarker.DeselectAllMarkers();

//        // Clear UI selection
//        EventSystem.current.SetSelectedGameObject(null);

//        Debug.Log("Deselected all items");
//    }

//    private float CalculateTimelineEnd()
//    {
//        float maxTime = 0f;

//        foreach (var controller in timeline.GetVideoControllers())
//            maxTime = Mathf.Max(maxTime, controller.getvc().getEnd());
//        foreach (var controller in timeline.GetImageControllers())
//            maxTime = Mathf.Max(maxTime, controller.getic().getEnd());
//        foreach (var controller in timeline.GetAudioControllers())
//            maxTime = Mathf.Max(maxTime, controller.getac().getEnd());
//        foreach (var controller in timeline.modelControllerList)
//            maxTime = Mathf.Max(maxTime, controller.getmc().getEnd());

//        return maxTime;
//    }

//    private bool IsTypingInInputField()
//    {
//        GameObject selected = EventSystem.current.currentSelectedGameObject;
//        return selected != null && (
//            selected.GetComponent<TMPro.TMP_InputField>() != null ||
//            selected.GetComponent<UnityEngine.UI.InputField>() != null
//        );
//    }

//    // Debug helper - shows current selection state
//    [ContextMenu("Show Selection State")]
//    public void ShowSelectionState()
//    {
//        var barManager = timeline.GetBarManager();
//        var selectedBars = barManager.GetSelectedBars();
//        var selectedEvents = EnhancedEventMarker.GetSelectedMarkers();

//        Debug.Log("=== SELECTION STATE ===");
//        Debug.Log($"Selected content bars: {selectedBars.Count}");
//        Debug.Log($"Selected event markers: {selectedEvents.Count}");

//        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
//        if (currentSelected != null)
//        {
//            Debug.Log($"Currently focused UI object: {currentSelected.name}");
//        }

//        var trackHeaders = FindObjectsOfType<TrackHeader>();
//        int selectedTracks = 0;
//        foreach (var header in trackHeaders)
//        {
//            if (header.IsSelected()) selectedTracks++;
//        }
//        Debug.Log($"Selected track headers: {selectedTracks}");
//    }
//}