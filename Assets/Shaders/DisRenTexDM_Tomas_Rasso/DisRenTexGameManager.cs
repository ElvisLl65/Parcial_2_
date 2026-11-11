using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DisRenTexDrawMesh
{
    public class DisRenTexGameManager : MonoBehaviour
    {
        private struct MeshCatalogEntry
        {
            public Mesh mesh;
            public string displayName;
            public Vector3 centerOffset;
            public float normalizedScale;
            public float baseRadius;
        }

        private struct ColorPreset
        {
            public string name;
            public Color color;

            public ColorPreset(string name, Color color)
            {
                this.name = name;
                this.color = color;
            }
        }

        private struct FruitInstance
        {
            public int catalogIndex;
            public int colorIndex;
            public Vector3 worldPos;
            public Quaternion initialRotation;
            public float sizeMultiplier;
            public float spinSpeed;
            public float phaseOffset;
            public bool isTarget;
        }

        [Header("Materiales de los 3 Shaders")]
        [SerializeField] private Material fruitMaterial = null;         // M_InstancedItem (Custom/Instanced_Fruit)
        [SerializeField] private Material rtViewerMaterial = null;      // M_RenderTargetViewer (Custom/RT_Display)

        [Header("Referencias de Escena")]
        [SerializeField] private Camera mainCamera = null;
        [SerializeField] private BinocularCameraEffect binocularEffect = null;

        [Header("Catálogo de Frutas (PolyOne)")]
        [SerializeField] private Mesh[] fruitMeshes = null;
        [SerializeField] private string[] fruitNames = null;

        [Header("Tablero y Población (DrawMeshInstanced)")]
        [SerializeField] private int columns = 12;
        [SerializeField] private int rows = 7;
        [SerializeField] private Vector3 boardCenter = new Vector3(-4.0f, 0f, -10.5f);
        [SerializeField] private Vector2 boardSize = new Vector2(26f, 15.5f);

        [Header("Controles de Cámara Binocular")]
        [SerializeField] private float panSpeed = 11.5f;
        [SerializeField] private float minCamHeight = 7.5f;
        [SerializeField] private float maxCamHeight = 18.0f;
        [SerializeField] private float zoomSpeed = 4.5f;

        [Header("Render Textures (Solo lectura en Play)")]
        [SerializeField] private RenderTexture captureRenderTexture;
        [SerializeField] private RenderTexture targetPreviewRenderTexture;

        private readonly List<MeshCatalogEntry> _catalog = new List<MeshCatalogEntry>();
        private readonly List<FruitInstance> _fruits = new List<FruitInstance>();
        private List<int>[] _meshBuckets;

        private readonly Matrix4x4[] _batchMatrices = new Matrix4x4[1023];
        private readonly Vector4[] _batchColors = new Vector4[1023];
        private readonly float[] _batchSpinSpeeds = new float[1023];
        private readonly float[] _batchPhases = new float[1023];

        private readonly Matrix4x4[] _previewMatrix = new Matrix4x4[1];
        private readonly Vector4[] _previewColors = new Vector4[1];
        private readonly float[] _previewSpinSpeeds = new float[] { 1.8f };
        private readonly float[] _previewPhases = new float[] { 0f };

        private ColorPreset[] _colorPresets;
        private MaterialPropertyBlock _instancedPropBlock;
        private MaterialPropertyBlock _previewPropBlock;

        private Camera _captureCamera;
        private Camera _targetPreviewCamera;

        private int _targetCatalogIndex = -1;
        private int _targetColorIndex;
        private int _captureCount;
        private bool _hasCapture;
        private bool _isCapturing;
        private bool _isRtModalOpen;

        private float _camHeight = 14.21f;
        private Vector3 _dragStartMouse;
        private Vector3 _lastDragMouse;
        private bool _isDragging;
        private bool _pointerDownOnWorld;

        // UI references (solo Objetivo arriba a la izquierda + Modal Tab)
        private Font _uiFont;
        private Text _targetLabelText;
        private Text _scoreText;
        private Text _feedbackText;
        private RawImage _targetPreviewImage;
        private GameObject _rtModalObj;
        private RawImage _rtModalImage;
        private float _feedbackTimer;

        private const int PreviewLayer = 31;

        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int SpinSpeedID = Shader.PropertyToID("_SpinSpeed");
        private static readonly int PhaseOffsetID = Shader.PropertyToID("_PhaseOffset");
        private static readonly int PivotCenterID = Shader.PropertyToID("_PivotCenter");
        private static readonly int CapturedRTID = Shader.PropertyToID("_CapturedRT");

        private void Awake()
        {
            _instancedPropBlock = new MaterialPropertyBlock();
            _previewPropBlock = new MaterialPropertyBlock();

            _colorPresets = new[]
            {
                new ColorPreset("Natural", Color.white),
                new ColorPreset("Rojo", new Color(1.25f, 0.45f, 0.45f, 1f)),
                new ColorPreset("Dorado", new Color(1.25f, 1.00f, 0.42f, 1f)),
                new ColorPreset("Verde", new Color(0.55f, 1.25f, 0.55f, 1f)),
                new ColorPreset("Celeste", new Color(0.52f, 1.05f, 1.35f, 1f)),
                new ColorPreset("Violeta", new Color(0.95f, 0.55f, 1.30f, 1f)),
                new ColorPreset("Rosa", new Color(1.30f, 0.58f, 0.95f, 1f))
            };

            ResolveReferences();
            BuildCatalog();
            HideStaticFruitsInScene();
            SetupCamerasAndRenderTextures();
            BuildHUD();
        }

        private void Start()
        {
            if (_catalog.Count < 2 || fruitMaterial == null)
            {
                ShowFeedback("Faltan meshes de frutas o M_InstancedItem en DisRenTexGameManager.", Color.red);
                return;
            }

            PopulateNewRound();
        }

        private void ResolveReferences()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (binocularEffect == null && mainCamera != null)
                binocularEffect = mainCamera.GetComponent<BinocularCameraEffect>();

            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.20f, 0.38f, 0.56f, 1f);
                mainCamera.cullingMask &= ~(1 << PreviewLayer);
                mainCamera.cullingMask |= (1 << BinocularCameraEffect.BinocularQuadLayer);
                mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                _camHeight = Mathf.Clamp(mainCamera.transform.position.y, minCamHeight, maxCamHeight);
                mainCamera.transform.position = new Vector3(boardCenter.x, _camHeight, boardCenter.z);
            }
        }

        private void BuildCatalog()
        {
            _catalog.Clear();

            if (fruitMeshes != null && fruitMeshes.Length > 0)
            {
                for (int i = 0; i < fruitMeshes.Length; i++)
                {
                    Mesh m = fruitMeshes[i];
                    if (m == null) continue;

                    string label = (fruitNames != null && i < fruitNames.Length && !string.IsNullOrWhiteSpace(fruitNames[i]))
                        ? fruitNames[i]
                        : CleanFruitName(m.name);

                    AddCatalogEntry(m, label);
                }
            }

            _meshBuckets = new List<int>[_catalog.Count];
            for (int i = 0; i < _meshBuckets.Length; i++)
                _meshBuckets[i] = new List<int>(16);
        }

        private void AddCatalogEntry(Mesh mesh, string displayName)
        {
            Bounds b = mesh.bounds;
            float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            float normScale = maxDim > 0.0001f ? (1.45f / maxDim) : 1f;
            float baseRadius = maxDim * 0.5f * normScale;

            _catalog.Add(new MeshCatalogEntry
            {
                mesh = mesh,
                displayName = displayName,
                centerOffset = b.center,
                normalizedScale = normScale,
                baseRadius = baseRadius
            });
        }

        private static string CleanFruitName(string raw)
        {
            string s = raw.Replace("SM_Fruits_", "").Replace("_1", "").Replace("_2", "").Replace("_3", "").Replace("_4", "").Replace("_R1", "").Replace("_R2", "").Replace("_Y1", "").Replace("_P2", "");
            return s.Replace('_', ' ').Trim();
        }

        private static void HideStaticFruitsInScene()
        {
            MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
            foreach (MeshRenderer mr in renderers)
            {
                if (mr != null && mr.gameObject.name.StartsWith("SM_Fruits_"))
                    mr.enabled = false;
            }
        }

        private void SetupCamerasAndRenderTextures()
        {
            captureRenderTexture = new RenderTexture(640, 400, 24, RenderTextureFormat.ARGB32)
            {
                name = "CapturaRT",
                filterMode = FilterMode.Bilinear
            };
            captureRenderTexture.Create();
            ClearRenderTexture(captureRenderTexture, new Color(0.08f, 0.11f, 0.16f, 1f));

            targetPreviewRenderTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32)
            {
                name = "ObjetivoPreviewRT",
                filterMode = FilterMode.Bilinear
            };
            targetPreviewRenderTexture.Create();
            ClearRenderTexture(targetPreviewRenderTexture, new Color(0.11f, 0.16f, 0.24f, 1f));

            if (rtViewerMaterial != null)
                rtViewerMaterial.SetTexture(CapturedRTID, captureRenderTexture);

            int worldMask = ~((1 << PreviewLayer) | (1 << BinocularCameraEffect.BinocularQuadLayer));

            GameObject capGo = new GameObject("CaptureRTCamera");
            capGo.transform.SetParent(transform, false);
            _captureCamera = capGo.AddComponent<Camera>();
            _captureCamera.clearFlags = CameraClearFlags.SolidColor;
            _captureCamera.backgroundColor = new Color(0.20f, 0.38f, 0.56f, 1f);
            _captureCamera.fieldOfView = 45f;
            _captureCamera.nearClipPlane = 0.2f;
            _captureCamera.farClipPlane = 100f;
            _captureCamera.cullingMask = worldMask;
            _captureCamera.depth = -2f;
            _captureCamera.targetTexture = captureRenderTexture;
            _captureCamera.enabled = false;

            GameObject prevGo = new GameObject("TargetPreviewRTCamera");
            prevGo.transform.SetParent(transform, false);
            prevGo.transform.position = new Vector3(0f, 500.95f, -3.8f);
            prevGo.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
            _targetPreviewCamera = prevGo.AddComponent<Camera>();
            _targetPreviewCamera.clearFlags = CameraClearFlags.SolidColor;
            _targetPreviewCamera.backgroundColor = new Color(0.11f, 0.16f, 0.24f, 1f);
            _targetPreviewCamera.fieldOfView = 35f;
            _targetPreviewCamera.nearClipPlane = 0.2f;
            _targetPreviewCamera.farClipPlane = 20f;
            _targetPreviewCamera.cullingMask = 1 << PreviewLayer;
            _targetPreviewCamera.depth = -3f;
            _targetPreviewCamera.targetTexture = targetPreviewRenderTexture;
            _targetPreviewCamera.enabled = true;
        }

        private static void ClearRenderTexture(RenderTexture rt, Color color)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, color);
            RenderTexture.active = prev;
        }

        private void PopulateNewRound()
        {
            _fruits.Clear();
            for (int i = 0; i < _meshBuckets.Length; i++)
                _meshBuckets[i].Clear();

            int prevTarget = _targetCatalogIndex;
            _targetCatalogIndex = Random.Range(0, _catalog.Count);
            if (_catalog.Count > 1 && _targetCatalogIndex == prevTarget)
                _targetCatalogIndex = (_targetCatalogIndex + 1) % _catalog.Count;

            _targetColorIndex = Random.Range(0, _colorPresets.Length);

            int totalCells = Mathf.Max(4, columns * rows);
            float cellW = boardSize.x / columns;
            float cellH = boardSize.y / rows;

            List<int> candidateCells = new List<int>(totalCells);
            Vector2 camXZ = mainCamera != null
                ? new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.z)
                : new Vector2(boardCenter.x, boardCenter.z);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int idx = r * columns + c;
                    Vector3 pos = GetCellCenter(c, r, cellW, cellH);
                    if (Vector2.Distance(new Vector2(pos.x, pos.z), camXZ) > 3.2f)
                        candidateCells.Add(idx);
                }
            }

            int targetCellIndex = candidateCells.Count > 0
                ? candidateCells[Random.Range(0, candidateCells.Count)]
                : Random.Range(0, totalCells);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int cellIdx = r * columns + c;
                    Vector3 basePos = GetCellCenter(c, r, cellW, cellH);
                    Vector3 jitter = new Vector3(
                        Random.Range(-cellW * 0.25f, cellW * 0.25f),
                        Random.Range(-0.45f, 0.45f),
                        Random.Range(-cellH * 0.25f, cellH * 0.25f)
                    );

                    bool isTarget = cellIdx == targetCellIndex;
                    int meshIdx = isTarget ? _targetCatalogIndex : PickDistractorMeshIndex(_targetCatalogIndex);
                    int colorIdx = isTarget ? _targetColorIndex : Random.Range(0, _colorPresets.Length);
                    float sizeMult = isTarget ? Random.Range(0.98f, 1.18f) : Random.Range(0.62f, 1.48f);

                    float dir = Random.value > 0.5f ? 1f : -1f;
                    int fruitIndex = _fruits.Count;

                    _fruits.Add(new FruitInstance
                    {
                        catalogIndex = meshIdx,
                        colorIndex = colorIdx,
                        worldPos = basePos + jitter,
                        initialRotation = Quaternion.Normalize(Random.rotation),
                        sizeMultiplier = sizeMult,
                        spinSpeed = Random.Range(1.2f, 3.0f) * dir,
                        phaseOffset = Random.Range(0f, Mathf.PI * 2f),
                        isTarget = isTarget
                    });

                    _meshBuckets[meshIdx].Add(fruitIndex);
                }
            }

            UpdateTargetHUD();
        }

        private Vector3 GetCellCenter(int col, int row, float cellW, float cellH)
        {
            float startX = boardCenter.x - boardSize.x * 0.5f + cellW * 0.5f;
            float startZ = boardCenter.z - boardSize.y * 0.5f + cellH * 0.5f;
            return new Vector3(startX + col * cellW, boardCenter.y, startZ + row * cellH);
        }

        private int PickDistractorMeshIndex(int excludeIndex)
        {
            if (_catalog.Count <= 1) return 0;
            int idx = Random.Range(0, _catalog.Count - 1);
            return idx >= excludeIndex ? idx + 1 : idx;
        }

        private void Update()
        {
            if (_catalog.Count == 0 || fruitMaterial == null)
                return;

            if (_feedbackTimer > 0f)
            {
                _feedbackTimer -= Time.deltaTime;
                if (_feedbackTimer <= 0f && _feedbackText != null)
                    _feedbackText.text = string.Empty;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleRtModal();
            }

            if (_isCapturing)
                return;

            // La rotación de las frutas ocurre en Custom/Instanced_Fruit (vertex shader), sin rotarlas en Update().
            HandleCameraNavigation();
            HandleCaptureInput();
        }

        private void LateUpdate()
        {
            if (_catalog.Count == 0 || fruitMaterial == null)
                return;

            DrawFruitsInstancedForCamera(mainCamera);
            DrawTargetPreviewMesh();

            if (_isCapturing && _captureCamera != null && _captureCamera.enabled)
            {
                DrawFruitsInstancedForCamera(_captureCamera);
            }
        }

        private void HandleCameraNavigation()
        {
            if (mainCamera == null) return;

            Vector3 pos = mainCamera.transform.position;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                Vector3 move = new Vector3(h, 0f, v).normalized * (panSpeed * Time.deltaTime);
                pos += move;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _camHeight = Mathf.Clamp(_camHeight - scroll * zoomSpeed * 0.35f, minCamHeight, maxCamHeight);
            }

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            if (Input.GetMouseButtonDown(0) && !overUI)
            {
                _pointerDownOnWorld = true;
                _dragStartMouse = Input.mousePosition;
                _lastDragMouse = Input.mousePosition;
                _isDragging = false;
            }

            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                _lastDragMouse = Input.mousePosition;
            }

            if (Input.GetMouseButton(0) && _pointerDownOnWorld)
            {
                if (!_isDragging && Vector3.Distance(Input.mousePosition, _dragStartMouse) > 8f)
                    _isDragging = true;

                if (_isDragging)
                {
                    Vector3 delta = Input.mousePosition - _lastDragMouse;
                    float dragScale = _camHeight * 0.00145f;
                    pos.x -= delta.x * dragScale;
                    pos.z -= delta.y * dragScale;
                    _lastDragMouse = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                Vector3 delta = Input.mousePosition - _lastDragMouse;
                float dragScale = _camHeight * 0.00145f;
                pos.x -= delta.x * dragScale;
                pos.z -= delta.y * dragScale;
                _lastDragMouse = Input.mousePosition;
            }

            float halfW = boardSize.x * 0.48f;
            float halfH = boardSize.y * 0.48f;
            pos.x = Mathf.Clamp(pos.x, boardCenter.x - halfW, boardCenter.x + halfW);
            pos.z = Mathf.Clamp(pos.z, boardCenter.z - halfH, boardCenter.z + halfH);
            pos.y = _camHeight;

            mainCamera.transform.position = pos;
        }

        private void HandleCaptureInput()
        {
            if (_isRtModalOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    ToggleRtModal();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                TryCaptureTargetInView();
            }

            if (Input.GetMouseButtonUp(0) && _pointerDownOnWorld)
            {
                bool wasDragging = _isDragging;
                _pointerDownOnWorld = false;
                _isDragging = false;

                if (!wasDragging)
                    TryPickFruitAtCursor(Input.mousePosition);
            }
        }

        private void TryPickFruitAtCursor(Vector2 mousePos)
        {
            if (mainCamera == null) return;

            if (binocularEffect != null && !binocularEffect.IsScreenPointInsideBinoculars(mousePos))
            {
                ShowFeedback("Solo puedes seleccionar objetos dentro de los binoculares.", new Color(1f, 0.78f, 0.35f));
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            int hitIndex = -1;
            float bestScore = float.MaxValue;

            for (int i = 0; i < _fruits.Count; i++)
            {
                FruitInstance f = _fruits[i];
                MeshCatalogEntry entry = _catalog[f.catalogIndex];
                float hitRadius = entry.baseRadius * f.sizeMultiplier * 1.05f;

                Vector3 toFruit = f.worldPos - ray.origin;
                float alongRay = Vector3.Dot(toFruit, ray.direction);
                if (alongRay <= 0f) continue;

                float perpDist = Vector3.Cross(ray.direction, toFruit).magnitude;
                if (perpDist <= hitRadius)
                {
                    float normalizedOffset = perpDist / Mathf.Max(hitRadius, 0.01f);
                    float score = normalizedOffset + alongRay * 0.04f;
                    if (score < bestScore)
                    {
                        bestScore = score;
                        hitIndex = i;
                    }
                }
            }

            if (hitIndex < 0)
            {
                ShowFeedback("Haz clic sobre una fruta dentro de los binoculares.", new Color(0.85f, 0.9f, 1f));
                return;
            }

            FruitInstance picked = _fruits[hitIndex];
            if (picked.isTarget)
            {
                StartCoroutine(CaptureAndRepopulateRoutine(picked));
            }
            else
            {
                string pickedName = _catalog[picked.catalogIndex].displayName;
                string pickedColor = _colorPresets[picked.colorIndex].name;
                string targetName = _catalog[_targetCatalogIndex].displayName;
                ShowFeedback($"Eso es {pickedName} ({pickedColor}). Busca: {targetName}.", new Color(1f, 0.45f, 0.45f));
            }
        }

        public void TryCaptureTargetInView()
        {
            if (_isCapturing || mainCamera == null) return;

            for (int i = 0; i < _fruits.Count; i++)
            {
                FruitInstance f = _fruits[i];
                if (!f.isTarget) continue;

                Vector3 vp = mainCamera.WorldToViewportPoint(f.worldPos);
                bool visible = vp.z > 0f && (binocularEffect == null || binocularEffect.IsViewportPointInsideBinoculars(vp));

                if (visible)
                {
                    StartCoroutine(CaptureAndRepopulateRoutine(f));
                }
                else
                {
                    ShowFeedback("Mueve los binoculares hasta que el objetivo esté visible en los círculos.", new Color(1f, 0.78f, 0.35f));
                }
                return;
            }
        }

        private IEnumerator CaptureAndRepopulateRoutine(FruitInstance targetFruit)
        {
            _isCapturing = true;

            Vector3 focusPos = targetFruit.worldPos;
            _captureCamera.transform.position = new Vector3(focusPos.x, focusPos.y + 5.2f, focusPos.z);
            _captureCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _captureCamera.targetTexture = captureRenderTexture;
            _captureCamera.enabled = true;

            if (binocularEffect != null)
                binocularEffect.TriggerFlash();

            yield return new WaitForEndOfFrame();

            _captureCamera.enabled = false;
            _hasCapture = true;
            _captureCount++;

            if (rtViewerMaterial != null)
                rtViewerMaterial.SetTexture(CapturedRTID, captureRenderTexture);

            if (_rtModalImage != null)
                _rtModalImage.texture = captureRenderTexture;

            ShowFeedback($"¡Captura #{_captureCount} guardada en RT! Re-populando pantalla...", new Color(0.35f, 1f, 0.55f));

            PopulateNewRound();
            _isCapturing = false;
        }

        private void DrawFruitsInstancedForCamera(Camera cam)
        {
            if (cam == null || _meshBuckets == null) return;

            for (int meshIdx = 0; meshIdx < _catalog.Count; meshIdx++)
            {
                List<int> indices = _meshBuckets[meshIdx];
                int total = indices.Count;
                if (total == 0) continue;

                MeshCatalogEntry entry = _catalog[meshIdx];
                int offset = 0;

                while (offset < total)
                {
                    int batchCount = Mathf.Min(1023, total - offset);
                    for (int k = 0; k < batchCount; k++)
                    {
                        FruitInstance f = _fruits[indices[offset + k]];
                        float scale = entry.normalizedScale * f.sizeMultiplier;
                        _batchMatrices[k] = Matrix4x4.TRS(f.worldPos, f.initialRotation, Vector3.one * scale)
                                          * Matrix4x4.Translate(-entry.centerOffset);

                        Color c = _colorPresets[f.colorIndex].color;
                        _batchColors[k] = new Vector4(c.r, c.g, c.b, c.a);
                        _batchSpinSpeeds[k] = f.spinSpeed;
                        _batchPhases[k] = f.phaseOffset;
                    }

                    _instancedPropBlock.Clear();
                    _instancedPropBlock.SetVector(PivotCenterID, entry.centerOffset);
                    _instancedPropBlock.SetVectorArray(ColorID, _batchColors);
                    _instancedPropBlock.SetFloatArray(SpinSpeedID, _batchSpinSpeeds);
                    _instancedPropBlock.SetFloatArray(PhaseOffsetID, _batchPhases);

                    Graphics.DrawMeshInstanced(
                        entry.mesh,
                        0,
                        fruitMaterial,
                        _batchMatrices,
                        batchCount,
                        _instancedPropBlock,
                        UnityEngine.Rendering.ShadowCastingMode.On,
                        true,
                        0,
                        cam);

                    offset += batchCount;
                }
            }
        }

        private void DrawTargetPreviewMesh()
        {
            if (_targetPreviewCamera == null || _targetCatalogIndex < 0 || _targetCatalogIndex >= _catalog.Count)
                return;

            MeshCatalogEntry entry = _catalog[_targetCatalogIndex];
            ColorPreset preset = _colorPresets[_targetColorIndex];

            Vector3 previewPos = new Vector3(0f, 500f, 0f);
            Quaternion previewRot = Quaternion.Normalize(Quaternion.Euler(18f, 0f, 0f));
            float scale = entry.normalizedScale * 1.1f;

            _previewMatrix[0] = Matrix4x4.TRS(previewPos, previewRot, Vector3.one * scale)
                              * Matrix4x4.Translate(-entry.centerOffset);

            _previewColors[0] = new Vector4(preset.color.r, preset.color.g, preset.color.b, preset.color.a);

            _previewPropBlock.Clear();
            _previewPropBlock.SetVector(PivotCenterID, entry.centerOffset);
            _previewPropBlock.SetVectorArray(ColorID, _previewColors);
            _previewPropBlock.SetFloatArray(SpinSpeedID, _previewSpinSpeeds);
            _previewPropBlock.SetFloatArray(PhaseOffsetID, _previewPhases);

            Graphics.DrawMeshInstanced(
                entry.mesh,
                0,
                fruitMaterial,
                _previewMatrix,
                1,
                _previewPropBlock,
                UnityEngine.Rendering.ShadowCastingMode.Off,
                false,
                PreviewLayer,
                _targetPreviewCamera);
        }

        private void UpdateTargetHUD()
        {
            if (_targetCatalogIndex < 0 || _targetCatalogIndex >= _catalog.Count) return;

            string fruitName = _catalog[_targetCatalogIndex].displayName;
            string colorName = _colorPresets[_targetColorIndex].name;

            if (_targetLabelText != null)
                _targetLabelText.text = $"{fruitName}\n<color=#B8D4F5><size=16>Color: {colorName}</size></color>";

            if (_scoreText != null)
                _scoreText.text = $"Capturas: {_captureCount}";
        }

        private void ShowFeedback(string message, Color color)
        {
            if (_feedbackText == null) return;
            _feedbackText.text = message;
            _feedbackText.color = color;
            _feedbackTimer = 2.6f;
        }

        private void ToggleRtModal()
        {
            if (!_hasCapture)
            {
                ShowFeedback("Primero realiza una captura para ver la RT ampliada.", new Color(1f, 0.78f, 0.35f));
                return;
            }

            _isRtModalOpen = !_isRtModalOpen;
            if (_rtModalObj != null)
                _rtModalObj.SetActive(_isRtModalOpen);
        }

        private void BuildHUD()
        {
            _uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_uiFont == null)
                _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            GameObject canvasGo = new GameObject("DisRenTexHUD_Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // 1. Top-Left: Target Object Panel
            GameObject targetPanel = CreatePanel("TargetPanel", canvasGo.transform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -24f), new Vector2(275f, 305f), new Color(0.06f, 0.08f, 0.13f, 0.88f));

            CreateText("TargetHeader", targetPanel.transform, "OBJETIVO A BUSCAR", 16, FontStyle.Bold,
                new Color(1f, 0.84f, 0.32f, 1f), TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), new Vector2(-16f, 28f));

            GameObject previewBorder = CreatePanel("PreviewBorder", targetPanel.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -42f), new Vector2(166f, 166f), new Color(0.28f, 0.42f, 0.60f, 1f));

            GameObject previewRawGo = new GameObject("TargetPreviewRT");
            previewRawGo.transform.SetParent(previewBorder.transform, false);
            RectTransform previewRt = previewRawGo.AddComponent<RectTransform>();
            previewRt.anchorMin = Vector2.zero;
            previewRt.anchorMax = Vector2.one;
            previewRt.offsetMin = new Vector2(3f, 3f);
            previewRt.offsetMax = new Vector2(-3f, -3f);
            _targetPreviewImage = previewRawGo.AddComponent<RawImage>();
            _targetPreviewImage.texture = targetPreviewRenderTexture;

            _targetLabelText = CreateText("TargetName", targetPanel.transform, "...", 20, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -214f), new Vector2(-16f, 52f));
            _targetLabelText.supportRichText = true;

            _scoreText = CreateText("ScoreText", targetPanel.transform, "Capturas: 0", 16, FontStyle.Bold,
                new Color(0.55f, 0.95f, 0.65f, 1f), TextAnchor.MiddleCenter,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 10f), new Vector2(-16f, 26f));

            // 2. Top-Center: Feedback Banner
            _feedbackText = CreateText("FeedbackText", canvasGo.transform, "", 20, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(820f, 40f));

            // 3. Enlarged Modal for Inspecting RT in Build (Tab) using M_RenderTargetViewer (Custom/RT_Display)
            _rtModalObj = CreatePanel("RTModalBackdrop", canvasGo.transform,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.80f));

            GameObject modalCard = CreatePanel("RTModalCard", _rtModalObj.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(720f, 510f), new Color(0.08f, 0.11f, 0.16f, 1f));

            CreateText("ModalTitle", modalCard.transform, "VISTA AMPLIADA DE RENDER TEXTURE (RT)", 18, FontStyle.Bold,
                new Color(0.45f, 0.85f, 1f, 1f), TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -12f), new Vector2(-24f, 32f));

            GameObject modalRawGo = new GameObject("ModalRTImage");
            modalRawGo.transform.SetParent(modalCard.transform, false);
            RectTransform modalRawRect = modalRawGo.AddComponent<RectTransform>();
            modalRawRect.anchorMin = new Vector2(0.5f, 0.5f);
            modalRawRect.anchorMax = new Vector2(0.5f, 0.5f);
            modalRawRect.pivot = new Vector2(0.5f, 0.5f);
            modalRawRect.anchoredPosition = new Vector2(0f, 4f);
            modalRawRect.sizeDelta = new Vector2(656f, 410f);
            _rtModalImage = modalRawGo.AddComponent<RawImage>();
            _rtModalImage.texture = captureRenderTexture;
            if (rtViewerMaterial != null)
                _rtModalImage.material = rtViewerMaterial;

            CreateButton("CloseModalBtn", modalCard.transform, "Cerrar [Tab / Esc]",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 12f), new Vector2(170f, 34f),
                new Color(0.58f, 0.22f, 0.24f, 1f), ToggleRtModal);

            _rtModalObj.SetActive(false);
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            return go;
        }

        private Text CreateText(string name, Transform parent, string text, int fontSize, FontStyle style,
            Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(
                Mathf.Approximately(anchorMin.x, anchorMax.x) ? anchorMin.x : 0.5f,
                Mathf.Approximately(anchorMin.y, anchorMax.y) ? anchorMin.y : 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Text t = go.AddComponent<Text>();
            t.font = _uiFont;
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = alignment;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnGo = CreatePanel(name, parent, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta, bgColor);
            Button btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(onClick);
            CreateText("Label", btnGo.transform, label, 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }
    }
}
