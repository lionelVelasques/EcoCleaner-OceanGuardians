using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float boostMultiplier = 1.6f;

    [Header("Orientación de Costado")]
    [SerializeField] private float rotacionBaseZ = -90f;

    [Header("Sistema de Vidas / Casco")]
    [SerializeField] private int maxVidas = 3;
    [SerializeField] private float tiempoInvulnerabilidad = 1.5f;
    [SerializeField] private TextMeshProUGUI vidasText;
    [SerializeField] private AudioClip sonidoDanio;
    private int vidasActuales;
    private bool esInvulnerable = false;

    [Header("Capacidad de Bodega")]
    [SerializeField] private int maxCargoCapacity = 3;
    private int currentCargo = 0;

    [Header("Sistema de Batería")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float baseDrainRate = 2.5f;
    [SerializeField] private float boostDrainRate = 7.0f;
    [SerializeField] private Slider batterySlider;

    [Header("Sistema de Proyectiles y Luz")]
    [SerializeField] private GameObject prefabProyectil;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private Transform luzSubmarino;
    [SerializeField] private float costoEnergiaDisparo = 2.0f;
    [SerializeField] private AudioClip sonidoDisparo;

    [Header("Efectos de Audio del Jugador")]
    [SerializeField] private AudioClip sonidoRecoleccion;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private float currentBattery;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isBoosting;
    private bool isDead = false;
    private bool mirandoDerecha = true;

    public int MaxCargoCapacity => maxCargoCapacity;
    public int CurrentCargo => currentCargo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentBattery = maxBattery;
        vidasActuales = maxVidas;

        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        transform.localScale = Vector3.one;
        ActualizarRotacionVisual();

        if (batterySlider != null)
        {
            batterySlider.maxValue = maxBattery;
            batterySlider.value = currentBattery;
        }

        ActualizarUIVidas();
        UpdateCargoDisplay();
    }

    void Update()
    {
        if (isDead) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        isBoosting = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space)) && moveInput.sqrMagnitude > 0;

        if (moveX > 0.05f && !mirandoDerecha)
        {
            mirandoDerecha = true;
            ActualizarRotacionVisual();
        }
        else if (moveX < -0.05f && mirandoDerecha)
        {
            mirandoDerecha = false;
            ActualizarRotacionVisual();
        }

        if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
        {
            Disparar();
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

    private void ActualizarRotacionVisual()
    {
        float rotacionY = mirandoDerecha ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0f, rotacionY, rotacionBaseZ);
    }

    private void Disparar()
    {
        if (prefabProyectil == null || puntoDisparo == null) return;

        if (currentBattery > costoEnergiaDisparo)
        {
            currentBattery -= costoEnergiaDisparo;

            if (batterySlider != null)
            {
                batterySlider.value = currentBattery;
            }

            Vector2 direccionDisparo = (luzSubmarino != null) ? (Vector2)luzSubmarino.up : (mirandoDerecha ? Vector2.right : Vector2.left);

            GameObject bala = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);
            ProyectilDron proyectilScript = bala.GetComponent<ProyectilDron>();
            if (proyectilScript != null)
            {
                proyectilScript.ConfigurarDireccion(direccionDisparo);
            }

            if (audioSource != null && sonidoDisparo != null)
            {
                audioSource.PlayOneShot(sonidoDisparo);
            }
        }
    }

    public void RecibirDanio(int danio = 1)
    {
        if (isDead || esInvulnerable) return;

        vidasActuales -= danio;
        ActualizarUIVidas();

        if (audioSource != null && sonidoDanio != null)
        {
            audioSource.PlayOneShot(sonidoDanio);
        }

        if (vidasActuales <= 0)
        {
            Morir();
        }
        else if (gameObject.activeInHierarchy)
        {
            StartCoroutine(EfectoInvulnerabilidad());
        }
    }

    private IEnumerator EfectoInvulnerabilidad()
    {
        esInvulnerable = true;
        float tiempoFin = Time.time + tiempoInvulnerabilidad;

        while (Time.time < tiempoFin)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(0.1f);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        esInvulnerable = false;
    }

    private void ActualizarUIVidas()
    {
        if (vidasText != null)
        {
            vidasText.text = $"Vidas: {vidasActuales}/{maxVidas}";
        }
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
    }

    public bool AddCargo(int amount)
    {
        if (currentCargo + amount <= maxCargoCapacity)
        {
            currentCargo += amount;
            UpdateCargoDisplay();

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

        if (collision.CompareTag("Enemigo"))
        {
            RecibirDanio(1);
            return;
        }

        if (collision.CompareTag("Trash") && currentCargo < maxCargoCapacity)
        {
            TrashItem item = collision.GetComponent<TrashItem>();
            if (item != null)
            {
                collision.enabled = false;
                item.Collect(this);
            }
        }

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