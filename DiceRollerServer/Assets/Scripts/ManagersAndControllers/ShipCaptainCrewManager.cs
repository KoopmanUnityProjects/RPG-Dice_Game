using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCaptainCrewManager : MonoBehaviour
{
    const int MAX_NUM_OF_PLAYERS = 5;
    const float BET_SCREEN_TIMER = 30.0f;
    const float TIME_PER_TURN = 45.0f;

    SendManager sendManager;
    RollManager rollManager;

    List<ShipCaptainCrewPlayer> playingRound = new List<ShipCaptainCrewPlayer>();
    HashSet<Player> betScreenViewers = new HashSet<Player>();
    HashSet<Player> gameScreenViewers = new HashSet<Player>();

    ShipCaptainCrewPlayer currentPlayer;
    int playerIndex = 0;
    int turnNumber = 0;

    float startRoundTimer = -1.0f;
    float timeBetweenGames = 2.0f;
    string displayTimer = "--";

    Enums.ShipCaptainCrewTurnState turnState;
    bool gameInProgress = false;

    private void Start()
    {
        sendManager = GameManager.Instance.networkManager.sendManager;
        rollManager = GameManager.Instance.rollManager;
    }

    public void PlayerJoinsBetScreen(Player player)
    {
        AddPlayerToBetScreenViewers(player);
        sendManager.SendPlayerToSccBetScreen(player.ClientId, player.playersCharacter.money, displayTimer, gameInProgress, playingRound);
    }

    public void PlayerPlacedBet(Player player, int bet)
    {
        if (player.PlayerPlacedSCCBet(bet))
        {
            AddCharacterToPlayingList(player, bet);
        }
    }

    void AddCharacterToPlayingList(Player player, int bet)
    {
        AddPlayerToPlayingRound(player, bet);

        if (playingRound.Count <= 1)
        {
            StartCoroutine(nameof(HandleCountDownTimer));
        }

        foreach (Player viewer in betScreenViewers)
        {
            sendManager.AddPlayerToPlayingList(viewer.ClientId, player.name, bet);
        }
    }

    void StartNewGame()
    {
        gameInProgress = true;
        AdjustVewiersListForPlayers();
        Debug.Log($"Starting game with {playingRound.Count} players");
        playingRound.Insert(0, new ShipCaptainCrewPlayer("House"));

        turnNumber = 0;
        playerIndex = playingRound.Count - 1;
        StartNewTurn();
    }

    void StartNewTurn()
    {
        turnState = Enums.ShipCaptainCrewTurnState.ReadyToRoll;

        // move to next player, if done keep moving until one isn't done.
        IncrimentPlayer();
        while (currentPlayer.IsDone())
        {
            IncrimentPlayer();
        }

        StartCoroutine(nameof(HandleTurnTimer), currentPlayer);

        if (currentPlayer.IsComputer())
        {
            foreach (Player player in gameScreenViewers)
            {
                sendManager.UpdateSccPlayerTurn(player.ClientId, false, playerIndex);
            }

            StartCoroutine(nameof(HandleComputersTurn));
        }
        else
        {
            foreach (Player player in gameScreenViewers)
            {
                bool playersTurn = false;
                if (player == currentPlayer.GetPlayer())
                {
                    playersTurn = true;
                    Debug.Log("Its current players turn");
                }
                sendManager.UpdateSccPlayerTurn(player.ClientId, playersTurn, playerIndex);
            }
        }
    }

    void IncrimentPlayer()
    {
        playerIndex++;
        if (playerIndex >= playingRound.Count)
        {
            playerIndex = 0;
            turnNumber++;
        }
        currentPlayer = playingRound[playerIndex];
    }

    public void PlayerClickedDie(ushort fromClientId, int dieIndex)
    {
        // Make sure turn state is correct, and current player is clicking dice.
        if (turnState != Enums.ShipCaptainCrewTurnState.SelectingDice || (!currentPlayer.IsComputer() && currentPlayer.GetPlayer().ClientId != fromClientId))
        {
            return;
        }

        UIDie die = currentPlayer.GetDieFromIndex(dieIndex);
        if (CheckValue(die.Value) == currentPlayer.currentNeed)
        {
            RemoveDieAndAddProgress(die);
        }
    }

    void RemoveDieAndAddProgress(UIDie die)
    {
        currentPlayer.AddProgress(die);
        foreach (Player player in gameScreenViewers)
        {
            sendManager.ClearUiDie(player.ClientId, die.Index);
            sendManager.UpdatePlayerSccProgress(player.ClientId, playerIndex, currentPlayer.GetProgress());
        }
    }

    public void PlayerClickedActionButton(ushort fromClientId)
    {
        // make sure current player isn't computer, and matches Id of request
        if (!currentPlayer.IsComputer() && currentPlayer.GetPlayer().ClientId == fromClientId)
        {
            if (turnState == Enums.ShipCaptainCrewTurnState.ReadyToRoll)
            {
                turnState = Enums.ShipCaptainCrewTurnState.Rolling;
                sendManager.ClearActionButton(fromClientId);
                Debug.Log("Player is rolling Dice!");

                rollManager.RollDice(currentPlayer.GetDiceList(), DisplayRolledDice);
            }
            else if (turnState == Enums.ShipCaptainCrewTurnState.SelectingDice)
            {
                turnState = Enums.ShipCaptainCrewTurnState.EndTurn;
                sendManager.ClearActionButton(fromClientId);
                Debug.Log("Player ended turn");
                FinishTurn();
            }
        }
    }

    public void DisplayRolledDice(List<UIDie> dice)
    {
        // TODO dont do this unless its still the current players turn;
        //  may need a Player interface to make this happen so we can send
        // a player (thats not scc specific) along with the roll

        // TODO with other roll systems will need a check to determine leaners.
        bool leaners = false;

        if (leaners)
        {
            if (!currentPlayer.IsComputer())
            {
                sendManager.ShowActionButton(currentPlayer.GetPlayer().ClientId, "Reroll Leaners");
            }
            turnState = Enums.ShipCaptainCrewTurnState.Rerolling;
        }
        else
        {
            if (!currentPlayer.IsComputer())
            {
                sendManager.ShowActionButton(currentPlayer.GetPlayer().ClientId, "End Turn");
            }
            turnState = Enums.ShipCaptainCrewTurnState.SelectingDice;
        }

        Debug.Log($"Displaying {dice.Count} Rolled Dice");
        foreach (Player player in gameScreenViewers)
        {
            sendManager.ShowUiDice(player.ClientId, dice);
        }
    }

    public Enums.ShipCaptainCrewValue CheckValue(int dieValue)
    {
        if (dieValue == 1 || dieValue == 2)
            return Enums.ShipCaptainCrewValue.Ship;
        else if (dieValue == 9 || dieValue == 0)
            return Enums.ShipCaptainCrewValue.Captain;
        else if (dieValue >= 4 && dieValue <= 7)
            return Enums.ShipCaptainCrewValue.Crew;
        else
            return Enums.ShipCaptainCrewValue.Wench;
    }

    void FinishTurn()
    {
        turnState = Enums.ShipCaptainCrewTurnState.EndTurn;
        currentPlayer.ReindexDiceList();
        foreach (Player player in gameScreenViewers)
        {
            sendManager.ClearDice(player.ClientId);
            sendManager.UpdatePlayerSccProgress(player.ClientId, playerIndex, currentPlayer.GetProgress());
        }

        if (!currentPlayer.IsComputer() && currentPlayer.IsDone())
        {
            Debug.Log("Player Wins");
            int amountWon = currentPlayer.GetPlayerBet() * 2;
            string amountText = CurrencyManager.ConvertMoneyToString(amountWon, false);
            currentPlayer.GetPlayer().PlayerWonSCCGame(amountWon, turnNumber);
            sendManager.SendMessageToClient(currentPlayer.GetPlayer().ClientId, $"Congrats you won {amountText}");
        }

        if (!IsEndOfGame())
        {
            StartNewTurn();
        }
        else
        {
            StartCoroutine(nameof(AddTimeAtEndOfGame));
        }
    }

    bool IsEndOfGame()
    {
        if (playerIndex == 0 && currentPlayer.IsDone())
        {
            return true;
        }
       
        for (int i = 1; i < playingRound.Count; i++)
        {
            if (!playingRound[i].IsDone())
            {
                return false;
            }
        }
        
        return true;
    }

    void AdjustVewiersListForPlayers()
    {
        gameScreenViewers.Clear();
        List<string> playerNameList = new List<string>();
        List<Player> playerList = new List<Player>();

        foreach (ShipCaptainCrewPlayer sccPlayer in playingRound)
        {
            playerList.Add(sccPlayer.GetPlayer());
            playerNameList.Add(sccPlayer.GetPlayerName());
        }

        foreach (Player player in betScreenViewers)
        {
            if (playerList.Contains(player))
            {
                sendManager.SetupSccGame(player.ClientId, playerNameList);
                gameScreenViewers.Add(player);
            }
            else
            {
                sendManager.SetViewerSpectatorOption(player.ClientId);
            }
        }

        foreach (Player player in gameScreenViewers)
        {
            betScreenViewers.Remove(player);
        }
    }

    public void SpectaterJoined(ushort clientId)
    {
        List<Tuple<string, int>> PlayerInfo = new List<Tuple<string, int>>();

        foreach (ShipCaptainCrewPlayer sccPlayer in playingRound)
        {
            PlayerInfo.Add(new Tuple<string, int> (sccPlayer.GetPlayerName(), sccPlayer.GetProgress()));
        }

        gameScreenViewers.Add(GameManager.Instance.GetPlayer(clientId));
        sendManager.ShowSpectatorSccProgress(clientId, PlayerInfo);
        sendManager.UpdateSccPlayerTurn(clientId, false, playerIndex);
    }

    IEnumerator AddTimeAtEndOfGame()
    {
        for (int i = 1; i < playingRound.Count; i++)
        {
            if (!playingRound[i].IsDone() && !playingRound[i].IsComputer())
            {
                Player player = playingRound[i].GetPlayer();
                sendManager.SendMessageToClient(player.ClientId, "House Wins");
                player.PlayerLostSCCGame(playingRound[i].GetPlayerBet(), turnNumber);
            }
        }

        gameInProgress = false;
        yield return new WaitForSeconds(timeBetweenGames);
        playingRound.Clear();
        displayTimer = "--";
        foreach (Player player in gameScreenViewers)
        {
            PlayerJoinsBetScreen(player);
        }
        gameScreenViewers.Clear();
    }

    IEnumerator HandleCountDownTimer()
    {
        startRoundTimer = BET_SCREEN_TIMER;
        while (startRoundTimer > -0.5f)
        {
            yield return new WaitForSeconds(0.249f);
            startRoundTimer = startRoundTimer - 0.25f;
            int timeInSeconds = (int)startRoundTimer;
            displayTimer = timeInSeconds.ToString();
            foreach (Player player in betScreenViewers)
            {
                sendManager.UpdateSccGameTimer(player.ClientId, displayTimer);
            }
        }

        StartNewGame();
    }

    IEnumerator HandleComputersTurn()
    {
        // TODO  Add checks for ensuring its still computers turn at each step
        yield return new WaitForSeconds(1.0f);
        List<UIDie> computersDice = currentPlayer.GetDiceList();
        int totalDiceCount = computersDice.Count;

        if (turnState == Enums.ShipCaptainCrewTurnState.ReadyToRoll)
        {
            turnState = Enums.ShipCaptainCrewTurnState.Rolling;
            Debug.Log("Computer is rolling Dice!");
            rollManager.RollDice(computersDice, DisplayRolledDice);
        }

        while (turnState == Enums.ShipCaptainCrewTurnState.Rolling)
        {
            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.5f);
        while (turnState == Enums.ShipCaptainCrewTurnState.Rerolling)
        {
            Debug.Log("Computer is rerolling leaners");
            // DO stuff in here to loop until rerolling completed
            turnState = Enums.ShipCaptainCrewTurnState.SelectingDice;
        }

        yield return new WaitForSeconds(1.0f);
        while (turnState == Enums.ShipCaptainCrewTurnState.SelectingDice)
        {
            for (int i = 0; i < totalDiceCount; i++)
            {
                UIDie die = currentPlayer.GetDieFromIndex(i);
                Debug.Log($"Checking Die {i}");
                if (die != null && CheckValue(die.Value) == currentPlayer.currentNeed)
                {
                    yield return new WaitForSeconds(0.5f);
                    Debug.Log($"Found Need on die {i}");
                    RemoveDieAndAddProgress(die);
                    i = -1;
                }
            }

            yield return new WaitForSeconds(1.0f);

            turnState = Enums.ShipCaptainCrewTurnState.EndTurn;
        }

        FinishTurn();
    }

    IEnumerator HandleTurnTimer(ShipCaptainCrewPlayer player)
    {
        float roundTimer = TIME_PER_TURN;
        while (currentPlayer == player && roundTimer > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            roundTimer -= 1.0f;
        }

        if (currentPlayer == player && gameInProgress)
        {
            FinishTurn();
        }
    }

    #region Add And Remove people from viewing and playing lists.    
    public void AddPlayerToBetScreenViewers(Player player)
    {
        betScreenViewers.Add(player);
    }

    public void AddPlayerToPlayingRound(Player player, int bet)
    {
        ShipCaptainCrewPlayer sccPlayer = new ShipCaptainCrewPlayer(player, bet);
        playingRound.Add(sccPlayer);
    }

    public void RemovePlayerFromBetScreenViewers(Player player)
    {
        betScreenViewers.Remove(player);
    }

    public void RemovePlayerFromPlayingRound(ShipCaptainCrewPlayer player)
    {
        playingRound.Remove(player);
    }
    #endregion
}