using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrackHeader : MonoBehaviour
{
    // --- INSPECTOR'DA ATANACAK ALANLAR ---
    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI trackNameText;
    [SerializeField] private Button visibilityButton;
    [SerializeField] private Button lockButton;

    [Header("İkonlar (Butonların içindeki Image'lar)")]
    [SerializeField] private Image visibilityIcon;
    [SerializeField] private Image lockIcon;

    [Header("İkon Sprite'ları (Duruma Göre Değişecek)")]
    [SerializeField] private Sprite visibilityOnSprite; // Görünür hali (açık göz)
    [SerializeField] private Sprite visibilityOffSprite; // Gizli hali (kapalı göz)
    [SerializeField] private Sprite lockOnSprite;       // Kilitli hali (kapalı kilit)
    [SerializeField] private Sprite lockOffSprite;      // Açık hali (açık kilit)

    // --- DEĞİŞKENLER ---
    private int trackIndex;
    private TimelineGrid timelineGrid;
    private bool isVisible = true;
    private bool isLocked = false;

    void Start()
    {
        visibilityButton.onClick.AddListener(ToggleVisibility);
        lockButton.onClick.AddListener(ToggleLock);
        UpdateIcons(); // Başlangıçta doğru ikonları göstersin
    }

    public void Initialize(int index, TimelineGrid grid, string name)
    {
        trackIndex = index;
        timelineGrid = grid;
        trackNameText.text = name;
    }

    public void SetTrackName(string newName)
    {
        trackNameText.text = newName;
    }

    private void ToggleVisibility()
    {
        isVisible = !isVisible;
        timelineGrid.SetTrackVisibility(trackIndex, isVisible);
        UpdateIcons(); // İkonu güncelle
    }

    private void ToggleLock()
    {
        isLocked = !isLocked;
        timelineGrid.SetTrackLock(trackIndex, isLocked);
        UpdateIcons(); // İkonu güncelle
    }

    private void UpdateIcons()
    {
        if (visibilityIcon != null)
        {
            visibilityIcon.sprite = isVisible ? visibilityOnSprite : visibilityOffSprite;
        }

        if (lockIcon != null)
        {
            lockIcon.sprite = isLocked ? lockOnSprite : lockOffSprite;
        }
    }
}