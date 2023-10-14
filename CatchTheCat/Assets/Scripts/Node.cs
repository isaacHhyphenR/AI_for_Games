using System.Collections.Generic;
using UnityEngine;

public enum Directions
{
    NORTHEAST = 0,
    EAST,
    SOUTHEAST,
    SOUTHWEST,
    WEST,
    NORTHWEST,
    NUM_DIRECTIONS
}

public class Node : MonoBehaviour
{
    Node[] neighbours = new Node[(int)Directions.NUM_DIRECTIONS];
    [SerializeField] Renderer render;
    [SerializeField] Color blockedColor;
    [SerializeField] bool blocked = false;
    public Vector2 coordinates = Vector2.zero;
    [Tooltip("Points to the node that led to this node in the current pathfinding session.")]
    public Vector2 previous = Vector2.zero;
    public bool visited = false;

    bool isEdge = false;
    private void Start()
    {
        for (int i = 0; i < neighbours.Length; i++)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.GetChild(i).position, 0.15f);
            foreach(Collider col in hitColliders)
            {
                Node hitNode = col.GetComponent<Node>();
                if(hitNode != null)
                {
                    neighbours[i] = hitNode;
                    break;
                }
            }
            if (neighbours[i] == null)
            {
                isEdge = true;
                Block();
            }
        }
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] == null)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        if (blocked)
        {
            Block();
        }
    }

    public bool GetIsEdge()
    {
        return isEdge;
    }


    public void Block()
    {
        render.material.color = blockedColor;
        blocked = true;
    }
    public bool GetBlocked()
    {
        return blocked;
    }

    public Node GetNeighbourAt(int direction)
    {
        return neighbours[direction];
    }
    public Node GetNeighbourAt(Directions direction)
    {
        return GetNeighbourAt((int)direction);
    }

    public List<Node> GetUnblockedNeighbours()
    {
        List<Node> unblocked = new List<Node>();
        foreach(Node neighbour in neighbours)
        {
            if(neighbour != null && !neighbour.GetBlocked() && !neighbour.visited)
            {
                unblocked.Add(neighbour);
            }
        }
        return unblocked;
    }

    void ResetTurn()
    {
        visited = false;
    }

    private void OnEnable()
    {
        GameManager.ResetTurn += ResetTurn;
    }
    private void OnDisable()
    {
        GameManager.ResetTurn -= ResetTurn;
    }
}

