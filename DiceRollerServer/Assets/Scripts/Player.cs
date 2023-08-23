using UnityEngine;

public class Player : MonoBehaviour
{
    public Character playersCharacter;

    public ushort ClientId;

    public string Username;

    public int characterNumber;

    public void CreateCharacter(string name, int number)
    {
        if (playersCharacter == null)
        {
            playersCharacter = new Character();
        }

        Username = name;
        characterNumber = number;
        playersCharacter.characterName = name;
    }

    public bool SetIsGM(bool gm)
    {
        if (!gm && playersCharacter.characterName == "Blaze") // TODO add number with this
        {
            return false;
        }

        playersCharacter.isGm = gm;
        GameManager.Instance.saveManager.Save(this);
        return true;
    }

    public bool GetIsGM()
    {
        return playersCharacter.isGm;
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= playersCharacter.money)
        {
            playersCharacter.money -= amount;
            GameManager.Instance.saveManager.Save(this);
            return true;
        }

        Debug.Log($"Attempting to spend {amount} but character only has {playersCharacter.money}");

        return false;
    }

    public void EarnMoney(int amount)
    {
        playersCharacter.money += amount;

        // track most money had.
        if (playersCharacter.money > playersCharacter.mostMoney)
        {
            playersCharacter.mostMoney = playersCharacter.money;
        }

        GameManager.Instance.saveManager.Save(this);
    }

    public void SetMoney(int amount)
    {
        if (amount < 0)
        {
            amount = 0;
        }

        playersCharacter.money = amount;

        // track most money had.
        if (playersCharacter.money > playersCharacter.mostMoney)
        {
            playersCharacter.mostMoney = playersCharacter.money;
        }

        GameManager.Instance.saveManager.Save(this);
    }

    public bool PlayerPlacedSCCBet(int amount)
    {
        if (amount <= playersCharacter.money)
        {
            // track total bet
            playersCharacter.sccTotalBet += amount;

            // track highest bet
            if (playersCharacter.sccHighestBet < amount)
            {
                playersCharacter.sccHighestBet = amount;
            }

            // tack games played
            playersCharacter.sccGamesPlayed++;

            SpendMoney(amount);

            return true;
        }

        return false;
    }

    public void PlayerWonSCCGame(int amountWon, int turnNumber)
    {
        // track games won
        playersCharacter.sccGamesWon++;

        // track total rolls
        playersCharacter.sccTotalRolls += turnNumber;

        // track total won or lost
        playersCharacter.sccTotalWonorLost += amountWon;

        // track first turn wins
        if (turnNumber == 1)
        {
            playersCharacter.sccFirstTurnWins++;
        }

        // track biggest win
        if (amountWon > playersCharacter.sccBiggestWin)
        {
            playersCharacter.sccBiggestWin = amountWon;
        }

        EarnMoney(amountWon);
    }

    public void PlayerLostSCCGame(int amountBet, int turnNumber)
    {
        // track total rolls
        playersCharacter.sccTotalRolls += turnNumber;

        // track total won or lost
        playersCharacter.sccTotalWonorLost -= amountBet;

        // track biggest loss
        if (amountBet > playersCharacter.sccBiggestLoss)
        {
            playersCharacter.sccBiggestLoss = amountBet;
        }

        GameManager.Instance.saveManager.Save(this);
    }

    public void SendPlayerSccStats()
    {
        GameManager.Instance.networkManager.sendManager.SendPlayerStats(
            ClientId,
            playersCharacter.sccHighestBet,
            playersCharacter.sccBiggestWin,
            playersCharacter.sccBiggestLoss,
            playersCharacter.sccTotalBet,
            playersCharacter.sccTotalWonorLost,
            playersCharacter.mostMoney,
            playersCharacter.sccGamesPlayed,
            playersCharacter.sccGamesWon,
            playersCharacter.sccFirstTurnWins,
            playersCharacter.sccTotalRolls);
    }
}
