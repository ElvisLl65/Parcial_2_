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

    private GunInventory inventory;
    private GunScript currentGunScript;
    private MouseLookScript mouseLook;
    private Rigidbody playerRb;
    private PlayerMovementScript playerMovement;

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
    }

    void LateUpdate()
    {
        ApplyAimAndCrosshairSettings();

        if (hideBulletCounter)
        {
            HideBulletsHUD();
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
