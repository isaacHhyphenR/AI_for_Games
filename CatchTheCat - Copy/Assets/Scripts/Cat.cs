using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Cat : MonoBehaviour
{
    Node currentPosition;


    public void InitAgent(Node startPos)
    {
        startPos.SetCat(true);
        currentPosition = startPos;
    }    


    public Node TakeTurn(List<Node> path)
    {
        Node newPosition = path.Last();
        //moves on the board
        currentPosition.SetCat(false); newPosition.SetCat(true);
        currentPosition = newPosition;
        return newPosition;
    }
}
