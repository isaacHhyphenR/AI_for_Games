using UnityEngine;

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

    BoardSpace[,] displayBoard;
    Board currentBoardState;

    private void Start()
    {
        currentBoardState = new Board(gridSize);
        displayBoard = new BoardSpace[gridSize, gridSize];
        //Spawns the board squares
        float squareSize = squarePrefab.GetComponent<BoardSpace>().GetSize();
        Vector2 offset = new Vector2(squareSize * gridSize / 2, -squareSize * gridSize / 2);
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 newPos = new Vector3(x * squareSize - offset.x, 0, y * -squareSize - offset.y);
                displayBoard[x, y] = Instantiate(squarePrefab, newPos, Quaternion.identity).GetComponent<BoardSpace>();
                displayBoard[x, y].transform.SetParent(transform, false);
            }
        }
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
}
