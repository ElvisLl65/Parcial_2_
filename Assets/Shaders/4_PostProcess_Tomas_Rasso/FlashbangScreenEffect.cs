using UnityEngine;

[ExecuteInEditMode]
public class FlashbangScreenEffect : MonoBehaviour
{
    [Header("Material de Flashbang")]
    [Tooltip("Arrastrá acá el material M_FlashbangPostProcess")]
    public Material flashMaterial;

    [Header("Control en Runtime")]
    [Range(0f, 1f)]
    public float flashIntensity = 0f;

    [Tooltip("Tecla para simular que explota una flashbang")]
    public KeyCode triggerKey = KeyCode.Q;

    [Tooltip("Qué tan rápido se recupera la vista después del destello")]
    public float recoverySpeed = 0.45f;

    private static readonly int ScreenTexID = Shader.PropertyToID("_ScreenTex");
    private static readonly int FlashIntensityID = Shader.PropertyToID("FlashIntensity");
    private static readonly int FlashIntensityAltID = Shader.PropertyToID("_FlashIntensity");

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

        // Al apretar la tecla Q (o la tecla configurada), activa el destello al 100%
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(triggerKey))
        {
            TriggerFlash();
        }

        // Recuperación gradual de la vista
        if (Application.isPlaying && flashIntensity > 0f)
        {
            flashIntensity = Mathf.MoveTowards(flashIntensity, 0f, Time.deltaTime * recoverySpeed);
        }
    }

    public void TriggerFlash()
    {
        flashIntensity = 1f;
    }

    public void SetupCameraHook()
    {
        // Si este GameObject no tiene Camera (como en Sub3), se engancha a las cámaras activas
        if (GetComponent<Camera>() == null)
        {
            HookCamera(GameObject.FindGameObjectWithTag("SecondCamera"));
            if (Camera.main != null)
            {
                HookCamera(Camera.main.gameObject);
            }
        }
    }

    private void HookCamera(GameObject camObj)
    {
        if (camObj != null)
        {
            FlashbangCameraHook hook = camObj.GetComponent<FlashbangCameraHook>();
            if (hook == null)
            {
                hook = camObj.AddComponent<FlashbangCameraHook>();
            }
            hook.flashEffect = this;
        }
    }

    public void RenderFlash(RenderTexture source, RenderTexture destination)
    {
        if (enabled && gameObject.activeInHierarchy && flashMaterial != null && flashIntensity > 0.001f)
        {
            flashMaterial.SetTexture(ScreenTexID, source);
            flashMaterial.SetTexture("_MainTex", source);
            flashMaterial.SetFloat(FlashIntensityID, flashIntensity);
            flashMaterial.SetFloat(FlashIntensityAltID, flashIntensity);
            Graphics.Blit(source, destination, flashMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderFlash(source, destination);
    }

    void OnDisable()
    {
        if (flashMaterial != null)
        {
            flashMaterial.SetFloat(FlashIntensityID, 0f);
            flashMaterial.SetFloat(FlashIntensityAltID, 0f);
        }
    }
}

public class FlashbangCameraHook : MonoBehaviour
{
    public FlashbangScreenEffect flashEffect;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (flashEffect != null && flashEffect.enabled && flashEffect.gameObject.activeInHierarchy && flashEffect.flashIntensity > 0.001f)
        {
            flashEffect.RenderFlash(source, destination);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
