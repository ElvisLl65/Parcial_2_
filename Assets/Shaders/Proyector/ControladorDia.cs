using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorDia : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    [Range(0f, 24f)] public float horaDelDia = 12f; // Podés mover esto con un slider en el Inspector
    [SerializeField] private float velocidadTiempo = 1f;

    [Header("Componentes")]
    [SerializeField] private Projector componenteProyector; // Si usás el built-in Projector
    private Material _materialDinamico;

    private void Start()
    {
        if (componenteProyector != null)
        {
            // Creamos una instancia del material para poder editar sus propiedades en runtime
            _materialDinamico = componenteProyector.material;
        }
    }

    private void Update()
    {
        // Hace que el tiempo pase automáticamente en base a los segundos reales
        horaDelDia += Time.deltaTime * velocidadTiempo;
        if (horaDelDia >= 24f) horaDelDia = 0f;

        // 1. Simular la inclinación del sol (Rotación del proyector)
        // Mapeamos las horas para que el proyector se incline y estire las sombras de forma realista
        float anguloInclinacion = Mathf.Lerp(15f, 75f, Mathf.PingPong(horaDelDia / 12f, 1f));
        transform.rotation = Quaternion.Euler(anguloInclinacion, -45f, 0f);

        // 2. Simular el color de la luz según la hora actual
        if (_materialDinamico != null)
        {
            Color colorActual = Color.black; // De noche está apagado

            if (horaDelDia > 6f && horaDelDia <= 11f) // Mañana: Luz fría/azulada suave
            {
                colorActual = new Color(0.6f, 0.75f, 1f);
            }
            else if (horaDelDia > 11f && horaDelDia <= 17f) // Mediodía: Luz blanca/cálida potente
            {
                colorActual = Color.white;
            }
            else if (horaDelDia > 17f && horaDelDia <= 20f) // Atardecer: El naranja cálido de tu referencia
            {
                colorActual = new Color(1f, 0.45f, 0.1f);
            }

            // Inyectamos el color directamente a la propiedad de Amplify
            _materialDinamico.SetColor("_ColorLuz", colorActual);
        }
    }
}