using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InteraccionAgua : MonoBehaviour
{
    [Header("Configuración del Agua")]
    [SerializeField] private Renderer rendererAgua;
    [SerializeField] private float radioEfecto = 2.5f;
    [SerializeField] private float fuerzaImpacto = 0.6f;

    private MaterialPropertyBlock _propBlock;

    private void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (rendererAgua != null)
        {
            rendererAgua.GetPropertyBlock(_propBlock);

            // Convertimos la posición global de la esfera al espacio local del Quad
            Vector3 posicionLocalEsfera = rendererAgua.transform.InverseTransformPoint(transform.position);

            // Pasamos las variables usando los nombres exactos registrados en tu Amplify
            _propBlock.SetVector("_Epicentro", posicionLocalEsfera);
            _propBlock.SetFloat("_FuerzaImpacto", fuerzaImpacto);
            _propBlock.SetFloat("_RadioAgua", radioEfecto);

            rendererAgua.SetPropertyBlock(_propBlock);
        }
    }
}