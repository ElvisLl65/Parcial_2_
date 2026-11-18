using UnityEngine;

public class RotarCarta : MonoBehaviour
{
    public float velocidad = 40f;

    void Update()
    {
        transform.Rotate(0f, velocidad * Time.deltaTime, 0f, Space.World);
    }
}
