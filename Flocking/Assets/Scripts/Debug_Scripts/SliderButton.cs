using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderButton : MonoBehaviour
{
    [SerializeField] Slider parent;
    bool followMouse = false;

    // Update is called once per frame
    void Update()
    {
        if(followMouse)
        {
            if(!Input.GetMouseButton(0))//stop following mouse on mouse release
            {
                followMouse = false;
            }
            else
            {
                Vector3 newPos = new Vector3(Input.mousePosition.x, transform.position.y, transform.position.z);
                transform.position = parent.SliderPos(newPos);
            }
        }
    }

    public void OnClicked()
    {
        followMouse = true;
    }
}
