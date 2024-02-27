using UnityEngine;

public class GridSquare : MonoBehaviour
{
    [SerializeField] Renderer render;

    Coordinate coordinates;
    [Tooltip("The piece, if any, occupying this space.")]
    Piece piece = null;


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
