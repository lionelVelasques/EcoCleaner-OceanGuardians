using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡Colisión tóxica! Dron dañado.");

            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RecibirDanio(1);
            }
        }
    }
}