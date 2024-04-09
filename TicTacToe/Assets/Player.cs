using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Tooltip("Which character represents this player on the board")]
    [SerializeField] char character;
    [Tooltip("If true, AI will play for this character")]
    [SerializeField] bool isAi;

    public char Character()
    {
        return character;
    }

    public bool IsAi()
    {
        return isAi;
    }

    /// <summary>
    /// The AI returns the space it would like to select based on the current board state.
    /// </summary>
    /// <param name="initialState"></param>
    /// <returns></returns>
    public Vector2 ChooseMove(Board initialState)
    {
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        List<Node> stalemateNodes = new List<Node>();
        List<Node> winningNodes = new List<Node>();
        List<Node> losingParents = new List<Node>();

        openList.Add(new Node(initialState, null));
        //Checks all move paths using modified A*
        while (openList.Count > 0)
        {
            Node head = openList.First();
            closedList.Add(head);
            openList.Remove(head);
            Player playerWithTurn = GameManager.instance.NextPlayerObject(head.state.GetLastMove().character);
            if (playerWithTurn == null)
            {
                playerWithTurn = this;
            }
            head.children = GameManager.GenerateNextStates(head, playerWithTurn);
            //If this was your move, only consider if it does NOT allow the enemy to win
            if (playerWithTurn != this)
            {
                //Checks if it allows the enemy to win
                bool canLose = false;
                bool canStalemate = false;
                foreach (Node node in head.children)
                {
                    char winner = GameManager.IsWinningState(node.state);
                    if (winner == GameManager.STALEMATE)
                    {
                        canStalemate = true;
                    }
                    else if (winner != GameManager.EMPTY_SQUARE)
                    {
                        losingParents.Add(head);
                        canLose = true;
                        break;
                    }
                }
                //If it doesn't result in a loss, add its children to the stalemate list
                if (!canLose && canStalemate)
                {
                    foreach (Node node in head.children)
                    {
                        stalemateNodes.Add(node);
                    }
                }
                //If it does not result in a loss or end of game, add children to open list
                else if(!canLose)
                {
                    foreach (Node node in head.children)
                    {
                        if (!ListContainsState(closedList, node.state))
                        {
                            openList.Add(node);
                        }
                    }
                }
            }
            //If this was the enemy's move, no need to search this branch further if it allows you to win
            else
            {
                bool endNode = false;
                //Checks if any children result in a win or stalemate
                foreach (Node node in head.children)
                {
                    char winner = GameManager.IsWinningState(node.state);
                    //If this move lets you win, no need to search the tree further
                    if (winner == character)
                    {
                        winningNodes.Add(node);
                        endNode = true;
                        break;
                    }
                    if (winner == GameManager.STALEMATE)
                    {
                        stalemateNodes.Add(node);
                        endNode = true;
                        break;
                    }
                }
                //If this doesn't end the game, add its children to the open list
                if(!endNode)
                {
                    foreach (Node node in head.children)
                    {
                        if (!ListContainsState(closedList, node.state))
                        {
                            openList.Add(node);
                        }
                    }
                }
            }
        }
        /////////Once all nodes have been searched, try to trace a path from a winning or at least stalemate node
        //Blocks off all decisions that lead to loss, assuming enemy perfect play
        List<Node> closedLosingParents = new List<Node>();
        while (losingParents.Count > 0)
        {
            Node head = losingParents.First();
            losingParents.Remove(head);
            closedLosingParents.Add(head);
            //If the head was opponent's move, parent is automatically a loss because the enemy can choose head
            if (head.parent != null && head.state.GetLastMove().character != character)
            {
                losingParents.Add(head.parent);
                //move all losing children to the closed list so we don't have to deal with them
                foreach (Node child in head.parent.children)
                {
                    if (child != head && losingParents.Contains(child))
                    {
                        closedLosingParents.Add(child);
                        losingParents.Remove(child);
                    }
                }
            }
            //If the head was your move, check whether they can win no matter which choice you make from parent
            else if (head.parent != null)
            {
                bool allLosses = true;
                foreach (Node child in head.parent.children)
                {
                    if (child != head)
                    {
                        if (losingParents.Contains(child))
                        {
                            closedLosingParents.Add(child);
                            losingParents.Remove(child);
                        }
                        else
                        {
                            allLosses = false;
                        }
                    }
                }
                if (allLosses)
                {
                    losingParents.Add(head.parent);
                }
            }
        }
        //Adds all the stalemate nodes into the winning list so that it can search them all at once for a path the enemy will allow
        foreach (Node stalemate in stalemateNodes)
        {
            winningNodes.Add(stalemate);
        }
        foreach (Node winner in winningNodes)
        {
            bool clearPath = true;
            Node tail = winner;
            while (tail.parent.state != initialState)
            {
                if(closedLosingParents.Contains(tail))
                {
                    clearPath = false;
                    break;
                }
                tail = tail.parent;
            }
            if (clearPath)
            {
                if(stalemateNodes.Contains(winner))
                {
                    Debug.Log("Stalemate");
                }
                else
                {
                    Debug.Log("Win");
                }
                return new Vector2(tail.state.GetLastMove().x, tail.state.GetLastMove().y);
            }
        }
        //If there were no winnable or stalematable paths, take the first possible option
        for (int x = 0; x < initialState.GetGridSize(); x++)
        {
            for (int y = 0; y < initialState.GetGridSize(); y++)
            {
                if(initialState.GetValue(x,y) == GameManager.EMPTY_SQUARE)
                {
                    Debug.Log("Lose");
                    return new Vector2(x, y);
                }
            }
        }
        //This will never get called, but throws errors otherwise
        return new Vector2(0, 0);
    }

    bool ListContainsState(List<Node> list, Board state)
    {
        foreach (Node n in list)
        {
            if (n.state == state)
            {
                return true;
            }
        }
        return false;
    }
}
