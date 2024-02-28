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

    [SerializeField] WinPanel winPanel;

    [SerializeField] LayerMask _boardClickMask;
    public static LayerMask boardClickMask;
    [Tooltip("Whether players are currently allowed to make moves. False for win screen, etc.")]
    public static bool canPlay = true;

    static GameManager instance;

    private void Awake()
    {
        instance = this;
        players = _players;
        currentPlayer = players[currentPlayerIndex];
        currentPlayerIndicator = _currentPlayerIndicator;
        boardClickMask = _boardClickMask;
    }
    private void Start()
    {
        AdvanceTurn();
    }
    /// <summary>
    /// Turn order moves to next player
    /// </summary>
    public static void AdvanceTurn()
    {
        currentPlayer.EndTurn();
        currentPlayerIndex++;
        if(currentPlayerIndex >= players.Length)
        {
            currentPlayerIndex = 0;
        }
        currentPlayer = players[currentPlayerIndex];
        currentPlayer.StartTurn();
        currentPlayerIndicator.color = currentPlayer.GetColor();
    }

    public static void SelectPiece(Piece piece)
    {
        if(selectedPiece != null)
        {
            selectedPiece.Deselect();
        }
        selectedPiece = piece;
        if(selectedPiece != null )
        {
        }
    }
    public static Piece GetSelectedPiece()
    {
        return selectedPiece;
    }

    public static void PlayerLost(Player loser)
    {
        canPlay = false;
        Player winner = GetOtherPlayer(loser);
        instance.winPanel.gameObject.SetActive(true);
        instance.winPanel.GameWon(winner);
    }

    public static Player GetOtherPlayer(Player player)
    {
        Player other = players[0];
        if (other == player)
        {
            other = players[1];
        }
        return other;
    }
}
