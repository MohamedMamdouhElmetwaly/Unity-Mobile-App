using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingScreen;
    public GameObject mainUI;
    public Slider progressBar;
    public Text progressText;
    
    public Button virtualTourButton;
    public Button topViewButton;
    
    [Header("Camera Reference")]
    public CameraController cameraController;
    
    [Header("Joystick Reference")]
    public GameObject virtualJoystick;
    
    [Header("Zoom Buttons Reference")]
    public GameObject zoomInButton;
    public GameObject zoomOutButton;
    
    [Header("Movement Guide Reference")]
    public MovementGuideUI movementGuideUI;
    public Toggle guideToggle; // Reference to the GuideToggle button

    [Header("Touch Indicators Reference")]
    public TouchControlsUI touchControlsUI;
    
    private bool isLoadingComplete = false;
    
    void Start()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);
            
        if (mainUI != null)
            mainUI.SetActive(false);
            
        // If we have an active preset on the camera, apply it to the UI as well
        if (cameraController != null && cameraController.activePreset != null)
        {
            ApplyPreset(cameraController.activePreset);
        }
        else
        {
            // Hide joystick during loading
            if (virtualJoystick != null)
                virtualJoystick.SetActive(false);
                
            // Hide zoom buttons during loading
            if (zoomInButton != null)
                zoomInButton.SetActive(false);
                
            if (zoomOutButton != null)
                zoomOutButton.SetActive(false);
        }
        
        // Hide guide toggle during loading
        if (guideToggle != null)
            guideToggle.gameObject.SetActive(false);
            
        StartCoroutine(LoadingSequence());
        
        // Setup button listeners
        if (virtualTourButton != null)
            virtualTourButton.onClick.AddListener(OnVirtualTourClicked);
            
        if (topViewButton != null)
            topViewButton.onClick.AddListener(OnTopViewClicked);
    }
    
    IEnumerator LoadingSequence()
    {
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress += Time.deltaTime * 0.5f; // Loading speed
            progress = Mathf.Clamp01(progress);
            
            if (progressBar != null)
                progressBar.value = progress;
                
            if (progressText != null)
                progressText.text = $"Loading... {(int)(progress * 100)}%";
                
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        isLoadingComplete = true;
        
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
            
        if (mainUI != null)
            mainUI.SetActive(true);
            
        // Show UI elements based on preset if available
        if (cameraController != null && cameraController.activePreset != null)
        {
            ApplyPreset(cameraController.activePreset);
        }
        else
        {
            // Fallback defaults if no preset
            if (virtualJoystick != null)
                virtualJoystick.SetActive(true);
                
            if (zoomInButton != null)
                zoomInButton.SetActive(true);
                
            if (zoomOutButton != null)
                zoomOutButton.SetActive(true);
        }
        
        // Show guide toggle after loading completes
        if (guideToggle != null)
            guideToggle.gameObject.SetActive(true);
            
        // Guide is now controlled by toggle, not auto-shown after loading
    }
    
    public void OnVirtualTourClicked()
    {
        if (cameraController != null)
        {
            cameraController.SwitchToVirtualTour();
            Debug.Log("Switched to Virtual Tour mode");
        }
    }
    
    public void OnTopViewClicked()
    {
        if (cameraController != null)
        {
            cameraController.SwitchToTopView();
            Debug.Log("Switched to Top View mode");
        }
    }

    public void ApplyPreset(CameraAppSettingsPreset preset)
    {
        if (preset == null) return;
        
        // 1. Tell CameraController to apply its settings
        if (cameraController != null)
        {
            cameraController.ApplyPreset(preset);
        }
        
        // 2. Handle UI toggles
        if (virtualJoystick != null)
            virtualJoystick.SetActive(preset.joystickEnabled);
            
        if (zoomInButton != null)
            zoomInButton.SetActive(preset.zoomButtonsEnabled);
            
        if (zoomOutButton != null)
            zoomOutButton.SetActive(preset.zoomButtonsEnabled);
            
        if (touchControlsUI != null)
        {
            touchControlsUI.gameObject.SetActive(preset.touchIndicatorsEnabled);
            touchControlsUI.showIndicators = preset.touchIndicatorsEnabled;
        }
            
        if (movementGuideUI != null)
            movementGuideUI.gameObject.SetActive(preset.movementGuideEnabled);
            
        Debug.Log($"Applied app configuration from preset: {preset.name}");
    }
}