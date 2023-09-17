using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] float turnLength; //seconds per turn
    float turnTimer = 0;
    [SerializeField] Color deadColor = Color.white;
    [SerializeField] Color aliveColor = Color.yellow;
    
    public static event Action BeginTurn; //cells determine what they will do
    public static event Action EndTurn; //updates cells to match buffer

    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        turnTimer += Time.deltaTime;
        if(turnTimer >= turnLength)
        {
            Turn();
        }
    }

    void Turn()
    {
        BeginTurn.Invoke();
        EndTurn.Invoke();
        turnTimer = 0;
    }

    public static Color GetAliveColor()
    {
        return instance.aliveColor;
    }
    public static Color GetDeadColor()
    {
        return instance.deadColor;
    }

}
