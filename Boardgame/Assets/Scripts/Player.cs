using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;


public struct Move
{
    public Piece piece;
    public GridSquare destination;
    public Direction direction;
    public float weight;
    public Move(Piece _piece, GridSquare _destination, Direction _direction)
    {
        piece = _piece;
        destination = _destination;
        direction = _direction;
        weight = 1;
        SetWeights();
    }
    public void MakeMove()
    {
        piece.SetPosition(destination, direction);
        piece.EndMove();
        Debug.Log(piece.gameObject.name + " moves to " +  destination.gameObject.name + ". Weight: " + weight);
    }
    /// <summary>
    /// Calculates the AI's desire to perform this move
    /// </summary>
    /// <param name="destroysEnemy"></param>
    public void SetWeights()
    {
        if (destination.GetPiece() && destination.GetPiece() != piece)
        {
            weight *= piece.GetOwner().aiDestroyDesire;
        }
        if (piece.GetCanWin() && destination.GetCoordinates().y == GameManager.GetOtherPlayer(piece.GetOwner()).GetHomeRow())
        {
            weight += 100000000;
        }
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
    float aiTurnTimer = 0;
    List<Move> potentialMoves = new List<Move>();
    [Tooltip("The sum of every potentialMove's weight")]
    float totalMoveWeight = 0;
    int homerow;
    [Header("AI")]
    [Tooltip("If true, the AI will control all moves.")]
    [SerializeField] bool AI;
    [Tooltip("How long the AI will wait to take its turn")]
    [SerializeField] float aiTurnTime;
    [Tooltip("How much more this AI wants to make a move that will destory an enemy piece. Multiplicative")]
    public float aiDestroyDesire;

    public void StartTurn()
    {
        foreach (Piece piece in pieces)
        {
            piece.StartTurn();
        }
        //Checks for valid moves; if none, you lose! Also compiles for the AI
        FindPotentialMoves();
        if (AI)
        {
            aiTurnTimer = 0;
        }
    }
    public void EndTurn()
    {
        foreach (Piece piece in pieces)
        {
            piece.EndTurn();
        }
    }

    private void Update()
    {
        if(AI && GameManager.currentPlayer == this && GameManager.canPlay)
        {
            aiTurnTimer += Time.deltaTime;
            if(aiTurnTimer > aiTurnTime)
            {
                ChooseAnMakeRandomMove();

            }
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
        //If no possible moves, you lose
        if(potentialMoves.Count == 0)
        {
            GameManager.PlayerLost(this);
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
    /// Makes a random move based on their weights, then returns it
    /// </summary>
    Move ChooseAnMakeRandomMove()
    {
        Move moveMade = ChooseRandomMove();
        moveMade.MakeMove();
        return moveMade;
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
