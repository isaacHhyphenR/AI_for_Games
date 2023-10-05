using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Slider : MonoBehaviour
{
    [SerializeField] GameObject sliderButton;
    [SerializeField] float sliderLength; //how far you can move the slider
    [SerializeField] Vector2 extremeValues; //the amount represented by the far left/far right ends
    Vector2 extremePositions; //the min/max X pos the slider can be at
    [SerializeField] float sliderValue; //the slider's current value
    [SerializeField] TMP_Text minText; //displays the minimum value
    [SerializeField] TMP_Text maxText; //displays the maximum value
    [SerializeField] TMP_Text currentText; //displays the current value
    [SerializeField] bool canFloat = false; // true = can have decimals, false = needs int

    private void Start()
    {
        extremePositions.x = transform.position.x - (sliderLength / 2);
        extremePositions.y = transform.position.x + (sliderLength / 2);
        //sets the sliders position based on starting value
        Vector3 newPos = sliderButton.transform.position;
        newPos.x = (sliderValue + extremeValues.x) / (extremeValues.y - extremeValues.x); //finds proportion
        newPos.x *= sliderLength;//accounts for line size
        newPos.x += transform.position.x - (sliderLength/2); //otherwise it's in global space for some reason
        sliderButton.transform.position = newPos;
        //texts
        minText.text = "" + extremeValues.x;
        maxText.text = "" + extremeValues.y;
        UpdateValue();
    }

    public Vector3 SliderPos(Vector3 pos) //determines value based on slider position & clamps the slider's position
    {
        float newX = Mathf.Clamp(pos.x, extremePositions.x, extremePositions.y);
        sliderValue = (newX - extremePositions.x) / sliderLength; //determines porportional position
        sliderValue *= (extremeValues.y - extremeValues.x); //translates that into value
        sliderValue += extremeValues.x;
        UpdateValue();
        return new Vector3(newX,pos.y, pos.z);
    }

    void UpdateValue()
    {
        if(!canFloat)
        {
            sliderValue = (int)sliderValue;
        }
        else
        {
            sliderValue = (float)System.Math.Round(sliderValue, 1);
        }
        currentText.text = "" + sliderValue;
    }

    public float GetValue()
    {
        return sliderValue;
    }
}
