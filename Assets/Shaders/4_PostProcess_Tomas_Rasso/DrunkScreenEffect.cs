using UnityEngine;

[ExecuteInEditMode]
public class DrunkScreenEffect : MonoBehaviour
{
    [Header("Material de Mareo / Borracho")]
    [Tooltip("Arrastrá acá el material M_DrunkPostProcess")]
    public Material drunkMaterial;

    [Header("Ajustes en Tiempo Real")]
    [Range(0f, 0.15f)]
    public float drunkIntensity = 0.03f;

    public float zigzagSpeed = 3.5f;
    public float waveFrequency = 6f;

    private static readonly int ScreenTexID = Shader.PropertyToID("_ScreenTex");
    private static readonly int DrunkIntensityID = Shader.PropertyToID("DrunkIntensity");
    private static readonly int ZigzagSpeedID = Shader.PropertyToID("ZigzagSpeed");
    private static readonly int WaveFrequencyID = Shader.PropertyToID("WaveFrequency");

    void OnEnable()
    {
        SetupCameraHook();
    }

    void Start()
    {
        SetupCameraHook();
    }

    void Update()
    {
        SetupCameraHook();
    }

    public void SetupCameraHook()
    {
        // Si este GameObject no tiene Camera (como en SubEscena_SinShaders), se engancha a la cámara activa
        if (GetComponent<Camera>() == null)
        {
            GameObject camObj = GameObject.FindGameObjectWithTag("SecondCamera");
            if (camObj == null && Camera.main != null)
            {
                camObj = Camera.main.gameObject;
            }

            if (camObj != null)
            {
                DrunkCameraHook hook = camObj.GetComponent<DrunkCameraHook>();
                if (hook == null)
                {
                    hook = camObj.AddComponent<DrunkCameraHook>();
                }
                hook.drunkEffect = this;
            }
        }
    }

    public void RenderDrunk(RenderTexture source, RenderTexture destination)
    {
        if (enabled && gameObject.activeInHierarchy && drunkMaterial != null && drunkIntensity > 0.0001f)
        {
            drunkMaterial.SetTexture(ScreenTexID, source);
            drunkMaterial.SetTexture("_MainTex", source);
            drunkMaterial.SetFloat(DrunkIntensityID, drunkIntensity);
            drunkMaterial.SetFloat(ZigzagSpeedID, zigzagSpeed);
            drunkMaterial.SetFloat(WaveFrequencyID, waveFrequency);
            Graphics.Blit(source, destination, drunkMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderDrunk(source, destination);
    }
}

public class DrunkCameraHook : MonoBehaviour
{
    public DrunkScreenEffect drunkEffect;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (drunkEffect != null && drunkEffect.enabled && drunkEffect.gameObject.activeInHierarchy)
        {
            drunkEffect.RenderDrunk(source, destination);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
