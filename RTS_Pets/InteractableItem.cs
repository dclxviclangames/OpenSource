using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header(" то может взаимодействовать")]
    [Tooltip("ID игрока, который может использовать этот предмет. 0 = любой.")]
    public int RequiredPlayerID = 0;

    // ћетод, который провер€ет разрешение и запускает действие
    public bool CanInteract(int commanderID)
    {
        // 1. ≈сли RequiredPlayerID = 0, взаимодействовать может любой.
        // 2. »наче, ID командира должен соответствовать требуемому ID.
        return RequiredPlayerID == commanderID;
    }
}
