using UnityEngine;

public struct Move
{
    public int x;
    public int y;
    public char character;
    public Move(int _x, int _y, char _character)
    {
        x = _x;
        y = _y;
        character = _character;
    }

}

public class Board : MonoBehaviour
{
        int gridSize;
        char[,] grid;
        Move lastMove;
        public Board(int _gridSize)
        {
            gridSize = _gridSize;
            grid = new char[gridSize, gridSize];
            lastMove = new Move(-1, -1, GameManager.EMPTY_SQUARE);
        }
        public char GetValue(int x, int y)
        {
            return grid[x, y];
        }
        public char GetValue(Vector2 coordinate)
        {
            return GetValue((int)coordinate.x, (int)coordinate.y);
        }
        public void SetValue(int x, int y, char value)
        {
            grid[x, y] = value;
            SetLastMove(x, y, value);
        }
        public void SetValue(Vector2 coordinate, char value)
        {
            SetValue((int)coordinate.x, (int)coordinate.y, value);
        }

        public void SetLastMove(Move move)
        {
            lastMove = move;
        }
        public void SetLastMove(int x, int y, char character)
        {
            SetLastMove(new Move(x, y, character));
        }
        public Move GetLastMove()
        {
            return lastMove;
        }
        public char[,] GetGrid()
        {
            return grid;
        }
        public int GetGridSize()
        {
            return gridSize;
        }

        public void Inherit(Board parent)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    SetValue(x, y, parent.GetValue(x, y));
                }
            }
        }

    public override bool Equals(object other)
    {
        return this == (Board)other;
    }

    /// <summary>
    /// Evalutaes whether two boardstates are identical
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator ==(Board lhs, Board rhs)
    {
        for (int x = 0; x < lhs.gridSize; x++)
        {
            for (int y = 0; y < lhs.gridSize; y++)
            {
                if (lhs.grid[x, y] != rhs.grid[x, y])
                {
                    return false;
                }
            }
        }
        //If no differences, they are equal
        return true;
    }
    /// <summary>
    /// Evalutaes whether two boardstates are NOT identical
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator !=(Board lhs, Board rhs)
    {
        return !(lhs == rhs);
    }
}
