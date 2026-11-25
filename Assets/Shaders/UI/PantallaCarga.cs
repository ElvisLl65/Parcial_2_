using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PantallaCarga : MonoBehaviour
{
    public Slider barraProgreso;
    public TextMeshProUGUI textoPorcentaje;
    public float velocidadCarga = 0.3f;

    private float progreso = 0f;

    void OnEnable()
    {
        progreso = 0f;
        if (barraProgreso != null) barraProgreso.value = 0f;
    }

    void Update()
    {
        if (progreso < 1f)
        {
            progreso += Time.deltaTime * velocidadCarga;
            if (progreso > 1f) progreso = 1f;

            if (barraProgreso != null)
                barraProgreso.value = progreso;

            if (textoPorcentaje != null)
                textoPorcentaje.text = "CARGANDO... " + Mathf.RoundToInt(progreso * 100f) + "%";
        }
    }
}
