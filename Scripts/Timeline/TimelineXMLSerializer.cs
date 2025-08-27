using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using UnityEngine;

public class TimelineXMLSerializer : MonoBehaviour
{
    [Header("References")]
    public TimelineGrid timelineGrid;
    public TimelineConfig config;

    [Header("Model Registry")]
    public List<ModelEntry> modelRegistry = new List<ModelEntry>();

    [System.Serializable]
    public class ModelEntry
    {
        public string name;
        public GameObject prefab;
    }

    // Window data structure - Enhanced
    [System.Serializable]
    public class WindowData
    {
        public float posX;
        public float posY;
        public float width;
        public float height;
        public float anchorMinX = 0.5f;
        public float anchorMinY = 0.5f;
        public float anchorMaxX = 0.5f;
        public float anchorMaxY = 0.5f;
        public float pivotX = 0.5f;
        public float pivotY = 0.5f;
    }

    // ==================== EXPORT ====================

    public void ExportToXML(string filePath)
    {
        try
        {
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                CreateProjectElement()
            );

            doc.Save(filePath);
            Debug.Log($"Timeline exported to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Export failed: {e.Message}");
        }
    }

    private XElement CreateProjectElement()
    {
        var project = new XElement("TimelineProject",
            new XAttribute("version", "3.0"),
            new XAttribute("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            new XAttribute("resolution", $"{Screen.width}x{Screen.height}")
        );

        // Settings
        if (config != null)
        {
            project.Add(new XElement("Settings",
                new XAttribute("gridCellPixels", config.gridCellHorizontalPixelCount),
                new XAttribute("laneHeight", config.laneHeight),
                new XAttribute("snapThreshold", config.snapPixelThreshold),
                new XAttribute("enablePreviews", config.enablePreviews)
            ));
        }

        // Gather all clips with proper track assignment
        var allClips = new List<ClipExportData>();

        // Video clips
        var videoControllers = timelineGrid.GetVideoControllers();
        foreach (var controller in videoControllers)
        {
            var content = controller.getvc();
            if (content != null)
            {
                allClips.Add(new ClipExportData
                {
                    Content = content,
                    Type = "Video",
                    Controller = controller,
                    Track = GetContentTrack(content)
                });
            }
        }

        // Image clips
        var imageControllers = timelineGrid.GetImageControllers();
        foreach (var controller in imageControllers)
        {
            var content = controller.getic();
            if (content != null)
            {
                allClips.Add(new ClipExportData
                {
                    Content = content,
                    Type = "Image",
                    Controller = controller,
                    Track = GetContentTrack(content)
                });
            }
        }

        // Audio clips  
        var audioControllers = timelineGrid.GetAudioControllers();
        foreach (var controller in audioControllers)
        {
            var content = controller.getac();
            if (content != null)
            {
                allClips.Add(new ClipExportData
                {
                    Content = content,
                    Type = "Audio",
                    Controller = controller,
                    Track = GetContentTrack(content)
                });
            }
        }

        // Model clips
        foreach (var controller in timelineGrid.modelControllerList)
        {
            var content = controller.getmc();
            if (content != null)
            {
                allClips.Add(new ClipExportData
                {
                    Content = content,
                    Type = "Model",
                    Controller = controller,
                    Track = GetContentTrack(content)
                });
            }
        }

        // Group by tracks
        var tracks = allClips.GroupBy(c => c.Track).OrderBy(g => g.Key);

        var tracksElement = new XElement("Tracks");
        foreach (var trackGroup in tracks)
        {
            var trackElement = new XElement("Track",
                new XAttribute("id", trackGroup.Key)
            );

            var clipsElement = new XElement("Clips");
            foreach (var clipData in trackGroup.OrderBy(c => c.Content.getStart()))
            {
                clipsElement.Add(CreateClipElement(clipData));
            }

            trackElement.Add(clipsElement);
            tracksElement.Add(trackElement);
        }

        project.Add(tracksElement);
        return project;
    }

    private class ClipExportData
    {
        public Content Content;
        public string Type;
        public object Controller;
        public int Track;
    }

    private int GetContentTrack(Content content)
    {
        // Get actual track index from bar parent
        var contentToBar = GetPrivateField<Dictionary<Content, RectTransform>>(timelineGrid, "contentToBar");
        if (contentToBar != null && contentToBar.TryGetValue(content, out var bar))
        {
            var trackRows = GetPrivateField<List<RectTransform>>(timelineGrid, "trackRows");
            if (trackRows != null)
            {
                for (int i = 0; i < trackRows.Count; i++)
                {
                    if (bar.parent == trackRows[i])
                        return i;
                }
            }
        }
        return content.GetLayer(); // Fallback
    }

    private XElement CreateClipElement(ClipExportData clipData)
    {
        var content = clipData.Content;
        var clip = new XElement("Clip");

        // Common attributes
        clip.Add(new XAttribute("type", clipData.Type));
        clip.Add(new XAttribute("start", content.getStart().ToString("F3")));
        clip.Add(new XAttribute("track", clipData.Track));

        // Type-specific data
        switch (clipData.Type)
        {
            case "Video":
                var vc = content as VideoContent;
                clip.Add(new XAttribute("path", vc.getPath()));
                break;

            case "Image":
                var ic = content as ImageContent;
                clip.Add(new XAttribute("path", ic.getPath()));
                float imageDuration = timelineGrid.GetLengthOverride(ic, ic.getLength());
                clip.Add(new XAttribute("duration", imageDuration.ToString("F3")));
                break;

            case "Audio":
                var ac = content as AudioContent;
                clip.Add(new XAttribute("path", ac.getPath()));
                break;

            case "Model":
                var mc = content as ModelContent;

                // Get model prefab name
                var prefabField = typeof(ModelContent).GetField("_modelPrefab",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                GameObject prefab = prefabField?.GetValue(mc) as GameObject;

                if (prefab != null)
                {
                    string prefabName = GetModelName(prefab);
                    clip.Add(new XAttribute("prefabName", prefabName));
                }

                // Get animation name
                var animField = typeof(ModelContent).GetField("_initialAnimation",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                string animName = animField?.GetValue(mc) as string;

                if (!string.IsNullOrEmpty(animName))
                {
                    clip.Add(new XAttribute("animation", animName));
                }
                break;
        }

        // CRITICAL: Add window data for ALL content types
        var windowData = GetWindowData(content);
        if (windowData != null)
        {
            var windowElement = new XElement("Window",
                new XAttribute("posX", windowData.posX.ToString("F2")),
                new XAttribute("posY", windowData.posY.ToString("F2")),
                new XAttribute("width", windowData.width.ToString("F2")),
                new XAttribute("height", windowData.height.ToString("F2")),
                new XAttribute("anchorMinX", windowData.anchorMinX.ToString("F3")),
                new XAttribute("anchorMinY", windowData.anchorMinY.ToString("F3")),
                new XAttribute("anchorMaxX", windowData.anchorMaxX.ToString("F3")),
                new XAttribute("anchorMaxY", windowData.anchorMaxY.ToString("F3")),
                new XAttribute("pivotX", windowData.pivotX.ToString("F3")),
                new XAttribute("pivotY", windowData.pivotY.ToString("F3"))
            );
            clip.Add(windowElement);
        }
        else
        {
            // If no window exists, store default values
            var defaultWindow = new XElement("Window",
                new XAttribute("posX", "0"),
                new XAttribute("posY", "0"),
                new XAttribute("width", "-1"), // -1 indicates "use default"
                new XAttribute("height", "-1"),
                new XAttribute("anchorMinX", "0.5"),
                new XAttribute("anchorMinY", "0.5"),
                new XAttribute("anchorMaxX", "0.5"),
                new XAttribute("anchorMaxY", "0.5"),
                new XAttribute("pivotX", "0.5"),
                new XAttribute("pivotY", "0.5")
            );
            clip.Add(defaultWindow);
        }

        return clip;
    }

    private WindowData GetWindowData(Content content)
    {
        // Access windows dictionary
        var windowsField = timelineGrid.GetType().GetField("windows",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (windowsField != null)
        {
            var windows = windowsField.GetValue(timelineGrid) as System.Collections.IDictionary;
            if (windows != null && windows.Contains(content))
            {
                var win = windows[content];
                var rootField = win.GetType().GetField("root");
                var rootRT = rootField?.GetValue(win) as RectTransform;

                if (rootRT != null)
                {
                    return new WindowData
                    {
                        posX = rootRT.anchoredPosition.x,
                        posY = rootRT.anchoredPosition.y,
                        width = rootRT.rect.width,
                        height = rootRT.rect.height,
                        anchorMinX = rootRT.anchorMin.x,
                        anchorMinY = rootRT.anchorMin.y,
                        anchorMaxX = rootRT.anchorMax.x,
                        anchorMaxY = rootRT.anchorMax.y,
                        pivotX = rootRT.pivot.x,
                        pivotY = rootRT.pivot.y
                    };
                }
            }
        }

        return null;
    }

    // ==================== IMPORT ====================

    public void ImportFromXML(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        try
        {
            XDocument doc = XDocument.Load(filePath);
            var root = doc.Root;

            if (root.Name != "TimelineProject")
            {
                Debug.LogError("Invalid XML format");
                return;
            }

            // Check resolution compatibility
            string savedResolution = root.Attribute("resolution")?.Value;
            if (!string.IsNullOrEmpty(savedResolution))
            {
                Debug.Log($"XML was created at resolution: {savedResolution}, current: {Screen.width}x{Screen.height}");

                // Parse saved resolution for scaling
                string[] parts = savedResolution.Split('x');
                if (parts.Length == 2 && float.TryParse(parts[0], out float savedWidth) && float.TryParse(parts[1], out float savedHeight))
                {
                    timelineGrid.SetSavedResolution(savedWidth, savedHeight);
                }
            }

            // Clear existing timeline
            timelineGrid.ClearAll();

            // Apply settings
            var settingsElement = root.Element("Settings");
            if (settingsElement != null && config != null)
            {
                config.gridCellHorizontalPixelCount = GetIntAttribute(settingsElement, "gridCellPixels", 20);
                config.laneHeight = GetFloatAttribute(settingsElement, "laneHeight", 60f);
                config.snapPixelThreshold = GetFloatAttribute(settingsElement, "snapThreshold", 10f);
                config.enablePreviews = GetBoolAttribute(settingsElement, "enablePreviews", true);
            }

            // Import clips with proper track assignment
            var tracks = root.Element("Tracks")?.Elements("Track");
            if (tracks != null)
            {
                StartCoroutine(ImportClipsWithTracks(tracks));
            }

            Debug.Log($"Timeline imported from: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Import failed: {e.Message}\n{e.StackTrace}");
        }
    }

    private System.Collections.IEnumerator ImportClipsWithTracks(IEnumerable<XElement> tracks)
    {
        // First, ensure we have enough tracks
        int maxTrackId = 0;
        foreach (var track in tracks)
        {
            int trackId = GetIntAttribute(track, "id", 0);
            maxTrackId = Mathf.Max(maxTrackId, trackId);
        }

        // Ensure timeline has enough rows
        while (timelineGrid.GetTrackCount() <= maxTrackId)
        {
            timelineGrid.CreateNewTrack();
            yield return null;
        }

        // Collect and sort all clips
        var allClipData = new List<(XElement clip, int trackId, float startTime)>();

        foreach (var track in tracks)
        {
            int trackId = GetIntAttribute(track, "id", 0);
            var clips = track.Element("Clips")?.Elements("Clip");

            if (clips != null)
            {
                foreach (var clip in clips)
                {
                    float startTime = GetFloatAttribute(clip, "start", 0);
                    allClipData.Add((clip, trackId, startTime));
                }
            }
        }

        // Sort by start time
        allClipData.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        // Import clips in order
        foreach (var (clip, trackId, startTime) in allClipData)
        {
            yield return ImportClipToTrack(clip, trackId);
            yield return new WaitForSeconds(0.1f);
        }

        // Apply resolution scaling if needed
        timelineGrid.ApplyResolutionScaling();
    }

    private System.Collections.IEnumerator ImportClipToTrack(XElement clipElement, int trackId)
    {
        string type = clipElement.Attribute("type")?.Value;
        float startTime = GetFloatAttribute(clipElement, "start", 0);

        // First add the content
        bool added = false;
        Content addedContent = null;

        switch (type)
        {
            case "Video":
                string videoPath = clipElement.Attribute("path")?.Value;
                if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
                {
                    timelineGrid.addVideoClip(videoPath);
                    added = true;
                }
                break;

            case "Image":
                string imagePath = clipElement.Attribute("path")?.Value;
                float imageDuration = GetFloatAttribute(clipElement, "duration", 5f);

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    timelineGrid.addImage(imagePath);
                    yield return null;
                    timelineGrid.SetLastImageDuration(imageDuration);
                    added = true;
                }
                break;

            case "Audio":
                string audioPath = clipElement.Attribute("path")?.Value;
                if (!string.IsNullOrEmpty(audioPath) && File.Exists(audioPath))
                {
                    timelineGrid.addAudio(audioPath);
                    added = true;
                }
                break;

            case "Model":
                string prefabName = clipElement.Attribute("prefabName")?.Value;
                string animationName = clipElement.Attribute("animation")?.Value;

                if (!string.IsNullOrEmpty(prefabName))
                {
                    GameObject prefab = GetModelPrefab(prefabName);
                    if (prefab != null)
                    {
                        float modelDuration = GetAnimationDuration(prefab, animationName);
                        timelineGrid.addModelToTimeline(prefab, modelDuration);

                        yield return null;

                        if (!string.IsNullOrEmpty(animationName))
                        {
                            SetLastModelAnimation(animationName);
                        }
                        added = true;
                    }
                }
                break;
        }

        if (added)
        {
            yield return null;

            // Get the content that was just added
            addedContent = timelineGrid.GetLastAddedContent();

            if (addedContent != null)
            {
                // Move to correct track
                timelineGrid.MoveContentToTrack(addedContent, trackId);
                yield return null;

                // Set start time
                timelineGrid.SetContentStartTime(addedContent, startTime);
                yield return null;

                // CRITICAL: Apply window data
                var windowElement = clipElement.Element("Window");
                if (windowElement != null)
                {
                    yield return ApplyWindowData(addedContent, windowElement);
                }
            }
        }
    }

    private System.Collections.IEnumerator ApplyWindowData(Content content, XElement windowElement)
    {
        if (content == null || windowElement == null) yield break;

        // Wait for window to be created
        yield return new WaitForSeconds(0.1f);

        float posX = GetFloatAttribute(windowElement, "posX", 0);
        float posY = GetFloatAttribute(windowElement, "posY", 0);
        float width = GetFloatAttribute(windowElement, "width", -1);
        float height = GetFloatAttribute(windowElement, "height", -1);

        float anchorMinX = GetFloatAttribute(windowElement, "anchorMinX", 0.5f);
        float anchorMinY = GetFloatAttribute(windowElement, "anchorMinY", 0.5f);
        float anchorMaxX = GetFloatAttribute(windowElement, "anchorMaxX", 0.5f);
        float anchorMaxY = GetFloatAttribute(windowElement, "anchorMaxY", 0.5f);
        float pivotX = GetFloatAttribute(windowElement, "pivotX", 0.5f);
        float pivotY = GetFloatAttribute(windowElement, "pivotY", 0.5f);

        // Set anchors first
        timelineGrid.SetWindowAnchors(content,
            new Vector2(anchorMinX, anchorMinY),
            new Vector2(anchorMaxX, anchorMaxY),
            new Vector2(pivotX, pivotY));

        // Then set position and size
        if (width > 0 && height > 0) // Only apply if valid values
        {
            // Apply resolution scaling
            Vector2 scale = timelineGrid.GetResolutionScale();
            timelineGrid.SetWindowProperties(content,
                posX * scale.x,
                posY * scale.y,
                width * scale.x,
                height * scale.y);
        }

        yield return null;
    }

    // ==================== HELPER METHODS ====================

    private GameObject GetModelPrefab(string name)
    {
        var entry = modelRegistry.FirstOrDefault(m => m.name == name);
        return entry?.prefab;
    }

    private string GetModelName(GameObject prefab)
    {
        var entry = modelRegistry.FirstOrDefault(m => m.prefab == prefab);
        return entry?.name ?? prefab?.name ?? "Unknown";
    }

    private float GetAnimationDuration(GameObject modelPrefab, string animationName)
    {
        if (modelPrefab == null) return 10f;

        GameObject temp = Instantiate(modelPrefab);
        temp.SetActive(false);

        Animator animator = temp.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName)
                {
                    DestroyImmediate(temp);
                    return clip.length;
                }
            }
        }

        DestroyImmediate(temp);
        return 10f;
    }

    private void SetLastModelAnimation(string animationName)
    {
        if (timelineGrid.modelControllerList.Count > 0)
        {
            var lastController = timelineGrid.modelControllerList[^1];
            lastController.getmc()?.SetInitialAnimation(animationName);
        }
    }

    private T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (T)field.GetValue(obj) : default(T);
    }

    private float GetFloatAttribute(XElement element, string name, float defaultValue)
    {
        var attr = element.Attribute(name);
        return attr != null && float.TryParse(attr.Value, out float result) ? result : defaultValue;
    }

    private int GetIntAttribute(XElement element, string name, int defaultValue)
    {
        var attr = element.Attribute(name);
        return attr != null && int.TryParse(attr.Value, out int result) ? result : defaultValue;
    }

    private bool GetBoolAttribute(XElement element, string name, bool defaultValue)
    {
        var attr = element.Attribute(name);
        return attr != null && bool.TryParse(attr.Value, out bool result) ? result : defaultValue;
    }
}