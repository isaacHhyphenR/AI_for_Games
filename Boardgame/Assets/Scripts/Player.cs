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
    public const float BASE_MOVE_WEIGHT = 1;
    public Piece piece;
    public GridSquare destination;
    public Direction direction;
    public float weight;
    Player owner;
    public MoveType type;
    public Move(Piece _piece, GridSquare _destination, Direction _direction, MoveType _type, bool calculateWeights)
    {
        piece = _piece;
        destination = _destination;
        direction = _direction;
        type = _type;
        owner = piece.GetOwner();
        weight = BASE_MOVE_WEIGHT;
        if(calculateWeights)
        {
            SetWeights(true);
        }
    }
    public void MakeMove()
    {
        piece.SetPosition(destination.GetCoordinates(), direction);
        piece.EndMove();
    }
    /// <summary>
    /// Calculates the AI's desire to perform this move
    /// </summary>
    /// <param name="recursive">If true, will consider the moves that the piece could make next turn</param>
    public void SetWeights(bool recursive = false)
    {
        SetWeights(piece.GetHeadSquare(), piece.GetDirection(), recursive);
    }
    /// <summary>
    /// Calculates the AI's desire to perform this move
    /// </summary>
    /// <param name="recursive">If true, will consider the moves that the piece could make next turn</param>
    public string SetWeights(GridSquare startHead, Direction startDirection, bool recursive = false)
    {
        string mods = "";
        bool inHarmsWay = false;
        bool canDoThatNow = !recursive && CanDoThatNow();
        BoardState board = startHead.board;
        //Finds all the squares that will have your tail if you make the move, and those that have your tail when you start this move
        GridSquare[] tailSquares = board.SquaresInDirection(destination, GridManager.OppositeDirection(direction), piece.GetLength() - 1, false);
        GridSquare[] startTailSquares = board.SquaresInDirection(startHead, GridManager.OppositeDirection(startDirection), piece.GetLength() - 1, false);
        
        //Prioritise moving if you are currently in harms way
        foreach (Move opponentMove in owner.GetDangerousOpponentMoves())
        {
            if(startTailSquares.Contains(opponentMove.destination) && !Piece.MoveBlockedByPiece(opponentMove.piece.GetHeadSquare(board), opponentMove.destination, opponentMove.destination, opponentMove.direction, opponentMove.piece))
            {
                //It either yes or no, not mutiplicative of how many there are
                mods += " escape";
                inHarmsWay = true;
                weight *= owner.aiPreservePieceDesire;
                break;
            }
        }
        //Prioritize defending against enemy moves. WILL DEPRIORITIZE MOVES IF CURRENT POSITION IS BETTER DEFENCE
        foreach (Move opponentMove in owner.GetDangerousOpponentMoves())
        {
            bool canBlockNow = false;
            bool moveBlocked = false;
            if(opponentMove.destination == startHead && destination != startHead)
            {
                mods += " stay";
                weight /= opponentMove.weight;
            }
            else if (true)
            {
                //Finds all the intermediate squares that could block the opponent's move
                GridSquare[] blockingSquares;
                if(opponentMove.type == MoveType.TAIL_ROTATION)
                {
                    blockingSquares = board.SquaresInDirection(opponentMove.piece.GetTailLocation(), opponentMove.direction, opponentMove.piece.GetLength() - 1, false);
                }
                else if(opponentMove.type == MoveType.HEAD_ROTATION)
                {
                    blockingSquares = board.SquaresInDirection(opponentMove.piece.GetHeadSquare(board), GridManager.OppositeDirection(opponentMove.direction), opponentMove.piece.GetLength() - 1, false);
                }
                else
                {
                    blockingSquares = board.SquaresInDirection(opponentMove.piece.GetHeadSquare(board), opponentMove.direction, GridManager.DistanceToSquare(opponentMove.piece.GetHeadSquare(board),opponentMove.destination), false);
                }
                canBlockNow = !recursive && CanBlockNow(blockingSquares);
                //Checks if this move would block any of them
                foreach (GridSquare square in blockingSquares)
                {
                    //If you're already blocking them, don't move unless you would die
                    if (square == startHead && !inHarmsWay)
                    {
                        mods += " stay";
                        weight /= opponentMove.weight;
                        moveBlocked = true;
                        break;
                    }
                    else if (!canBlockNow && square == destination)
                    {
                        mods += " preserve(" + opponentMove.weight + ")";
                        weight *= opponentMove.weight;
                        moveBlocked = true;
                        break;
                    }
                    //If an ally already blocking them, don't bother
                    else if(square.GetPiece() && square.GetPiece().GetHeadSquare(board) == square && square.GetPiece().GetOwner() == owner && square.GetPiece() != piece)
                    {
                        moveBlocked = true;
                        break;
                    }
                }
            }
            if (!canBlockNow && !moveBlocked && opponentMove.destination == destination)
            {
                mods += " preserve(" + opponentMove.weight + ")";
                weight *= opponentMove.weight;
            }
        }
        //Deprioritise moving into enemy fire
        foreach (Move opponentMove in owner.GetOpponentMoves())
        {
            foreach(GridSquare square in tailSquares)
            {
                //If the opponent would land on you but is NOT the opponent you would destroy with this move, avoid it
                if(square == opponentMove.destination && destination.GetPiece() != opponentMove.piece && !Piece.MoveBlockedByPiece(opponentMove.piece.GetHeadSquare(board), opponentMove.destination, opponentMove.destination, opponentMove.direction, opponentMove.piece, piece))
                {
                    mods += " avoid";
                    weight /= owner.aiPreservePieceDesire;
                    if(owner.GetPieces().Count == 1)
                    {
                        weight /= owner.aiNotLoseDesire;
                    }
                }
            }
        }
        //Prioritize destroying opponent's pieces.
        if (!canDoThatNow && destination.GetPiece() && destination.GetPiece() != piece)
        {
            mods += " destroy";
            weight *= owner.aiDestroyDesire;
            //Prioritise enemy queen
            if (destination.GetPiece().GetCanWin())
            {
                weight *= 2;
            }
            //DEFINATELY prioritise destroying the last opponent piece
            if (GameManager.GetOtherPlayer(owner).GetPieces().Count == 1)
            {
                weight = owner.aiWinDesire;
            }
        }
        //Prioritise facing the enemy homerow
        if (GridManager.DirectionBetweenCoordinates(new Coordinate(startHead.GetCoordinates().x, owner.GetHomeRow()), new Coordinate(startHead.GetCoordinates().x, GameManager.GetOtherPlayer(owner).GetHomeRow())) == direction)
        {
            weight *= owner.aiFaceOffensivelyDesire;
        }
        //DEFINATELY prioritise getting your queen to opponent's homerow. Don't prioritize future moves winning if you could win now
        if (!canDoThatNow && piece.GetCanWin() && destination.GetCoordinates().y == GameManager.GetOtherPlayer(piece.GetOwner()).GetHomeRow() && !owner.GetPotentialMoves().Contains(this))
        {
            weight = piece.GetOwner().aiWinDesire;
        }
        //Consider the value of next turn's moves. Partially decided by average, partially by highest value move
        string nextMods = "";
        if (recursive)
        {
            //Calculates the weight of all next turn moves
            int nextTurnNumMoves = 0;
            float nextTurnTotalWeight = 0;
            float nextTurnMaxWeight = 0;
            List<Move> nextTurnMoves = piece.GetAllMoves(destination.GetCoordinates(), direction, false);
            foreach(Move move in nextTurnMoves)
            {
                string thisMod = move.SetWeights(destination,direction,false);
                nextTurnNumMoves++;
                nextTurnTotalWeight += move.weight;
                if(move.weight > nextTurnMaxWeight)
                {
                    nextTurnMaxWeight = move.weight;
                    if(thisMod != "")
                        nextMods += "\n" + thisMod;
                }
            }
            //translates that into weight for this turn
            if(nextTurnNumMoves != 0)
            {
                float averageWeight = nextTurnTotalWeight / nextTurnNumMoves;
                float nextTurnWeight = (averageWeight * owner.aiNextTurnAverageWeight) + (nextTurnMaxWeight * (1 - owner.aiNextTurnAverageWeight));
                mods += " next(" + nextTurnWeight + ")";
                //Lack of further options should not negate the value of the first move
                weight += weight * owner.aiWeightByTurn[1] * nextTurnWeight;
            }
        }

        //debug logs the result
        if(recursive)
        {
            Debug.Log(piece.gameObject.name + " " + direction + " to " + destination.gameObject.name + ". Weight: " + weight + mods + nextMods + "\n  (" + GameManager.turn + ")");
        }

        return mods;

    }
    /// <summary>
    /// Returns true if this move accomplishes something you could do this turn
    /// </summary>
    /// <returns></returns>
    bool CanDoThatNow()
    {
        foreach (Move potentialMove in owner.GetPotentialMoves())
        {
            if (potentialMove.destination == destination && potentialMove.piece == piece)
            {
                return true;
            }
        }
        return false;
    }

    //Checks whether there's a more direct move to block this move
    bool CanBlockNow(GridSquare[] squaresToBlock)
    {
        foreach (GridSquare square in squaresToBlock)
        {
            foreach (Move potentialMove in owner.GetPotentialMoves())
            {
                if (potentialMove.destination == square && potentialMove.piece == piece)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

public class Player : MonoBehaviour
{
    public string lostReason;
    List<Piece> pieces = new List<Piece>();
    [Tooltip("This player's primary color. Pieces will use a variant")]
    [SerializeField] Color color;
    [Tooltip("The pieces will be this much darker")]
    [SerializeField] float pieceDarkening;
    [Tooltip("The Y positions the pieces start on. Index 0 is always queen")]
    [SerializeField] int[] startingX;
    float aiTurnTimer = 0;
    [Tooltip("All the moves this player could possibly make based on the current borad state")]
    List<Move> potentialMoves = new List<Move>();
    [Tooltip("All the moves the OTHER player could possibly make based on the current board state")]
    List<Move> opponentMoves = new List<Move>();
    [Tooltip("All the moves the OTHER player could possibly make based on the current board state that would harm you")]
    List<Move> dangerousOpponentMoves = new List<Move>();
    [Tooltip("The sum of every potentialMove's weight")]
    float totalMoveWeight = 0;
    int homerow;
    [Tooltip("If true, the AI will control all moves.")]
    [SerializeField] bool AI;
    [Tooltip("How long the AI will wait to take its turn")]
    [SerializeField] float aiTurnTime;
    [Tooltip("The move the AI has chosen to make this turn")]
    Move chosenMove;
    [Header("AI Weights")]
    [Tooltip("How much this AI wants to make a move that will win the game. Flat")]
    public float aiWinDesire = 100000000;
    [Tooltip("How much this AI wants to make a move that will prevent the enemy from winning. Multiplicative")]
    public float aiNotLoseDesire = 10000;
    [Tooltip("How much more this AI wants to make a move that will destroy an enemy piece. Multiplicative")]
    public float aiDestroyDesire;
    [Tooltip("How much more this AI wants to make a move that will prevent them from losing a piece. Multiplicative")]
    public float aiPreservePieceDesire;
    [Tooltip("How much more this AI wants to make a move that will point it towards the opponent's homerow. Multiplicative")]
    public float aiFaceOffensivelyDesire;
    [Tooltip("How much this AI will factor in each turn to the future. Multiplicative")]
    public float[] aiWeightByTurn;
    [Tooltip("How much of the next turn's potential impact is average as opposed to the maximum value")]
    public float aiNextTurnAverageWeight;

    public void StartTurn()
    {
        foreach (Piece piece in pieces)
        {
            piece.StartTurn();
        }
        //Checks for valid moves; if none, you lose!
        if(!AnyValidMove())
        {
            lostReason = "cannot make any moves.";
            GameManager.PlayerLost(this);
        }
        if (AI)
        {
            aiTurnTimer = 0;
            TakeAITurn();
        }
    }
    /// <summary>
    /// Decoors all pieces
    /// </summary>
    public void EndTurn()
    {
        foreach (Piece piece in pieces)
        {
            piece.EndTurn();
        }
    }
    /// <summary>
    /// Waits a beat to make AI moves
    /// </summary>
    private void Update()
    {
        if(AI && GameManager.currentPlayer == this && GameManager.canPlay)
        {
            aiTurnTimer += Time.deltaTime;
            if(aiTurnTimer > aiTurnTime)
            {
                chosenMove.MakeMove();
            }
        }
    }
    /// <summary>
    /// Handles the AI taking their turn
    /// </summary>
    void TakeAITurn()
    {
        Debug.LogAssertion(GameManager.turn);
        totalMoveWeight = 0;
        opponentMoves = GameManager.GetOtherPlayer(this).FindPotentialMoves(false, true);
        dangerousOpponentMoves = FindDangerousEnemyMoves();
        //gets the list once so that when it gets weights, it can rule out redundant moves
        potentialMoves = FindPotentialMoves(false);
        potentialMoves = FindPotentialMoves(true);
        chosenMove = ChooseRandomMove();
    }
    /// <summary>
    /// Returns a list of every move the enemy could make that you want to prevent for some reason
    /// </summary>
    /// <returns></returns>
    List<Move> FindDangerousEnemyMoves()
    {
        //List<Move> opponentPotentialMoves = GameManager.GetOtherPlayer(this).FindPotentialMoves(false);
        List<Move> opponentPotentialMoves = opponentMoves;
        List <Move> dangerousMoves = new List<Move>();
        for (int i = 0; i < opponentPotentialMoves.Count; i++)
        {
            Move move = opponentPotentialMoves[i];
            //Try not to let them destroy your pieces
            if (move.destination.GetPiece() && move.destination.GetPiece().GetOwner() == this && move.destination.GetPiece().GetHeadLocation() != move.destination.GetCoordinates())
            {
                move.weight *= aiPreservePieceDesire;
                if (move.destination.GetPiece().GetCanWin())
                {
                    move.weight *= 2;
                }
                if (pieces.Count == 1)
                {
                    move.weight *= aiNotLoseDesire;
                }
            }
            //Try not to let them get queen to homerow
            if (move.piece.GetCanWin() && move.destination.GetCoordinates().y == homerow)
            {
                move.weight *= aiNotLoseDesire;
            }
            //If there's any reason to block it, add it to the list
            if (move.weight > Move.BASE_MOVE_WEIGHT)
            {
                dangerousMoves.Add(move);
            }
        }
        return dangerousMoves;
    }

    /// <summary>
    /// Returns a list of every move that this player could make based on the current board state
    /// </summary>
    /// <param name="calculateWeights">If false, leaves all their weights as default. Significantly reduces intensity</param>
    /// <param name="includeBlockedByPieces">If true, will include moves that other pieces currently block</param>
    List<Move> FindPotentialMoves(bool calculateWeights, bool includeBlockedByPieces = false)
    {
        //Compiles new list
        List<Move> moves = new List<Move>();
        foreach(Piece piece in pieces)
        {
            List<Move> pieceMoves = piece.GetAllMoves(calculateWeights, includeBlockedByPieces);
            foreach(Move move in pieceMoves)
            {
                moves.Add(move);
                totalMoveWeight += move.weight;
            }
        }
        return moves;
    }
    /// <summary>
    /// Returns whether there are any moves this player can make
    /// </summary>
    /// <returns></returns>
    bool AnyValidMove()
    {
        foreach (Piece piece in pieces)
        {
            if (piece.AnyValidMove())
            {
                return true;
            }
        }
        return false;
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
            lostReason = " has no more pieces";
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

    public List<Piece> GetPieces()
    {
        return pieces;
    }
    /// <summary>
    /// Returns the ALREADY CALCULATED list of moves the opponent could make that you have  areason to prevent; their weights represent your desire to prevent them
    /// </summary>
    /// <returns></returns>
    public List<Move> GetDangerousOpponentMoves()
    {
        return dangerousOpponentMoves;
    }
    /// <summary>
    /// Returns the ALREADY CALCULATED list of moves the opponent could make
    /// </summary>
    /// <returns></returns>
    public List<Move> GetOpponentMoves()
    {
        return opponentMoves;
    }

    public List<Move> GetPotentialMoves()
    {
        return potentialMoves;
    }
}
