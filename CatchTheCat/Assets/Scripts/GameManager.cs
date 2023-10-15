using UnityEngine;
using System.Collections.Generic;
using System;
public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject nodePrefab;
    [SerializeField] int sideSize;
    [SerializeField] Vector2 hexSize; 

    List<Node> nodes = new List<Node>();

    Vector2 catPos = Vector2.zero;
    float timer = 0;

    public static event Action ResetTurn;

    private void Awake()
    {
        for(int r = 0; r < sideSize; r++)
        {
            for(int c = 0; c < sideSize; c++)
            {
                Vector3 newPos = new Vector3(hexSize.x * c, 0, hexSize.y * -r);
                if(r % 2 != 0)
                {
                    newPos.x += hexSize.x / 2;
                }
                newPos += transform.position;
                //Creates the node
                GameObject newhex = Instantiate(nodePrefab, newPos,nodePrefab.transform.rotation);
                Node newNode = newhex.GetComponent<Node>();
                newNode.coordinates = new Vector2(c, r);
                nodes.Add(newNode);
            }
        }
        //sets the initial head at the center
        catPos = new Vector2(sideSize/2,sideSize/2);
    }

    int LinearizeCoordinates(Vector2 coordinates)
    {
        int index = (int)coordinates.y * sideSize + (int)coordinates.x;
        return index;
    }

    Node GetNodeAt(Vector2 coordinates)
    {
        return nodes[LinearizeCoordinates(coordinates)];
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > 0.5f)
        {
            timer = 0;
            List<Node> path = FindPathToEdge();
            ResetTurn?.Invoke();
        }
    }

    
    List<Node> FindPathToEdge()
    {
        //All the nodes you've visited so far
        List<Node> queue = new List<Node>();
        Node head = GetNodeAt(catPos);
        queue.Add(head);
        do
        {
            head = queue[0];
            head.visited = true;
            if (head.GetIsEdge())
            {
                break;
            }
            else
            {
                List<Node> neighbors = head.GetUnblockedNeighbours();
                foreach (Node neighbour in neighbors)
                {
                    queue.Add(neighbour);
                    neighbour.previous = head.coordinates;
                }
                queue.Remove(head);
            }

        } while (queue.Count > 0);
        //Retrace your steps to find the path
        List<Node> path = new List<Node>();
        while(head != GetNodeAt(catPos))
        {
            path.Add(head);
            head = GetNodeAt(head.previous);
        }


        return path;
    }
    
}
