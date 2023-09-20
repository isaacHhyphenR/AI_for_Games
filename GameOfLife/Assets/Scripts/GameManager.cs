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
        grid = new Cell[rows,columns];
        for (int r = 0; r < rows; r++)
        {
            for(int c = 0; c < columns; c++)
            {
                GameObject newCell = Instantiate(cellPrefab, new Vector3(transform.position.x + r, transform.position.y, transform.position.z - c), transform.rotation) as GameObject;
                grid[r,c] = newCell.GetComponent<Cell>();
            }
        }
        //adds wrap around neighbours
        for(int r = 0; r < rows; r++)
        {
            int leftR = r - 1;
            int rightR = r + 1;
            if (leftR == -1)
            {
                leftR = rows - 1;
            }
            if(rightR == rows)
            {
                rightR = 0;
            }
            //adds neighbours to the top row
            grid[r, 0].AddNeighbour(grid[r, columns - 1]);
            grid[r, 0].AddNeighbour(grid[leftR, columns - 1]);
            grid[r, 0].AddNeighbour(grid[rightR, columns - 1]);
            //adds that top row as neighbours to the bottom row
            grid[r, columns - 1].AddNeighbour(grid[r, 0]);
            grid[r, columns - 1].AddNeighbour(grid[leftR, 0]);
            grid[r, columns - 1].AddNeighbour(grid[rightR, 0]);
        }
        for (int c = 0; c < columns; c++)
        {
            int topC = c - 1;
            int bottomC = c + 1;
            if (topC == -1)
            {
                topC = columns - 1;
            }
            if (bottomC == columns)
            {
                bottomC = 0;
            }

            grid[0, c].AddNeighbour(grid[rows - 1, c]);
            grid[0, c].AddNeighbour(grid[rows - 1, topC]);
            grid[0, c].AddNeighbour(grid[rows - 1, bottomC]);

            grid[rows - 1, c].AddNeighbour(grid[0, c]);
            grid[rows - 1, topC].AddNeighbour(grid[0, c]);
            grid[rows - 1, bottomC].AddNeighbour(grid[0, c]);
        }
    }
    private void Update()
    {
        if(debug)
        {
            turnLength = 1/ debugPanel.gameSpeedSlider.GetValue();
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
    }
    public static bool GetDebug()
    {
        return instance.debug;
    }

}
