using UnityEngine;
using TMPro;

public class BoardSpace : MonoBehaviour
{
    [SerializeField] TMP_Text displayText;
    Vector2 coordinates;

    private void OnMouseDown()
    {
        if (displayText.text == "" && !GameManager.isAiTurn())
        {
            SelectSpace();
        }
    }

    public void SelectSpace()
    {
        GameManager.SpaceSelected(this);
    }

    /// <summary>
    /// Sets which character this space is displaying
    /// </summary>
    /// <param name="value"></param>
    public void SetDisplay(char value)
    {
        displayText.text = value + "";
    }
    /// <summary>
    /// Returns the character this space is displaying
    /// </summary>
    /// <returns></returns>
    public char GetValue()
    {
        return displayText.text[0];
    }
    public float GetSize()
    {
        return transform.localScale.x;
    }

    public void SetCoordinates(Vector2 _coordinates)
    {
        coordinates = _coordinates;
    }
    public Vector2 GetCoordinates()
    {
        return coordinates;
    }
}
