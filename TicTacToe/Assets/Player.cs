using UnityEngine;

public class Player : MonoBehaviour
{
    [Tooltip("Which character represents this player on the board")]
    [SerializeField] char character;
    [Tooltip("If true, AI will play for this character")]
    [SerializeField] bool isAi;

    public char Character()
    {
        return character;
    }

    public bool IsAi()
    {
        return isAi;
    }
}
