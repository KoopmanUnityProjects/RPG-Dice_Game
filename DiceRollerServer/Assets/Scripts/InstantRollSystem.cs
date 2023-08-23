using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantRollSystem : MonoBehaviour, IRollSystem
{
    Queue<Roll> rollQueue = new Queue<Roll>();
    bool waitingForRoll = false;

    public void AddRollToQueue(Roll roll)
    {
        rollQueue.Enqueue(roll);
        if (!waitingForRoll)
        {
            RollDiceInQueue();
        }
    }

    public void RollDiceInQueue()
    {
        waitingForRoll = true;

        RollingDice();
    }

    public void RollingDice()
    {
        rollQueue.TryDequeue(out Roll roll);
        foreach (UIDie die in roll.GetDice())
        {
            die.Value = Random.Range(0, 10);
        }

        roll.getCallback().Invoke(roll.GetDice());
        DoneWithRoll();
    }

    public void DoneWithRoll()
    {
        if (rollQueue.Count > 0)
        {
            RollDiceInQueue();
        }
        else
        {
            waitingForRoll = false;
        }
    }

}
