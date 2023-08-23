using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GmManager : MonoBehaviour
{
    SendManager sendManager;

    Dictionary<Guid, Player> playerList = new Dictionary<Guid, Player>();

    // TODO consider sending in player ID that is requesting this on each GM action so we can validate that they are a GM before making adjustment.

    // Start is called before the first frame update
    void Start()
    {
        sendManager = GameManager.Instance.networkManager.sendManager;
    }

    public void PlayerAttemptingToViewGMPage(ushort playerId)
    {
        if (CheckIfPlayerIsGM(playerId))
        {
            UpdatePlayerList(); 
            sendManager.SendPlayerToGmScreen(playerId);
            UpdateGmsPlayerList(playerId);
        }
        else
        {
            Debug.Log("Non GM attempted to view GM Page");
        }
    }

    public bool CheckIfPlayerIsGM(ushort playerId)
    {
        return GameManager.Instance.GetPlayer(playerId).GetIsGM();
    }

    public void SendGmMessageToPlayer(string playerIdGuid, string messageToSend)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }

    public void AddMoneyToPlayer(string playerIdGuid, int money)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);
            player.EarnMoney(money);
            string moneyString = CurrencyManager.ConvertMoneyToString(money, false);
            string messageToSend = $"{moneyString} has been added to your character.";
            sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }

    public void RemoveMoneyFromPlayer(string playerIdGuid, int money)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);
            string moneyString = CurrencyManager.ConvertMoneyToString(money, false);

            if (player.SpendMoney(money))
            {
                string moneyLeftString = CurrencyManager.ConvertMoneyToString(player.playersCharacter.money, false);
                string messageToSend = $"{moneyString} has been removed to your character. You have \n{moneyLeftString} remaining.";
                sendManager.SendMessageToClient(playerId, messageToSend);
            }
            else
            {
                player.SetMoney(0);
                string messageToSend = $"All your money has been removed.";
                sendManager.SendMessageToClient(playerId, messageToSend);
            }
        }
    }

    public void SetMoneyOnPlayer(string playerIdGuid, int money)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);
            player.SetMoney(money);
            string moneyString = CurrencyManager.ConvertMoneyToString(money, false);
            string messageToSend = $"Your money has been set to \n{moneyString}.";
            sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }

    public void GrantGmPowers(string playerIdGuid)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);
            player.SetIsGM(true);
            string messageToSend = $"Gm powers now course through your viens!";
            sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }

    public void RevokeGmPowers(string playerIdGuid, ushort gmThatUsedRemoval)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);
            string messageToSend;
            if (player.SetIsGM(false))
            {
                messageToSend = $"Gm powers no longer course through your viens.";
                sendManager.SendMessageToClient(playerId, messageToSend);
            }
            else // if returned false (currenlty only due to trying to remove Blazes GM powers.
            {
                Player GMToBePunished = GameManager.Instance.GetPlayer(gmThatUsedRemoval);
                messageToSend = "How Dare You! \nYou have be stricken of your GM powers for attempting that.";
                GMToBePunished.SetIsGM(false);
                sendManager.SendMessageToClient(playerId, messageToSend);
            }
        }
    }

    public void AddStatSkillXp(string playerIdGuid, int statIndex, int skillIndex, int amount)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);

            Enums.Skills playerSkill = (Enums.Skills)skillIndex;
            Enums.Stats playerStat = (Enums.Stats)statIndex;
            int prevSkillLevel = GameManager.Instance.experienceManager.GetLevel(player, playerSkill);
            int prevStatLevel = GameManager.Instance.experienceManager.GetLevel(player, playerStat);

            string messageToSend = "";
            if (statIndex >= 0 && skillIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerSkill, ExperienceManager.xpOperations.Add, amount);
                GameManager.Instance.experienceManager.ChangeXP(player, playerStat, ExperienceManager.xpOperations.Add, amount);
                messageToSend = $"You have gained {amount} xp in {playerStat} and {playerSkill}";
            }
            else if (statIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerStat, ExperienceManager.xpOperations.Add, amount);
                messageToSend = $"You have gained {amount} xp in {playerStat}";
            }
            else if (skillIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerSkill, ExperienceManager.xpOperations.Add, amount);
                messageToSend = $"You have gained {amount} xp in {playerSkill}";
            }

            GameManager.Instance.saveManager.Save(player);
            sendManager.SendMessageToClient(playerId, messageToSend);

            int currentSkillLevel = GameManager.Instance.experienceManager.GetLevel(player, playerSkill);
            int currentStatLevel = GameManager.Instance.experienceManager.GetLevel(player, playerStat);
            if (currentSkillLevel > prevSkillLevel && currentStatLevel > prevStatLevel)
            {
                messageToSend = $"You have inclreased your {playerStat} to level {currentStatLevel} and your {playerSkill} to level {currentSkillLevel}";
                sendManager.SendMessageToClient(playerId, messageToSend);
            }
            else if (currentSkillLevel > prevSkillLevel)
            {
                messageToSend = $"You have inclreased your {playerSkill} to level {currentSkillLevel}";
                sendManager.SendMessageToClient(playerId, messageToSend);
            }
            else if (currentStatLevel > prevStatLevel)
            {
                messageToSend = $"You have inclreased your {playerStat} to level {currentStatLevel}";
                sendManager.SendMessageToClient(playerId, messageToSend);
            }
        }
    }

    public void RemoveStatSkillXp(string playerIdGuid, int statIndex, int skillIndex, int amount)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);

            Enums.Skills playerSkill = (Enums.Skills)skillIndex;
            Enums.Stats playerStat = (Enums.Stats)statIndex;

            string messageToSend = "";
            if (statIndex >= 0 && skillIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerSkill, ExperienceManager.xpOperations.Remove, amount);
                GameManager.Instance.experienceManager.ChangeXP(player, playerStat, ExperienceManager.xpOperations.Remove, amount);
                messageToSend = $"Your {playerStat} and {playerSkill} have been lowered by {amount}";
            }
            else if (statIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerStat, ExperienceManager.xpOperations.Remove, amount);
                messageToSend = $"Your {playerSkill} has been lowered by {amount}";
            }
            else if (skillIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerSkill, ExperienceManager.xpOperations.Remove, amount);
                messageToSend = $"Your {playerStat} has been lowered by {amount}";
            }

            GameManager.Instance.saveManager.Save(player);
            sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }

    public void SetStatSkillXp(string playerIdGuid, int statIndex, int skillIndex, int amount)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);

            Enums.Skills playerSkill = (Enums.Skills)skillIndex;
            Enums.Stats playerStat = (Enums.Stats)statIndex;

            string messageToSend = "";
            if (statIndex >= 0 && skillIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerSkill, ExperienceManager.xpOperations.Set, amount);
                GameManager.Instance.experienceManager.ChangeXP(player, playerStat, ExperienceManager.xpOperations.Set, amount);
                messageToSend = $"Your {playerStat} and {playerSkill} have been set to {amount}";
            }
            else if (statIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerStat, ExperienceManager.xpOperations.Set, amount);
                messageToSend = $"Your {playerSkill} has been set to {amount}";
            }
            else if (skillIndex >= 0)
            {
                GameManager.Instance.experienceManager.ChangeXP(player, playerSkill, ExperienceManager.xpOperations.Set, amount);
                messageToSend = $"Your {playerStat} has been set to {amount}";
            }

            GameManager.Instance.saveManager.Save(player);
            sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }

    public void AddElementSpellXp(string playerIdGuid, int elementIndex, int spellIndex, int amount)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);

            //player.EarnMoney(money);
            //string moneyString = CurrencyManager.ConvertMoneyToString(money, false);
            //string messageToSend = $"{moneyString} has been added to your character.";
            //sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }

    public void RemoveElementSpellXp(string playerIdGuid, int elementIndex, int spellIndex, int amount)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);

            //player.EarnMoney(money);
            //string moneyString = CurrencyManager.ConvertMoneyToString(money, false);
            //string messageToSend = $"{moneyString} has been added to your character.";
            //sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }

    public void SetElementSpellXp(string playerIdGuid, int elementIndex, int spellIndex, int amount)
    {
        if (Guid.TryParse(playerIdGuid, out Guid Result))
        {
            ushort playerId = playerList[Result].ClientId;
            Player player = GameManager.Instance.GetPlayer(playerId);

            //player.EarnMoney(money);
            //string moneyString = CurrencyManager.ConvertMoneyToString(money, false);
            //string messageToSend = $"{moneyString} has been added to your character.";
            //sendManager.SendMessageToClient(playerId, messageToSend);
        }
    }



    void UpdatePlayerList()
    {
        List<Player> onlinePlayers = GameManager.Instance.GetAllPlayersOnline();

        // add missing players
        List<Player> playersToAdd = new List<Player>();
        foreach (Player player in onlinePlayers)
        {
            if (!playerList.ContainsValue(player))
            {
                playersToAdd.Add(player);
            }
        }
        foreach (Player player in playersToAdd)
        {
            playerList.Add(Guid.NewGuid(), player);
        }


        // remove players no longer online
        List<Guid> playersToRemoveFromList = new List<Guid>();
        foreach (var pair in playerList)
        {
            if (!onlinePlayers.Contains(pair.Value))
            {
                playersToRemoveFromList.Add(pair.Key);
            }
        }

        foreach (Guid idToRemove in playersToRemoveFromList)
        {
            playerList.Remove(idToRemove);
        }
    }

    void UpdateGmsPlayerList(ushort clientId)
    {
        List<Tuple<string, string>> ListOfPlayersIdsAndNames = new List<Tuple<string, string>>();

        foreach (var pair in playerList)
        {
            ListOfPlayersIdsAndNames.Add(new Tuple<string, string>(pair.Key.ToString(), pair.Value.playersCharacter.characterName));
        }

        sendManager.UpdateGmOnlinePlayerList(clientId, ListOfPlayersIdsAndNames);
    }
}
