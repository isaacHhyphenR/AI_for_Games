using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] Player[] _players;
    public static Player[] players;
    [SerializeField] Image _currentPlayerIndicator;
    public static Image currentPlayerIndicator;

    public static Player currentPlayer;
    static int currentPlayerIndex = 1;

    static Piece selectedPiece = null;

    [SerializeField] LayerMask _boardClickMask;
    public static LayerMask boardClickMask;

    private void Awake()
    {
        players = _players;
        currentPlayer = players[currentPlayerIndex];
        currentPlayerIndicator = _currentPlayerIndicator;
        boardClickMask = _boardClickMask;
        AdvanceTurn();
    }
    /// <summary>
    /// Turn order moves to next player
    /// </summary>
    public static void AdvanceTurn()
    {
        currentPlayerIndex++;
        if(currentPlayerIndex >= players.Length)
        {
            currentPlayerIndex = 0;
        }
        currentPlayer = players[currentPlayerIndex];
        currentPlayerIndicator.color = currentPlayer.GetColor();
    }

    public static void SelectPiece(Piece piece)
    {
        selectedPiece = piece;
        if(piece != null )
        {
            Debug.Log(selectedPiece.gameObject.name);
        }
    }
    public static Piece GetSelectedPiece()
    {
        return selectedPiece;
    }
}
