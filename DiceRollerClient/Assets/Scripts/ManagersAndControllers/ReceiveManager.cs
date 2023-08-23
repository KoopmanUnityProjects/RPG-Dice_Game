using RiptideNetworking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ReceiveManager : MonoBehaviour
{
    public enum ServerToClientId : ushort
    {
        // Error Handling
        reportError = 0,

        // Login 
        registerCharacter,
        logPlayerIn,

        // Dice Stuff
        clearDice,
        showUiDice,
        clearUiDie,
        clearActionButton,
        showActionButton,

        // ShipCaptainCrew
        sendPlayerToSccBetScreen,
        updateSccGameTimer,
        addPlayerToPlayingList,
        setupSccGame,
        updatePlayerSccProgress,
        setViewerSpectatorOption,
        updateSccPlayerTurn,
        updateSccSpectator,

        // Gm Options
        sendPlayerToGmScreen,
        updateGmOnlinePlayerList,

        // Statistics
        sendPlayerStats,

        // message Handling
        sendMessageToClient,
    }

    [MessageHandler((ushort)ServerToClientId.reportError)]
    private static void ReportError(Message message)
    {
        try
        {
            string errorMessage = message.GetString();
            Debug.Log(errorMessage);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.registerCharacter)]
    private static void RegisterCharacter(Message message)
    {
        try
        {
            string characterName = message.GetString();
            int characterNumber = message.GetInt();
            Debug.Log($"RegisterCharacter: {characterName} {characterNumber}");
            GameManager.Instance.SaveCharacterToRegistery(characterName, characterNumber);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.sendMessageToClient)]
    private static void SendMessage(Message message)
    {
        try
        {
            string clientMessage = message.GetString();
            GameManager.Instance.UI.messageManager.AddMessage(clientMessage);
            Debug.Log(clientMessage);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.logPlayerIn)]
    private static void LogInPlayer(Message message)
    {
        try
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            GameManager.Instance.UI.canvasManager.TurnOnCanvas(CanvasManager.canvases.GameSelectionCanvas);

        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.clearDice)]
    private static void ClearDice(Message message)
    {
        try
        {
            GameManager.Instance.UI.dieManager.ClearDice();
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.showUiDice)]
    private static void ShowUiDice(Message message)
    {
        try
        {
            int numberOfDice = message.GetInt();
            Debug.Log($"Received {numberOfDice} to display");
            for (int i = 0; i < numberOfDice; i++)
            {
                int dieIndex = message.GetInt();
                int dieColor = message.GetInt();
                int value = message.GetInt();
                Debug.Log($"Die {numberOfDice}: index-{dieIndex}: colorvalue-{dieColor}: value-{value}");
                GameManager.Instance.UI.dieManager.SetUIDie(dieIndex, (Enums.DieColor)dieColor, value);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.sendPlayerToSccBetScreen)]
    private static void SendPlayerToSccBetScreen(Message message)
    {
        try
        {
            int money = message.GetInt();
            string timer = message.GetString();
            bool gameInProgress = message.GetBool();
            int numberOfPlayers = message.GetInt();

            List<Tuple<string, int>> playersPlaying = new List<Tuple<string, int>>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                string playerName = message.GetString();
                int bet = message.GetInt();
                playersPlaying.Add(Tuple.Create(playerName, bet));
            }

            GameManager.Instance.currentPlayer.character.money = money;
            GameManager.Instance.sccManager.JoinBetScreen(timer, gameInProgress, playersPlaying);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.updateSccGameTimer)]
    private static void UpdateSSCGameTimer(Message message)
    {
        try
        {
            string timerValue = message.GetString();
            GameManager.Instance.sccManager.UpdateSccTimer(timerValue);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.addPlayerToPlayingList)]
    private static void AddPlayerToPlayingList(Message message)
    {
        try
        {
            string playerName = message.GetString();
            int playerBet = message.GetInt();
            GameManager.Instance.sccManager.AddPlayerObjectToListView(playerName, playerBet);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.setupSccGame)]
    private static void SetupSccGame(Message message)
    {
        try
        {
            int numberOfPlayers = message.GetInt();

            List<string> playerList = new List<string>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                string playerName = message.GetString();
                playerList.Add(playerName);
            }

            GameManager.Instance.sccManager.JoinGameScreen(playerList);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.updatePlayerSccProgress)]
    private static void UpdatePlayerSccProgress(Message message)
    {
        try
        {
            int playerNumber = message.GetInt();
            int progress = message.GetInt();
            Debug.Log($"updating player {playerNumber} to progress {progress}");

            GameManager.Instance.UI.sccUiManager.SetPlayerProgress(playerNumber, progress);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.setViewerSpectatorOption)]
    private static void SetViewerSpectatorOption(Message message)
    {
        try
        {
            GameManager.Instance.sccManager.SwitchToGameInProgressGroup();
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.updateSccPlayerTurn)]
    private static void UpdateSccPlayerTurn(Message message)
    {
        try
        {
            bool isMyTurn = message.GetBool();
            int currentPlayerIndex = message.GetInt();
            GameManager.Instance.sccManager.UpdatePlayersTurn(isMyTurn, currentPlayerIndex);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.clearUiDie)]
    private static void ClearUiDie(Message message)
    {
        try
        {
            int dieIndex = message.GetInt();
            GameManager.Instance.UI.dieManager.HideUIDie(dieIndex);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.clearActionButton)]
    private static void ClearActionButton(Message message)
    {
        try
        {
            GameManager.Instance.UI.dieManager.HideActionButton();
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.showActionButton)]
    private static void ShowActionButton(Message message)
    {
        try
        {
            string buttonText = message.GetString();
            GameManager.Instance.UI.dieManager.DisplayActionButton(buttonText);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.updateSccSpectator)]
    private static void UpdateSccSpectator(Message message)
    {
        try
        {
            List<string> players = new List<string>();
            List<int> playerProgress = new List<int>();

            // get Houses progress
            playerProgress.Add(message.GetInt());

            // get players names and progress
            int numberOfPlayers = message.GetInt();

            for (int i = 0; i < numberOfPlayers; i++)
            {
                string name = message.GetString();
                players.Add(name);
                int progress = message.GetInt();
                playerProgress.Add(progress);
            }

            GameManager.Instance.sccManager.JoinGameScreen(players);
            for (int i = 0; i < playerProgress.Count; i++)
            {
                for (int j = 0; j <= playerProgress[i]; j++)
                {
                    GameManager.Instance.UI.sccUiManager.SetPlayerProgress(i, j);
                }
            }

        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.sendPlayerStats)]
    private static void SendPlayerStats(Message message)
    {
        try
        {
            int highestBet = message.GetInt();
            int biggestWin = message.GetInt();
            int biggestLoss = message.GetInt();
            int totalBet = message.GetInt();
            int totalWonOrLost = message.GetInt();

            int mostMoneyHad = message.GetInt();
            int gamesPlayed = message.GetInt();
            int gamesWon = message.GetInt();
            int firstTurnWins = message.GetInt();
            int totalRolls = message.GetInt();

            GameManager.Instance.statManager.sccStats.DisplayStats(
                highestBet, biggestWin, biggestLoss, totalBet, totalWonOrLost,
                mostMoneyHad, gamesPlayed, gamesWon, firstTurnWins, totalRolls);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.sendPlayerToGmScreen)]
    private static void SendPlayerToGmScreen(Message message)
    {
        try
        {
            GameManager.Instance.gmOptionsManager.MoveCharacterToGmOptionScreen();
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ServerToClientId.updateGmOnlinePlayerList)]
    private static void UpdateGmOnlinePlayerList(Message message)
    {
        try
        {
            // get players names and progress
            int numberOfPlayers = message.GetInt();

            Debug.Log($"Currently {numberOfPlayers} online");

            Dictionary<string, string> players = new Dictionary<string, string>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                string id = message.GetString();
                string name = message.GetString();
                players.Add(id, name);
            }

            GameManager.Instance.gmOptionsManager.UpdateCharacterList(players);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }
}
