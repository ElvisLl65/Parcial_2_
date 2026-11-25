using System.Collections;
using UnityEngine;

public class EasyFPSFirstPersonViewer : MonoBehaviour
{
    [Header("Configuración Primera Persona")]
    [Tooltip("Arma de Easy FPS a equipar (NewGun_auto o NewGun_semi)")]
    public string weaponName = "NewGun_auto";

    [Tooltip("Mantener siempre apuntando con el arma como en la foto")]
    public bool keepAiming = true;

    [Tooltip("Ocultar la mira / retícula en pantalla")]
    public bool hideCrosshair = true;

    [Tooltip("Desactivar todos los sonidos del juego")]
    public bool disableAllSounds = true;

    [Tooltip("Ocultar el conteo de balas abajo a la izquierda")]
    public bool hideBulletCounter = true;

    [Header("Efecto de Sangre (Post Process)")]
    [Tooltip("Material del efecto de sangre (M_BloodPostProcess)")]
    public Material bloodMaterial;

    [Range(0f, 1f)]
    [Tooltip("Intensidad base del efecto de sangre")]
    public float bloodIntensity = 1f;

    [Range(0f, 1f)]
    [Tooltip("Radio de apertura del centro limpio")]
    public float bloodRadius = 0.3f;

    [Range(0f, 1f)]
    [Tooltip("Borde / transición de la sangre hacia los bordes")]
    public float bloodEdge = 0.6f;

    [Range(0f, 1f)]
    [Tooltip("Fuerza de salpicadura de ruido en la sangre")]
    public float splatterStrength = 0.35f;

    [Tooltip("Color de la sangre")]
    public Color bloodColor = new Color(0.5882353f, 0f, 0f, 1f);

    [Header("Efecto de Curación (Post Process)")]
    [Tooltip("Material del efecto de curación (M_HealPostProcess)")]
    public Material healMaterial;

    private GunInventory inventory;
    private GunScript currentGunScript;
    private MouseLookScript mouseLook;
    private Rigidbody playerRb;
    private PlayerMovementScript playerMovement;
    private HealScreenEffect healEffect;

    void Awake()
    {
        // 1. Configurar Player y físicas para que no caiga ni se mueva
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerRb = playerObj.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = true;
                playerRb.useGravity = false;
            }

            playerMovement = playerObj.GetComponent<PlayerMovementScript>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            MeshRenderer mr = playerObj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = false;
            }
        }

        // 2. Ocultar contador de FPS en pantalla
        if (mouseLook == null)
            mouseLook = FindObjectOfType<MouseLookScript>();
        if (mouseLook != null)
        {
            mouseLook.showFps = false;
        }

        // 3. Configurar inventario para NewGun_auto
        if (inventory == null)
            inventory = FindObjectOfType<GunInventory>();

        if (inventory != null)
        {
            inventory.gunsIHave.Clear();
            inventory.gunsIHave.Add(weaponName);

            // Spawnear el arma de inmediato
            inventory.StopAllCoroutines();
            StartCoroutine(SpawnImmediately());
        }

        // 4. Silenciar todo el audio global
        if (disableAllSounds)
        {
            SilenceAllAudio();
        }

        // 5. Ocultar el conteo de balas de la pantalla
        if (hideBulletCounter)
        {
            HideBulletsHUD();
        }

        // 6. Configurar hooks de post-procesamiento en la cámara
        SetupCameraHooks();
        ApplyMaterialParameters();
    }

    void Start()
    {
        SetupCameraHooks();
        ApplyMaterialParameters();
    }

    IEnumerator SpawnImmediately()
    {
        yield return null;
        if (inventory == null)
            inventory = FindObjectOfType<GunInventory>();

        if (inventory != null && inventory.currentGun == null)
        {
            inventory.StartCoroutine("Spawn", 0);
        }

        if (disableAllSounds)
        {
            SilenceAllAudio();
        }

        if (hideBulletCounter)
        {
            HideBulletsHUD();
        }

        SetupCameraHooks();
        ApplyMaterialParameters();
    }

    void Update()
    {
        ApplyAimAndCrosshairSettings();

        if (disableAllSounds)
        {
            SilenceAllAudio();
        }

        if (hideBulletCounter)
        {
            HideBulletsHUD();
        }

        SetupCameraHooks();
        ApplyMaterialParameters();
    }

    void LateUpdate()
    {
        ApplyAimAndCrosshairSettings();

        if (hideBulletCounter)
        {
            HideBulletsHUD();
        }

        ApplyMaterialParameters();
    }

    public void SetupCameraHooks()
    {
        GameObject targetCamObj = GameObject.FindGameObjectWithTag("SecondCamera");
        if (targetCamObj == null && Camera.main != null)
        {
            targetCamObj = Camera.main.gameObject;
        }

        if (targetCamObj != null)
        {
            // Hook de sangre
            BloodCameraHook bloodHook = targetCamObj.GetComponent<BloodCameraHook>();
            if (bloodHook == null)
            {
                bloodHook = targetCamObj.AddComponent<BloodCameraHook>();
            }
            bloodHook.viewer = this;

            // Hook de curación (HealScreenEffect)
            healEffect = targetCamObj.GetComponent<HealScreenEffect>();
            if (healEffect == null)
            {
                healEffect = targetCamObj.AddComponent<HealScreenEffect>();
            }
            if (healEffect.healMaterial == null && healMaterial != null)
            {
                healEffect.healMaterial = healMaterial;
            }
            if (healEffect.bloodMaterial == null && bloodMaterial != null)
            {
                healEffect.bloodMaterial = bloodMaterial;
            }
        }
    }

    public void ApplyMaterialParameters()
    {
        if (bloodMaterial != null)
        {
            // La sangre se desvanece suavemente mientras se presiona R (curación)
            float effectiveBlood = bloodIntensity;
            if (healEffect != null)
            {
                effectiveBlood *= Mathf.Clamp01(1f - healEffect.currentIntensity);
            }

            bloodMaterial.SetFloat("_BloodIntensity", effectiveBlood);
            bloodMaterial.SetFloat("_BloodRadius", bloodRadius);
            bloodMaterial.SetFloat("_BloodEdge", bloodEdge);
            bloodMaterial.SetFloat("_SplatterStrength", splatterStrength);
            bloodMaterial.SetColor("_BloodColor", bloodColor);
            bloodMaterial.SetVector("_Vector0", new Vector4(0.5f, 0.5f, 0f, 0f));
        }
    }

    void OnEnable()
    {
        ApplyMaterialParameters();
    }

    void OnDisable()
    {
        if (bloodMaterial != null)
        {
            bloodMaterial.SetFloat("_BloodIntensity", 0f);
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (bloodMaterial != null)
        {
            ApplyMaterialParameters();
            bloodMaterial.SetTexture("_TextureSample0", src);
            Graphics.Blit(src, dest, bloodMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    private void SilenceAllAudio()
    {
        AudioListener.volume = 0f;
        AudioListener.pause = true;

        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] != null)
            {
                sources[i].mute = true;
                sources[i].volume = 0f;
                sources[i].playOnAwake = false;
                if (sources[i].isPlaying)
                {
                    sources[i].Stop();
                }
            }
        }
    }

    private void HideBulletsHUD()
    {
        GameObject hud = GameObject.Find("HUD_bullets");
        if (hud != null)
        {
            MeshRenderer mr = hud.GetComponent<MeshRenderer>();
            if (mr != null && mr.enabled)
            {
                mr.enabled = false;
            }
        }
    }

    private void ApplyAimAndCrosshairSettings()
    {
        if (currentGunScript == null)
        {
            currentGunScript = FindObjectOfType<GunScript>();
        }

        if (currentGunScript != null)
        {
            // 1. Ocultar la mira / retícula ("la mira no la hagas")
            if (hideCrosshair)
            {
                currentGunScript.horizontal_crosshair = null;
                currentGunScript.vertical_crosshair = null;
            }

            // 2. Mantener la pose de apuntar en primera persona (ADS como en la foto)
            if (keepAiming)
            {
                currentGunScript.restPlacePosition = currentGunScript.aimPlacePosition;
                currentGunScript.currentGunPosition = currentGunScript.aimPlacePosition;

                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    mainCam.fieldOfView = currentGunScript.cameraZoomRatio_aiming;
                }

                GameObject secCamObj = GameObject.FindGameObjectWithTag("SecondCamera");
                if (secCamObj != null)
                {
                    Camera secCam = secCamObj.GetComponent<Camera>();
                    if (secCam != null)
                    {
                        secCam.fieldOfView = currentGunScript.secondCameraZoomRatio_aiming;
                    }
                }
            }
        }
    }
}

public class BloodCameraHook : MonoBehaviour
{
    public EasyFPSFirstPersonViewer viewer;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (viewer != null && viewer.enabled && viewer.gameObject.activeInHierarchy && viewer.bloodMaterial != null)
        {
            viewer.ApplyMaterialParameters();
            viewer.bloodMaterial.SetTexture("_TextureSample0", src);
            Graphics.Blit(src, dest, viewer.bloodMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
