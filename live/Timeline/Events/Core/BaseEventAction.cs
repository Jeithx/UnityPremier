using UnityEngine;

/// <summary>
/// Tüm event action'ları için base class
/// Common functionality ve helper methods sağlar
/// </summary>
public abstract class BaseEventAction : IEventAction
{
    public abstract string ActionType { get; }
    public virtual bool RequiresContinuousUpdate => false;

    public abstract void Execute(EventActionData actionData);
    public abstract void Undo(EventActionData actionData);

    public virtual bool IsValid(EventActionData actionData)
    {
        if (actionData == null) return false;
        if (string.IsNullOrEmpty(actionData.targetObjectName)) return false;
        return true;
    }

    /// <summary>
    /// Scene'den GameObject bul (name ve path ile)
    /// </summary>
    protected GameObject FindTargetObject(EventActionData actionData)
    {
        if (string.IsNullOrEmpty(actionData.targetObjectName))
        {
            Debug.LogWarning($"[{ActionType}] Target object name is empty");
            return null;
        }

        GameObject target = null;

        // Önce path ile bulmaya çalış (daha güvenilir)
        if (!string.IsNullOrEmpty(actionData.targetObjectPath))
        {
            Transform foundTransform = FindTransformByPath(actionData.targetObjectPath);
            if (foundTransform != null)
            {
                target = foundTransform.gameObject;
            }
        }

        // Path ile bulamadıysa, name ile ara
        if (target == null)
        {
            target = GameObject.Find(actionData.targetObjectName);
        }

        if (target == null)
        {
            Debug.LogWarning($"[{ActionType}] Could not find target object: {actionData.targetObjectName}");
        }

        return target;
    }

    /// <summary>
    /// Timeline content'lerini de dahil ederek target bulma (genişletilmiş versiyon)
    /// </summary>
    protected Content FindTimelineContent(EventActionData actionData, TimelineGrid timeline)
    {
        Debug.Log($"[BaseEventAction] FindTimelineContent called for: '{actionData.targetObjectName}'");

        if (timeline == null || string.IsNullOrEmpty(actionData.targetObjectName)) return null;
        string targetName = actionData.targetObjectName;

        // Video content'lerde ara
        Debug.Log($"[BaseEventAction] Checking {timeline.GetVideoControllers().Count} video controllers");
        foreach (var videoController in timeline.GetVideoControllers())
        {
            var videoContent = videoController.getvc();
            if (videoContent != null && IsMatchingContent(targetName, "Video", videoContent))
            {
                Debug.Log($"[BaseEventAction] FOUND VIDEO MATCH for '{targetName}'");
                return videoContent;
            }
        }

        // Image content'lerde ara  
        Debug.Log($"[BaseEventAction] Checking {timeline.GetImageControllers().Count} image controllers");
        foreach (var imageController in timeline.GetImageControllers())
        {
            var imageContent = imageController.getic();
            if (imageContent != null && IsMatchingContent(targetName, "Image", imageContent))
            {
                Debug.Log($"[BaseEventAction] FOUND IMAGE MATCH for '{targetName}'");
                return imageContent;
            }
        }

        // Audio content'lerde ara
        Debug.Log($"[BaseEventAction] Checking {timeline.GetAudioControllers().Count} audio controllers");
        foreach (var audioController in timeline.GetAudioControllers())
        {
            var audioContent = audioController.getac();
            if (audioContent != null && IsMatchingContent(targetName, "Audio", audioContent))
            {
                Debug.Log($"[BaseEventAction] FOUND AUDIO MATCH for '{targetName}'");
                return audioContent;
            }
        }

        // Model content'lerde ara
        Debug.Log($"[BaseEventAction] Checking {timeline.modelControllerList.Count} model controllers");
        foreach (var modelController in timeline.modelControllerList)
        {
            var modelContent = modelController.getmc();
            if (modelContent != null && IsMatchingContent(targetName, "Model", modelContent))
            {
                Debug.Log($"[BaseEventAction] FOUND MODEL MATCH for '{targetName}'");
                return modelContent;
            }
        }

        Debug.LogWarning($"[BaseEventAction] NO CONTENT FOUND for '{targetName}'");
        return null;
    }

    /// <summary>
    /// Content'in target name ile eşleşip eşleşmediğini kontrol et
    /// </summary>
    private bool IsMatchingContent(string targetName, string contentType, Content content)
    {
        TimelineGrid timeline = GameObject.FindObjectOfType<TimelineGrid>();
        if (timeline == null) return false;

        // VIDEO CONTROLLER İNDEX BULMA
        if (contentType == "Video" && content is VideoContent)
        {
            var videoControllers = timeline.GetVideoControllers();
            for (int i = 0; i < videoControllers.Count; i++)
            {
                if (videoControllers[i].getvc() == content)
                {
                    string expectedName = $"Video_{i}";
                    Debug.Log($"[BaseEventAction] Video matching: '{targetName}' vs '{expectedName}'");
                    return targetName == expectedName;
                }
            }
        }

        // IMAGE CONTROLLER İNDEX BULMA
        else if (contentType == "Image" && content is ImageContent)
        {
            var imageControllers = timeline.GetImageControllers();
            for (int i = 0; i < imageControllers.Count; i++)
            {
                if (imageControllers[i].getic() == content)
                {
                    string expectedName = $"Image_{i}";
                    Debug.Log($"[BaseEventAction] Image matching: '{targetName}' vs '{expectedName}'");
                    return targetName == expectedName;
                }
            }
        }

        // AUDIO CONTROLLER İNDEX BULMA
        else if (contentType == "Audio" && content is AudioContent)
        {
            var audioControllers = timeline.GetAudioControllers();
            for (int i = 0; i < audioControllers.Count; i++)
            {
                if (audioControllers[i].getac() == content)
                {
                    string expectedName = $"Audio_{i}";
                    Debug.Log($"[BaseEventAction] Audio matching: '{targetName}' vs '{expectedName}'");
                    return targetName == expectedName;
                }
            }
        }

        // MODEL CONTROLLER İNDEX BULMA  
        else if (contentType == "Model" && content is ModelContent)
        {
            var modelControllers = timeline.modelControllerList;
            for (int i = 0; i < modelControllers.Count; i++)
            {
                if (modelControllers[i].getmc() == content)
                {
                    string expectedName = $"Model_{i}";
                    Debug.Log($"[BaseEventAction] Model matching: '{targetName}' vs '{expectedName}'");
                    return targetName == expectedName;
                }
            }
        }

        Debug.Log($"[BaseEventAction] NO MATCH: {targetName} for {contentType}");
        return false;
    }

    /// <summary>
    /// Timeline content'i veya scene GameObject'i bul (unified method)
    /// </summary>
    protected object FindTargetUnified(EventActionData actionData, TimelineGrid timeline)
    {
        // Önce scene'de GameObject ara
        GameObject sceneObject = FindTargetObject(actionData);
        if (sceneObject != null) return sceneObject;

        // Timeline content'lerde ara
        Content timelineContent = FindTimelineContent(actionData, timeline);
        if (timelineContent != null) return timelineContent;

        Debug.LogWarning($"[{ActionType}] Could not find target: {actionData.targetObjectName}");
        return null;
    }

    /// <summary>
    /// Hierarchy path'i ile Transform bul
    /// Örnek path: "Canvas/Panel/Button"
    /// </summary>
    private Transform FindTransformByPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        string[] parts = path.Split('/');
        Transform current = null;

        // Root object'i bul
        GameObject rootObj = GameObject.Find(parts[0]);
        if (rootObj == null) return null;

        current = rootObj.transform;

        // Path'i takip et
        for (int i = 1; i < parts.Length; i++)
        {
            current = current.Find(parts[i]);
            if (current == null) return null;
        }

        return current;
    }

    /// <summary>
    /// GameObject'in hierarchy path'ini oluştur
    /// </summary>
    public static string GetObjectPath(GameObject obj)
    {
        if (obj == null) return "";

        string path = obj.name;
        Transform current = obj.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    /// <summary>
    /// Action execution'ını log'la (debugging için)
    /// </summary>
    protected void LogExecution(EventActionData actionData, string operation)
    {
        Debug.Log($"[{ActionType}] {operation} on '{actionData.targetObjectName}' " +
                 $"(Event: {actionData.actionId})");
    }
}