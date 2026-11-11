using UnityEngine;
using UnityEngine.UI;

namespace DisRenTexDrawMesh
{
    [RequireComponent(typeof(Camera))]
    public class BinocularCameraEffect : MonoBehaviour
    {
        [Header("Material de Binoculares (M_Binoculars -> Custom/Binoculars_Distance)")]
        [SerializeField] private MeshRenderer binocularQuadRenderer = null;
        [SerializeField] private Material binocularMaterial = null;

        [Header("Parámetros de los Binoculares")]
        [Range(0.15f, 0.50f)] public float binocularRadius = 0.31f;
        [Range(0.005f, 0.12f)] public float binocularSoftness = 0.04f;
        [Range(0.05f, 0.35f)] public float eyeSeparation = 0.19f;
        [SerializeField] private float quadDistance = 0.5f;
        [SerializeField] private float flashDecaySpeed = 4.5f;

        private Camera _cam;
        private MaterialPropertyBlock _quadPropBlock;
        private Image _flashImage;
        private float _flashAmount;

        private static readonly int LensRadiusID = Shader.PropertyToID("_LensRadius");
        private static readonly int LensSoftnessID = Shader.PropertyToID("_LensSoftness");
        private static readonly int EyeSeparationID = Shader.PropertyToID("_EyeSeparation");

        public const int BinocularQuadLayer = 30;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _quadPropBlock = new MaterialPropertyBlock();

            SetupBinocularQuad();
            BuildFlashOverlay();
        }

        public void TriggerFlash()
        {
            _flashAmount = 0.85f;
        }

        public bool IsScreenPointInsideBinoculars(Vector2 screenPoint)
        {
            if (Screen.width <= 0 || Screen.height <= 0)
                return false;

            Vector2 viewport = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
            return IsViewportPointInsideBinoculars(viewport);
        }

        public bool IsViewportPointInsideBinoculars(Vector2 viewport)
        {
            if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
                return false;

            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            Vector2 p = new Vector2((viewport.x - 0.5f) * aspect, viewport.y - 0.5f);

            float dLeft = Vector2.Distance(p, new Vector2(-eyeSeparation, 0f));
            float dRight = Vector2.Distance(p, new Vector2(eyeSeparation, 0f));
            return Mathf.Min(dLeft, dRight) <= binocularRadius;
        }

        private void SetupBinocularQuad()
        {
            if (binocularQuadRenderer == null)
            {
                Transform child = transform.Find("BinocularMaskQuad");
                if (child != null)
                    binocularQuadRenderer = child.GetComponent<MeshRenderer>();
            }

            if (binocularQuadRenderer != null)
            {
                binocularQuadRenderer.gameObject.layer = BinocularQuadLayer;
                if (binocularMaterial != null)
                    binocularQuadRenderer.sharedMaterial = binocularMaterial;
                binocularQuadRenderer.enabled = true;
            }
        }

        private void BuildFlashOverlay()
        {
            GameObject canvasGo = new GameObject("BinocularFlash_Canvas");
            canvasGo.transform.SetParent(transform, false);

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 6;

            GameObject flashGo = new GameObject("BinocularFlashImage");
            flashGo.transform.SetParent(canvasGo.transform, false);
            RectTransform flashRect = flashGo.AddComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;

            _flashImage = flashGo.AddComponent<Image>();
            _flashImage.color = new Color(1f, 1f, 1f, 0f);
            _flashImage.raycastTarget = false;
        }

        private void Update()
        {
            if (_flashAmount > 0f)
            {
                _flashAmount = Mathf.Max(0f, _flashAmount - Time.deltaTime * flashDecaySpeed);
                if (_flashImage != null)
                    _flashImage.color = new Color(1f, 1f, 1f, _flashAmount);
            }
            else if (_flashImage != null && _flashImage.color.a > 0f)
            {
                _flashImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        private void LateUpdate()
        {
            if (binocularQuadRenderer == null || _cam == null)
                return;

            float frustumHeight = 2f * quadDistance * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * _cam.aspect;

            Transform qTransform = binocularQuadRenderer.transform;
            qTransform.localPosition = new Vector3(0f, 0f, quadDistance);
            qTransform.localRotation = Quaternion.identity;
            qTransform.localScale = new Vector3(frustumWidth, frustumHeight, 1f);

            binocularQuadRenderer.GetPropertyBlock(_quadPropBlock);
            _quadPropBlock.SetFloat(LensRadiusID, binocularRadius);
            _quadPropBlock.SetFloat(LensSoftnessID, binocularSoftness);
            _quadPropBlock.SetFloat(EyeSeparationID, eyeSeparation);
            binocularQuadRenderer.SetPropertyBlock(_quadPropBlock);
        }
    }
}
