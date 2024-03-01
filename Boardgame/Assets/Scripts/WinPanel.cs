using UnityEngine;
using TMPro;
public class WinPanel : MonoBehaviour
{
    [SerializeField] TMP_Text winText;
    [SerializeField] TMP_Text winReasonText;

    public void GameWon(Player winner)
    {
        winText.color = winner.GetColor();
        winText.text = winner.GetName() + " Wins!";
        winReasonText.text = GameManager.GetOtherPlayer(winner).GetName() + " " + GameManager.GetOtherPlayer(winner).lostReason;
    }
}
