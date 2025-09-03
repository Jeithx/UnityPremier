using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Event action'ının serialize edilebilir data'sı
/// </summary>
[System.Serializable]
public class EventActionData
{
    [Header("Action Information")]
    public string actionType;
    public string actionId; // Unique identifier for this action instance

    [Header("Target")]
    public string targetObjectName; // GameObject name (scene'deki object referansı için)
    public string targetObjectPath; // Hierarchy path for better object finding

    [Header("Timing")]
    public float delay = 0f; 
    public float duration = 1f; //this is only needed if the action is time-based (like tweening)

    [Header("Parameters")]
    public List<ActionParameter> parameters = new List<ActionParameter>();

    // Constructor
    public EventActionData()
    {
        actionId = System.Guid.NewGuid().ToString();
    }

    public EventActionData(string type)
    {
        actionType = type;
        actionId = System.Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Parameter değeri al
    /// </summary>
    public T GetParameter<T>(string name, T defaultValue = default(T))
    {
        var param = parameters.Find(p => p.name == name);
        if (param == null) return defaultValue;

        try
        {
            if (typeof(T) == typeof(Vector3))
            {
                return (T)(object)StringToVector3(param.value);
            }
            else if (typeof(T) == typeof(Color))
            {
                return (T)(object)StringToColor(param.value);
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)(object)bool.Parse(param.value);
            }
            else if (typeof(T) == typeof(float))
            {
                return (T)(object)float.Parse(param.value);
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(param.value);
            }
            else
            {
                return (T)(object)param.value;
            }
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Parameter değeri ayarla
    /// </summary>
    public void SetParameter<T>(string name, T value)
    {
        var param = parameters.Find(p => p.name == name);
        if (param == null)
        {
            param = new ActionParameter { name = name };
            parameters.Add(param);
        }

        if (typeof(T) == typeof(Vector3))
        {
            param.value = Vector3ToString((Vector3)(object)value);
        }
        else if (typeof(T) == typeof(Color))
        {
            param.value = ColorToString((Color)(object)value);
        }
        else
        {
            param.value = value.ToString();
        }

        param.type = typeof(T).Name;
    }

    // Helper methods for complex type conversions
    private Vector3 StringToVector3(string value)
    {
        var parts = value.Replace("(", "").Replace(")", "").Split(',');
        return new Vector3(
            float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
            float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture),
            float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture)
        );
    }

    private string Vector3ToString(Vector3 vector)
    {
        return string.Format(CultureInfo.InvariantCulture, "({0},{1},{2})",
            vector.x, vector.y, vector.z);
    }

    private Color StringToColor(string value)
    {
        var parts = value.Replace("RGBA(", "").Replace(")", "").Split(',');
        return new Color(
            float.Parse(parts[0].Trim()),
            float.Parse(parts[1].Trim()),
            float.Parse(parts[2].Trim()),
            float.Parse(parts[3].Trim())
        );
    }

    private string ColorToString(Color color)
    {
        return $"RGBA({color.r},{color.g},{color.b},{color.a})";
    }
}

/// <summary>
/// Action parameter'ı için key-value pair
/// </summary>
[System.Serializable]
public class ActionParameter
{
    public string name;
    public string value;
    public string type; // Parameter tipini tutmak için (debugging için yararlı)
}