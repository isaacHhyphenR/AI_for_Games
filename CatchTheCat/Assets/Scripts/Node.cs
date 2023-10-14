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

    private void Start()
    {
        for (int i = 0; i < neighbours.Length; i++)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.GetChild(i).position, 0.1f);
            neighbours[i] = hitColliders[0].GetComponent<Node>();
        }
        if(blocked)
        {
            Block();
        }
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
            if(neighbour != null && !neighbour.GetBlocked())
            {
                unblocked.Add(neighbour);
            }
        }
        return unblocked;
    }
}

