using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnMarker : MonoBehaviour
{
    [SerializeField] Color catColor;
    [SerializeField] Color catcherColor;
    [SerializeField] string catText;
    [SerializeField] string catcherText;
    [SerializeField] Image background;
    [SerializeField] TMP_Text text;

    public void SetTurn(bool cat)
    {
        if (cat)
        {
            background.color = catColor;
            text.text = catText;
        }
        else
        {
            background.color = catcherColor;
            text.text = catcherText;
        }
    }
}
