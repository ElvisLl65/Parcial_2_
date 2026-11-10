using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulsoEscanner : MonoBehaviour
{
    [Header("Configuración del Escáner")]
    [SerializeField] private float radioMaximo = 20f;       // Hasta dónde se expande el pulso
    [SerializeField] private float velocidadExpansion = 15f; // Qué tan rápido crece la esfera

    private Vector3 _escalaInicial;
    private float _radioActual = 0f;
    private bool _activado = false;

    private void Start()
    {
        // Al arrancar, la esfera empieza totalmente encogida e invisible
        _escalaInicial = Vector3.zero;
        transform.localScale = _escalaInicial;
    }

    private void Update()
    {
        // Activamos el "Modo Detective" al presionar la tecla E
        if (Input.GetKeyDown(KeyCode.E) && !_activado)
        {
            _activado = true;
            _radioActual = 0f;
        }

        if (_activado)
        {
            // Hacemos que la esfera crezca de forma uniforme en el tiempo
            _radioActual += Time.deltaTime * velocidadExpansion;
            transform.localScale = new Vector3(_radioActual, _radioActual, _radioActual);

            // Si supera el radio máximo, se reinicia y se apaga hasta el próximo pulso
            if (_radioActual >= radioMaximo)
            {
                _activado = false;
                transform.localScale = Vector3.zero;
            }
        }
    }
}