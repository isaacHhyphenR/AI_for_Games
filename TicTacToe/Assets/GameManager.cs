using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public struct Board
{
    int gridSize;
    char[,] grid;
    public Board(int _gridSize)
    {
        gridSize = _gridSize;
        grid = new char[gridSize, gridSize];
    }
    public char GetValue(int x, int y)
    {
        return grid[x, y];
    }
    public char GetValue(Vector2 coordinate)
    {
        return GetValue((int)coordinate.x, (int)coordinate.y);
    }
    public void SetValue(int x, int y, char value)
    {
        grid[x, y] = value;
    }
    public void SetValue(Vector2 coordinate, char value)
    {
        SetValue((int)coordinate.x, (int)coordinate.y, value);
    }
    public char[,] GetGrid()
    {
        return grid;
    }

    public void Inherit(Board parent)
    {
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize; y++)
            {
                SetValue(x, y, parent.GetValue(x, y));
            }
        }
    }
}

public class GameManager : MonoBehaviour
{
    [Tooltip("The board will be this many squares wide & deep.")]
    [SerializeField] int gridSize;
    [Tooltip("The objects correlating to each player.")]
    [SerializeField] Player[] players = new Player[2];
    [Tooltip("The prefab used to display the physical board")]
    [SerializeField] GameObject squarePrefab;

    int currentPlayer;
    BoardSpace[,] displayBoard;
    Board currentBoardState;

    public static GameManager instance;

    ///EVENTS
    //Space Selected
    public static UnityEvent<BoardSpace> spaceSelected = new UnityEvent<BoardSpace>();
    public static void SpaceSelected(BoardSpace space)
    {
        spaceSelected.Invoke(space);
    }
    //New Turn
    public static UnityEvent<Player> newTurn = new UnityEvent<Player>();
    public static void NewTurn(Player player)
    {
        newTurn.Invoke(player);
    }

    ///FUNCTIONS
    private void OnEnable()
    {
        spaceSelected.AddListener(space => OnSpaceSelected(space));
    }
    private void Start()
    {
        instance = this;

        currentBoardState = new Board(gridSize);
        displayBoard = new BoardSpace[gridSize, gridSize];
        //Spawns the board squares
        float squareSize = squarePrefab.GetComponent<BoardSpace>().GetSize();
        Vector2 offset = new Vector2(squareSize * gridSize / 2 - (squareSize/2), -squareSize * gridSize / 2 + (squareSize / 2));
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 newPos = new Vector3(x * squareSize - offset.x, 0, y * -squareSize - offset.y);
                displayBoard[x, y] = Instantiate(squarePrefab, newPos, Quaternion.identity).GetComponent<BoardSpace>();
                displayBoard[x, y].transform.SetParent(transform, false);
                displayBoard[x, y].SetCoordinates(new Vector2(x, y));
                //Nulls out the grid
                currentBoardState.SetValue(x, y, ' ');
            }
        }
        //Displays the current player
        NewTurn(players[currentPlayer]);
    }

    /// <summary>
    /// Sets both the currentBoardState & displayBoard to match the newBoardState
    /// </summary>
    /// <param name="board"></param>
    void UpdateBoard(Board newBoardState)
    {
        for(int x = 0; x < gridSize; x++)
        {
            for(int y = 0; y < gridSize ; y++)
            {
                currentBoardState.SetValue(x,y,newBoardState.GetValue(x, y));
                displayBoard[x, y].SetDisplay(newBoardState.GetValue(x, y));
            }
        }
    }

    void OnSpaceSelected(BoardSpace space)
    {
        space.SetDisplay(players[currentPlayer].Character());
        currentBoardState.SetValue(space.GetCoordinates(), players[currentPlayer].Character());
        //Advances player
        currentPlayer++;
        if(currentPlayer >= players.Length)
        {
            currentPlayer = 0;
        }
        NewTurn(players[currentPlayer]);
    }

    public static bool isAiTurn()
    {
        return instance.players[instance.currentPlayer].IsAi();
    }
}
