using RiptideNetworking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SendManager : MonoBehaviour
{
    private Server server;

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

    public void SetServer(Server newServer)
    {
        server = newServer;
    }

    public void SendErrorToClient(ushort clientID, string error)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.reportError);
        message.AddString(error);
        server.Send(message, clientID);
        Debug.Log($"Sending error to client: {error}");
    }

    public void SendMessageToClient(ushort clientID, string messageToSend)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.sendMessageToClient);
        message.AddString(messageToSend);
        server.Send(message, clientID);
        Debug.Log($"Sending message to client: {messageToSend}");
    }

    public void RegisterClientsCharacter(ushort clientID, string name, int number)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.registerCharacter);
        message.AddString(name);
        message.AddInt(number);
        server.Send(message, clientID);
        Debug.Log($"Registering clients character Name:{name}  Number:{number}");
    }

    public void LogPlayerIn(ushort clientID)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.logPlayerIn);
        server.Send(message, clientID);
        Debug.Log($"Log Player In");
    }

    public void ClearDice(ushort clientID)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.clearDice);
        server.Send(message, clientID);
        Debug.Log($"Clearing Players Dice");
    }

    public void ShowUiDice(ushort clientID, List<UIDie> diceList)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.showUiDice);
        message.AddInt(diceList.Count);
        foreach (UIDie die in diceList)
        {
            message.AddInt(die.Index);
            message.AddInt(die.Color);
            message.AddInt(die.Value);
        }

        server.Send(message, clientID);
    }

    public void SendPlayerToSccBetScreen(ushort clientID, int money, string timeLeft, bool gameInProgress, List<ShipCaptainCrewPlayer> playersPlayingNextRound)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.sendPlayerToSccBetScreen);
        message.AddInt(money);
        message.AddString(timeLeft);
        message.AddBool(gameInProgress);

        // dont include house as player
        message.AddInt(playersPlayingNextRound.Count - 1);
        for (int i = 1; i < playersPlayingNextRound.Count; i++)
        {
            message.AddString(playersPlayingNextRound[i].GetPlayerName());
            message.AddInt(playersPlayingNextRound[i].GetPlayerBet());
        }

        server.Send(message, clientID);
        Debug.Log($"Sending Player To SccBetScreen");
    }

    public void UpdateSccGameTimer(ushort clientID, string gameTimer)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.updateSccGameTimer);
        message.AddString(gameTimer);
        server.Send(message, clientID);
        Debug.Log($"Sending sccTimer to client: {gameTimer}");
    }

    public void AddPlayerToPlayingList(ushort clientID, string playerName, int betAmount)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.addPlayerToPlayingList);
        message.AddString(playerName);
        message.AddInt(betAmount);
        server.Send(message, clientID);
        Debug.Log($"Adding player to playing list.  Player: {playerName}  Bet: {betAmount}");
    }

    public void SetupSccGame(ushort clientID, List<string> playerNames)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.setupSccGame);
        message.AddInt(playerNames.Count);
        foreach (string name in playerNames)
        {
            message.AddString(name);
        }
        server.Send(message, clientID);
        Debug.Log($"Setting up SCC game");
    }

    public void UpdatePlayerSccProgress(ushort clientID, int playerNumber, int progress)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.updatePlayerSccProgress);
        message.AddInt(playerNumber);
        message.AddInt(progress);

        server.Send(message, clientID);
        Debug.Log($"updating player {playerNumber} to progress {progress}");
    }

    public void SetViewerSpectatorOption(ushort clientID)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.setViewerSpectatorOption);
 
        server.Send(message, clientID);
        Debug.Log("GameStarted player didn't bet so now has spectate option.");
    }

    public void UpdateSccPlayerTurn(ushort clientID, bool playersTurn, int currentTurnPlayerIndex)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.updateSccPlayerTurn);
        message.AddBool(playersTurn);
        message.AddInt(currentTurnPlayerIndex);
 
        server.Send(message, clientID);
        Debug.Log($"Index of current turn: {currentTurnPlayerIndex}  is it this players turn? {playersTurn}");
    }

    public void ClearUiDie(ushort clientID, int dieIndex)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.clearUiDie);
        message.AddInt(dieIndex);
 
        server.Send(message, clientID);
        Debug.Log($"clearing die from index {dieIndex}");
    }

    public void ClearActionButton(ushort clientID)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.clearActionButton);  
        server.Send(message, clientID);
        Debug.Log($"clearing action button");
    }

    public void ShowActionButton(ushort clientID, string buttonText)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.showActionButton);
        message.AddString(buttonText);

        server.Send(message, clientID);
        Debug.Log($"setting action button to {buttonText}");
    }

    public void ShowSpectatorSccProgress(ushort clientID, List<Tuple<string, int>> playersProgress)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.updateSccSpectator);

        // add houses progress spearately
        message.AddInt(playersProgress[0].Item2);

        // add all players names and progress
        message.AddInt(playersProgress.Count - 1);
        for (int i = 1; i < playersProgress.Count; i++)
        {
            message.AddString(playersProgress[i].Item1);
            message.AddInt(playersProgress[i].Item2);
        }

        server.Send(message, clientID);
        Debug.Log($"updating spectator on players progress");
    }

    public void SendPlayerStats(ushort clientId, int highestBet, int biggestWin, int biggestLoss,
        int totalBet, int totalWonOrLost, int mostMoneyHad,
        int gamesPlayed, int gamesWon, int firstTurnWins, int totalRolls)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.sendPlayerStats);

        // add houses progress spearately
        message.AddInt(highestBet);
        message.AddInt(biggestWin);
        message.AddInt(biggestLoss);
        message.AddInt(totalBet);
        message.AddInt(totalWonOrLost);
        message.AddInt(mostMoneyHad);
        message.AddInt(gamesPlayed);
        message.AddInt(gamesWon);
        message.AddInt(firstTurnWins);
        message.AddInt(totalRolls);

        server.Send(message, clientId);
        Debug.Log("Sending player stats");
    }

    public void SendPlayerToGmScreen(ushort clientID)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.sendPlayerToGmScreen);
        server.Send(message, clientID);
        Debug.Log($"Sending Player to Gm Options Screen");
    }

    public void UpdateGmOnlinePlayerList(ushort clientID, List<Tuple<string, string>> players)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.updateGmOnlinePlayerList);
        message.AddInt(players.Count);
        foreach (var player in players)
        {
            // add ID
            message.AddString(player.Item1);

            // add name
            message.AddString(player.Item2);
        }
   
        server.Send(message, clientID);
        Debug.Log($"Updating Online PlayerList for GM to show {players.Count} players");
    }
}
