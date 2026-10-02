using UnityEngine;

// INHERITANCE: Hereda atributos y comportamiento de TrashItem
public class TireTrash : TrashItem
{
    void Awake()
    {
        ItemName = "Neumático Desechado";
        ScoreValue = 25;
        CargoSize = 2; // Ocupa 2 espacios en la bodega por ser residuo pesado
    }

    // POLYMORPHISM: Sobrescritura de la acción de recolección
    protected override void OnCollected()
    {
        Debug.Log($"¡Residuo pesado recogido! {ItemName} genera mayor recompensa.");
    }
}