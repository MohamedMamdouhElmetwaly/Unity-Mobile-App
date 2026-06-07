using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float movementSpeed = 1000f;
    public float lookSensitivity = 8f;
    public float zoomSpeed = 100f; // Increased for HIGH SPEED zoom
    public float smoothSpeed = 10f;
    public float scrollSensitivity = 2f; // Added for extra scroll speed
    
    [Header("Input Settings")]
    public bool enableMouseInput = false; // SET TO FALSE FOR TOUCH TESTING
    
    [Header("Virtual Joystick")]
    public VirtualJoystick movementJoystick;
    
    [Header("Virtual Tour Settings")]
    public Vector3 virtualTourStartPosition = new Vector3(-64.42599f, 1689.906f, -1695.566f);
    public Vector3 virtualTourStartRotation = new Vector3(5.192f, -179.347f, -0.001f);
    public float virtualTourFOV = 60f;
    
    [Header("Preset Settings")]
    public CameraAppSettingsPreset activePreset;
    
    [Header("Top View Settings")]
    public Vector3 topViewPosition = new Vector3(-20, 11991.48f, -1523);
    public Vector3 topViewRotation = new Vector3(90, 0, 0);
    public float topViewOrthographicSize = 6997.486f;
    
    private Camera cam;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetFOV;
    private float targetOrthographicSize;
    
    private Vector2 moveInput;
    private Vector2 lookDelta;
    private float zoomInput;
    
    private bool isVirtualTourMode = false; // Start in Top View mode
    private bool isMoving = false;
    private bool isLooking = false;
    
    private Vector2 lastMousePosition;
    private float rotationX = 0f;
    private float rotationY = 0f;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        if (activePreset != null)
        {
            ApplyPreset(activePreset);
        }
            
        // Start in Top View mode
        cam.orthographic = true;
        cam.orthographicSize = topViewOrthographicSize;
        targetOrthographicSize = topViewOrthographicSize;
        targetPosition = topViewPosition;
        targetRotation = Quaternion.Euler(topViewRotation);
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        
        rotationX = topViewRotation.x;
        rotationY = topViewRotation.y;
        
        targetFOV = cam.fieldOfView;
        
        Debug.Log($"[CameraStart] Initialized in Top View mode");
        Debug.Log($"[CameraStart] Position: {transform.position}, Rotation: {transform.rotation.eulerAngles}");
        Debug.Log($"[CameraStart] rotationX: {rotationX}, rotationY: {rotationY}");
        Debug.Log($"[CameraStart] topViewPosition: {topViewPosition}");
    }
    
    public void ApplyPreset(CameraAppSettingsPreset preset)
    {
        if (preset == null) return;
        
        activePreset = preset;
        
        movementSpeed = preset.movementSpeed;
        lookSensitivity = preset.lookSensitivity;
        zoomSpeed = preset.zoomSpeed;
        smoothSpeed = preset.smoothSpeed;
        
        topViewPosition = preset.topViewPosition;
        topViewRotation = preset.topViewRotation;
        topViewOrthographicSize = preset.topViewOrthographicSize;
        
        virtualTourStartPosition = preset.virtualTourStartPosition;
        virtualTourStartRotation = preset.virtualTourStartRotation;
        virtualTourFOV = preset.virtualTourFOV;

        // Apply to camera state
        if (isVirtualTourMode)
        {
            targetFOV = virtualTourFOV;
        }
        else
        {
            targetOrthographicSize = topViewOrthographicSize;
        }
        
        // GameManager will handle UI toggles via its own ApplyPreset or by observing activePreset
    }

    [ContextMenu("Save Current to Preset")]
    public void SaveToPreset()
    {
        if (activePreset == null)
        {
            Debug.LogError("No active preset assigned to save to!");
            return;
        }
        
        activePreset.movementSpeed = movementSpeed;
        activePreset.lookSensitivity = lookSensitivity;
        activePreset.zoomSpeed = zoomSpeed;
        activePreset.smoothSpeed = smoothSpeed;
        
        activePreset.topViewPosition = topViewPosition;
        activePreset.topViewRotation = topViewRotation;
        activePreset.topViewOrthographicSize = topViewOrthographicSize;
        
        activePreset.virtualTourStartPosition = virtualTourStartPosition;
        activePreset.virtualTourStartRotation = virtualTourStartRotation;
        activePreset.virtualTourFOV = virtualTourFOV;
        
        if (isVirtualTourMode)
        {
            activePreset.virtualTourFOV = targetFOV;
            virtualTourFOV = targetFOV; // Sync local field too
        }

        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(activePreset);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Saved current settings to preset: {activePreset.name}");
        #endif
    }
    
    void Update()
    {
        // Debug heartbeat to confirm script is running
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"[CameraHeartbeat] Script is running on {gameObject.name}. Active Mode: {(isVirtualTourMode ? "VirtualTour" : "TopView")}");
        }
        
        HandleInput();
        UpdateCameraTransform();
    }
    
    void HandleInput()
    {
        // Reset zoom input each frame before accumulating
        zoomInput = 0f;

        // Mouse input for editor testing
        if (enableMouseInput)
        {
            HandleMouseInput();
        }
        
        // Touch input for mobile
        HandleTouchInput();
        
        // Virtual joystick input
        HandleJoystickInput();
        
        // New Input System scroll wheel zoom
        if (Mouse.current != null)
        {
            Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
            if (Mathf.Abs(scrollDelta.y) > 0.01f)
            {
                float normalizedScroll = scrollDelta.y / 120f; // Normalize
                zoomInput += normalizedScroll * 20f; // Increased sensitivity
                Debug.Log($"[ZoomInput] NEW Input scroll: {scrollDelta.y}, normalized: {normalizedScroll}, zoomInput: {zoomInput}");
            }
        }

        if (Mathf.Abs(zoomInput) > 0.0001f)
        {
            Debug.Log($"[ZoomInput] === TOTAL ZOOM: {zoomInput} ===");
        }
        else
        {
            // Debug every 2 seconds if no zoom detected
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log("[ZoomInput] No scroll detected. Try scrolling in Game View!");
            }
        }
    }
    
    void HandleMouseInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        // Left click + drag for movement
        if (Mouse.current.leftButton.isPressed)
        {
            if (!isMoving)
            {
                isMoving = true;
                lastMousePosition = Mouse.current.position.ReadValue();
            }
            
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = currentMousePos - lastMousePosition;
            
            if (isVirtualTourMode)
            {
                // Virtual Tour: Movement in all directions including vertical
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;
                
                Vector3 moveDir = right * mouseDelta.x * Time.deltaTime * movementSpeed * 0.8f;
                moveDir += forward * mouseDelta.y * Time.deltaTime * movementSpeed * 0.8f;
                targetPosition += moveDir;
            }
            else
            {
                // Top View: Move in world horizontal plane based on camera Y rotation
                Vector3 worldForward = Quaternion.Euler(0, rotationY, 0) * Vector3.forward;
                Vector3 worldRight = Quaternion.Euler(0, rotationY, 0) * Vector3.right;
                
                Vector3 moveDir = worldRight * mouseDelta.x * Time.deltaTime * movementSpeed * 0.8f;
                moveDir += worldForward * mouseDelta.y * Time.deltaTime * movementSpeed * 0.8f;
                
                // Ensure movement stays on horizontal plane
                moveDir.y = 0;
                
                targetPosition += moveDir;
            }
            
            lastMousePosition = currentMousePos;
        }
        else
        {
            isMoving = false;
        }
        
        // Right click + drag for look around - ENABLED IN BOTH MODES
        if (Mouse.current.rightButton.isPressed)
        {
            if (!isLooking)
            {
                isLooking = true;
                lastMousePosition = Mouse.current.position.ReadValue();
                Debug.Log("Right-click look started");
            }
            
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = currentMousePos - lastMousePosition;
            
            if (mouseDelta.magnitude > 0.1f)
            {
                // Look around in both modes
                rotationY += mouseDelta.x * lookSensitivity * 0.5f;
                rotationX -= mouseDelta.y * lookSensitivity * 0.5f;
                rotationX = Mathf.Clamp(rotationX, -90f, 90f);
                
                targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
                Debug.Log($"Look - Rot: ({rotationX:F1}, {rotationY:F1})");
            }
            
            lastMousePosition = currentMousePos;
        }
        else
        {
            if (isLooking)
            {
                Debug.Log("Right-click released");
            }
            isLooking = false;
        }
    }
    
    void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;
        
        var touches = Touchscreen.current.touches;
        
        // Determine screen zones for left/right touch
        float screenWidth = Screen.width;
        float leftZoneEnd = screenWidth * 0.4f; // Left 40% for movement
        
        // ONE FINGER - Movement on left, Look Around on right
        if (touches[0].isInProgress && !touches[1].isInProgress)
        {
            var touch = touches[0];
            Vector2 touchPos = touch.position.ReadValue();
            Vector2 touchDelta = touch.delta.ReadValue();
            
            // Check if touch is on joystick - if so, skip this handling
            if (movementJoystick != null && IsTouchOnJoystick(touchPos))
            {
                return; // Joystick will handle this
            }
            
            // Left side: Move
            if (touchPos.x < leftZoneEnd)
            {
                if (isVirtualTourMode)
                {
                    // Virtual Tour: Move in camera direction (full 3D)
                    Vector3 forward = transform.forward;
                    Vector3 right = transform.right;
                    
                    Vector3 moveDir = forward * touchDelta.y * Time.deltaTime * movementSpeed * 0.4f;
                    moveDir += right * touchDelta.x * Time.deltaTime * movementSpeed * 0.4f;
                    targetPosition += moveDir;
                }
                else
                {
                    // Top View: Move in world horizontal plane based on camera Y rotation
                    Vector3 worldForward = Quaternion.Euler(0, rotationY, 0) * Vector3.forward;
                    Vector3 worldRight = Quaternion.Euler(0, rotationY, 0) * Vector3.right;
                    
                    Vector3 moveDir = worldForward * touchDelta.y * Time.deltaTime * movementSpeed * 0.4f;
                    moveDir += worldRight * touchDelta.x * Time.deltaTime * movementSpeed * 0.4f;
                    
                    // Ensure movement stays on horizontal plane
                    moveDir.y = 0;
                    
                    targetPosition += moveDir;
                }
            }
            // Right side: Look around (same for both modes)
            else
            {
                rotationY += touchDelta.x * lookSensitivity * 0.2f;
                rotationX -= touchDelta.y * lookSensitivity * 0.2f;
                rotationX = Mathf.Clamp(rotationX, -90f, 90f);
                targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
            }
        }
        
        // TWO FINGERS - Pinch to Zoom (works in both modes)
        if (touches[0].isInProgress && touches[1].isInProgress)
        {
            Debug.Log("[Touch] Two fingers detected - pinch zoom active");
            
            var touch0 = touches[0];
            var touch1 = touches[1];
            
            Vector2 touch0Pos = touch0.position.ReadValue();
            Vector2 touch1Pos = touch1.position.ReadValue();
            Vector2 touch0Delta = touch0.delta.ReadValue();
            Vector2 touch1Delta = touch1.delta.ReadValue();
            
            Vector2 touch0Prev = touch0Pos - touch0Delta;
            Vector2 touch1Prev = touch1Pos - touch1Delta;
            
            float prevMagnitude = (touch0Prev - touch1Prev).magnitude;
            float currentMagnitude = (touch0Pos - touch1Pos).magnitude;
            
            float difference = currentMagnitude - prevMagnitude;
            zoomInput += difference * 0.05f;
            
            Debug.Log($"[Pinch] Prev: {prevMagnitude:F1}, Current: {currentMagnitude:F1}, Diff: {difference:F2}, ZoomInput: {zoomInput:F3}");
        }
    }
    
    bool IsTouchOnJoystick(Vector2 touchPosition)
    {
        if (movementJoystick == null || movementJoystick.joystickBackground == null)
            return false;
        
        RectTransform joystickRect = movementJoystick.joystickBackground;
        return RectTransformUtility.RectangleContainsScreenPoint(joystickRect, touchPosition, null);
    }
    
    void HandleJoystickInput()
    {
        if (movementJoystick == null) return;
        
        Vector2 joystickInput = movementJoystick.GetInputVector();
        
        if (joystickInput.magnitude > 0.1f)
        {
            Debug.Log($"[Joystick] Input: ({joystickInput.x:F2}, {joystickInput.y:F2}), Mode: {(isVirtualTourMode ? "VirtualTour" : "TopView")}");
            
            if (isVirtualTourMode)
            {
                // Virtual Tour: Move in camera direction (full 3D)
                Vector3 forward = transform.forward;
                Vector3 right = transform.right;
                
                Vector3 moveDir = right * joystickInput.x * Time.deltaTime * movementSpeed * 1.5f;
                moveDir += forward * joystickInput.y * Time.deltaTime * movementSpeed * 1.5f;
                targetPosition += moveDir;
                
                Debug.Log($"[Joystick] VirtualTour moveDir: {moveDir}, newTarget: {targetPosition}");
            }
            else
            {
                // Top View: Move in world horizontal plane based on camera Y rotation
                // Use rotationY to determine horizontal forward/right directions
                Vector3 worldForward = Quaternion.Euler(0, rotationY, 0) * Vector3.forward;
                Vector3 worldRight = Quaternion.Euler(0, rotationY, 0) * Vector3.right;
                
                Vector3 moveDir = worldRight * joystickInput.x * Time.deltaTime * movementSpeed * 1.5f;
                moveDir += worldForward * joystickInput.y * Time.deltaTime * movementSpeed * 1.5f;
                
                // Ensure movement stays on horizontal plane (Y doesn't change)
                moveDir.y = 0;
                
                targetPosition += moveDir;
                
                Debug.Log($"[Joystick] TopView rotY: {rotationY:F1}, worldForward: {worldForward}, worldRight: {worldRight}, moveDir: {moveDir}, newTarget: {targetPosition}");
            }
        }
    }
    
    void UpdateCameraTransform()
    {
        // Handle zoom - HIGH SPEED
        if (Mathf.Abs(zoomInput) > 0.0001f)
        {
            Debug.Log($"[Zoom] Processing zoomInput: {zoomInput}, Mode: {(isVirtualTourMode ? "VirtualTour" : "TopView")}");
            
            if (isVirtualTourMode)
            {
                // Virtual tour: adjust field of view
                float oldFOV = targetFOV;
                targetFOV -= zoomInput * zoomSpeed * 0.1f;
                targetFOV = Mathf.Clamp(targetFOV, 20f, 90f);
                Debug.Log($"[Zoom] Virtual Tour FOV: {oldFOV:F1} → {targetFOV:F1}, Current: {cam.fieldOfView:F1}");
            }
            else
            {
                // Top view: adjust orthographic size for zoom - ULTRA HIGH SPEED
                if (cam.orthographic)
                {
                    float oldSize = targetOrthographicSize;
                    // Scale zoom based on current size to make it feel consistent across zoom levels
                    float sizeFactor = targetOrthographicSize / 5000f;
                    float zoomAmount = zoomInput * zoomSpeed * 20f * Mathf.Max(sizeFactor, 0.2f);
                    
                    targetOrthographicSize -= zoomAmount;
                    targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 10f, 15000f);
                    
                    Debug.Log($"[Zoom] Top View Size: {oldSize:F1} → {targetOrthographicSize:F1}, Current: {cam.orthographicSize:F1}, ZoomAmount: {zoomAmount:F1}");
                }
            }
        }
        
        // Detect large position changes
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        // Smooth camera transitions
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        
        if (isVirtualTourMode)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * smoothSpeed);
        }
        else if (cam.orthographic)
        {
            // Always ensure targetOrthographicSize is initialized
            if (targetOrthographicSize <= 0) targetOrthographicSize = cam.orthographicSize;
            
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthographicSize, Time.deltaTime * smoothSpeed);
        }
    }

    public void SwitchToVirtualTour()
    {
        isVirtualTourMode = true;
        
        if (cam == null)
            cam = GetComponent<Camera>();
        
        if (cam != null)
        {
            cam.orthographic = false;
            targetFOV = virtualTourFOV;
            cam.fieldOfView = virtualTourFOV;
        }
        
        targetPosition = virtualTourStartPosition;
        rotationX = virtualTourStartRotation.x;
        rotationY = virtualTourStartRotation.y;
        targetRotation = Quaternion.Euler(virtualTourStartRotation);
    }
    
    public void SwitchToTopView()
    {
        isVirtualTourMode = false;
        
        if (cam == null)
            cam = GetComponent<Camera>();
        
        if (cam != null)
        {
            cam.orthographic = true;
            targetOrthographicSize = topViewOrthographicSize;
            cam.orthographicSize = topViewOrthographicSize;
        }
        
        targetPosition = topViewPosition;
        rotationX = topViewRotation.x;
        rotationY = topViewRotation.y;
        targetRotation = Quaternion.Euler(topViewRotation);
    }
    
    // TEMPORARY: Public methods for testing zoom with buttons
    public void TestZoomIn()
    {
        // Directly modify the camera for immediate zoom
        if (isVirtualTourMode)
        {
            targetFOV -= 5f;
            targetFOV = Mathf.Clamp(targetFOV, 20f, 90f);
            Debug.Log($"[TEST] Zoom IN - FOV: {targetFOV}");
        }
        else
        {
            if (cam.orthographic)
            {
                targetOrthographicSize -= 500f;
                targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 10f, 15000f);
                Debug.Log($"[TEST] Zoom IN - Orthographic Size: {targetOrthographicSize}");
            }
        }
    }
    
    public void TestZoomOut()
    {
        // Directly modify the camera for immediate zoom
        if (isVirtualTourMode)
        {
            targetFOV += 5f;
            targetFOV = Mathf.Clamp(targetFOV, 20f, 90f);
            Debug.Log($"[TEST] Zoom OUT - FOV: {targetFOV}");
        }
        else
        {
            if (cam.orthographic)
            {
                targetOrthographicSize += 500f;
                targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, 10f, 15000f);
                Debug.Log($"[TEST] Zoom OUT - Orthographic Size: {targetOrthographicSize}");
            }
        }
    }
}