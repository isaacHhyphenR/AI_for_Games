using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Catcher : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    public Node TakeTurn(List<Node> path)
    {
        if(path.Count == 0)
        {
            int randPos = Random.Range(0, gameManager.GetNumNodes());
            while(gameManager.GetNodeAt(randPos).GetBlocked() || gameManager.GetNodeAt(randPos).GetIsCat())
            {
                randPos = Random.Range(0, gameManager.GetNumNodes());
            }
            path.Add(gameManager.GetNodeAt(randPos));
        }
        Node position = path.First();
        //moves on the board
        position.Block();
        return position;
    }
}
