using UnityEngine;

public class DistanceHoleController : MonoBehaviour
{
    public Transform player;
    public Material wallMaterial;
    public float heightOffset = 0.9f;

    void Update()
    {
        if (player != null && wallMaterial != null)
        {
            Vector3 position = player.position;
            position.y += heightOffset;

            wallMaterial.SetVector("_PlayerPosition", position);
        }
    }
}