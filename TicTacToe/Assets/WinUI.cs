using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinUI : MonoBehaviour
{
    [SerializeField] TMP_Text victorDisplay;
    [SerializeField] Image background;
    [SerializeField] float clickActivationTime;
    float activationTimer = 0;
    bool toggled = false;

    private void OnEnable()
    {
        GameManager.stalemate.AddListener(OnStalemate);
        GameManager.playerWon.AddListener(victor => OnPlayerWon(victor));
    }

    private void Update()
    {
        if (toggled)
        {
            activationTimer += Time.deltaTime;
        }
    }
    void OnPlayerWon(Player victor)
    {
        ToggleUI(true);
        victorDisplay.text = victor.Character() + " won the game!";
    }
    void OnStalemate()
    {
        ToggleUI(true);
        victorDisplay.text = "Stalemate!";
    }

    void ToggleUI(bool on)
    {
        activationTimer = 0;
        toggled = on;
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(toggled);
        }
        background.enabled = toggled;
    }

    public void Replay()
    {
        if(activationTimer > clickActivationTime)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
