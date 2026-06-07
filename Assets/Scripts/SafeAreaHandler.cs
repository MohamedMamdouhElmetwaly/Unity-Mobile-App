using UnityEngine;

/// <summary>
/// Adjusts a RectTransform to fit within the device safe area.
/// Useful for handling notches, rounded corners, and device rotation.
/// </summary>
public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2Int lastScreenSize = new Vector2Int(0, 0);
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }
    
    void Update()
    {
        // Check if safe area or screen size changed (handles rotation)
        if (lastSafeArea != Screen.safeArea || 
            lastScreenSize.x != Screen.width || 
            lastScreenSize.y != Screen.height)
        {
            ApplySafeArea();
        }
    }
    
    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        
        // Convert safe area to anchors
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        
        // Cache values to detect changes
        lastSafeArea = safeArea;
        lastScreenSize.x = Screen.width;
        lastScreenSize.y = Screen.height;
        
        Debug.Log($"SafeArea applied: {safeArea}, Screen: {Screen.width}x{Screen.height}");
    }
}
