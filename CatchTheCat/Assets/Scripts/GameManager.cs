using UnityEngine;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject nodePrefab;
    [SerializeField] int sideSize;
    [SerializeField] Vector2 hexSize; 

    List<Node> nodes = new List<Node>();

    Vector2 head = Vector2.zero;
    float timer = 0;

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
    }

    int LinearizeCoordinates(Vector2 coordinates)
    {
        int index = (int)coordinates.y * sideSize + (int)coordinates.x;
        return index;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > 0.5f)
        {
            timer = 0;
            nodes[LinearizeCoordinates(head)].Block();
            //head moves to a random neighbour of the current head
            List<Node> neighbours = nodes[LinearizeCoordinates(head)].GetUnblockedNeighbours();
            int rand = Random.Range(0, neighbours.Count);
            head = neighbours[rand].coordinates;
        }
    }

}
