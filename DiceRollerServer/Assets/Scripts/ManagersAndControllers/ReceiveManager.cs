using RiptideNetworking;
using System;
using UnityEngine;

public class ReceiveManager : MonoBehaviour
{
    public enum ClientToServerId : ushort
    {
        createCharacter = 0,
        logIntoCharacter,
        claimCharacter,

        // ship captain crew options
        loadPlayersSccBetPage,
        placeSccBet,
        sccActionButtonClicked,
        clickedSccDice,
        sccSpectateButtonClicked,

        // GM Options
        loadPlayersGmScreen,
        sendGmMessageToPlayer,
        gmAddMoneyToPlayer,
        gmRemoveMoneyFromPlayer,
        gmSetMoneyOnPlayer,
        gmGrantGmPower,
        gmRevokeGmPower,
        gmAddStatSkillXp,
        gmRemoveStatSkillXp,
        gmSetStatSkillXp,
        gmAddElementSpellXp,
        gmRemoveElementSpellXp,
        gmSetElementSpellXp,

        loadPlayersSccStatPage,
    }

    [MessageHandler((ushort)ClientToServerId.createCharacter)]
    private static void CreateCharacter(ushort fromClientId, Message message)
    {
        try
        {
            string name = message.GetString();
            int number = message.GetInt();
            Debug.Log($"Received Creating Character request from client: Name={name} number={number}");
            GameManager.Instance.CreateCharacter(fromClientId, name, number);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.logIntoCharacter)]
    private static void LogIntoCharacter(ushort fromClientId, Message message)
    {
        try
        {
            string name = message.GetString();
            int number = message.GetInt();
            Debug.Log($"Received Character login request from client: Name={name} number={number}");
            GameManager.Instance.LogIntoCharacter(fromClientId, name, number, false);
        }
        catch (Exception ex)
        {

            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.claimCharacter)]
    private static void ClaimCharacter(ushort fromClientId, Message message)
    {
        try
        {
            string name = message.GetString();
            int number = message.GetInt();
            Debug.Log($"Received Claim Character request from client: Name={name} number={number}");
            GameManager.Instance.LogIntoCharacter(fromClientId, name, number, true);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.loadPlayersSccBetPage)]
    private static void LoadPlayersSccBetPage(ushort fromClientId, Message message)
    {
        try
        {
            Debug.Log($"player joining SSC Bet Page");
            GameManager.Instance.SendPlayerToSSCBetPage(fromClientId);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.placeSccBet)]
    private static void PlaceSccBet(ushort fromClientId, Message message)
    {
        try
        {
            int bet = message.GetInt();
            Debug.Log($"player placed bet of {bet}");
            GameManager.Instance.PlayerPlacedSccbet(fromClientId, bet);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.sccActionButtonClicked)]
    private static void SccActionButtonClicked(ushort fromClientId, Message message)
    {
        try
        {
            GameManager.Instance.sccManager.PlayerClickedActionButton(fromClientId);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.clickedSccDice)]
    private static void ClickedSccDice(ushort fromClientId, Message message)
    {
        try
        {
            int dieIndex = message.GetInt();
            GameManager.Instance.sccManager.PlayerClickedDie(fromClientId, dieIndex);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.sccSpectateButtonClicked)]
    private static void SccSpectateButtonClicked(ushort fromClientId, Message message)
    {
        try
        {
            Debug.Log("Player trying to spectate");
            GameManager.Instance.sccManager.SpectaterJoined(fromClientId);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.loadPlayersSccStatPage)]
    private static void LoadPlayersSccStatPage(ushort fromClientId, Message message)
    {
        try
        {
            Debug.Log("Player loading stat page");
            GameManager.Instance.LoadPlayerStats(fromClientId);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.loadPlayersGmScreen)]
    private static void LoadPlayersGmScreen(ushort fromClientId, Message message)
    {
        try
        {
            Debug.Log("Player attempting to join GM screen");
            GameManager.Instance.gmManager.PlayerAttemptingToViewGMPage(fromClientId);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.sendGmMessageToPlayer)]
    private static void SendGmMessageToPlayer(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            string messageToSend = message.GetString();
            Debug.Log($"sending message {messageToSend} to playerID {playerId}");

            GameManager.Instance.gmManager.SendGmMessageToPlayer(playerId, messageToSend);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmAddMoneyToPlayer)]
    private static void gmAddMoneyToPlayer(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int money = message.GetInt();
            Debug.Log($"adding {money} amount of money for player");

            GameManager.Instance.gmManager.AddMoneyToPlayer(playerId, money);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmRemoveMoneyFromPlayer)]
    private static void gmRemoveMoneyFromPlayer(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int money = message.GetInt();
            Debug.Log($"removing {money} amount of money from player");

            GameManager.Instance.gmManager.RemoveMoneyFromPlayer(playerId, money);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmSetMoneyOnPlayer)]
    private static void GmSetMoneyOnPlayer(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int money = message.GetInt();
            Debug.Log($"setting {money} amount of money on player");

            GameManager.Instance.gmManager.SetMoneyOnPlayer(playerId, money);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmGrantGmPower)]
    private static void GmGrantGmPower(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            Debug.Log($"Giving a player GM Powers");

            GameManager.Instance.gmManager.GrantGmPowers(playerId);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmRevokeGmPower)]
    private static void GmRevokeGmPower(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            Debug.Log($"removing Gm powers from player");

            GameManager.Instance.gmManager.RevokeGmPowers(playerId, fromClientId);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmAddStatSkillXp)]
    private static void GmAddStatSkillXp(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int statIndex = message.GetInt();
            int skillIndex = message.GetInt();
            int amount = message.GetInt();

            GameManager.Instance.gmManager.AddStatSkillXp(playerId, statIndex, skillIndex, amount);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmRemoveStatSkillXp)]
    private static void GmRemoveStatSkillXp(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int statIndex = message.GetInt();
            int skillIndex = message.GetInt();
            int amount = message.GetInt();

            GameManager.Instance.gmManager.RemoveStatSkillXp(playerId, statIndex, skillIndex, amount);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmSetStatSkillXp)]
    private static void GmSetStatSkillXp(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int statIndex = message.GetInt();
            int skillIndex = message.GetInt();
            int amount = message.GetInt();

            GameManager.Instance.gmManager.SetStatSkillXp(playerId, statIndex, skillIndex, amount);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmAddElementSpellXp)]
    private static void GmAddElementSpellXp(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int elementIndex = message.GetInt();
            int spellIndex = message.GetInt();
            int amount = message.GetInt();

            GameManager.Instance.gmManager.AddElementSpellXp(playerId, elementIndex, spellIndex, amount);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmRemoveElementSpellXp)]
    private static void GmRemoveElementSpellXp(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int elementIndex = message.GetInt();
            int spellIndex = message.GetInt();
            int amount = message.GetInt();

            GameManager.Instance.gmManager.RemoveElementSpellXp(playerId, elementIndex, spellIndex, amount);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }

    [MessageHandler((ushort)ClientToServerId.gmSetElementSpellXp)]
    private static void GmSetElementSpellXp(ushort fromClientId, Message message)
    {
        try
        {
            string playerId = message.GetString();
            int elementIndex = message.GetInt();
            int spellIndex = message.GetInt();
            int amount = message.GetInt();

            GameManager.Instance.gmManager.SetElementSpellXp(playerId, elementIndex, spellIndex, amount);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception: " + ex.Message);
        }
    }
}
