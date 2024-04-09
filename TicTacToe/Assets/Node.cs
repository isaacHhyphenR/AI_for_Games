using System.Collections.Generic;

public class Node
{
    public Board state;
    public Node parent;
    public List<Node> children;
    public Node(Board _state, Node _parent)
    {
        state = _state;
        parent = _parent;
    }
}
