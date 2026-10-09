using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Textos")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI cargoText;
    [SerializeField] private TextMeshProUGUI winText;

    [Header("UI Botones")]
    [SerializeField] private GameObject returnButton;

    [Header("UI Derrota (Game Over)")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI Menú de Pausa")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider musicVolumeSlider;
    private bool isPaused = false;

    [Header("Efectos de Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicaFondoAudioSource; // Arrastra MusicaFondo aquí
    [SerializeField] private AudioClip sonidoVictoria;
    [SerializeField] private AudioClip sonidoDerrota;
    [SerializeField] private AudioClip sonidoDescargaBoya;

    [Header("Efecto Visual Océano Limpio (Transición)")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private SpriteRenderer fondoSprite;
    [SerializeField] private Light2D luzGlobal2D;
    [SerializeField] private Color aguaLimpiaColor = new Color(0.12f, 0.72f, 0.85f, 1f);
    [SerializeField] private float duracionAclarado = 2.5f;

    private int ecoCredits = 0;
    private int totalRecycled = 0;
    private string activePlayerName = "Player";
    private bool isGameWon = false;

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

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (mainCam == null)
        {
            mainCam = Camera.main;
        }

        BuscarAudioMusica();
    }

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            activePlayerName = PlayerPrefs.GetString("PlayerName");
        }
        else if (MenuManager.Instance != null && !string.IsNullOrEmpty(MenuManager.Instance.playerName))
        {
            activePlayerName = MenuManager.Instance.playerName;
        }

        if (winText != null) winText.gameObject.SetActive(false);
        if (returnButton != null) returnButton.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Inicializar volumen y slider
        float volumenGuardado = PlayerPrefs.GetFloat("MusicaVolumen", 0.5f);
        if (volumenGuardado <= 0.05f) volumenGuardado = 0.5f;

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = volumenGuardado;
        }

        CambiarVolumenMusica(volumenGuardado);
        UpdateScoreUI();
    }

    void Update()
    {
        // Abrir y cerrar pausa con la tecla Escape (ESC)
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameWon && (gameOverPanel == null || !gameOverPanel.activeSelf))
        {
            if (isPaused)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }

        if (!isGameWon)
        {
            CheckWinCondition();
        }
    }

    public void PausarJuego()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ReanudarJuego()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void BuscarAudioMusica()
    {
        if (musicaFondoAudioSource == null)
        {
            GameObject obj = GameObject.Find("MusicaFondo");
            if (obj != null)
            {
                musicaFondoAudioSource = obj.GetComponent<AudioSource>();
            }
        }
    }

    // Llamado por el Slider (Dynamic float)
    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        BuscarAudioMusica();

        if (musicaFondoAudioSource != null)
        {
            musicaFondoAudioSource.volume = nuevoVolumen;
        }

        // Control maestro de audio de Unity
        AudioListener.volume = nuevoVolumen;

        PlayerPrefs.SetFloat("MusicaVolumen", nuevoVolumen);
        PlayerPrefs.Save();
    }

    public void DepositTrash(int amount)
    {
        totalRecycled += amount;
        ecoCredits += amount * 10;

        if (audioSource != null && sonidoDescargaBoya != null)
        {
            audioSource.PlayOneShot(sonidoDescargaBoya);
        }

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

    public void CheckWinCondition()
    {
        if (isGameWon) return;

        int remainingTrash = GameObject.FindGameObjectsWithTag("Trash").Length;
        int remainingHazards = GameObject.FindGameObjectsWithTag("Hazard").Length;
        int remainingEnemies = GameObject.FindGameObjectsWithTag("Enemigo").Length;
        int totalThreats = remainingHazards + remainingEnemies;

        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        int cargoActual = (player != null) ? player.CurrentCargo : 0;

        if (remainingTrash == 0 && totalThreats == 0 && cargoActual == 0)
        {
            ShowVictory();
        }
    }

    private void ShowVictory()
    {
        isGameWon = true;
        GuardarRecordLocal(ecoCredits);

        StartCoroutine(TransicionAguaLimpia());

        if (audioSource != null && sonidoVictoria != null)
        {
            audioSource.PlayOneShot(sonidoVictoria);
        }

        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = $"¡OCÉANO TOTALMENTE DESCONTAMINADO!\n¡Gran trabajo, {activePlayerName}!\nAmenazas neutralizadas y residuos reciclados\nODS 14 Cumplido | EcoCréditos: {ecoCredits}";
        }

        if (returnButton != null)
        {
            returnButton.SetActive(true);
        }
    }

    private IEnumerator TransicionAguaLimpia()
    {
        float tiempo = 0f;

        Color camInicio = (mainCam != null) ? mainCam.backgroundColor : Color.black;
        Color spriteInicio = (fondoSprite != null) ? fondoSprite.color : Color.white;
        float luzInicio = (luzGlobal2D != null) ? luzGlobal2D.intensity : 0.4f;

        while (tiempo < duracionAclarado)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracionAclarado;

            if (mainCam != null)
            {
                mainCam.backgroundColor = Color.Lerp(camInicio, aguaLimpiaColor, t);
            }

            if (fondoSprite != null)
            {
                fondoSprite.color = Color.Lerp(spriteInicio, Color.white, t);
            }

            if (luzGlobal2D != null)
            {
                luzGlobal2D.intensity = Mathf.Lerp(luzInicio, 1.0f, t);
                luzGlobal2D.color = Color.Lerp(luzGlobal2D.color, Color.white, t);
            }

            yield return null;
        }
    }

    public void GameOver()
    {
        if (isGameWon) return;

        GuardarRecordLocal(ecoCredits);

        if (audioSource != null && sonidoDerrota != null)
        {
            audioSource.PlayOneShot(sonidoDerrota);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = $"FIN DE LA MISIÓN\nJugador: {activePlayerName}\nEcoCréditos finales: {ecoCredits}";
            if (returnButton != null) returnButton.SetActive(true);
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        GuardarRecordLocal(ecoCredits);
        SceneManager.LoadScene(0);
    }

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