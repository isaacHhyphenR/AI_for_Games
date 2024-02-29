using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MoveType
{
    DASH,
    HEAD_ROTATION,
    TAIL_ROTATION
}

public struct Move
{
    public Piece piece;
    public GridSquare destination;
    public Direction direction;
    private MoveType type;
    public float weight;
    public Move(Piece _piece, GridSquare _destination, Direction _direction, MoveType _type)
    {
        piece = _piece;
        destination = _destination;
        direction = _direction;
        type = _type;
        weight = 1;
    }

    public bool MakeMove()
    {
        Debug.Log(piece.gameObject.name + " moves to " + destination.gameObject.name);
        if(type == MoveType.DASH)
        {
            return piece.TryMoveToSpace(piece.GetHeadLocation(), destination, direction);
        }
        if (type == MoveType.HEAD_ROTATION)
        {
            return piece.TryMoveToSpace(destination, piece.GetHeadLocation(), direction);
        }
        if (type == MoveType.TAIL_ROTATION)
        {
            return piece.TryMoveToSpace(piece.GetHeadLocation(), destination, direction);
        }
        return false;
    }
}

public class Player : MonoBehaviour
{
    List<Piece> pieces = new List<Piece>();
    [Tooltip("This player's primary color. Pieces will use a variant")]
    [SerializeField] Color color;
    [Tooltip("The pieces will be this much darker")]
    [SerializeField] float pieceDarkening;
    [Tooltip("The Y positions the pieces start on. Index 0 is always queen")]
    [SerializeField] int[] startingX;
    [Tooltip("If true, the AI will control all moves.")]
    [SerializeField] bool AI;
    List<Move> potentialMoves = new List<Move>();
    [Tooltip("The sum of every potentialMove's weight")]
    float totalMoveWeight = 0;
    int homerow;

    public void StartTurn()
    {
        foreach (Piece piece in pieces)
        {
            piece.StartTurn();
        }
        if (AI)
        {
            FindPotentialMoves();
            Move moveToMake = ChooseRandomMove();
            moveToMake.MakeMove();
            GameManager.AdvanceTurn();
        }
    }
    public void EndTurn()
    {
        foreach (Piece piece in pieces)
        {
            piece.EndTurn();
        }
    }

    void FindPotentialMoves()
    {
        //Clears last turn's list
        potentialMoves.Clear();
        totalMoveWeight = 0;
        //Compiles new list
        foreach(Piece piece in pieces)
        {
            List<Move> pieceMoves = piece.GetAllMoves();
            foreach(Move move in pieceMoves)
            {
                potentialMoves.Add(move);
                totalMoveWeight += move.weight;
            }
        }
    }
    /// <summary>
    /// Randomly chooses a move based on their weights
    /// </summary>
    Move ChooseRandomMove()
    {
        float rand = Random.Range(0, totalMoveWeight);
        float weightConsumed = 0;
        foreach(Move move in potentialMoves)
        {
            weightConsumed += move.weight;
            if(weightConsumed > rand)
            {
                return move;
            }
        }
        return potentialMoves.First();
    }

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

    public bool IsAI()
    {
        return AI;
    }
}
