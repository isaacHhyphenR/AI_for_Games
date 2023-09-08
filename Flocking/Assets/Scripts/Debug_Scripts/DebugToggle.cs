using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugToggle : MonoBehaviour
{
    public void OnClicked()
    {
        GameManager.instance.SetDebug(!GameManager.instance.GetDebug());
    }
}
