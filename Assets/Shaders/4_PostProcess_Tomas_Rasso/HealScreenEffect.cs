using UnityEngine;

public class HealScreenEffect : MonoBehaviour
{
    [Header("Efecto de Curación (Post Process)")]
    [Tooltip("Arrastrá acá el material M_HealPostProcess")]
    public Material healMaterial;

    [Header("Interacción con Sangre")]
    [Tooltip("Material de sangre para desvanecerlo mientras te curás")]
    public Material bloodMaterial;

    [Header("Configuración de Tecla R")]
    [Tooltip("Tecla para activar el destello de curación")]
    public KeyCode healKey = KeyCode.R;

    [Tooltip("Velocidad con la que aparece el contorno amarillo al presionar R")]
    public float fadeInSpeed = 5f;

    [Tooltip("Velocidad con la que se desvanece suavemente al soltar R")]
    public float fadeOutSpeed = 2f;

    [Tooltip("Duración mínima del brillo si solo tocás R una vez")]
    public float minFlashDuration = 0.45f;

    [Header("Estado")]
    [Range(0f, 1f)]
    public float currentIntensity = 0f;

    private float _flashTimer = 0f;
    private static readonly int HealIntensityID = Shader.PropertyToID("_HealIntensity");
    private static readonly int AltHealIntensityID = Shader.PropertyToID("HealIntensity");
    private static readonly int BloodIntensityID = Shader.PropertyToID("_BloodIntensity");

    void Update()
    {
        // 1. Si presiona R una vez, asegura un destello mínimo visible
        if (Input.GetKeyDown(healKey))
        {
            _flashTimer = minFlashDuration;
        }

        // 2. Si mantiene apretada R, mantiene el temporizador activo
        if (Input.GetKey(healKey))
        {
            _flashTimer = Mathf.Max(_flashTimer, 0.05f);
        }

        // 3. Transición suave de intensidad
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            currentIntensity = Mathf.MoveTowards(currentIntensity, 1f, Time.deltaTime * fadeInSpeed);
        }
        else
        {
            currentIntensity = Mathf.MoveTowards(currentIntensity, 0f, Time.deltaTime * fadeOutSpeed);
        }

        // 4. Actualizar material de curación
        if (healMaterial != null)
        {
            healMaterial.SetFloat(HealIntensityID, currentIntensity);
            healMaterial.SetFloat(AltHealIntensityID, currentIntensity);
        }

        // 5. Desvanecer la sangre al curar (se va con R y vuelve al soltarla)
        if (bloodMaterial != null)
        {
            float bloodFade = 1f - currentIntensity;
            bloodMaterial.SetFloat(BloodIntensityID, bloodFade);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (healMaterial != null && currentIntensity > 0.001f)
        {
            healMaterial.SetTexture("_MainTex", source);
            healMaterial.SetTexture("_Maintex", source);
            healMaterial.SetFloat(HealIntensityID, currentIntensity);
            healMaterial.SetFloat(AltHealIntensityID, currentIntensity);
            Graphics.Blit(source, destination, healMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
