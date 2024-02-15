using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] Renderer render;
    [SerializeField] LineRenderer line;
    [SerializeField] Vector3 mountainScale;
    Vector3 normalScale;
    [HideInInspector] public Sector sector;
    [HideInInspector] public Vector3 position;

    public List<Cell> neighbours = new List<Cell>();

    bool passable = true;

    //A* data
    public float heuristic = -1;
    public float costSoFar = 0;
    public float expectedCost = 0;
    public Cell parent = null;
    private void Start()
    {
        position = line.transform.position;
        normalScale = transform.localScale;
    }
    public void ResetPathfinding()
    {
        heuristic = -1;
        costSoFar = 0;
        expectedCost = 0;
        parent = null;
    }

    public void CalculateHeuristic(Vector3 destination)
    {
        heuristic = Mathf.Abs((destination - position).magnitude);
    }
    public float GetHeuristic()
    {
        return heuristic;
    }
    public float CostToMove(Vector3 origin)
    {
        return Mathf.Abs((origin - position).magnitude);
    }

    public float GetSize()
    {
        return transform.localScale.x;
    }
    public Vector3 GetSize(bool getVector)
    {
        return transform.localScale;
    }

    public void SetColor(Color color)
    {
        render.material.color = color;
    }

    public void AddNeighbour(Cell neighbour)
    {
        neighbours.Add(neighbour);
    }

    //Draws a line from here to destination
    public void DrawLine(Vector3 destination)
    {
        line.enabled = true;
        line.SetPositions(new Vector3[2] { position, destination });
        sector.SetHasLines(true);
    }
    public void ClearLine()
    {
        line.enabled = false;
    }

    //left click
    private void OnMouseDown()
    {
        //Left click + Shift sets destination
        if (Input.GetKey("right shift") || Input.GetKey("left shift"))
        {
            GridManager.instance.SetDestination(this);
        }
        //Normal left click sets origin
        else
        {
            GridManager.instance.SetOrigin(this);
        }
    }
    void OnMouseOver()
    {
        //Right click toggles passable
        if(Input.GetMouseButtonDown(1))
        {
            passable = !passable;
            if(passable)
            {
                transform.localScale = normalScale;
            }
            else
            {
                transform.localScale = mountainScale;
            }
        }
    }

    public bool GetPassable()
    {
        return passable;
    }
}
