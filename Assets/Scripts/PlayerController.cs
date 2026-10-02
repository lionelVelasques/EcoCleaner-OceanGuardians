using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Ajustes de Movimiento e Inercia")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float boostMultiplier = 1.6f;

    [Header("Capacidad de Bodega")]
    // ENCAPSULATION
    [SerializeField] private int maxCargoCapacity = 3;
    private int currentCargo = 0;

    [Header("Sistema de Batería")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float baseDrainRate = 2.5f;   // Consumo por segundo al moverse
    [SerializeField] private float boostDrainRate = 7.0f;  // Consumo por segundo con turbo
    [SerializeField] private Slider batterySlider;

    [Header("Efectos de Audio del Jugador")]
    [SerializeField] private AudioClip sonidoRecoleccion; // Clip de burbuja / recolección

    private AudioSource audioSource;
    private float currentBattery;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isBoosting;
    private bool isDead = false;

    // ENCAPSULATION: Getters públicos con protección de datos
    public int MaxCargoCapacity => maxCargoCapacity;
    public int CurrentCargo => currentCargo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        currentBattery = maxBattery;

        if (batterySlider != null)
        {
            batterySlider.maxValue = maxBattery;
            batterySlider.value = currentBattery;
        }

        UpdateCargoDisplay();
    }

    void Update()
    {
        if (isDead) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        // Propulsión turbo con Shift o Espacio
        isBoosting = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space)) && moveInput.sqrMagnitude > 0;

        if (moveX != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveX), 1, 1);
        }

        GestorBateria();
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float weightPenalty = (currentCargo >= maxCargoCapacity) ? 0.7f : 1.0f;
        float finalSpeed = (isBoosting ? moveSpeed * boostMultiplier : moveSpeed) * weightPenalty;

        rb.linearVelocity = moveInput * finalSpeed;
    }

    private void GestorBateria()
    {
        if (moveInput.sqrMagnitude > 0)
        {
            float drain = isBoosting ? boostDrainRate : baseDrainRate;
            currentBattery -= drain * Time.deltaTime;
        }

        currentBattery = Mathf.Clamp(currentBattery, 0, maxBattery);

        if (batterySlider != null)
        {
            batterySlider.value = currentBattery;
        }

        if (currentBattery <= 0 && !isDead)
        {
            Morir();
        }
    }

    public void Morir()
    {
        if (isDead) return;

        isDead = true;
        rb.linearVelocity = Vector2.zero;
        Debug.LogWarning("¡Dron destruido o sin batería!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    public void RechargeBattery()
    {
        currentBattery = maxBattery;
        if (batterySlider != null)
        {
            batterySlider.value = currentBattery;
        }
        Debug.Log("¡Batería recargada al 100% en la Boya!");
    }

    public bool AddCargo(int amount)
    {
        if (currentCargo + amount <= maxCargoCapacity)
        {
            currentCargo += amount;
            UpdateCargoDisplay();

            // Reproduce el sonido de recolección si hay espacio y se guardó el residuo
            if (audioSource != null && sonidoRecoleccion != null)
            {
                audioSource.PlayOneShot(sonidoRecoleccion);
            }

            return true;
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // 1. Colisión con peligros ambientales
        if (collision.CompareTag("Hazard"))
        {
            Morir();
            return;
        }

        // 2. Recolección de residuos (solo si la bodega no está llena)
        if (collision.CompareTag("Trash") && currentCargo < maxCargoCapacity)
        {
            TrashItem item = collision.GetComponent<TrashItem>();
            if (item != null)
            {
                collision.enabled = false;
                item.Collect(this);
            }
        }

        // 3. Descarga en la Boya y recarga de batería
        if (collision.CompareTag("Buoy"))
        {
            RechargeBattery();

            if (currentCargo > 0)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DepositTrash(currentCargo);
                }

                currentCargo = 0;
                UpdateCargoDisplay();
                Debug.Log("¡Residuos descargados en la Boya con éxito!");
            }
        }
    }

    private void UpdateCargoDisplay()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateCargoUI(currentCargo, maxCargoCapacity);
        }
    }
}