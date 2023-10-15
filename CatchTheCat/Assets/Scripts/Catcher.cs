using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Catcher : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    public Node TakeTurn(List<Node> path)
    {
        Node position;
        //If the cat is at the edge, just block somewhere random
        if (path.Count == 0)
        {
            int randPos = Random.Range(0, gameManager.GetNumNodes());
            while(gameManager.GetNodeAt(randPos).GetBlocked() || gameManager.GetNodeAt(randPos).GetIsCat())
            {
                randPos = Random.Range(0, gameManager.GetNumNodes());
            }
            position = gameManager.GetNodeAt(randPos);
        }
        //Normally, block the edge the cat aims for
        else if(path.First().GetIsEdge())
        {
            position = path.First();
        }
        //if you've got the cat trapped, move in for the kill
        else
        {
            position = path.Last();
        }
        //moves on the board
        position.Block();
        return position;
    }
}
