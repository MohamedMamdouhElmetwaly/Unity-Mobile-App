using UnityEngine;
using UnityEngine.UI;

public class MovementGuideUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject guidePanel;
    public CanvasGroup canvasGroup;
    public Toggle guideToggle;
    
    [Header("Auto Hide Settings")]
    public bool autoHide = true;
    public float autoHideDelay = 5f;
    
    [Header("Guide Settings")]
    public bool guideEnabled = false; // Start unchecked by default
    
    private float hideTimer = 0f;
    private bool isVisible = false;
    private const string GUIDE_ENABLED_KEY = "MovementGuideEnabled";
    
    void Start()
    {
        // Load saved preference, default to FALSE (unchecked)
        guideEnabled = PlayerPrefs.GetInt(GUIDE_ENABLED_KEY, 0) == 1;
        
        // Setup toggle
        if (guideToggle != null)
        {
            guideToggle.isOn = guideEnabled;
            guideToggle.onValueChanged.AddListener(OnGuideToggleChanged);
        }
        
        // Ensure canvas group exists
        if (canvasGroup == null && guidePanel != null)
        {
            canvasGroup = guidePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = guidePanel.AddComponent<CanvasGroup>();
        }
        
        // Show guide only if toggle is ON, hide if OFF
        if (guideEnabled)
        {
            ShowGuide();
        }
        else
        {
            HideGuide();
        }
    }
    
    void Update()
    {
        // Auto-hide countdown - disabled for toggle-controlled behavior
        // Guide now stays visible when toggle is ON
    }
    
    public void ShowGuide()
    {
        if (!guideEnabled) return;
        
        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
            isVisible = true;
            hideTimer = autoHideDelay;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }
    
    public void HideGuide()
    {
        if (guidePanel != null)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            guidePanel.SetActive(false);
            isVisible = false;
        }
    }
    
    public void SetGuideVisible(bool visible)
    {
        if (visible)
        {
            ShowGuide();
        }
        else
        {
            HideGuide();
        }
    }
    
    public void SetGuideEnabled(bool enabled)
    {
        guideEnabled = enabled;
        
        // Save preference
        PlayerPrefs.SetInt(GUIDE_ENABLED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
        
        // Update toggle if it exists
        if (guideToggle != null && guideToggle.isOn != enabled)
        {
            guideToggle.isOn = enabled;
        }
        
        // Show or hide based on enabled state
        if (enabled)
        {
            ShowGuide();
        }
        else
        {
            HideGuide();
        }
    }
    
    public void OnGuideToggleChanged(bool isOn)
    {
        SetGuideEnabled(isOn);
    }
}
