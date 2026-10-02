using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Referencias de UI")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_Text bestScoreText;

    [Header("Datos Persistentes")]
    public string playerName;
    public string bestPlayerName;
    public int bestScore;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CargarDatos();
    }

    private void Start()
    {
        ActualizarTextoMejorPuntaje();
    }

    public void OnStartGamePressed()
    {
        if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
        {
            playerName = nameInputField.text.Trim();
        }
        else
        {
            playerName = "Player";
        }

        SceneManager.LoadScene(1); // Carga la escena del juego (SampleScene)
    }

    public void ActualizarTextoMejorPuntaje()
    {
        if (bestScoreText != null)
        {
            if (bestScore > 0)
            {
                bestScoreText.text = $"Mejor Puntaje: {bestPlayerName} : {bestScore}";
            }
            else
            {
                bestScoreText.text = "Mejor Puntaje: 0";
            }
        }
    }

    public void GuardarNuevoRecord(int nuevoPuntaje)
    {
        if (nuevoPuntaje > bestScore)
        {
            bestScore = nuevoPuntaje;
            bestPlayerName = playerName;

            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.SetString("BestPlayer", bestPlayerName);
            PlayerPrefs.Save();
        }
    }

    private void CargarDatos()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestPlayerName = PlayerPrefs.GetString("BestPlayer", "");
    }
}