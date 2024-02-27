using UnityEngine;

public class Piece : MonoBehaviour
{
    [Tooltip("If yes, placing this piece's head on the opponent's home row wins the game.")]
    [SerializeField] bool canWin;
    [Tooltip("The blocks that make up this piece. Index 0 should be the head.")]
    [SerializeField] GameObject[] blocks;
    GridSquare headSquare;
    GridSquare tailSquare;
    [Tooltip("How far above the grid the pieces sit")]
    [SerializeField] float heightOffset;
    [Tooltip("The player who owns this piece")]
    Player owner;
    Direction direction;
    bool mouseDown = false;

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
            if(GameManager.GetSelectedPiece() == this && mouseDown)
            {
                mouseDown = false;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, GameManager.boardClickMask))
                {
                    //Trys to rotate. if cannot rotate, trys to move
                    GridSquare destination = hit.collider.GetComponent<GridSquare>();
                    if(TryRotateToSpace(destination) || TryMoveToSpace(destination))
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
        //Trys to rotate from the head
        int rotateDistance = GridManager.DistanceToSquare(headSquare, destination);
        Direction rotateDirection = GridManager.DirectionBetweenSquares(destination, headSquare);
        if (rotateDistance == blocks.Length - 1 && GridManager.AreDirectionsAdjacent(direction, rotateDirection))
        {
            SetPosition(headSquare, rotateDirection);
            return true;
        }
        //If not, trys to rotate from the tail
        rotateDistance = GridManager.DistanceToSquare(tailSquare, destination);
        rotateDirection = GridManager.DirectionBetweenSquares(tailSquare, destination);
        if (rotateDistance == blocks.Length - 1 && GridManager.AreDirectionsAdjacent(direction, rotateDirection))
        {
            SetPosition(GridManager.SquareInDirection(tailSquare, rotateDirection, blocks.Length - 1), rotateDirection);
            return true;
        }
        //Debug.Log(tailSquare.GetCoordinates().x + "," + tailSquare.GetCoordinates().x + "\n" + headSquare.GetCoordinates().x + "," + headSquare.GetCoordinates().y + "\n" + rotateDistance + "\n" + rotateDirection + ", " + direction);
        //If cannot rotate from head or tail, returns false
        return false;
    }
    /// <summary>
    /// If it successfulyl moves to a space, returns true. Otherwise, returns false.
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    bool TryMoveToSpace(GridSquare destination)
    {
        //If you're facing the correct direction, move
        if(GridManager.DirectionBetweenSquares(headSquare, destination) == direction)
        {
            SetPosition(destination, direction);
            return true;
        }
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
        transform.position = headPosition.transform.position + new Vector3(0,heightOffset,0);
        headSquare = headPosition;
        tailSquare = GridManager.SquareInDirection(headPosition, GridManager.OppositeDirection(headDirection), blocks.Length - 1);
        direction = headDirection;
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
    }
    /// <summary>
    /// Sets the player in char of this piece
    /// </summary>
    /// <param name="_owner"></param>
    public void SetOwner(Player _owner)
    {
        owner = _owner;
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
        if (GameManager.currentPlayer == owner)
        {
            GameManager.SelectPiece(this);
        }
    }

    void Deselect()
    {
    }
    /// <summary>
    /// Call when you successfully move this piece
    /// </summary>
    void EndMove()
    {
        GameManager.SelectPiece(null);
        GameManager.AdvanceTurn();
    }
}
