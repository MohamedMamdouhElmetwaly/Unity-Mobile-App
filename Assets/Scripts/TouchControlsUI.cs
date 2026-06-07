using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TouchControlsUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leftSideIndicator;
    public GameObject rightSideIndicator;
    public Text instructionText;
    
    [Header("Settings")]
    public bool showIndicators = true;
    public float fadeAfterSeconds = 5f;
    
    private float displayTimer = 0f;
    private CanvasGroup leftCanvasGroup;
    private CanvasGroup rightCanvasGroup;
    private CanvasGroup textCanvasGroup;
    
    void Start()
    {
        // Auto-create UI if not assigned
        if (leftSideIndicator == null || rightSideIndicator == null)
        {
            CreateTouchIndicators();
        }
        
        // Get or add canvas groups for fading
        if (leftSideIndicator != null)
        {
            leftCanvasGroup = leftSideIndicator.GetComponent<CanvasGroup>();
            if (leftCanvasGroup == null)
                leftCanvasGroup = leftSideIndicator.AddComponent<CanvasGroup>();
        }
        if (rightSideIndicator != null)
        {
            rightCanvasGroup = rightSideIndicator.GetComponent<CanvasGroup>();
            if (rightCanvasGroup == null)
                rightCanvasGroup = rightSideIndicator.AddComponent<CanvasGroup>();
        }
        if (instructionText != null)
        {
            textCanvasGroup = instructionText.GetComponent<CanvasGroup>();
            if (textCanvasGroup == null)
                textCanvasGroup = instructionText.gameObject.AddComponent<CanvasGroup>();
        }
        
        displayTimer = fadeAfterSeconds;
    }
    
    void Update()
    {
        if (!showIndicators) return;
        
        // Fade out after time
        if (displayTimer > 0)
        {
            displayTimer -= Time.deltaTime;
            
            if (displayTimer <= 0)
            {
                FadeOutIndicators();
            }
        }
        
        // Show indicators when user touches screen
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            bool anyTouchActive = false;
            for (int i = 0; i < Touchscreen.current.touches.Count; i++)
            {
                if (Touchscreen.current.touches[i].isInProgress)
                {
                    anyTouchActive = true;
                    break;
                }
            }
            
            if (anyTouchActive)
            {
                displayTimer = fadeAfterSeconds;
                ShowIndicators();
            }
        }
    }
    
    void CreateTouchIndicators()
    {
        // This is a placeholder - in reality you'd create UI elements here
        // or assign them in the Inspector
        Debug.Log("Touch indicators should be created in the UI Canvas");
    }
    
    void ShowIndicators()
    {
        if (leftCanvasGroup != null) leftCanvasGroup.alpha = 0.7f;
        if (rightCanvasGroup != null) rightCanvasGroup.alpha = 0.7f;
        if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;
    }
    
    void FadeOutIndicators()
    {
        if (leftCanvasGroup != null) leftCanvasGroup.alpha = 0.1f;
        if (rightCanvasGroup != null) rightCanvasGroup.alpha = 0.1f;
        if (textCanvasGroup != null) textCanvasGroup.alpha = 0.3f;
    }
    
    public void SetInstructionText(string mode)
    {
        if (instructionText != null)
        {
            if (mode == "TopView")
            {
                instructionText.text = "Left: Move | Right: Look Around | 2 Fingers: Zoom";
            }
            else if (mode == "VirtualTour")
            {
                instructionText.text = "Left: Move | Right: Look Around | 2 Fingers: Zoom";
            }
        }
    }
}
