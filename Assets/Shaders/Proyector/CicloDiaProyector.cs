using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CicloDiaProyector : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    [Tooltip("Velocidad a la que pasan las horas del día")]
    public float velocidadTiempo = 1f;
    [Range(0f, 24f)]
    public float horaDelDia = 8f; // Empieza a las 8 de la mañana

    [Header("Colores por Momento del Día")]
    public Color colorManana = new Color(1f, 0.75f, 0.4f);   // Naranja suave
    public Color colorMediodia = new Color(1f, 0.98f, 0.9f); // Blanco brillante
    public Color colorTarde = new Color(1f, 0.45f, 0.2f);    // Naranja rojizo intenso
    public Color colorNoche = new Color(0.15f, 0.25f, 0.6f); // Azul luna

    [Header("Intensidades")]
    public float intensidadDia = 2.5f;
    public float intensidadNoche = 0.5f;

    private Material materialProyector;

    void Start()
    {
        // Obtenemos automáticamente el material que tiene asignado este Proyector
        Projector proj = GetComponent<Projector>();
        if (proj != null)
        {
            materialProyector = proj.material;
        }
    }

    void Update()
    {
        // 1. Avanzar el reloj de 0 a 24 horas en bucle
        horaDelDia += Time.deltaTime * velocidadTiempo;
        if (horaDelDia >= 24f) horaDelDia = 0f;

        // 2. Rotar el proyector simulando el movimiento del sol en el cielo
        // A las 6am entra muy inclinado, a las 12pm apunta casi vertical, a las 18pm vuelve a inclinarse
        float anguloX = Mathf.Lerp(25f, 85f, Mathf.Sin((horaDelDia / 24f) * Mathf.PI));
        float anguloY = Mathf.Lerp(-45f, 45f, horaDelDia / 24f);
        transform.rotation = Quaternion.Euler(anguloX, anguloY, 0f);

        if (materialProyector == null) return;

        // 3. Cambiar el color y la intensidad según la hora
        Color colorActual;
        float intensidadActual;

        if (horaDelDia >= 6f && horaDelDia < 12f) // Mañana (6:00 a 12:00)
        {
            float t = (horaDelDia - 6f) / 6f;
            colorActual = Color.Lerp(colorManana, colorMediodia, t);
            intensidadActual = Mathf.Lerp(1.5f, intensidadDia, t);
        }
        else if (horaDelDia >= 12f && horaDelDia < 18f) // Tarde (12:00 a 18:00)
        {
            float t = (horaDelDia - 12f) / 6f;
            colorActual = Color.Lerp(colorMediodia, colorTarde, t);
            intensidadActual = Mathf.Lerp(intensidadDia, 1.5f, t);
        }
        else if (horaDelDia >= 18f && horaDelDia < 21f) // Anochecer (18:00 a 21:00)
        {
            float t = (horaDelDia - 18f) / 3f;
            colorActual = Color.Lerp(colorTarde, colorNoche, t);
            intensidadActual = Mathf.Lerp(1.5f, intensidadNoche, t);
        }
        else // Noche y Madrugada (21:00 a 6:00)
        {
            colorActual = colorNoche;
            intensidadActual = intensidadNoche;
        }

        // 4. Enviar los datos al Shader de Amplify
        materialProyector.SetColor("_ColordeLuz", colorActual);
        materialProyector.SetFloat("_Intensidad", intensidadActual);
    }
}