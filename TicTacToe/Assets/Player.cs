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

    /// <summary>
    /// The AI returns the space it would like to select based on the current board state.
    /// </summary>
    /// <param name="currentBoardState"></param>
    /// <returns></returns>
    public Vector2 ChooseMove(Board currentBoardState)
    {


        return new Vector2(1, 1);
    }
}
