using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    List<Piece> pieces = new List<Piece>();
    [Tooltip("This player's primary color. Pieces will use a variant")]
    [SerializeField] Color color;
    [Tooltip("The pieces will be this much darker")]
    [SerializeField] float pieceDarkening;
    [Tooltip("The Y positions the pieces start on. Index 0 is always queen")]
    [SerializeField] int[] startingX;
    int homerow;

    /// <summary>
    /// Gives this player control of a new piece
    /// </summary>
    /// <param name="piece"></param>
    public void AddPiece(Piece piece)
    {
        if (pieces.Count > 0)
        {
            piece.gameObject.name = gameObject.name + "_Pawn" + pieces.Count;
        }
        else
        {
            piece.gameObject.name = gameObject.name + "_Queen";
        }
        pieces.Add(piece);
        piece.SetHeadColor(GetPieceColor());
        piece.SetOwner(this);
    }
    /// <summary>
    /// Removes the specified piece from your control
    /// </summary>
    /// <param name="piece"></param>
    public void RemovePiece(Piece piece)
    {
        pieces.Remove(piece);
        //If you're out of pieces, lose
        if(pieces.Count == 0)
        {
            GameManager.PlayerLost(this);
        }
    }

    public void StartTurn()
    {
        foreach(Piece piece in pieces)
        {
            piece.StartTurn();
        }
    }
    public void EndTurn()
    {
        foreach(Piece piece in pieces)
        {
            piece.EndTurn();
        }
    }

    /// <summary>
    /// Sets the row that this player considers home
    /// </summary>
    /// <param name="row"></param>
    public void SetHomeRow(int row)
    {
        homerow = row;
    }
    public int GetHomeRow()
    {
        return homerow;
    }
    public int GetStartingX(int index)
    {
        return startingX[index];
    }
    public Color GetColor()
    {
        return color;
    }
    public Color GetPieceColor()
    {
        return color * pieceDarkening;
    }
    public string GetName()
    {
        return gameObject.name;
    }
}
