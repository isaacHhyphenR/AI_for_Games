using UnityEngine;
using TMPro;

public class CurrentPlayer : MonoBehaviour
{
    [SerializeField] TMP_Text display;

    private void OnEnable()
    {
        GameManager.newTurn.AddListener(player => SetPlayer(player));
    }

    void SetPlayer(Player player)
    {
        display.text = player.Character() + "";
    }
}
