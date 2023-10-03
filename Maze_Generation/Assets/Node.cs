using Unity.VisualScripting;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool visited = false;
    public GameObject[] walls;
    public Renderer renderMat;

    public int x;
    public int y;
    public void Activate()
    {
        renderMat.material.color = new Color(renderMat.material.color.r, renderMat.material.color.g, renderMat.material.color.b, 1f);
    }
    public void DeActivate()
    {
        renderMat.material.color = new Color(renderMat.material.color.r, renderMat.material.color.g, renderMat.material.color.b, 0f);
    }
}
