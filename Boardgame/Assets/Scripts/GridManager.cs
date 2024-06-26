using Unity.VisualScripting;
using UnityEngine;

public enum Direction
{
    NORTH = 0,
    EAST,
    SOUTH,
    WEST,
    DIAGONAL
}

[System.Serializable]

/// <summary>
/// An integer X Y pair
/// </summary>
public struct Coordinate
{
    public int x;
    public int y;
    public Coordinate(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public override string ToString()
    {
        return "" + x + "," + y;
    }

    public static bool operator ==(Coordinate lhs, Coordinate rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y;
    }
    public static bool operator !=(Coordinate lhs, Coordinate rhs)
    {
        return !(lhs == rhs);
    }
}
/// <summary>
/// A Hypthetical boardstate
/// </summary>
public struct BoardState
{
    public GridSquare[,] grid;
    public Piece[,] PlayerPieces;
    public BoardState(GridSquare[,] _grid)
    {
        grid = new GridSquare[_grid.GetLength(0), _grid.GetLength(1)];
        PlayerPieces = new Piece[2,3];
    }
    /// <summary>
    /// Sets the grid & pieces to match the parent
    /// </summary>
    /// <param name="parent"></param>
    public void Inherit(BoardState parent)
    {
        //Sets the grid
        for(int x = 0; x < GridManager.GridSize().x; x++)
        {
            for (int y = 0; y < GridManager.GridSize().y; y++)
            {
                grid[x, y] = GameObject.Instantiate(parent.grid[x, y]);
                grid[x, y].board = this;
            }
        }
        //Sets pieces
        for(int i = 0; i < PlayerPieces.GetLength(0); i++)
        {
            for (int j = 0; j < PlayerPieces.GetLength(1); j++)
            {
                //Must SetPosition after instantiate to line up all of the GridSquare objects
                PlayerPieces[i,j] = GameObject.Instantiate(parent.PlayerPieces[i,j]);
                PlayerPieces[i,j].SetBoard(this);
                PlayerPieces[i,j].SetPosition(parent.PlayerPieces[i,j].GetPosition());
            }
        }
    }
    /// <summary>
    /// Returns the gridsquare in the specified direciton & distance to the start
    /// </summary>
    /// <param name="start"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public GridSquare SquareInDirection(GridSquare start, Direction direction, int distance)
    {
        Coordinate end = GridManager.CoordinateInDirection(start.GetCoordinates(), direction, distance);
        if (end.x < grid.GetLength(0) && end.y < grid.GetLength(1) && end.x >= 0 && end.y >= 0)
        {
            return grid[end.x, end.y];
        }
        return null;
    }
    /// <summary>
    /// Returns all of the gridsquares within distance of start along the specified direction. If includeStart is set to true, start will be the first entry
    /// </summary>
    /// <param name="start"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public GridSquare[] SquaresInDirection(GridSquare start, Direction direction, int distance, bool includeStart)
    {
        int offset = 0;
        int squaresLength = 0;
        GridSquare[] squares = new GridSquare[distance];
        if (includeStart)
        {
            offset = 1;
            squaresLength++;
            squares = new GridSquare[distance + 1];
            squares[0] = start;
        }
        //Finds every square in the direction
        for (int i = 0; i < distance; i++)
        {
            GridSquare newSquare = SquareInDirection(start, direction, i + 1);
            if (newSquare != null)
            {
                squaresLength++;
                squares[i + offset] = newSquare;
            }
            //If it exits the board, resize and return squares
            else
            {
                GridSquare[] tempSquares = new GridSquare[squaresLength];
                for (int j = 0; j < squaresLength; j++)
                {
                    tempSquares[j] = squares[j];
                }
                return tempSquares;
            }
        }
        return squares;
    }

    /// <summary>
    /// Returns the first piece occupying squares in the direction & distance from start, along with the square it was encountered on. Returns a null collision is none
    /// </summary>
    /// <param name="start"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public Collision FirstPieceEncountered(GridSquare start, Direction direction, int distance)
    {
        GridSquare[] squaresToSearch = SquaresInDirection(start, direction, distance, false);
        foreach (GridSquare square in squaresToSearch)
        {
            if (square && square.GetPiece())
            {
                return new Collision(square.GetPiece(), square);
            }
        }
        return new Collision(null, null);
    }

    public GridSquare GetSquare(Coordinate coordinate)
    {
        if(grid == null)
        {
            Debug.Log("Grid null on GetSquare");
        }
        return grid[coordinate.x, coordinate.y];
    }

    public static bool operator ==(BoardState lhs, BoardState rhs)
    {
        for(int i = 0; i < lhs.PlayerPieces.GetLength(0); i++)
        {
            for (int j = 0; j < lhs.PlayerPieces.GetLength(1); j++)
            {
                if (lhs.PlayerPieces[i,j].GetPosition() == rhs.PlayerPieces[i,j].GetPosition())
                {
                    return false;
                }
            }
        }
        //If not differences, they are equal
        return true;
    }
    public static bool operator !=(BoardState lhs, BoardState rhs)
    {
        return !(lhs == rhs);
    }
}

/// <summary>
/// Specifies a square and a piece
/// </summary>
public struct Collision
{
    public Piece piece;
    public GridSquare square;
    public Collision(Piece _piece, GridSquare _square)
    {
        piece = _piece;
        square = _square;
    }
}

public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("The size, in squares, of the grid")]
    [SerializeField] Coordinate gridSize;
    [Tooltip("The prefab to build the grid out of")]
    [SerializeField] GameObject gridPrefab;
    [Header("Pieces")]
    [Tooltip("The prefab to spawn the pawns")]
    [SerializeField] GameObject pawnPrefab;
    [Tooltip("The prefab to spawn the queens")]
    [SerializeField] GameObject queenPrefab;

    BoardState primaryBoard;
    float squareSize;

    static GridManager instance;
    GridSquare[,] grid;

    void Start()
    {
        instance = this;
        GenerateGrid();
        SpawnPieces();
    }
    /// <summary>
    /// Generates a full grid, including adding colors and neighbours
    /// </summary>
    void GenerateGrid()
    {
        bool rowStartsFirstColor = true;
        //Determines size for the grid
        squareSize = gridPrefab.GetComponent<GridSquare>().GetSize();
        grid = new GridSquare[gridSize.x, gridSize.y];
        Vector2 offset = new Vector2(squareSize * gridSize.x / 2, -squareSize * gridSize.y / 2);
        //Sets up grid
        for (int y = 0; y < gridSize.y; y++)
        {
            bool firstColor = rowStartsFirstColor;
            rowStartsFirstColor = !rowStartsFirstColor;
            for (int x = 0; x < gridSize.x; x++)
            {
                Vector3 newPos = new Vector3(x * squareSize - offset.x, 0, y * -squareSize - offset.y);
                GridSquare newSquare = Instantiate(gridPrefab, newPos, Quaternion.identity).GetComponent<GridSquare>();
                newSquare.transform.SetParent(transform, false);
                newSquare.SetCoordinates(new Coordinate(x, y));
                newSquare.gameObject.name = "Square_" + (x + 1) + "," + (y + 1);
                grid[x, y] = newSquare;
                //Checkerboard colors
                if (firstColor)
                {
                    newSquare.SetColor(GameManager.players[0].GetColor());
                }
                else
                {
                    newSquare.SetColor(GameManager.players[1].GetColor());
                }
                firstColor = !firstColor;
            }
        }
        //Sets the player's homerows
        GameManager.players[0].SetHomeRow(0);
        GameManager.players[1].SetHomeRow(gridSize.y - 1);
        primaryBoard = new BoardState(grid);
    }
    /// <summary>
    /// Spawns the initial 2 pawns & 1 queen per side
    /// </summary>
    void SpawnPieces()
    {
        bool firstDirection = true;
        Direction dir;
        foreach (Player player in GameManager.players)
        {
            //Determines direction
            if (firstDirection)
            {
                dir = Direction.NORTH;
            }
            else
            {
                dir = Direction.SOUTH;
            }
            firstDirection = !firstDirection;
            //Queen
            Piece piece = Instantiate(queenPrefab, player.transform).GetComponent<Piece>();
            player.AddPiece(piece);
            piece.SetBoard(primaryBoard);
            if(piece.GetBoard().grid == null)
            {
                Debug.Log("WTF");
            }
            piece.SetPosition(new Coordinate(player.GetStartingX(0), player.GetHomeRow()), dir);
            //Pawn1
            piece = Instantiate(pawnPrefab, player.transform).GetComponent<Piece>();
            player.AddPiece(piece);
            piece.SetBoard(primaryBoard);
            piece.SetPosition(new Coordinate(player.GetStartingX(1), player.GetHomeRow()), dir);
            //Pawn2
            piece = Instantiate(pawnPrefab, player.transform).GetComponent<Piece>();
            player.AddPiece(piece);
            piece.SetBoard(primaryBoard);
            piece.SetPosition(new Coordinate(player.GetStartingX(2), player.GetHomeRow()), dir);
        }
        //Adds the pieces to the primary board
        for (int i = 0; i < GameManager.players.Length; i++)
        {
            for(int j = 0; j < GameManager.players[i].GetPieces().Count; i++)
            {
                primaryBoard.PlayerPieces[i, j] = GameManager.players[i].GetPieces()[j];
                GameManager.players[i].GetPieces()[j].SetBoard(primaryBoard);
            }
        }
    }
    /// <summary>
    /// Returns the Direction between two coordinates. DIAGONAL is not valid
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static Direction DirectionBetweenCoordinates(Coordinate start, Coordinate end)
    {
        if (end.y > start.y && end.x == start.x)
        {
            return Direction.SOUTH;
        }
        else if (end.y < start.y && end.x == start.x)
        {
            return Direction.NORTH;
        }
        else if (end.x > start.x && end.y == start.y)
        {
            return Direction.EAST;
        }
        else if (end.x < start.x && end.y == start.y)
        {
            return Direction.WEST;
        }
        return Direction.DIAGONAL;
    }
    /// <summary>
    /// Returns the Direction between two positions. DIAGONAL is not valid
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static Direction DirectionBetweenPositions(Vector3 start, Vector3 end)
    {
        return DirectionBetweenCoordinates(new Coordinate((int)start.x, (int)start.z), new Coordinate((int)end.x, (int)end.z));
    }
    /// <summary>
    /// Returns the Direction between two gridquares. DIAGONAL is not valid
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static Direction DirectionBetweenSquares(GridSquare start, GridSquare end)
    {
        return DirectionBetweenCoordinates(start.GetCoordinates(), end.GetCoordinates());
    }

    /// <summary>
    /// Returns the coordinates in the specified direction & distance to starts
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static Coordinate CoordinateInDirection(Coordinate start, Direction direction, int distance)
    {
        Coordinate end = start;
        if(direction == Direction.NORTH)
        {
            end.y -= distance;
        }
        else if(direction == Direction.SOUTH)
        {
            end.y += distance;
        }
        else if (direction == Direction.EAST)
        {
            end.x += distance;
        }
        else if (direction == Direction.WEST)
        {
            end.x -= distance;
        }
        return end;
    }

    /// <summary>
    /// Returns the larger of X or Y distance between 2 coordinates
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static int DistanceToCoordinate(Coordinate start, Coordinate end)
    {
        int xDist = Mathf.Abs(start.x - end.x);
        int yDist = Mathf.Abs(start.y - end.y);
        return Mathf.Max(xDist, yDist);
    }
    /// <summary>
    /// Returns the larger of X or Y coordinate distance between 2 squares
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static int DistanceToSquare(GridSquare start, GridSquare end)
    {
        return DistanceToCoordinate(start.GetCoordinates(), end.GetCoordinates());
    }

    /// <summary>
    /// Returns the direction that faces opposite to the input
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Direction OppositeDirection(Direction direction)
    {
        if (direction == Direction.NORTH)
        {
            return Direction.SOUTH;
        }
        if (direction == Direction.SOUTH)
        {
            return Direction.NORTH;
        }
        if (direction == Direction.EAST)
        {
            return Direction.WEST;
        }
        return Direction.EAST;
    }
    /// <summary>
    /// Returns true if the two directions are 90 degrees to each other
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static bool AreDirectionsAdjacent(Direction start, Direction end)
    {
        if(start == Direction.NORTH && (end == Direction.EAST || end == Direction.WEST))
        {
            return true;
        }
        if (start == Direction.SOUTH && (end == Direction.EAST || end == Direction.WEST))
        {
            return true;
        }
        if (start == Direction.EAST && (end == Direction.NORTH || end == Direction.SOUTH))
        {
            return true;
        }
        if (start == Direction.WEST && (end == Direction.NORTH || end == Direction.SOUTH))
        {
            return true;
        }
        return false;
    }

    public static Coordinate GridSize()
    {
        return instance.gridSize;
    }

    public static BoardState PrimaryBoard()
    {
        return instance.primaryBoard;
    }
}
