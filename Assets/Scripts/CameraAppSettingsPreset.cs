using UnityEngine;

[CreateAssetMenu(fileName = "NewCameraAppPreset", menuName = "Presets/Camera App Settings Preset")]
public class CameraAppSettingsPreset : ScriptableObject
{
    [Header("Camera Settings")]
    public float movementSpeed = 1000f;
    public float lookSensitivity = 8f;
    public float zoomSpeed = 100f;
    public float smoothSpeed = 10f;

    [Header("Top View Settings")]
    public Vector3 topViewPosition = new Vector3(-141.73f, 14627.48f, -1259.46f);
    public Vector3 topViewRotation = new Vector3(90, 0, 0);
    public float topViewOrthographicSize = 6997.486f;

    [Header("Virtual Tour Settings")]
    public Vector3 virtualTourStartPosition = new Vector3(-64.42599f, 4325.906f, -1695.566f);
    public Vector3 virtualTourStartRotation = new Vector3(5.192f, -179.347f, -0.001f);
    public float virtualTourFOV = 60f;

    [Header("App Features")]
    public bool joystickEnabled = true;
    public bool zoomButtonsEnabled = true;
    public bool touchIndicatorsEnabled = true;
    public bool movementGuideEnabled = false;
}
