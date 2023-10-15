using UnityEngine;
using TMPro;

public class PauseButton : MonoBehaviour
{
    [SerializeField] TMP_Text text;


    private void Start()
    {
        if (GameManager.paused)
        {
            text.text = "Play";
        }
        else
        {
            text.text = "Pause";
        }
    }
    public void TogglePause()
    {
        GameManager.paused = !GameManager.paused;
        if(GameManager.paused)
        {
            text.text = "Play";
        }
        else
        {
            text.text = "Pause";
        }
    }
}

