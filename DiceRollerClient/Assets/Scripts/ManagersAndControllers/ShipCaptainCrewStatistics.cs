using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShipCaptainCrewStatistics : MonoBehaviour
{
    [SerializeField]
    TMP_Text HighestBetText;

    [SerializeField]
    TMP_Text BiggestWinText;

    [SerializeField]
    TMP_Text BiggestLossText;

    [SerializeField]
    TMP_Text TotalBetText;

    [SerializeField]
    TMP_Text AverageBetText;

    [SerializeField]
    TMP_Text TotalWonOrLostText;

    [SerializeField]
    TMP_Text MostMoneyHadText;

    [SerializeField]
    TMP_Text GamesPlayedText;

    [SerializeField]
    TMP_Text GamesWonText;

    [SerializeField]
    TMP_Text GamesLostText;

    [SerializeField]
    TMP_Text FirstTurnWinsText;

    [SerializeField]
    TMP_Text RollsPerGameText;

    [SerializeField]
    TMP_Text TotalRollsText;

    CanvasManager canvasManager;
    SendManager sendManager;

    private void Start()
    {
        canvasManager = GameManager.Instance.UI.canvasManager;
        sendManager = GameManager.Instance.networkManager.sendManager;
    }

    public void ViewSccStatPage()
    {
        Debug.Log("Viewing Stat Page");
        canvasManager.TurnOnCanvas(CanvasManager.canvases.ShipCaptainCrewStatisticsCanvas);
        sendManager.LoadPlayersSccStatPage();
    }

    public void DisplayStats(int highestBet, int biggestWin, int biggestLoss, 
        int totalBet, int totalWonOrLost, int mostMoneyHad, 
        int gamesPlayed, int gamesWon, int firstTurnWins, int totalRolls)
    {
        DisplayHighestBet(highestBet);
        DisplayBiggestWin(biggestWin);
        DisplayBiggestLoss(biggestLoss);
        DisplayTotalBet(totalBet);

        int averageBet = totalBet / gamesPlayed;
        DisplayAverageBet(averageBet);

        DisplayTotalWonOrLost(totalWonOrLost);
        DisplayMostMoneyHad(mostMoneyHad);

        DisplayGamesPlayed(gamesPlayed);
        DisplayGamesWon(gamesWon);

        int gamesLost = gamesPlayed - gamesWon;
        DisplayGamesLost(gamesLost);
        DisplayFirstTurnWins(firstTurnWins);

        int rollsPerGame = totalRolls / gamesPlayed;
        DisplayRollsPerGame(rollsPerGame);
        DisplayTotalRolls(totalRolls);
    }

    private void DisplayHighestBet(int highestBet)
    {
        HighestBetText.text = CurrencyManager.ConvertMoneyToString(highestBet, false);
    }

    private void DisplayBiggestWin(int biggestWin)
    {
        BiggestWinText.text = CurrencyManager.ConvertMoneyToString(biggestWin, false);
    }

    private void DisplayBiggestLoss(int biggestLoss)
    {
        BiggestLossText.text = CurrencyManager.ConvertMoneyToString(biggestLoss, false);
    }

    private void DisplayTotalBet(int totalBet)
    {
        TotalBetText.text = CurrencyManager.ConvertMoneyToString(totalBet, false);
    }

    private void DisplayAverageBet(int averageBet)
    {
        AverageBetText.text = CurrencyManager.ConvertMoneyToString(averageBet, false);
    }

    private void DisplayTotalWonOrLost(int totalWonOrLost)
    {
        TotalWonOrLostText.text = CurrencyManager.ConvertMoneyToString(totalWonOrLost, false);
    }

    private void DisplayMostMoneyHad(int mostMoneyHad)
    {
        MostMoneyHadText.text = CurrencyManager.ConvertMoneyToString(mostMoneyHad, false);
    }

    private void DisplayGamesPlayed(int gamesPlayed)
    {
        GamesPlayedText.text = gamesPlayed.ToString();
    }

    private void DisplayGamesWon(int gamesWon)
    {
        GamesWonText.text = gamesWon.ToString();
    }

    private void DisplayGamesLost(int gamesLost)
    {
        GamesLostText.text = gamesLost.ToString();
    }

    private void DisplayFirstTurnWins(int firstTurnWins)
    {
        FirstTurnWinsText.text = firstTurnWins.ToString();
    }

    private void DisplayRollsPerGame(int rollsPerGame)
    {
        RollsPerGameText.text = rollsPerGame.ToString();
    }

    private void DisplayTotalRolls(int totalRolls)
    {
        TotalRollsText.text = totalRolls.ToString();
    }
}
