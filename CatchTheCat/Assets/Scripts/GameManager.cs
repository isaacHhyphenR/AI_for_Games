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
            //List<Node> path = FindPathToEdge();
            //path[0].Block();
            //catPos = path[0].coordinates;
            /* This code draws a random path across the board
                nodes[LinearizeCoordinates(head)].Block();
                //head moves to a random neighbour of the current head
                List<Node> neighbours = nodes[LinearizeCoordinates(head)].GetUnblockedNeighbours();
                int rand = Random.Range(0, neighbours.Count);
                head = neighbours[rand].coordinates;
            */
            ResetTurn?.Invoke();
        }
    }

    /*
    List<Node> FindPathToEdge()
    {
        //All the nodes you've visited so far
        List<Vector2> queue = new List<Vector2>();
        queue.Add(catPos);
        while(queue.Count > 0)
        {
            Node head = GetNodeAt(queue[0]);
            head.visited = true;

        }
        /*
        while (!foundEdge && )
        {
            nodes[LinearizeCoordinates(head)].visited = true;
            List<Node> neighbours = nodes[LinearizeCoordinates(head)].GetUnblockedNeighbours();
            //If you've found the edge, congrats!
            if( neighbours.Count < (int)Directions.NUM_DIRECTIONS)
            {
                foundEdge = true;
            }
            //if you haven't found the edge, add the new neighbours to the path
            else
            {
                foreach( Node neighbour in neighbours)
                {
                    if(!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        neighbour.previous = head;
                    }
                }
                headIndex++;
            }
        }
        return visited;
    }
    */
}
