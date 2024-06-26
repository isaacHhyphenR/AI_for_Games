using System.Collections.Generic;
using System.Linq;
using UnityEngine;


enum Direction
{
    RIGHT,
    BOTTOM
}
public class Maze : MonoBehaviour
{
    [SerializeField] GameObject nodePrefab;
    int width;
    int height;
    float wallSize;
    List<Node> nodes = new List<Node>();
    List<GameObject> freeWalls = new List<GameObject>(); //all the walls not part of a node
    [SerializeField] GameObject wallPrefab;
    [Tooltip("How many seconds pass between each iteration")]
    [SerializeField] float tickSpeed;
    float tickTimer = 0;

    List<Node> stack = new List<Node>();
    Node head;

    bool run = true; //whether it should currently run

    [SerializeField] Slider speedSlider;
    [SerializeField] Slider rowsSlider;
    [SerializeField] Slider columnsSlider;

    private void Update()
    {
        tickTimer -= Time.deltaTime * speedSlider.GetValue();

        //determines whether the user wants an iteration to occur
        if(Input.GetKey(KeyCode.Space))
        {
            run = true;
        }
        //runs an iteration if conditions are correct
        if(run && tickTimer < 0 && head != null)
        {
            head.DeActivate(); //previous head no longer red
            tickTimer = tickSpeed;
            if (stack.Count > 0)
            {
                MazeIteration();
            }
        }
    }
    //deletes exiting maze
    public void ClearMaze()
    {
        for(int i = nodes.Count - 1; i  >= 0; i--)
        {
            Destroy(nodes[i].gameObject);
        }
        nodes.Clear();
        stack.Clear();
        //get rid of bounding walls
        for (int i = freeWalls.Count - 1; i >= 0; i--)
        {
            Destroy(freeWalls[i]);
        }

    }

    public void GenerateMaze()
    {
        ClearMaze();
        height = (int)rowsSlider.GetValue();
        width = (int)columnsSlider.GetValue();
        wallSize = wallPrefab.transform.localScale.x;
        //generates the left wall
        for (int y = 0; y < height; y++)
        {
            GameObject rightWall = Instantiate(wallPrefab, transform.position, transform.rotation) as GameObject;
            rightWall.transform.position = new Vector3(-0.5f * wallSize, 0, -y * wallSize) + transform.position;
            rightWall.transform.Rotate(0, 90, 0);
            rightWall.name = "Left" + 0 + "," + y;
            freeWalls.Add(rightWall);
        }
        //generates walls and nodes
        for (int x = 0; x < width; x++) //The vector is 1d
        {
            //generates top wall
            GameObject topWall = Instantiate(wallPrefab, transform.position, transform.rotation) as GameObject;
            topWall.transform.position = new Vector3(x * wallSize, 0, wallSize / 2) + transform.position;
            topWall.name = "Top" + x + "," + 0;
            freeWalls.Add(topWall);
            //generates the nodes for this column
            for (int y = 0; y < height; y++)
            {
                GameObject newNodeObj = Instantiate(nodePrefab, new Vector3(x * wallSize, 0, -y * wallSize) + transform.position, transform.rotation);
                Node newNode = newNodeObj.GetComponent<Node>(); //have to flip x & y some reason
                newNode.x = x;
                newNode.y = y;
                nodes.Add(newNode);
            }
        }


        //inits the stack
        stack.Add(GetNodeAt(0, 0));
        head = stack.Last();
    }

    ///Conducts 1 round of maze work
    void MazeIteration()
    {
        head = stack.Last();
        int currentX = head.x;
        int currentY = head.y;
        head.visited = true;
        head.Activate(); //head shows red

        //Gets the unvisited neighbors
        List<Vector2> unvisitedNeighbors = GetUnvisitedNeighborsOf(currentX, currentY);

        //if the current node has no valid neighbours, revert to previous current node
        if (unvisitedNeighbors.Count < 1)
        {
            stack.RemoveAt(stack.Count - 1);
        }
        //if there's a valid neighbour, open the wall between them & move the head to that position
        else
        {
            //if there's multiple neighbours, choose a random one
            int nextNodeIndex = Random.Range(0,unvisitedNeighbors.Count); 
            //breaks down the next node for human ease of reading
            int nextX = (int)unvisitedNeighbors[nextNodeIndex].x;
            int nextY = (int)unvisitedNeighbors[nextNodeIndex].y;


            //Breaks the relevant wall
            if (nextY < currentY) //up
            {
                GetNodeAt(nextX, nextY).walls[(int)Direction.BOTTOM].SetActive(false);
            }
            else if (nextX > currentX) //right
            {
                GetNodeAt(currentX, currentY).walls[(int)Direction.RIGHT].SetActive(false); ;
            }
            else if (nextY > currentY) //down
            {
                GetNodeAt(currentX, currentY).walls[(int)Direction.BOTTOM].SetActive(false); ;
            }
            else if (nextX < currentX) //left
            {
                GetNodeAt(nextX, nextY).walls[(int)Direction.RIGHT].SetActive(false); ;
            }

            //moves the head, marks the new head as visited
            stack.Add(GetNodeAt(nextX, nextY));
        }
        
    }
    

    ///returns the node at the given coordinate
    Node GetNodeAt(int x, int y)
    {
        int index = height * x + y; //converts 2d coordinates into a 1d index
        if(index < nodes.Count)
        {
            return nodes[index];
        }
        else
        {
            return null;
        }
    }

    ///Returns all the neighbors of the node at Pos that are not yet visited
    List<Vector2> GetUnvisitedNeighborsOf(int x, int y)
    {
        List<Vector2> unvisitedNeighbors = new List<Vector2>();
        //checks whether each potential neighbor is both A: a valid coordinate & B: unvisited
        if (y - 1 >= 0 && !GetNodeAt(x, y - 1).visited) //up
        {
            unvisitedNeighbors.Add(new Vector2(x, y - 1));
        }
        if (x + 1 < width && !GetNodeAt(x + 1, y).visited) //right
        {
            unvisitedNeighbors.Add(new Vector2(x + 1, y));
        }
        if (y + 1 < height && !GetNodeAt(x, y + 1).visited) //down
        {
            unvisitedNeighbors.Add(new Vector2(x, y + 1));
        }
        if (x - 1 >= 0 && !GetNodeAt(x - 1, y).visited) //left
        {
            unvisitedNeighbors.Add(new Vector2(x - 1, y));
        }

        return unvisitedNeighbors;
    }
}
