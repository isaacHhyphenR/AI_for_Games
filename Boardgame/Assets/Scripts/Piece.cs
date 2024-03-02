using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [Tooltip("If yes, placing this piece's head on the opponent's home row wins the game.")]
    [SerializeField] bool canWin;
    [Tooltip("The blocks that make up this piece. Index 0 should be the head.")]
    [SerializeField] GameObject[] blocks;
    int length;
    GridSquare[] currentSquares;
    [Tooltip("How far above the grid the pieces sit")]
    [SerializeField] float heightOffset;
    [Tooltip("The player who owns this piece")]
    Player owner;
    [Tooltip("The direction the piece is currently facing")]
    Direction direction;
    bool mouseDown = false;
    [Tooltip("This effect will play when the piece is destroyed in combat")]
    [SerializeField] GameObject destructionParticle;
    [Tooltip("This renderer will light up when this piece is selected/selectable")]
    [SerializeField] Renderer outline;
    [SerializeField] Color selectedColor;
    [SerializeField] Color selectableColor;


    private void Update()
    {
        //If you're selected, check whether a valid space is clicked
        if(Input.GetMouseButton(1))
        {
            mouseDown = true;
        }
        //On releasing right mouse, check for movement
        else
        {
            if(GameManager.GetSelectedPiece() == this && mouseDown && GameManager.canPlay)
            {
                mouseDown = false;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, GameManager.boardClickMask))
                {
                    GridSquare destination = hit.collider.GetComponent<GridSquare>();
                    //Trys to rotate. if cannot rotate, trys to move
                    if (CanRotateToSpace(destination) || CanDashToSpace(destination))
                    {
                        Direction moveDirection = GridManager.DirectionBetweenSquares(GetTailLocation(), destination);
                        //Slightly different rules if rotating around the tail
                        if (moveDirection == Direction.DIAGONAL)
                        {
                            moveDirection = GridManager.DirectionBetweenSquares(destination, GetHeadLocation());
                            destination = GetHeadLocation();
                        }
                        SetPosition(destination,moveDirection);
                        EndMove();
                    }
                }
            }
            mouseDown = false;
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.currentPlayer == owner && !owner.IsAI())
        {
            GameManager.SelectPiece(this);
            Select();
        }
    }
    /// <summary>
    /// If it sucessfully rotates to a space, returns true. Otherwise, returns false
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    bool CanRotateToSpace(GridSquare destination)
    {
        return (CanRotateFromTo(GetHeadLocation(), destination, true) || CanRotateFromTo(GetTailLocation(), destination, false));
    }
    /// <summary>
    /// Returns whether the piece could rotate to the space
    /// </summary>
    /// <param name="start"></param>
    /// <param name="destination"></param>
    /// <param name="startIsHead">Whether the head is the stationary pivot point for the rotation</param>
    /// <returns></returns>
    bool CanRotateFromTo(GridSquare start, GridSquare destination, bool startIsHead)
    {
        int rotateDistance = GridManager.DistanceToSquare(start, destination);
        Direction rotateDirection = GridManager.DirectionBetweenSquares(start, destination);
        GridSquare headDestination = destination;
        if (startIsHead)
        {
            headDestination = start;
        }
        //Trys the actual rotation
        if (rotateDistance == length - 1 && GridManager.AreDirectionsAdjacent(direction, rotateDirection))
        {
            return !MoveBlockedByPiece(start, destination, headDestination, rotateDirection);
        }
        return false;
    }
    /// <summary>
    /// Returns whether the piece could dash to the space
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    bool CanDashToSpace(GridSquare destination)
    {
        //If you're facing the correct direction, move
        if(GridManager.DirectionBetweenSquares(currentSquares.First(), destination) == direction)
        {
            return !MoveBlockedByPiece(GetHeadLocation(), destination, destination, direction);
        }
        return false;
    }


    /// <summary>
    /// Returns true if there is a piece blocking the way
    /// </summary>
    /// <returns></returns>
    bool MoveBlockedByPiece(GridSquare start, GridSquare destination, GridSquare headDestination, Direction direction, Piece exception = null)
    {
        return MoveBlockedByPiece(start, destination, headDestination, direction, this, exception);
    }
    /// <summary>
    /// Returns true if there is a piece blocking the way
    /// </summary>
    /// <returns></returns>
    public static bool MoveBlockedByPiece(GridSquare start, GridSquare destination, GridSquare headDestination, Direction direction, Piece piece, Piece exception = null)
    {
        Collision collision = GridManager.FirstPieceEncountered(start, direction, GridManager.DistanceToSquare(start, destination));
        //You can move if there's no piece in the way
        if (collision.piece == null || collision.piece == piece || collision.piece == exception)
        {
            return false;
        }
        //If there's a piece in the way, you can destroy it if it's a differnet player AND
        //you are not landing on its head AND it's your head landing on it
        if (collision.piece.GetOwner() != piece.owner &&
            collision.piece.GetHeadLocation() != collision.square && collision.square == headDestination)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Places the piece's head on the designated square with the head facing in the designated direction
    /// </summary>
    /// <param name="headPosition"></param>
    /// <param name="stemDirection"></param>
    public void SetPosition(GridSquare headPosition, Direction headDirection)
    {
        length = blocks.Length;
        //Tells the current gridsquares it's gone
        if (currentSquares != null)
        {
            foreach (GridSquare square in currentSquares)
            {
                square.SetPiece(null);
            }
        }
        //Moves the piece itself
        transform.position = headPosition.transform.position + new Vector3(0,heightOffset,0);
        direction = headDirection;
        currentSquares = GridManager.SquaresInDirection(headPosition, GridManager.OppositeDirection(direction), length - 1, true);
        switch (direction)
        {
            case Direction.NORTH:
                transform.localEulerAngles = new Vector3(0, 90,0);
                break;
            case Direction.EAST:
                transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case Direction.SOUTH:
                transform.localEulerAngles = new Vector3(0, 270, 0);
                break;
            case Direction.WEST:
                transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
        }
        //Tells the new squares it has arrived
        foreach (GridSquare square in currentSquares)
        {
            if(square.GetPiece())
            {
                square.GetPiece().Destroy();
            }
            square.SetPiece(this);
        }
        //Checks if it conquered the opponent's home row
        if(canWin && GetHeadLocation().GetCoordinates().y == GameManager.GetOtherPlayer(owner).GetHomeRow())
        {
            GameManager.GetOtherPlayer(owner).lostReason = "lost their homerow.";
            GameManager.PlayerLost(GameManager.GetOtherPlayer(owner));
        }
    }
    /// <summary>
    /// Sets the player in char of this piece
    /// </summary>
    /// <param name="_owner"></param>
    public void SetOwner(Player _owner)
    {
        owner = _owner;
    }
    public Player GetOwner()
    {
        return owner;
    }

    /// <summary>
    /// Returns whether this piece will win the game upon placing it's head on the opponent's homerow
    /// </summary>
    /// <returns></returns>
    public bool GetCanWin()
    {
        return canWin;
    }

    /// <summary>
    /// Removes the piece from the game
    /// </summary>
    private void Destroy()
    {
        foreach(GameObject block in blocks)
        {
            ParticleSystem particle = Instantiate(destructionParticle, block.transform.position, block.transform.rotation).GetComponent<ParticleSystem>();
            var main = particle.main;
            main.startColor = owner.GetPieceColor();
        }
        owner.RemovePiece(this);
        Destroy(gameObject);
    }

    void Select()
    {
        outline.material.color = selectedColor;
    }
    public void Deselect()
    {
        if (GameManager.currentPlayer == owner)
        {
            outline.material.color = selectableColor;
        }
        else
        {
            outline.material.color = Color.clear;
        }
    }

    /// <summary>
    /// Sets the head to the specified color
    /// </summary>
    /// <param name="color"></param>
    public void SetHeadColor(Color headColor)
    {
        blocks[0].GetComponent<Renderer>().material.color = headColor;
    }
    /// <summary>
    /// Call when you successfully move this piece
    /// </summary>
    public void EndMove()
    {
        GameManager.SelectPiece(null);
        GameManager.AdvanceTurn();
    }
    /// <summary>
    /// Returns the gridSquare where this piece's head is located
    /// </summary>
    /// <returns></returns>
    public GridSquare GetHeadLocation()
    {
        return currentSquares.First();
    }
    /// <summary>
    /// Returns the gridSquare where this piece's tail is located
    /// </summary>
    /// <returns></returns>
    public GridSquare GetTailLocation()
    {
        return currentSquares.Last();
    }
    /// <summary>
    /// Sets the outline color to selectable
    /// </summary>
    public void StartTurn()
    {
        outline.material.color = selectableColor;
    }
    /// <summary>
    /// Sets the outline to clear
    /// </summary>
    public void EndTurn()
    {
        outline.material.color = Color.clear;
    }

    /// <summary>
    /// Returns all moves this piece can make from the specified position
    /// </summary>
    /// <param name="calculateWeights">If false, leaves all their weights as default. Significantly reduces intensity</param>
    /// <param name="includeBlockedByPieces">If true, will include moves that other pieces currently block</param>
    /// <returns></returns>
    public List<Move> GetAllMoves(GridSquare startHead, Direction startDirection, bool calculateWeights, bool includeBlockedByPieces = false)
    {
        GridSquare startTail = GridManager.SquareInDirection(startHead, GridManager.OppositeDirection(startDirection), GetLength() - 1);
        List<Move> moves = new List<Move>();
        //Checks all possible head rotations
        for(int i = 0; i < 4; i++)
        {
            Direction newDir = (Direction)i;
            if (GridManager.AreDirectionsAdjacent(startDirection, newDir))
            {
                GridSquare destination = GridManager.SquareInDirection(startHead, GridManager.OppositeDirection(newDir), length - 1);
                if (destination != null && (includeBlockedByPieces || !MoveBlockedByPiece(startHead, destination, startHead, GridManager.OppositeDirection(newDir))))
                {
                    moves.Add(new Move(this, startHead, newDir, MoveType.HEAD_ROTATION, calculateWeights));
                }
            }
        }
        //Checks all possible tail rotations
        for (int i = 0; i < 4; i++)
        {
            Direction newDir = (Direction)i;
            if (GridManager.AreDirectionsAdjacent(startDirection, newDir))
            {
                GridSquare destination = GridManager.SquareInDirection(startTail, newDir, length - 1);
                if (destination != null && (includeBlockedByPieces || !MoveBlockedByPiece(startTail, destination, destination, newDir)))
                {
                    moves.Add(new Move(this, destination, newDir, MoveType.TAIL_ROTATION, calculateWeights));
                }
            }
        }
        //Checks all possible dashes
        GridSquare[] dashSquares = GridManager.SquaresInDirection(startHead, startDirection, 100, false);
        for (int i = 0; i < dashSquares.Length; i++)
        {
            if (!includeBlockedByPieces && MoveBlockedByPiece(startHead, dashSquares[i], dashSquares[i], direction))
            {
                break;
            }
            else if (!includeBlockedByPieces && dashSquares[i].GetPiece())
            {
                moves.Add(new Move(this, dashSquares[i], startDirection, MoveType.DASH, calculateWeights));
                break;
            }
            else
            {
                moves.Add(new Move(this, dashSquares[i], startDirection, MoveType.DASH, calculateWeights));
            }
        }

        return moves;
    }
    /// <summary>
    /// Returns all moves the piece can make from its current position
    /// </summary>
    /// <param name="calculateWeights">If false, leaves all their weights as default. Significantly reduces intensity</param>
    /// <param name="includeBlockedByPieces">If true, will include moves that other pieces currently block</param>
    /// <returns></returns>
    public List<Move> GetAllMoves(bool calculateWeights, bool includeBlockedByPieces = false)
    {
        return GetAllMoves(GetHeadLocation(), direction, calculateWeights, includeBlockedByPieces);
    }
    /// <summary>
    /// Returns whether this piece can make any valid moves
    /// </summary>
    /// <returns></returns>
    public bool AnyValidMove()
    {
        //Checks all possible head rotations
        for (int i = 0; i < 4; i++)
        {
            Direction newDir = (Direction)i;
            if (GridManager.AreDirectionsAdjacent(direction, newDir))
            {
                GridSquare destination = GridManager.SquareInDirection(GetHeadLocation(), GridManager.OppositeDirection(newDir), length - 1);
                if (destination != null && !MoveBlockedByPiece(GetHeadLocation(), destination, GetHeadLocation(), GridManager.OppositeDirection(newDir)))
                {
                    return true;
                }
            }
        }
        //Checks all possible tail rotations
        for (int i = 0; i < 4; i++)
        {
            Direction newDir = (Direction)i;
            if (GridManager.AreDirectionsAdjacent(direction, newDir))
            {
                GridSquare destination = GridManager.SquareInDirection(GetTailLocation(), newDir, length - 1);
                if (!(destination != null && !MoveBlockedByPiece(GetTailLocation(), destination, destination, newDir)))
                {
                    return true;
                }
            }
        }
        //Checks all possible dashes
        GridSquare[] dashSquares = GridManager.SquaresInDirection(GetHeadLocation(), direction, 100, false);
        for (int i = 0; i < dashSquares.Length; i++)
        {
            if (!MoveBlockedByPiece(GetHeadLocation(), dashSquares[i], dashSquares[i], direction))
            {
                return true;
            }
        }
        return false;
    }

    public Direction GetDirection()
    {
        return direction;
    }

    public int GetLength()
    {
        return length;
    }
}
