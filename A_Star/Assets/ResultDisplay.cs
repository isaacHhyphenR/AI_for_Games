using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultDisplay : MonoBehaviour
{
    [SerializeField] Image statusImage;
    [SerializeField] Color successColor;
    [SerializeField] Color failureColor;
    [SerializeField] TMP_Text statusText;
    [SerializeField] string successText;
    [SerializeField] string failureText;

    bool status = false;
    public void SetStatus(bool success)
    {
        status = success;
        if(status)
        {
            statusImage.color = successColor;
            statusText.text = successText;
        }
        else
        {
            statusImage.color = failureColor;
            statusText.text = failureText;
        }
    }
}
