using UnityEngine;

public class FloatingHazard : MonoBehaviour
{
    [Header("Parámetros de Patrulla")]
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float moveDistance = 3.0f;
    [SerializeField] private bool moveVertical = true; // Si es true se mueve arriba/abajo, si es false izquierda/derecha

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Movimiento senoidal suave simulando flotabilidad o corriente marina
        float offset = Mathf.Sin(Time.time * speed) * moveDistance;

        if (moveVertical)
        {
            transform.position = startPosition + new Vector3(0f, offset, 0f);
        }
        else
        {
            transform.position = startPosition + new Vector3(offset, 0f, 0f);
        }
    }
}