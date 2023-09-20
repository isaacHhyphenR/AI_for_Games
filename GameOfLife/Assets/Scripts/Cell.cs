using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] bool alive = false;
    bool tempState = false;
    Cell[] neighbours = new Cell[8];
    [SerializeField] Renderer cellRenderer;
    int neighboursSet = 0;

    private void Start()
    {
        SetNeighbours();
        UpdateColor();
        GameManager.BeginTurn += CalculateState;
        GameManager.EndTurn += UpdateBuffer;
    }

    void SetNeighbours() //adds all nearby cells to the neighbours array
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.x);
        foreach(Collider collider in colliders)
        {
            Cell neighbour = collider.gameObject.GetComponent<Cell>();
            if (neighbour != null && neighbour != this)
            {
                neighbours[neighboursSet] = neighbour;
                neighboursSet++;
            }
        }
    }

    void CalculateState()
    {
        int livingNeighbours = 0;
        if(neighbours.Length > 0)
        {
            foreach (Cell neighbour in neighbours)
            {
                if (neighbour != null && neighbour.GetAlive())
                {
                    livingNeighbours++;
                }
            }
        }
        // sets state to alive if rules met, otherwise die
        if (alive && (livingNeighbours == 2 || livingNeighbours == 3))
        {
            tempState = true;
        }
        else if (!alive && livingNeighbours == 3)
        {
            tempState = true;
        }
        else
        {
            tempState = false;
        }
    }
    void UpdateBuffer()
    {
        alive = tempState;
        UpdateColor();
    }

    void UpdateColor()
    {
        if (alive)
        {
            cellRenderer.material.color = GameManager.GetAliveColor();
        }
        else
        {
            cellRenderer.material.color = GameManager.GetDeadColor();
        }
    }
    public void AddNeighbour(Cell newNeighbour) //to add neighbours from the other edge
    {
        bool contains = false;
        foreach(Cell neighbour in neighbours)
        {
            if(neighbour == newNeighbour)
            {
                contains = true;
                break;
            }
        }
        if(!contains)
        {
            neighbours[neighboursSet] = newNeighbour;
            neighboursSet++;
        }
    }
    public bool GetAlive()
    {
        return alive;
    }

    private void OnMouseDown()
    {
        alive = !alive;
        tempState = alive;
        UpdateColor();
    }

    private void OnDisable()
    {
        GameManager.BeginTurn -= CalculateState;
        GameManager.EndTurn -= UpdateBuffer;
    }
}
