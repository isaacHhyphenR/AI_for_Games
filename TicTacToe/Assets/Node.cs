using UnityEngine;

public class Node
{
    public Board state;
    public Node parent;
    public Node(Board _state, Node _parent)
    {
        state = _state;
        parent = _parent;
    }
}
