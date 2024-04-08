using System.Collections.Generic;
using System.Linq;
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
        Node stalemateNode = null;
        Node winningNode = null;

        openList.Add(new Node(initialState, null));
        //Checks all move paths using modified A*
        while (openList.Count > 0 && winningNode == null)
        {
            Node head = openList.First();
            closedList.Add(openList.First());
            openList.Remove(openList.First());
            Player playerWithTurn = GameManager.instance.GetPlayer(head.state.GetLastMove().character);
            if (playerWithTurn == null)
            {
                playerWithTurn = this;
            }
            List<Node> tempList = GameManager.GenerateNextStates(head, playerWithTurn);
            //If this was your move, only consider if it does NOT allow the enemy to win
            if(playerWithTurn == this)
            {
                //Checks if it allows the enemy to win
                bool canLose = false;
                foreach (Node node in tempList)
                {
                    char winner = GameManager.IsWinningState(node.state);
                    if (winner != GameManager.EMPTY_SQUARE && winner != GameManager.STALEMATE)
                    {
                        canLose = true;
                        break;
                    }
                }
                //If it does not, add its children to the open list
                if (!canLose)
                {
                    foreach (Node node in tempList)
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
                foreach (Node node in tempList)
                {
                    char winner = GameManager.IsWinningState(node.state);
                    //If this move lets you win, no need to search the tree further
                    if (winner == character)
                    {
                        winningNode = node;
                        endNode = true;
                        break;
                    }
                    if (winner == GameManager.STALEMATE)
                    {
                        stalemateNode = node;
                        endNode = true;
                        break;
                    }
                }
                //If this doesn't end the game, add its children to the open list
                if(!endNode)
                {
                    foreach (Node node in tempList)
                    {
                        if (!ListContainsState(closedList, node.state))
                        {
                            openList.Add(node);
                        }
                    }
                }
            }
        }
        //Once all nodes have been searched, try to trace a path from a winning or at least stalemate node
        Node tail = null;
        if(winningNode != null)
        {
            tail = winningNode;
        }
        else if(stalemateNode != null)
        {
            tail = stalemateNode;
        }
        if (tail != null)
        {
            while(tail.parent != initialState)
            {
                tail = tail.parent;
            }
            return new Vector2(tail.state.GetLastMove().x, tail.state.GetLastMove().y);
        }
        //If there were no winnable or stalematable paths, take the first possible option
        else
        {
            for (int x = 0; x < initialState.GetGridSize(); x++)
            {
                for (int y = 0; y < initialState.GetGridSize(); y++)
                {
                    if(initialState.GetValue(x,y) == GameManager.EMPTY_SQUARE)
                    {
                        return new Vector2(x, y);
                    }
                }
            }
        }
        //This will never get called, but throws errors otherwise
        return new Vector2(0, 0);


        /*
        //Generates the starting nodes from the current state
        openList = GameManager.GenerateNextStates(initialState,this);
        //Checks all subsequent moves using modified A*
        while (openList.Count > 0)
        {
            Node head = openList.First();
            openList.Remove(head);
            closedList.Add(head);
            //Checks whether this node is an endstate.
            char winner = GameManager.IsWinningState(head.state);
            if(winner == character)
            {
                winningNodes.Add(head);
                continue;
            }
            if (winner == GameManager.STALEMATE)
            {
                stalemateNodes.Add(head);
                continue;
            }
            if (winner != GameManager.EMPTY_SQUARE)
            {
                willLose.Add(head.parent);
                continue;
            }
            //If you're still here the node is not an end node, so add its children to the open list for investigation
            List<Node> tempNodes = GameManager.GenerateNextStates(head.state, this);
            foreach(Node n in tempNodes)
            {
                if(!ListContainsState(closedList,n.state) && !ListContainsState(openList, n.state) && !ListContainsState(willLose,n.state))
                {
                    openList.Add(n);
                }
            }
        }
        //Once you've generated all board states, choose a winning path
        foreach(Node winner in winningNodes)
        {
            //Make sure this node won't result in a loss

        }
        */
    }


    List<Node> RemoveNodeFromList(List<Node> list, Board board)
    {
        List<Node> safeList = new List<Node>();
        foreach(Node n in list)
        {
            if(n.state != board)
            {
                safeList.Add(n);
            }
        }
        return safeList;
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
    bool ListContainsState(List<Board> list, Board state)
    {
        foreach (Board b in list)
        {
            if (b == state)
            {
                return true;
            }
        }
        return false;
    }
    bool ArrayContainsState(Board[] array, Board state)
    {
        foreach(Board b in array)
        {
            if(b == state)
            {
                return true;
            }
        }
        return false;
    }
}
