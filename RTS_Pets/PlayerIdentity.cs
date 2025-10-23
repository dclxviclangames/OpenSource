using UnityEngine;

public class PlayerIdentity : MonoBehaviour
{
    // ID, который однозначно присваиваетс€ игроку (например, 1, 2, 3...)
    public int PlayerID = 0;

    // ¬ мультиплеере: здесь можно использовать PhotonView.Owner.ActorNumber;
    // Ќо дл€ синглплеера достаточно простого инкремента.
}

