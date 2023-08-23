using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShipCaptainCrewPlayer
{
    // this could change for other scc versions.
    const int numberOfDice = 5;

    List<UIDie> diceList;

    bool isComputer;
    Player player;
    int bet;
    string playerName;
    int progress;

    public Enums.ShipCaptainCrewValue currentNeed;
    bool winner;

    public ShipCaptainCrewPlayer(string name)
    {
        isComputer = true;
        playerName = name;
        InitPlayer();
    }

    public ShipCaptainCrewPlayer(Player sccPlayer, int betAmount)
    {
        player = sccPlayer;
        bet = betAmount;
        playerName = player.playersCharacter.characterName;
        InitPlayer();
    }

    public void InitPlayer()
    {
        progress = 0;
        SetNeed();
        diceList = new List<UIDie>();
        for (int i = 0; i < 5; i++)
        {
            UIDie newDie = new UIDie();
            newDie.Index = i;
            newDie.Color = Random.Range(0, 6);
            newDie.Value = -1;
            diceList.Add(newDie);
        }
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public int GetPlayerBet()
    {
        return bet;
    }

    public Player GetPlayer()
    {
        return player;
    }

    public List<UIDie> GetDiceList()
    {
        return diceList;
    }

    public bool IsComputer()
    {
        return isComputer;
    }

    void SetNeed()
    {
        if (progress == 0)
        {
            currentNeed = Enums.ShipCaptainCrewValue.Ship;
        }
        else if (progress == 1)
        {
            currentNeed = Enums.ShipCaptainCrewValue.Captain;
        }
        else
        {
            currentNeed = Enums.ShipCaptainCrewValue.Crew;
        }
    }

    public bool IsDone()
    {
        return (progress == numberOfDice);
    }

    public void AddProgress(UIDie die)
    {
        progress++;
        diceList.Remove(die);

        if (IsDone())
        {
            winner = true;
        }
        else
        {
            SetNeed();
        }
    }

    public int GetProgress()
    {
        return progress;
    }

    // this is only used so that when I send dice to client after a roll
    // the dice will show up in indexes 0 to diceleft-1
    public void ReindexDiceList()
    {
        for (int i = 0; i < diceList.Count; i++)
        {
            diceList[i].Index = i;
        }
    }

    public UIDie GetDieFromIndex(int index)
    {
        return diceList.Where(x => x.Index == index).FirstOrDefault();
    }
}
