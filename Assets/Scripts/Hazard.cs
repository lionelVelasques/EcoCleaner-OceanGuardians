using UnityEngine;
using UnityEngine.SceneManagement;

public class Hazard : MonoBehaviour
{
    [Header("Efecto de daño")]
    [SerializeField] private float reloadDelay = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detecta si lo que entró en contacto es el jugador
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡Colisión tóxica! Dron dañado.");

            // Oculta el jugador para simular que explotó/se averió
            collision.gameObject.SetActive(false);

            // Reinicia el nivel tras una breve pausa
            Invoke(nameof(RestartLevel), reloadDelay);
        }
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}