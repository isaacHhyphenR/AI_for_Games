using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    bool takeTurnNow = false;

    public static GameManager instance;
    public static bool gameOver = false;


    public static char EMPTY_SQUARE = ' ';
    public static char STALEMATE = '\n';

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
    //Player Won
    public static UnityEvent<Player> playerWon = new UnityEvent<Player>();
    public static void PlayerWon(Player player)
    {
        playerWon.Invoke(player);
    }
    //Stalemate
    public static UnityEvent stalemate = new UnityEvent();
    public static void Stalemate()
    {
        stalemate.Invoke();
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
                currentBoardState.SetValue(x, y, EMPTY_SQUARE);
            }
        }
        //Displays the current player
        NewTurn(players[currentPlayer]);
    }

    private void Update()
    {
        if(!gameOver)
        {
            if (isAiTurn() && takeTurnNow)
            {
                takeTurnNow = false;
                Vector2 selectedMove = players[currentPlayer].ChooseMove(currentBoardState);
                displayBoard[(int)selectedMove.x, (int)selectedMove.y].SelectSpace();
            }
            else if (isAiTurn())
            {
                //So that the player's turn gets displayed before the Ai calculates
                takeTurnNow = true;
            }
        }
    }

    void OnSpaceSelected(BoardSpace space)
    {
        space.SetDisplay(players[currentPlayer].Character());
        currentBoardState.SetValue(space.GetCoordinates(), players[currentPlayer].Character());
        //Checks if game over
        char winner = IsWinningState(currentBoardState);
        if(winner == STALEMATE)
        {
            Stalemate();
        }
        else if(GetPlayer(winner) != null)
        {
            gameOver = true;
            PlayerWon(GetPlayer(winner));
        }
        //If the game must continue, then it does
        else
        {
            //Advances player
            currentPlayer = NextPlayer(currentPlayer);
            NewTurn(players[currentPlayer]);
        }
    }

    public static bool isAiTurn()
    {
        return instance.players[instance.currentPlayer].IsAi();
    }
    /// <summary>
    /// Returns the player object with this character
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public Player GetPlayer(char character)
    {
        foreach(Player player in players)
        {
            if(player.Character() == character)
            {
                return player;
            }
        }
        //If not a valid character, return null
        return null;
    }

    /// <summary>
    /// If one of the chars has won in state, returns that char. Returns '\n' if stalemate, returns ' ' otherwise
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static char IsWinningState(Board state)
    {
        char currentWinner = EMPTY_SQUARE;
        ///Checks columns
        for (int x = 0; x < state.GetGridSize(); x++)
        {
            currentWinner = state.GetValue(x, 0);
            for (int y = 1; y < state.GetGridSize(); y++)
            {
                //If the line is broken, try the next column
                if (currentWinner == EMPTY_SQUARE || currentWinner != state.GetValue(x, y))
                {
                    currentWinner = EMPTY_SQUARE;
                    break;
                }
            }
            //If one player controls the whole column, return that
            if (currentWinner != EMPTY_SQUARE)
            {
                return currentWinner;
            }
        }
        ///Checks rows
        for (int y = 0; y < state.GetGridSize(); y++)
        {
            currentWinner = state.GetValue(0, y);
            for (int x = 1; x < state.GetGridSize(); x++)
            {
                //If the line is broken, try the next row
                if (currentWinner == EMPTY_SQUARE || currentWinner != state.GetValue(x, y))
                {
                    currentWinner = EMPTY_SQUARE;
                    break;
                }
            }
            //If one player controls the whole row, return that
            if (currentWinner != EMPTY_SQUARE)
            {
                return currentWinner;
            }
        }
        ///Checks Top Left - Bottom Right diagonal
        {
            currentWinner = state.GetValue(0, 0);
            for (int x = 1; x < state.GetGridSize(); x++)
            {
                //If the line is broken, try the next row
                if (currentWinner == EMPTY_SQUARE || currentWinner != state.GetValue(x, x))
                {
                    currentWinner = EMPTY_SQUARE;
                    break;
                }
            }
            //If one player controls the whole row, return that
            if (currentWinner != EMPTY_SQUARE)
            {
                return currentWinner;
            }
        }
        ///Checks Top Right - Bottom Left diagonal
        {
            currentWinner = state.GetValue(0, state.GetGridSize() - 1);
            for (int x = 1; x < state.GetGridSize(); x++)
            {
                //If the line is broken, try the next row
                if (currentWinner == EMPTY_SQUARE || currentWinner != state.GetValue(x, state.GetGridSize() - 1 - x))
                {
                    currentWinner = EMPTY_SQUARE;
                    break;
                }
            }
            //If one player controls the whole row, return that
            if (currentWinner != EMPTY_SQUARE)
            {
                return currentWinner;
            }
        }
        ///If no-one won, check if there are any spaces left to play
        for (int x = 0; x < state.GetGridSize(); x++)
        {
            for (int y = 0; y < state.GetGridSize(); y++)
            {
                if(state.GetValue(x,y) == EMPTY_SQUARE)
                {
                    return EMPTY_SQUARE;
                }
            }
        }
        ///If no square left, stalemant
        return STALEMATE;
    }

    /// <summary>
    /// Returns an array of all the potential board state that could result from the current one
    /// </summary>
    /// <param name="initialState"></param>
    /// <param name="currentPlayer"></param>
    /// <returns></returns>
    public static List<Node> GenerateNextStates(Node initialState, Player currentPlayer)
    {
        return GenerateNextStates(initialState, currentPlayer.Character());
    }
    /// <summary>
    /// Returns an array of all the potential board state that could result from the current one
    /// </summary>
    /// <param name="initialState"></param>
    /// <param name="currentPlayer"></param>
    /// <returns></returns>
    public static List<Node> GenerateNextStates(Node initialState, char currentPlayer)
    {
        List<Node> nodes = new List<Node>();
        Board initialBoard = initialState.state;
        int statesGenerated = 0;
        for(int x = 0; x < initialBoard.GetGridSize(); x++)
        {
            for (int y = 0; y < initialBoard.GetGridSize(); y++)
            {
                //If X,Y is not yet claimed, claims it
                if(initialBoard.GetValue(x,y) == EMPTY_SQUARE)
                {
                    Board newBoard = new Board(instance.gridSize);
                    newBoard.Inherit(initialBoard);
                    newBoard.SetValue(x, y, currentPlayer);
                    nodes.Add(new Node(newBoard, initialState));
                    statesGenerated++;
                }
            }
        }
        return nodes;
    }


    /// <summary>
    /// Returns the index of the player who will take a turn after current
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public int NextPlayer(char current)
    {
        return NextPlayer(GetPlayer(current));
    }
    /// <summary>
    /// Returns the index of the player who will take a turn after current
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public int NextPlayer(int current)
    {
        current++;
        if(current >= players.Length)
        {
            current = 0;
        }
        return current;
    }
    /// <summary>
    /// Returns the index of the player who will take a turn after current
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public int NextPlayer(Player current)
    {
        for(int i = 0; i < players.Length; i++)
        {
            if (players[i] == current)
            {
                return NextPlayer(i);
            }
        }
        //If current isn't in the array somehow
        return currentPlayer;
    }
    /// <summary>
    /// Returns the index of the player who will take a turn after current
    /// </summary>
    /// <param name="current"></param>
    /// <returns></returns>
    public Player NextPlayerObject(char current)
    {
        return players[NextPlayer(GetPlayer(current))];
    }
}
