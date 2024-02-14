using UnityEngine;

struct Sector
{
    public Cell[,] cells;
    Color sectorColor;
    GameObject cellPrefab;
    Vector3 sectorPosition;
    float cellSize;
    int width;
    public Vector2 sectorID;
    public bool hasLines;
    public Sector(int sectorWidth, GameObject cell, Color color, Vector3 position, Vector2 ID)
    {
        cells = new Cell[sectorWidth, sectorWidth];
        sectorColor = color;
        cellPrefab = cell;
        cellSize = cell.GetComponent<Cell>().GetSize();
        sectorPosition = position;
        width = sectorWidth;
        sectorID = ID;
        hasLines = false;
        GenerateCells();
    }

    public void GenerateCells()
    {
        //The corner from which the cells begin generating
        Vector3 originPosition = sectorPosition;
        originPosition.x -= width * cellSize / 2;
        originPosition.z -= width * cellSize / 2;
        //Creates all the cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                GameObject newCellObj = GameObject.Instantiate(cellPrefab, originPosition, cellPrefab.transform.rotation);
                Cell newCell = newCellObj.GetComponent<Cell>();
                newCell.transform.position = new Vector3(newCell.transform.position.x + (x * cellSize), newCell.transform.position.y, newCell.transform.position.z + (y * cellSize));
                newCell.SetColor(sectorColor);
                newCell.name = (int)sectorID.x + "," + (int)sectorID.y + "_Cell_" + x + "," + y;
                newCell.sector = sectorID;
                cells[x,y] = newCell;
            }
        }
    }
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
    Sector[,] sectors;
    float cellSize;

    public static GridManager instance;

    private void Start()
    {
        sectors = new Sector[gridWidth, gridWidth];
        cellSize = cellPrefab.GetComponent<Cell>().GetSize();
        //The height & width (in Unity units) of a sector.
        float sectorActualWidth = cellSize * sectorWidth;
        //The corner from which the cells begin generating
        Vector3 originPosition = transform.position;
        originPosition.x -= (sectorActualWidth * gridWidth / 2) - sectorActualWidth / 2;
        originPosition.z -= (sectorActualWidth * gridWidth / 2) - sectorActualWidth / 2;
        //Generates each sector
        for (int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridWidth; y++)
            {
                Vector3 pos = new Vector3(originPosition.x + x * sectorActualWidth, originPosition.y, originPosition.z + y * sectorActualWidth);
                Sector newSector = new Sector(sectorWidth, cellPrefab, GenerateColor(lightness), pos, new Vector2(x,y));
                sectors[x,y] = newSector;
            }
        }
        //Instance
        GridManager.instance = this;
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

    //Draws a path
    public void Pathfind(Cell origin)
    {
        //Clears all lines currently drawn on the board
        foreach(Sector sector in sectors)
        {
            if (sector.hasLines)
            {
                foreach(Cell cell in sector.cells)
                {
                    cell.ClearLine();
                }
            }
        }
        //Pathfinds
        foreach(Cell cell in sectors[(int)origin.sector.x, (int)origin.sector.y].cells)
        {
            cell.DrawLine(origin.linePos.position);
            sectors[(int)cell.sector.x, (int)cell.sector.y].hasLines = true;
        }
    }
}
