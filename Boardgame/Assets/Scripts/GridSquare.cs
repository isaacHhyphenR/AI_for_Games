using UnityEngine;

public class GridSquare : MonoBehaviour
{
    [SerializeField] Renderer render;

    GridSquare[] neighbours = new GridSquare[4];

    Coordinate coordinates;
    [Tooltip("The piece, if any, occupying this space.")]
    Piece piece = null;

    /// <summary>
    /// Sets the neighbouring gridsquare in the specified direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public void SetNeighbour(Direction direction, GridSquare neighbour)
    {
        neighbours[(int)direction] = neighbour;
    }
    /// <summary>
    /// Returns the neighbouring gridsquare in the specified direction; null if none
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public GridSquare GetNeighbour(Direction direction)
    {
        return neighbours[(int)direction];
    }

    public void SetCoordinates(Coordinate _coordinates)
    {
        coordinates = _coordinates;
    }
    public Coordinate GetCoordinates()
    {
        return coordinates;
    }

    public void SetColor(Color newColor)
    {
        render.material.color = newColor;
    }
    public float GetSize()
    {
        return transform.localScale.x;
    }

    public void SetPiece(Piece _piece)
    {
        piece = _piece;
    }
    public Piece GetPiece()
    {
        return piece;
    }
}
