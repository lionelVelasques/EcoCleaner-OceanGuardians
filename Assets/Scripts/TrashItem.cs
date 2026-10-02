using UnityEngine;

public class TrashItem : MonoBehaviour
{
    // ENCAPSULATION
    [SerializeField] private string itemName = "Botella Plástica";
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private int cargoSize = 1;

    public string ItemName
    {
        get => itemName;
        set
        {
            if (!string.IsNullOrEmpty(value))
                itemName = value;
        }
    }

    public int ScoreValue
    {
        get => scoreValue;
        set
        {
            if (value >= 0)
                scoreValue = value;
        }
    }

    public int CargoSize
    {
        get => cargoSize;
        set => cargoSize = Mathf.Max(1, value);
    }

    // ABSTRACTION: Lógica de recolección común
    public void Collect(PlayerController player)
    {
        if (player.AddCargo(CargoSize))
        {
            OnCollected();
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("¡Bodega llena! Descarga en la Boya de Reciclaje.");
        }
    }

    // POLYMORPHISM: Método virtual para especializar efectos en clases hijas
    protected virtual void OnCollected()
    {
        Debug.Log($"Recolectado: {ItemName} (+{ScoreValue} pts)");
    }
}