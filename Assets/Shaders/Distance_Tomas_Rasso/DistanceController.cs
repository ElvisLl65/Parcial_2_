using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Updates the Distance cutout shader material with the character position,
/// active camera position, and circular cutout parameters in Edit and Play modes.
/// </summary>
[ExecuteAlways]
public class DistanceController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Material cabinMaterial;
    [SerializeField] private Camera targetCamera;

    [Header("Ajustes del Agujero")]
    [Range(0.5f, 10f)] public float holeRadius = 2.2f;
    [Range(0.01f, 3f)] public float holeSoftness = 0.4f;
    [SerializeField] private float characterHeightOffset = 1.0f;

    private static readonly int CharPosID = Shader.PropertyToID("_CharacterPos");
    private static readonly int CamPosID = Shader.PropertyToID("_CameraPos");
    private static readonly int HoleRadID = Shader.PropertyToID("_HoleRad");
    private static readonly int HoleSoftID = Shader.PropertyToID("_HoleSoft");

    private void OnEnable()
    {
        Camera.onPreCull += HandleCameraPreCull;
    }

    private void OnDisable()
    {
        Camera.onPreCull -= HandleCameraPreCull;
    }

    private void LateUpdate()
    {
        Camera cam = ResolveFallbackCamera();
        if (cam != null)
        {
            ApplyShaderProperties(cam.transform.position);
        }
    }

    private void HandleCameraPreCull(Camera cam)
    {
        if (cam == null || cam.cameraType == CameraType.Preview || cam.cameraType == CameraType.Reflection)
            return;

        if (Application.isPlaying && targetCamera != null && cam != targetCamera && cam.cameraType != CameraType.SceneView)
            return;

        ApplyShaderProperties(cam.transform.position);
    }

    private Camera ResolveFallbackCamera()
    {
        if (Application.isPlaying)
        {
            return targetCamera != null ? targetCamera : Camera.main;
        }

#if UNITY_EDITOR
        if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            return SceneView.lastActiveSceneView.camera;
#endif

        return targetCamera != null ? targetCamera : Camera.main;
    }

    private void ApplyShaderProperties(Vector3 camPos)
    {
        if (cabinMaterial == null)
            return;

        Vector3 charPos = transform.position + Vector3.up * characterHeightOffset;

        cabinMaterial.SetVector(CharPosID, charPos);
        cabinMaterial.SetVector(CamPosID, camPos);
        cabinMaterial.SetFloat(HoleRadID, holeRadius);
        cabinMaterial.SetFloat(HoleSoftID, holeSoftness);
    }
}
