using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorEscenas : MonoBehaviour
{
    static SelectorEscenas instancia;
    bool mostrarAyuda = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Crear()
    {
        if (instancia != null || SceneManager.sceneCountInBuildSettings < 2) return;
        var go = new GameObject("SelectorEscenas");
        instancia = go.AddComponent<SelectorEscenas>();
        DontDestroyOnLoad(go);
    }

    void Update()
    {
        int actual = SceneManager.GetActiveScene().buildIndex;
        int total = SceneManager.sceneCountInBuildSettings;

        if (Input.GetKeyDown(KeyCode.PageDown)) Cargar((actual + 1) % total);
        if (Input.GetKeyDown(KeyCode.PageUp)) Cargar((actual - 1 + total) % total);

        for (int i = 0; i < Mathf.Min(total, 12); i++)
        {
            if (Input.GetKeyDown(KeyCode.F1 + i)) Cargar(i);
        }

        if (Input.GetKeyDown(KeyCode.H)) mostrarAyuda = !mostrarAyuda;
    }

    void Cargar(int indice)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(indice);
    }

    void OnGUI()
    {
        if (!mostrarAyuda) return;
        string nombre = SceneManager.GetActiveScene().name;
        int actual = SceneManager.GetActiveScene().buildIndex + 1;
        int total = SceneManager.sceneCountInBuildSettings;
        GUI.Label(new Rect(10, Screen.height - 30, 900, 25),
            "Escena " + actual + "/" + total + ": " + nombre + "   |   F1-F" + Mathf.Min(total, 12) + " o RePag/AvPag para cambiar   |   H oculta este texto");
    }
}
