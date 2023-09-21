using System;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] float turnLength; //seconds per turn
    float turnTimer = 0;
    [SerializeField] Color deadColor = Color.white;
    [SerializeField] Color aliveColor = Color.yellow;

    [SerializeField] int rows = 5;
    [SerializeField] int columns = 5;
    [SerializeField] GameObject cellPrefab;
    Cell[,] grid;

    [SerializeField] bool paused;
    [SerializeField] TMP_Text pauseText;
    int turnsElapsed = 0;
    [SerializeField] TMP_Text turnText;

    public static event Action BeginTurn; //cells determine what they will do
    public static event Action EndTurn; //updates cells to match buffer


    bool debug = true; //whether you're currently in debug mode
    [SerializeField] DebugPanel debugPanel;

    private void Awake()
    {
        instance = this;
        SetPauseText();
        //generate cells
        GenerateGrid(columns, rows, false);
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        if(debug)
        {
            turnLength = 1/ debugPanel.gameSpeedSlider.GetValue();
            int sliderColumns = (int)debugPanel.columnsSlider.GetValue();
            int sliderRows = (int)debugPanel.rowsSlider.GetValue();
            if (columns != sliderColumns || rows != sliderRows)
            {
                GenerateGrid(sliderColumns, sliderRows, true);
            }
        }
        if(!paused)
        {
            turnTimer += Time.deltaTime;
            if (turnTimer >= turnLength)
            {
                Turn();
            }
        }
    }

    void GenerateGrid(int newColumns, int newRows, bool keepOldState)
    {
        bool[,] gridState = new bool[rows, columns];
        if (keepOldState)//stores the state of the old grid to replicate living & dead, clears the grid
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (grid[r,c] != null)
                    {
                        gridState[r, c] = grid[r, c].GetAlive();
                        grid[r, c].deactivated = true;
                        Destroy(grid[r, c].gameObject);
                    }
                }
            }
        }
        //builds a new grid
        grid = new Cell[newRows, newColumns];
        for (int r = 0; r < newRows; r++)
        {
            for (int c = 0; c < newColumns; c++)
            {
                GameObject newCell = Instantiate(cellPrefab, new Vector3(transform.position.x + c, transform.position.y, transform.position.z - r), transform.rotation) as GameObject;
                grid[r, c] = newCell.GetComponent<Cell>();
                if(keepOldState && r < rows && c < columns) //keeps the old state
                {
                    grid[r, c].SetAlive(gridState[r,c]);
                }
            }
        }
        //adds wrap around neighbours
        for (int r = 0; r < newRows; r++)
        {
            int leftR = r - 1;
            int rightR = r + 1;
            if (leftR == -1)
            {
                leftR = newRows - 1;
            }
            if (rightR == newRows)
            {
                rightR = 0;
            }
            //adds neighbours to the top row
            grid[r, 0].AddNeighbour(grid[r, newColumns - 1]);
            grid[r, 0].AddNeighbour(grid[leftR, newColumns - 1]);
            grid[r, 0].AddNeighbour(grid[rightR, newColumns - 1]);
            //adds that top row as neighbours to the bottom row
            grid[r, newColumns - 1].AddNeighbour(grid[r, 0]);
            grid[r, newColumns - 1].AddNeighbour(grid[leftR, 0]);
            grid[r, newColumns - 1].AddNeighbour(grid[rightR, 0]);
        }
        for (int c = 0; c < newColumns; c++)
        {
            int topC = c - 1;
            int bottomC = c + 1;
            if (topC == -1)
            {
                topC = newColumns - 1;
            }
            if (bottomC == newColumns)
            {
                bottomC = 0;
            }

            grid[0, c].AddNeighbour(grid[newRows - 1, c]);
            grid[0, c].AddNeighbour(grid[newRows - 1, topC]);
            grid[0, c].AddNeighbour(grid[newRows - 1, bottomC]);

            grid[newRows - 1, c].AddNeighbour(grid[0, c]);
            grid[newRows - 1, topC].AddNeighbour(grid[0, c]);
            grid[newRows - 1, bottomC].AddNeighbour(grid[0, c]);
        }

        columns = newColumns;
        rows = newRows;
    }
    void Turn()
    {
        BeginTurn.Invoke();
        EndTurn.Invoke();
        turnTimer = 0;
        turnsElapsed++;
        turnText.text = "" + turnsElapsed;
    }

    public static Color GetAliveColor()
    {
        return instance.aliveColor;
    }
    public static Color GetDeadColor()
    {
        return instance.deadColor;
    }

    public static void TogglePause()
    {
        instance.paused = !instance.paused;
        instance.SetPauseText();
    }

    void SetPauseText()
    {
        if (instance.paused)
        {
            instance.pauseText.text = "Play";
        }
        else
        {
            instance.pauseText.text = "Pause";
        }
    }

    public static void SetDebug(bool on)
    {
        instance.debug = on;
        instance.debugPanel.gameObject.SetActive(on);
    }
    public static bool GetDebug()
    {
        return instance.debug;
    }

}
