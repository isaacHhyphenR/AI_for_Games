using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] Renderer render;
    [SerializeField] LineRenderer line;
    [HideInInspector] public Vector2 sector;
    public Transform linePos;

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
    //Draws a line from here to destination
    public void DrawLine(Vector3 destination)
    {
        line.enabled = true;
        line.SetPositions(new Vector3[2] {linePos.position, destination });
    }
    public void ClearLine()
    {
        line.enabled = false;
    }

    private void OnMouseDown()
    {
        GridManager.instance.Pathfind(this);
    }
}
