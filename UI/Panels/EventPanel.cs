using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarkerUI : MonoBehaviour
{
    [Header("References")]
    public GameObject panel;         
    public Button[] optionButtons; 
    public Image markerVisual; 

    private void Awake()
    {
        //if (panel != null)
        //    panel.SetActive(false);

        foreach (var btn in optionButtons)
        {
            string label = null;
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) label = tmp.text;

            if (label != null)
            {
                btn.onClick.AddListener(() => OnOptionSelected(label));
            }
        }
    }

    //public void ShowPanel()
    //{
    //    if (panel != null)
    //        panel.SetActive(true);
    //}

    private void OnOptionSelected(string label)
    {
        Color selected = Color.white;

        switch (label.ToLower())
        {
            case "success":
                selected = Color.green;
                break;
            case "attention":
                selected = Color.yellow;
                break;
            case "ınfo":
                selected = Color.blue;
                break;
            case "error":
                selected = Color.red;
                break;
            default:
                selected = Color.white;
                break;
        }

        if (markerVisual != null)
            markerVisual.color = selected;

        if (panel != null)
            panel.SetActive(false);
    }
}
