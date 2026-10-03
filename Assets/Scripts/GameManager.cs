using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Textos")]
    [SerializeField] private TextMeshProUGUI scoreText;      // Muestra Jugador, EcoCréditos y descargados
    [SerializeField] private TextMeshProUGUI cargoText;      // Muestra Bodega (ej: 2/3)
    [SerializeField] private TextMeshProUGUI winText;        // Pantalla de victoria

    [Header("UI Botones")]
    [SerializeField] private GameObject returnButton;       // Botón para volver al menú principal (Victoria)

    [Header("UI Derrota (Game Over)")]
    [SerializeField] private GameObject gameOverPanel;      // Panel GameOver con imagen y botones

    [Header("Efectos de Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoVictoria;
    [SerializeField] private AudioClip sonidoDerrota;
    [SerializeField] private AudioClip sonidoDescargaBoya;   // Sonido al descargar residuos en la boya

    private int ecoCredits = 0;
    private int totalRecycled = 0;
    private string activePlayerName = "Player";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Si no se asignó en el Inspector, busca el componente en el mismo GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Start()
    {
        // Reanudar el tiempo si venía pausado de una partida previa
        Time.timeScale = 1f;

        // 1. Obtener el nombre del jugador (busca primero en PlayerPrefs y luego en MenuManager)
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            activePlayerName = PlayerPrefs.GetString("PlayerName");
        }
        else if (MenuManager.Instance != null && !string.IsNullOrEmpty(MenuManager.Instance.playerName))
        {
            activePlayerName = MenuManager.Instance.playerName;
        }

        // Ocultar texto de victoria y botón de retorno al inicio
        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }

        if (returnButton != null)
        {
            returnButton.SetActive(false);
        }

        // Asegurar que el panel de Game Over comience desactivado
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateScoreUI();
    }

    public void DepositTrash(int amount)
    {
        totalRecycled += amount;
        ecoCredits += amount * 10; // Cada residuo otorga 10 EcoCréditos

        // Reproducir sonido de caja registradora / éxito al descargar
        if (audioSource != null && sonidoDescargaBoya != null)
        {
            audioSource.PlayOneShot(sonidoDescargaBoya);
        }

        // Guardar récord en tiempo real
        GuardarRecordLocal(ecoCredits);

        UpdateScoreUI();
        CheckWinCondition();
    }

    public void UpdateCargoUI(int current, int max)
    {
        if (cargoText != null)
        {
            cargoText.text = $"Bodega: {current}/{max}";
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Jugador: {activePlayerName} | Reciclados: {totalRecycled} | EcoCréditos: {ecoCredits}";
        }
    }

    private void CheckWinCondition()
    {
        // Se gana cuando no queda basura flotando en el mar
        GameObject[] remainingTrash = GameObject.FindGameObjectsWithTag("Trash");

        if (remainingTrash.Length == 0)
        {
            ShowVictory();
        }
    }

    private void ShowVictory()
    {
        GuardarRecordLocal(ecoCredits);

        // Reproducir sonido de victoria
        if (audioSource != null && sonidoVictoria != null)
        {
            audioSource.PlayOneShot(sonidoVictoria);
        }

        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = $"¡ZONA DESCONTAMINADA!\n¡Excelente trabajo, {activePlayerName}!\nODS 14 Cumplido\nEcoCréditos obtenidos: {ecoCredits}";
        }

        // Activar el botón de regreso al menú
        if (returnButton != null)
        {
            returnButton.SetActive(true);
        }
    }

    // Se ejecuta al agotarse la batería o por daño letal
    public void GameOver()
    {
        GuardarRecordLocal(ecoCredits);

        // Reproducir sonido de derrota
        if (audioSource != null && sonidoDerrota != null)
        {
            audioSource.PlayOneShot(sonidoDerrota);
        }

        // Muestra el panel con los botones de Reintentar y Volver al Menú
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Pausa el juego
        }
        else if (winText != null)
        {
            // Plan de respaldo por si el panel no fue asignado
            winText.gameObject.SetActive(true);
            winText.text = $"FIN DE LA MISIÓN\nJugador: {activePlayerName}\nEcoCréditos finales: {ecoCredits}";
            if (returnButton != null) returnButton.SetActive(true);
        }
    }

    // Método para el botón REINTENTAR dentro del GameOverPanel
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; // Reanuda el tiempo antes de recargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Método para cargar la escena de Menú (índice 0 en Build Settings)
    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // Reanuda el tiempo antes de regresar
        GuardarRecordLocal(ecoCredits);
        SceneManager.LoadScene(0);
    }

    // Método auxiliar seguro para almacenar récord
    private void GuardarRecordLocal(int puntaje)
    {
        int recordActual = PlayerPrefs.GetInt("RecordEcoCredits", 0);
        if (puntaje > recordActual)
        {
            PlayerPrefs.SetInt("RecordEcoCredits", puntaje);
            PlayerPrefs.Save();
        }

        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.GuardarNuevoRecord(puntaje);
        }
    }
}