using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField] Heuristic heuristic;

    public void SetHeuristic()
    {
        GridManager.heuristic = heuristic;
    }
    public void ToggleDiagonal()
    {
        GridManager.diagonal = !GridManager.diagonal;
    }
}
