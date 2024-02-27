using UnityEngine;
using TMPro;
public class WinPanel : MonoBehaviour
{
    [SerializeField] TMP_Text winText;

    public void GameWon(Player winner)
    {
        winText.color = winner.GetColor();
        winText.text = winner.GetName() + " Wins!";
    }
}
