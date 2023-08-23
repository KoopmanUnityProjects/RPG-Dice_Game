using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RollManager;

public class RollManager : MonoBehaviour
{
    public delegate void RollReturnFunction(List<UIDie> dice);

    IRollSystem instantRollSystem;

    private void Start()
    {
        instantRollSystem = GetComponentInChildren<InstantRollSystem>();
    }

    // TODO This will have to be changed once I have more roll systems.
    public void RollDice(List<UIDie> diceToRoll, RollReturnFunction callback)
    {
        Roll roll = new Roll(diceToRoll, callback);
        instantRollSystem.AddRollToQueue(roll);
    }
}

public class Roll
{
    List<UIDie> dice;
    RollReturnFunction callback;


    public Roll(List<UIDie> diceToRoll, RollReturnFunction returnFunction)
    {
        dice = diceToRoll;
        callback = returnFunction;
    }
    
    public List<UIDie> GetDice()
    {
        return dice;
    }

    public RollReturnFunction getCallback()
    {
        return callback;
    }
}

