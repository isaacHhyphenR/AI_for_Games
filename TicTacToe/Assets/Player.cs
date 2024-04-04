using UnityEngine;

public class Player : MonoBehaviour
{
    [Tooltip("Which character represents this player on the board")]
    [SerializeField] char character;

    public char Character()
    {
        return character;
    }
}
