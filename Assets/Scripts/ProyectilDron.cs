using UnityEngine;

public class ProyectilDron : MonoBehaviour
{
    [SerializeField] private float velocidad = 14f;
    [SerializeField] private float tiempoDeVida = 2.5f;

    private Vector2 direccion = Vector2.right;

    public void ConfigurarDireccion(Vector2 nuevaDireccion)
    {
        direccion = nuevaDireccion.normalized;

        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        transform.position += (Vector3)direccion * velocidad * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hazard") || collision.CompareTag("Enemigo"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Obstaculo"))
        {
            Destroy(gameObject);
        }
    }
}