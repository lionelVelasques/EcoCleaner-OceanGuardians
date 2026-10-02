using UnityEngine;

public class SeguirDireccionLuz : MonoBehaviour
{
    private SpriteRenderer playerSprite;

    void Start()
    {
        // Busca el SpriteRenderer en el padre (Player)
        playerSprite = GetComponentInParent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerSprite != null)
        {
            // Si el sprite está volteado a la izquierda
            if (playerSprite.flipX)
            {
                // Apunta a la izquierda (ajusta a 180 o el ángulo frontal inverso)
                transform.localRotation = Quaternion.Euler(0, 0, 180f);
            }
            else
            {
                // Apunta a la derecha (posición original hacia adelante)
                transform.localRotation = Quaternion.Euler(0, 0, 0f);
            }
        }
    }
}