using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Joystick Components")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;
    
    [Header("Settings")]
    public float handleRange = 50f;
    public float deadZone = 0.1f;
    
    [Header("Output")]
    public Vector2 inputVector = Vector2.zero;
    
    private Vector2 joystickPosition = Vector2.zero;
    private bool isDragging = false;
    
    void Start()
    {
        if (joystickBackground == null)
            joystickBackground = GetComponent<RectTransform>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        OnDrag(eventData);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        inputVector = Vector2.zero;
        
        // Reset handle to center
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (joystickBackground == null) return;
        
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );
        
        // Normalize position based on handle range
        position = Vector2.ClampMagnitude(position, handleRange);
        joystickPosition = position;
        
        // Calculate input vector (-1 to 1)
        inputVector = position / handleRange;
        
        // Apply dead zone
        if (inputVector.magnitude < deadZone)
        {
            inputVector = Vector2.zero;
        }
        
        // Move the handle visually
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = position;
        }
    }
    
    public Vector2 GetInputVector()
    {
        return inputVector;
    }
    
    public float GetHorizontal()
    {
        return inputVector.x;
    }
    
    public float GetVertical()
    {
        return inputVector.y;
    }
}
