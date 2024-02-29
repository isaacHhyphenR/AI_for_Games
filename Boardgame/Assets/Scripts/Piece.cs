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
                    //Trys to rotate. if cannot rotate, trys to move
                    GridSquare destination = hit.collider.GetComponent<GridSquare>();
                    if(TryRotateToSpace(destination) || TryDashToSpace(destination))
                    {
                        EndMove();
                    }
                }
            }
            mouseDown = false;
        }
    }
    /// <summary>
    /// If it sucessfully rotates to a space, returns true. Otherwise, returns false
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    bool TryRotateToSpace(GridSquare destination)
    {
        return (RotateFromTo(currentSquares.First(), destination, true) || RotateFromTo(currentSquares.Last(), destination, false));
    }
    /// <summary>
    /// Tryes to rotate from the specified square to the specified quare
    /// </summary>
    /// <param name="start"></param>
    /// <param name="destination"></param>
    /// <param name="startIsHead">Determines the direction calculation</param>
    /// <returns></returns>
    bool RotateFromTo(GridSquare start, GridSquare destination, bool startIsHead)
    {
        int rotateDistance = GridManager.DistanceToSquare(start, destination);
        Direction rotateDirection = GridManager.DirectionBetweenSquares(start, destination);
        if(startIsHead)
        {
            rotateDirection = GridManager.DirectionBetweenSquares(destination, start);
        }
        //Trys the actual rotation
        if (rotateDistance == length - 1 && GridManager.AreDirectionsAdjacent(direction, rotateDirection))
        {
            //If you're rotating around the head the head doesn't move, so you need to pretend you were moving from the destination
            if(startIsHead)
            {
                return TryMoveToSpace(destination, start, rotateDirection);
            }
            else
            {
                return TryMoveToSpace(start, destination, rotateDirection);
            }
        }
        return false;
    }
    /// <summary>
    /// If it successfully dashes to a space, returns true. Otherwise, returns false.
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    bool TryDashToSpace(GridSquare destination)
    {
        //If you're facing the correct direction, move
        if(GridManager.DirectionBetweenSquares(currentSquares.First(), destination) == direction)
        {
            return TryMoveToSpace(currentSquares.First(), destination, direction);
        }
        return false;
    }
    /// <summary>
    /// Attempts to move to the specified position; position should already have been checked for move validity EXCEPT for other pieces
    /// </summary>
    /// <param name="start">The location from which the piece begins its move</param>
    /// <param name="head">The location the piece is trying to place its head</param>
    /// <param name="newDirection"></param>
    /// <returns></returns>
    public bool TryMoveToSpace(GridSquare start, GridSquare destination, Direction newDirection)
    {
        Collision collision = GridManager.FirstPieceEncountered(start, newDirection, GridManager.DistanceToSquare(start,destination));
        //You can move if there's no piece in the way
        if (collision.piece == null || collision.piece == this)
        {
            SetPosition(destination, newDirection);
            return true;
        }
        //If there's a piece in the way, you can destroy it if it's a differnet player AND
        //you are not landing on it's head && it's your head landing on it
        if (collision.piece != null && collision.piece.GetOwner() != owner &&
            collision.piece.GetHeadLocation() != collision.square && collision.square == destination)
        {
            collision.piece.Destroy();
            SetPosition(destination, newDirection);
            return true;
        }
        //If there's a piece you can't destroy, return false
        return false;
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
        //Tells the new squares its arrived
        foreach (GridSquare square in currentSquares)
        {
            square.SetPiece(this);
        }
        //Checks if it conquered the opponent's home row
        if(canWin && GetHeadLocation().GetCoordinates().y == GameManager.GetOtherPlayer(owner).GetHomeRow())
        {
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

    private void OnMouseDown()
    {
        if (GameManager.currentPlayer == owner && !owner.IsAI())
        {
            GameManager.SelectPiece(this);
            Select();
        }
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
    /// Call when you successfully move this piece
    /// </summary>
    void EndMove()
    {
        GameManager.SelectPiece(null);
        owner.EndTurn();
    }
    /// <summary>
    /// Returns the gridsquare where this piece's head is located
    /// </summary>
    /// <returns></returns>
    public GridSquare GetHeadLocation()
    {
        return currentSquares.First();
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
    /// Returns true if there is an enemy piece whose head blocks the move
    /// </summary>
    /// <param name="space"></param>
    /// <returns></returns>
    bool MoveBlockedByEnemy(GridSquare start, GridSquare destination, Direction direction)
    {
        Collision collision = GridManager.FirstPieceEncountered(start, direction, GridManager.DistanceToSquare(start, destination));
        //You can move if there's no piece in the way
        if (collision.piece == null || collision.piece == this)
        {
            return true;
        }
        //If there's a piece in the way, you can destroy it if it's a differnet player AND
        //you are not landing on it's head && it's your head landing on it
        if (collision.piece != null && collision.piece.GetOwner() != owner &&
            collision.piece.GetHeadLocation() != collision.square && collision.square == destination)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns all moves this piece can make
    /// </summary>
    /// <returns></returns>
    public List<Move> GetAllMoves()
    {
        List<Move> moves = new List<Move>();
        //Checks all possible head rotations
        for(int i = 0; i < 4; i++)
        {
            GridSquare destination = GridManager.SquareInDirection(GetHeadLocation(), (Direction)i, length - 1);
            if(destination != null && !MoveBlockedByEnemy(GetHeadLocation(), destination, (Direction)i))
            {
                moves.Add(new Move(this, destination, (Direction)i, MoveType.HEAD_ROTATION));
            }
        }

        return moves;
    }
}
