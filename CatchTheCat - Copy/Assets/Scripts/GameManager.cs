using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] Cat catAgent;
    [SerializeField] Catcher catcherAgent;

    [SerializeField] GameObject nodePrefab;
    [SerializeField] int sideSize;
    [SerializeField] Vector2 hexSize;
    [SerializeField] int startingBlockers;

    List<Node> nodes = new List<Node>();

    Node catPos;
    float timer = 0;
    [SerializeField] float turnTime;
    bool catTurn = false;

    public static event Action ResetTurn;

    [SerializeField] TurnMarker turnMarker;

    public static bool paused = false;

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
        catPos = GetNodeAt(new Vector2(sideSize/2,sideSize/2));
        catAgent.InitAgent(catPos);
        //random starting blockers
        for (int i = 0; i < startingBlockers; i++)
        {
            int randPos = UnityEngine.Random.Range(0, nodes.Count);
            while (randPos == LinearizeCoordinates(catPos.coordinates) || nodes[randPos].GetBlocked())
            {
                randPos = UnityEngine.Random.Range(0, nodes.Count);
            }
            nodes[randPos].Block();
        }

        //
        turnMarker.SetTurn(catTurn);
    }

    private void Update()
    {
        if(!paused)
        {
            timer += Time.deltaTime;
            if (timer > turnTime)
            {
                TakeTurn();
            }
        }
    }


    void TakeTurn()
    {
        timer = 0;

        List<Node> path = FindPathToEdge();

        if (catTurn)
        {
            //If you're already at the edge, win
            if (catPos.GetIsEdge())
            {
                SceneManager.LoadScene("CatWin");
            }
            //If you're trapped, lose
            else if (path.Count == 0)
            {
                SceneManager.LoadScene("CatLose");
            }
            else
            {
                catPos = catAgent.TakeTurn(path);
            }
        }
        else
        {
            catcherAgent.TakeTurn(path);
        }
        //switch turn
        catTurn = !catTurn;
        turnMarker.SetTurn(catTurn);
    }

    int LinearizeCoordinates(Vector2 coordinates)
    {
        int index = (int)coordinates.y * sideSize + (int)coordinates.x;
        return index;
    }

    public Node GetNodeAt(Vector2 coordinates)
    {
        return nodes[LinearizeCoordinates(coordinates)];
    }
    public Node GetNodeAt(int index)
    {
        return nodes[index];
    }


    List<Node> FindPathToEdge()
    {
        ResetTurn?.Invoke();
        //All the nodes you've visited so far
        List<Node> queue = new List<Node>();
        Node head = catPos;
        queue.Add(head);
        do
        {
            head = queue[0];
            head.Visit();
            if (head.GetIsEdge())
            {
                break;
            }
            else
            {
                List<Node> neighbors = head.GetUnblockedNeighbours();
                foreach (Node neighbour in neighbors)
                {
                    if(!queue.Contains(neighbour))
                    {
                        queue.Add(neighbour);
                        neighbour.previous = head.coordinates;
                    }
                }
                queue.Remove(head);
            }

        } while (queue.Count > 0);
        //Retrace your steps to find the path
        List<Node> path = new List<Node>();
        while(head != catPos)
        {
            head.SetPath(path.Count);
            path.Add(head);
            head = GetNodeAt(head.previous);
        }

        return path;
    }
    

    public int GetNumNodes()
    {
        return nodes.Count;
    }
}
