using UnityEngine;

public class RotarCarta : MonoBehaviour
{
    public float amplitud = 60f;
    public float frecuencia = 1.2f;

    Quaternion rotacionInicial;

    void Start()
    {
        rotacionInicial = transform.rotation;
    }

    void Update()
    {
        float angulo = Mathf.Sin(Time.time * frecuencia) * amplitud;
        transform.rotation = Quaternion.Euler(0f, angulo, 0f) * rotacionInicial;
    }
}
