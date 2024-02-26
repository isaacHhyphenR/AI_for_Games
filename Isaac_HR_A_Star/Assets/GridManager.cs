using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public struct Sector
{
    public Cell[,] cells;
    Color sectorColor;
    GameObject cellPrefab;
    Vector3 sectorPosition;
    float cellSize;
    int width;
    public Vector2 sectorID;
    public Sector(int sectorWidth, GameObject cell, Color color, Vector3 position, Vector2 ID)
    {
        cells = new Cell[sectorWidth, sectorWidth];
        sectorColor = color;
        cellPrefab = cell;
        cellSize = cell.GetComponent<Cell>().GetSize();
        sectorPosition = position;
        width = sectorWidth;
        sectorID = ID;
        GenerateCells();
    }

    public void GenerateCells()
    {
        //The corner from which the cells begin generating
        Vector3 originPosition = sectorPosition;
        originPosition.x -= width * cellSize / 2;
        originPosition.z += width * cellSize / 2;
        //Creates all the cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                Cell newCell = Cell.Instantiate(cellPrefab, new Vector3(originPosition.x + (x * cellSize), originPosition.y, originPosition.z + (-y * cellSize)), cellPrefab.transform.rotation, sectorColor);
                newCell.name = (int)sectorID.x + "," + (int)sectorID.y + "_Cell_" + x + "," + y;
                newCell.sector = this;
                cells[x, y] = newCell;
            }
        }
    }

    public void ClearLines()
    {
        foreach (Cell cell in cells)
        {
            cell.ClearLine();
        }
    }
}

public enum Heuristic
{
    MANHATTAN,
    EUCLIDEAN,
    DIAGONAL
}

public class GridManager : MonoBehaviour
{
    [Tooltip("The prefab for each individual cell.")]
    [SerializeField] GameObject cellPrefab;
    [Tooltip("The lightness of the grid colors, 0-1")]
    [SerializeField] float lightness;
    [Tooltip("The height & width (in cells) of a sector.")]
    [SerializeField] int sectorWidth;
    [Tooltip("The height & width (in sectors) of the overall grid.")]
    [SerializeField] int gridWidth;
    [Tooltip("The UI script that displays whether the pathfind was succesful or not.")]
    [SerializeField] ResultDisplay results;
    Sector[,] sectors;
    float cellSize;

    Cell origin;
    Cell destination;

    public static GridManager instance;
    public static Heuristic heuristic = Heuristic.DIAGONAL;
    public static bool diagonal = true;

    private void Start()
    {
        sectors = new Sector[gridWidth, gridWidth];
        cellSize = cellPrefab.GetComponent<Cell>().GetSize();
        //The height & width (in Unity units) of a sector.
        float sectorActualWidth = cellSize * sectorWidth;
        //The corner from which the cells begin generating
        Vector3 originPosition = transform.position;
        originPosition.x -= (sectorActualWidth * gridWidth / 2) - sectorActualWidth / 2;
        originPosition.z += (sectorActualWidth * gridWidth / 2) - sectorActualWidth / 2;
        //Generates each sector
        for (int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridWidth; y++)
            {
                Vector3 pos = new Vector3(originPosition.x + x * sectorActualWidth, originPosition.y, originPosition.z + (-y * sectorActualWidth));
                Sector newSector = new Sector(sectorWidth, cellPrefab, GenerateColor(lightness), pos, new Vector2(x,y));
                sectors[x,y] = newSector;
            }
        }
        GenerateCellNeighbours();
        //Instance
        GridManager.instance = this;
    }

    private void GenerateCellNeighbours()
    {
        int maxSector = gridWidth - 1;
        int maxCell = sectorWidth - 1;
        //Cycles through every sector
        for( int i = 0; i < gridWidth; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                Sector sector = sectors[i, j];
                //Cycles through every cell
                for (int x = 0; x < sectorWidth; x++)
                {
                    for (int y = 0; y < sectorWidth; y++)
                    {
                        ///NORTH EAST
                        if (x > 0 && y > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sector.cells[x - 1, y - 1]);
                        }
                        else if(x == 0 && y == 0 && i > 0 && j > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i - 1, j - 1].cells[maxCell, maxCell]);
                        }
                        else if (x == 0 && i > 0 && y > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i - 1, j].cells[maxCell, y - 1]);
                        }
                        else if (y == 0 && j > 0 && x > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i, j - 1].cells[x - 1, maxCell]);
                        }
                        ///NORTH
                        if (y > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sector.cells[x, y - 1]);
                        }
                        else if (j > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i, j - 1].cells[x, maxCell]);
                        }
                        ///NORTH WEST
                        if (x < maxCell && y > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sector.cells[x + 1, y - 1]);
                        }
                        else if (x == maxCell && y == 0 && i < maxSector && j > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i + 1, j - 1].cells[0, maxCell]);
                        }
                        else if (x == maxCell && i < maxSector && y > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i + 1, j].cells[0, y - 1]);
                        }
                        else if (y == 0 && j > 0 && x < maxCell)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i, j - 1].cells[x + 1, maxCell]);
                        }
                        ///WEST
                        if (x < maxCell)
                        {
                            sector.cells[x, y].AddNeighbour(sector.cells[x + 1, y]);
                        }
                        else if(i < maxSector)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i + 1, j].cells[0, y]);
                        }
                        ///SOUTH WEST
                        if (x < maxCell && y < maxCell)
                        {
                            sector.cells[x, y].AddNeighbour(sector.cells[x + 1, y + 1]);
                        }
                        else if (x == maxCell && y == maxCell && i < maxSector && j < maxSector)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i + 1, j + 1].cells[0, 0]);
                        }
                        else if (x == maxCell && i < maxSector && y < maxCell)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i + 1, j].cells[0, y + 1]);
                        }
                        else if (y == maxCell && j < maxSector && x < maxCell)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i, j + 1].cells[x + 1, 0]);
                        }
                        ///SOUTH
                        if (y < maxCell)
                        {
                            sector.cells[x, y].AddNeighbour(sector.cells[x, y + 1]);
                        }
                        else if (j < maxSector)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i, j + 1].cells[x, 0]);
                        }
                        ///SOUTH EAST
                        if (x > 0 && y < maxCell)
                        {
                            sector.cells[x, y].AddNeighbour(sector.cells[x - 1, y + 1]);
                        }
                        else if (x == 0 && y == maxCell && i > 0 && j < maxSector)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i - 1, j + 1].cells[maxCell, 0]);
                        }
                        else if (x == 0 && i > 0 && y < maxCell)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i - 1, j].cells[maxCell, y + 1]);
                        }
                        else if (y == maxCell && j < maxSector && x > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i, j + 1].cells[x - 1, 0]);
                        }
                        ///EAST
                        if (x > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sector.cells[x - 1, y]);
                        }
                        else if (i > 0)
                        {
                            sector.cells[x, y].AddNeighbour(sectors[i - 1, j].cells[maxCell, y]);
                        }
                    }
                }
            }
        }
    }

    public static Color GenerateColor(float lightness)
    {
        //Generates the RGB values for the color
        float r = Random.Range(0, 1f);
        float g = Random.Range(0, 1f);
        float b = Random.Range(0, 1f);
        Vector3 hue = new Vector3(r, g, b);
        hue.Normalize();
        //hue *= lightness;
        //Uses those values to create a color
        Color returnColor = new Color();
        returnColor.r = hue.x;
        returnColor.g = hue.y;
        returnColor.b = hue.z;
        return returnColor;
    }

    /// <summary>
    /// Finds a path between origin & destination
    /// </summary>
    public void Pathfind()
    {
        bool foundPath = false;
        //Only Pathfinds if it has an origin & a destination
        if(origin != null && destination != null)
        {
            //Clears all lines currently drawn on the board
            foreach (Sector sector in sectors)
            {
                sector.ClearLines();
            }

            List<Cell> openList = new List<Cell>();
            List<Cell> closedList = new List<Cell>();
            openList.Add(origin);
            //Searches for the end
            while(openList.Count > 0 && !foundPath)
            {
                Cell head = openList.First();
                //Sets the open list cell with the lowest expected cost as head
                foreach (Cell cell in openList)
                {
                    cell.expectedCost = cell.costSoFar + cell.CalculateHeuristic(destination.position);
                    if ( cell.expectedCost < head.expectedCost)
                    {
                        head = cell;
                    }
                }
                //Removes the head from the open list & searches its successors
                openList.Remove(head);
                closedList.Add(head);
                foreach(Cell cell in head.neighbours)
                {
                    //Cannot cross diagonals if the game rule is false
                    if(cell.GetPassable() && (diagonal || head.CheckStraighAlignment(cell)))
                    {
                        //If it's the goal, congrats!
                        if (cell == destination)
                        {
                            cell.parent = head;
                            foundPath = true;
                            break;
                        }
                        //If it's not on the closed list, move on to open list
                        else if (!closedList.Contains(cell))
                        {
                            float newCellCost = head.costSoFar + cell.CostToMove(head.position);
                            //If it's not on the open list, add it
                            if (!openList.Contains(cell))
                            {
                                cell.parent = head;
                                cell.costSoFar = newCellCost;
                                cell.expectedCost = cell.costSoFar + cell.CalculateHeuristic(destination.position);
                                openList.Add(cell);
                            }
                            //If it's on the open list but this is a better path, overwrite the old path
                            else if (newCellCost < cell.costSoFar)
                            {
                                cell.parent = head;
                                cell.costSoFar = newCellCost;
                                cell.expectedCost = cell.costSoFar + cell.CalculateHeuristic(destination.position);
                            }
                        }
                    }
                }
            }
            //Retraces its steps to draw the path
            if(foundPath)
            {
                TracePath();
            }
            results.SetStatus(foundPath);
        }
    }
    /// <summary>
    /// Draws the calulated A* path onto the grid
    /// </summary>
    void TracePath()
    {
        Cell head = destination;
        while(head != origin)
        {
            head.DrawLine(head.parent.position);
            head = head.parent;
        }
    }

    public void SetOrigin(Cell newOrigin)
    {
        origin = newOrigin;
        Pathfind();
    }
    public void SetDestination(Cell newDestination)
    {
        destination = newDestination;
        Pathfind();
    }
}
