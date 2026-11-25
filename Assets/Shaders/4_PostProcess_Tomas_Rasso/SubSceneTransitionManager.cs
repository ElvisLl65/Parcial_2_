using System.Collections;
using UnityEngine;

public class SubSceneTransitionManager : MonoBehaviour
{
    [Header("Configuración de Transición")]
    [Tooltip("Tecla para ciclar entre escenas")]
    public KeyCode transitionKey = KeyCode.Space;

    [Tooltip("Duración del fundido de transición (en segundos)")]
    public float transitionDuration = 0.5f;

    [Header("Referencias de Escenas")]
    [Tooltip("Escena principal (con sangre y curación)")]
    public GameObject mainSceneContainer;

    [Tooltip("Sub2 (con DrunkPostProcess)")]
    public GameObject sub2Container;

    [Tooltip("Sub3")]
    public GameObject sub3Container;

    [Header("Estado Actual")]
    [Tooltip("0 = Principal (Sangre/Heal), 1 = Sub2 (Drunk), 2 = Sub3")]
    public int currentSceneIndex = 0;

    private bool _isTransitioning = false;
    private float _fadeAlpha = 0f;
    private Texture2D _fadeTexture;

    void Awake()
    {
        // Crear textura de 1x1 para el fundido a negro
        _fadeTexture = new Texture2D(1, 1);
        _fadeTexture.SetPixel(0, 0, Color.black);
        _fadeTexture.Apply();

        ApplySceneState();
    }

    void Update()
    {
        // Al apretar ESPACIO, cicla: Principal -> Sub2 -> Sub3 -> Principal
        if (Input.GetKeyDown(transitionKey) && !_isTransitioning)
        {
            StartCoroutine(DoSceneTransition());
        }

        // Asegurar que al apretar Q en Sub3 detone el flashbang
        if (Input.GetKeyDown(KeyCode.Q) && currentSceneIndex == 2)
        {
            FlashbangScreenEffect flash = sub3Container != null ? sub3Container.GetComponentInChildren<FlashbangScreenEffect>(true) : null;
            if (flash != null)
            {
                flash.TriggerFlash();
            }
        }
    }

    IEnumerator DoSceneTransition()
    {
        _isTransitioning = true;

        // 1. Fundido a negro (Fade In)
        float timer = 0f;
        while (timer < transitionDuration * 0.5f)
        {
            timer += Time.deltaTime;
            _fadeAlpha = Mathf.Clamp01(timer / (transitionDuration * 0.5f));
            yield return null;
        }
        _fadeAlpha = 1f;

        // 2. Cambiar al siguiente índice de escena (0 -> 1 -> 2 -> 0)
        currentSceneIndex = (currentSceneIndex + 1) % 3;
        ApplySceneState();

        // 3. Fundido desde negro a la nueva escena (Fade Out)
        timer = 0f;
        while (timer < transitionDuration * 0.5f)
        {
            timer += Time.deltaTime;
            _fadeAlpha = Mathf.Clamp01(1f - (timer / (transitionDuration * 0.5f)));
            yield return null;
        }
        _fadeAlpha = 0f;

        _isTransitioning = false;
    }

    private void ApplySceneState()
    {
        bool isMain = (currentSceneIndex == 0);
        bool isSub2 = (currentSceneIndex == 1);
        bool isSub3 = (currentSceneIndex == 2);

        if (mainSceneContainer != null)
        {
            mainSceneContainer.SetActive(isMain);
        }
        if (sub2Container != null)
        {
            sub2Container.SetActive(isSub2);
        }
        if (sub3Container != null)
        {
            sub3Container.SetActive(isSub3);
        }

        // Control de efectos de la escena principal (sangre y curación)
        EasyFPSFirstPersonViewer viewer = FindObjectOfType<EasyFPSFirstPersonViewer>();
        if (viewer != null)
        {
            viewer.enabled = isMain;
            if (viewer.bloodMaterial != null)
            {
                viewer.bloodMaterial.SetFloat("_BloodIntensity", isMain ? viewer.bloodIntensity : 0f);
            }
        }

        HealScreenEffect heal = FindObjectOfType<HealScreenEffect>();
        if (heal != null)
        {
            heal.enabled = isMain;
        }

        // Limpiar flashbang al salir de Sub3
        FlashbangScreenEffect flash = sub3Container != null ? sub3Container.GetComponentInChildren<FlashbangScreenEffect>(true) : null;
        if (flash != null && !isSub3)
        {
            flash.flashIntensity = 0f;
            if (flash.flashMaterial != null)
            {
                flash.flashMaterial.SetFloat("FlashIntensity", 0f);
                flash.flashMaterial.SetFloat("_FlashIntensity", 0f);
            }
        }
    }

    void OnGUI()
    {
        if (_fadeAlpha > 0.001f && _fadeTexture != null)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, _fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _fadeTexture);
            GUI.color = oldColor;
        }
    }
}
