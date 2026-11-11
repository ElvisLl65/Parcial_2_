using UnityEngine;

[ExecuteAlways]
public class DistanceController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Material cabinMaterial;

    [Header("Ajustes del Agujero")]
    [Range(0.5f, 10f)] public float holeRadius = 3.0f;
    [Range(0.01f, 2f)] public float holeSoftness = 0.5f;

    private static readonly int CharPosID = Shader.PropertyToID("_CharacterPos");
    private static readonly int CamPosID = Shader.PropertyToID("_CameraPos");
    private static readonly int HoleRadID = Shader.PropertyToID("HoleRad");
    private static readonly int HoleSoftID = Shader.PropertyToID("HoleSoft");

    void Update()
    {
        if (cabinMaterial == null) return;

        // Detecta la cámara: si orbitas en la ventana Scene usa esa; si estás en Game usa Camera.main
        Vector3 camPos = Vector3.zero;
        if (Camera.current != null)
        {
            camPos = Camera.current.transform.position;
        }
        else if (Camera.main != null)
        {
            camPos = Camera.main.transform.position;
        }

        // Posición del torso de Man_04
        Vector3 charPos = transform.position + Vector3.up * 1.0f;

        // Mandar los datos al material en tiempo real
        cabinMaterial.SetVector(CharPosID, charPos);
        cabinMaterial.SetVector(CamPosID, camPos);
        cabinMaterial.SetFloat(HoleRadID, holeRadius);
        cabinMaterial.SetFloat(HoleSoftID, holeSoftness);
    }
}