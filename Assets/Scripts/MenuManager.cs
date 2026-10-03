using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("Datos de Jugador")]
    public string playerName = "Player";
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI recordText;

    [Header("Paneles UI")]
    [SerializeField] private GameObject panelInstrucciones;

    void Awake()
    {
        // Instancia simple por escena, asegurando tiempo normal
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        // Cargar el nombre si ya se había guardado previamente
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
            if (nameInput != null)
            {
                nameInput.text = playerName;
            }
        }

        CargarRecord();

        if (panelInstrucciones != null)
        {
            panelInstrucciones.SetActive(false);
        }
    }

    private void GuardarNombre()
    {
        if (nameInput != null && !string.IsNullOrEmpty(nameInput.text))
        {
            playerName = nameInput.text;
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
        }
    }

    // Botón Nivel 1 (Escena 'main')
    public void JugarNivel1()
    {
        GuardarNombre();
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    // Botón Nivel 2 (Escena 'Nivel2')
    public void JugarNivel2()
    {
        GuardarNombre();
        Time.timeScale = 1f;
        SceneManager.LoadScene(2);
    }

    // Botón ¿CÓMO JUGAR?
    public void MostrarInstrucciones()
    {
        if (panelInstrucciones != null)
        {
            panelInstrucciones.SetActive(true);
        }
    }

    // Botón ENTENDIDO / VOLVER
    public void OcultarInstrucciones()
    {
        if (panelInstrucciones != null)
        {
            panelInstrucciones.SetActive(false);
        }
    }

    public void GuardarNuevoRecord(int nuevoPuntaje)
    {
        int recordActual = PlayerPrefs.GetInt("RecordEcoCredits", 0);
        if (nuevoPuntaje > recordActual)
        {
            PlayerPrefs.SetInt("RecordEcoCredits", nuevoPuntaje);
            PlayerPrefs.Save();
        }
    }

    private void CargarRecord()
    {
        int record = PlayerPrefs.GetInt("RecordEcoCredits", 0);
        if (recordText != null)
        {
            recordText.text = $"Mejor Puntaje: {record}";
        }
    }
}