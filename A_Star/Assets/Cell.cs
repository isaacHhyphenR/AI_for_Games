using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] Renderer render;
    [SerializeField] LineRenderer line;
    [SerializeField] Vector3 mountainScale;
    [SerializeField] float swampMultiplier;
    Vector3 normalScale;
    [HideInInspector] public Sector sector;
    [HideInInspector] public Vector3 position;
    Color normalColor;
    Color swampColor;
    bool swamp = false;
    bool passable = true;

    [HideInInspector] public List<Cell> neighbours = new List<Cell>();

    //A* data
    [HideInInspector] public float heuristic = -1;
    [HideInInspector] public float costSoFar = 0;
    [HideInInspector] public float expectedCost = 0;
    [HideInInspector] public Cell parent = null;
    private void Start()
    {
        position = line.transform.position;
        normalScale = transform.localScale;
        normalColor = render.material.color;
        swampColor = render.material.color * 0.5f;
    }
    public void ResetPathfinding()
    {
        heuristic = -1;
        costSoFar = 0;
        expectedCost = 0;
        parent = null;
    }
    /// <summary>
    /// Calculates & returns the estimated cost from this cell to the destination
    /// </summary>
    /// <param name="destination"></param>
    public float CalculateHeuristic(Vector3 destination)
    {
        if(GridManager.heuristic == Heuristic.MANHATTAN)
        {
            float xDist = Mathf.Abs(destination.x - position.x);
            float yDist = Mathf.Abs(destination.z - position.z);
            heuristic = xDist + yDist;
        }
        else if (GridManager.heuristic == Heuristic.DIAGONAL)
        {
            float xDist = Mathf.Abs(destination.x - position.x);
            float yDist = Mathf.Abs(destination.z - position.z);
            heuristic = Mathf.Max(xDist, yDist);
        }
        else if(GridManager.heuristic == Heuristic.EUCLIDEAN)
        {
            heuristic = Mathf.Abs((destination - position).magnitude);
        }
        return heuristic;
    }

    public float CostToMove(Vector3 origin)
    {
        float baseCost = Mathf.Abs((origin - position).magnitude);
        if(swamp)
        {
            baseCost *= swampMultiplier;
        }
        return baseCost;
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
    }
    public void ClearLine()
    {
        line.enabled = false;
    }
    /// <summary>
    /// Returns true if the two cells are either vertically or horizontally aligned (NOT diagonal)
    /// </summary>
    /// <param name="neighbour"></param>
    /// <returns></returns>
    public bool CheckStraighAlignment(Cell neighbour)
    {
        return neighbour.position.x == position.x || neighbour.position.z == position.z;
    }

    private void OnMouseDown()
    {
        if(GetPassable())
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
    }
    void OnMouseOver()
    {
        //Right click toggles swamp
        if (Input.GetMouseButtonDown(1) && (Input.GetKey("right shift") || Input.GetKey("left shift")))
        {
            swamp = !swamp;
            if(swamp)
            {
                SetColor(swampColor);
            }
            else
            {
                SetColor(normalColor);
            }
        }
        //Right click toggles passable
        else if (Input.GetMouseButtonDown(1))
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
    /// <summary>
    /// Instantiates and returns a Cell
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static Cell Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject newCell = GameObject.Instantiate(prefab, position, rotation);
        return newCell.GetComponent<Cell>();
    }
    /// <summary>
    /// Instantiates and returns a Cell
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static Cell Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Color color)
    {
        GameObject newCell = GameObject.Instantiate(prefab, position, rotation);
        newCell.GetComponent<Cell>().SetColor(color);
        return newCell.GetComponent<Cell>();
    }
}
