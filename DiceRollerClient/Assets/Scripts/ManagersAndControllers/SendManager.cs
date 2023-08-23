using RiptideNetworking;
using UnityEngine;

public class SendManager : MonoBehaviour
{
    private Client client;

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

    public void SetClient(Client newClient)
    {
        client = newClient;
    }

    public void CreateCharacter(string name, int number)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.createCharacter);
        message.AddString(name);
        message.AddInt(number);
        client.Send(message);
        Debug.Log($"creating character Name ({name}) and number({number})");
    }

    public void LogIntoCharacter(string name, int number)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.logIntoCharacter);
        message.AddString(name);
        message.AddInt(number);
        client.Send(message);
        Debug.Log($"logging into character Name ({name}) and number({number})");
    }

    public void ClaimCharacter(string name, int number)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.claimCharacter);
        message.AddString(name);
        message.AddInt(number);
        client.Send(message);
        Debug.Log($"cliaming character Name ({name}) and number({number})");
    }

    public void LoadPlayersSccBetPage()
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.loadPlayersSccBetPage);
        client.Send(message);
        Debug.Log($"Player Joining SSC Bet page");
    }

    public void PlaceSccBet(int betAmount)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.placeSccBet);
        message.AddInt(betAmount);
        client.Send(message);
        Debug.Log($"Placing bet of {betAmount}");
    }

    public void SccActionButtonClicked()
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.sccActionButtonClicked);
        client.Send(message);
        Debug.Log($"Player Rolling Dice");
    }

    public void ClickedSccDice(int dieIndex)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.clickedSccDice);
        message.AddInt(dieIndex);
        client.Send(message);
        Debug.Log($"Player Clicked on Die {dieIndex}");
    }

    public void SccSpectateButtonClicked()
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.sccSpectateButtonClicked);
        client.Send(message);
        Debug.Log($"SpectateButtonClicked");
    }

    public void LoadPlayersSccStatPage()
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.loadPlayersSccStatPage);
        client.Send(message);
        Debug.Log($"Spectate Button Clicked");
    }

    public void LoadPlayersGmScreen()
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.loadPlayersGmScreen);
        client.Send(message);
        Debug.Log($"Attempting to view GM Options");
    }

    public void SendGmMessageToPlayer(string playerId, string messageToSend)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.sendGmMessageToPlayer);
        message.AddString(playerId);
        message.AddString(messageToSend);
        client.Send(message);
        Debug.Log($"sending message {messageToSend} to playerID {playerId}");
    }

    public void GmAddMoneyToPlayer(string playerId, int money)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmAddMoneyToPlayer);
        message.AddString(playerId);
        message.AddInt(money);
        client.Send(message);
        Debug.Log($"adding {money} amount of money for player");
    }

    public void GmRemoveMoneyFromPlayer(string playerId, int money)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmRemoveMoneyFromPlayer);
        message.AddString(playerId);
        message.AddInt(money);
        client.Send(message);
        Debug.Log($"removing {money} amount of money from player");
    }

    public void GmSetMoneyOnPlayer(string playerId, int money)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmSetMoneyOnPlayer);
        message.AddString(playerId);
        message.AddInt(money);
        client.Send(message);
        Debug.Log($"setting {money} amount of money on player");
    }

    public void GmGrantGmPower(string playerId)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmGrantGmPower);
        message.AddString(playerId);
        client.Send(message);
        Debug.Log($"Granting GM Power");
    }

    public void GmRevokeGmPower(string playerId)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmRevokeGmPower);
        message.AddString(playerId);
        client.Send(message);
        Debug.Log($"Revoking Gm Power");
    }

    public void GmAddStatSkillXp(string playerId, int statIndex, int skillIndex, int amount)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmAddStatSkillXp);
        message.AddString(playerId);

        // -1 because server doesn't use the "NONE" the client does as a selection Option
        message.AddInt(statIndex - 1);
        message.AddInt(skillIndex - 1);
        message.AddInt(amount);
        client.Send(message);
    }

    public void GmRemoveStatSkillXp(string playerId, int statIndex, int skillIndex, int amount)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmRemoveStatSkillXp);
        message.AddString(playerId);

        // -1 because server doesn't use the "NONE" the client does as a selection Option
        message.AddInt(statIndex - 1);
        message.AddInt(skillIndex - 1);
        message.AddInt(amount);
        client.Send(message);
    }

    public void GmSetStatSkillXp(string playerId, int statIndex, int skillIndex, int amount)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmSetStatSkillXp);
        message.AddString(playerId);

        // -1 because server doesn't use the "NONE" the client does as a selection Option
        message.AddInt(statIndex - 1);
        message.AddInt(skillIndex - 1);
        message.AddInt(amount);
        client.Send(message);
    }

    public void GmAddElementSpellXp(string playerId, int elementIndex, int spellIndex, int amount)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmAddStatSkillXp);
        message.AddString(playerId);

        // -1 because server doesn't use the "NONE" the client does as a selection Option
        message.AddInt(elementIndex - 1);
        message.AddInt(spellIndex - 1);
        message.AddInt(amount);
        client.Send(message);
    }

    public void GmRemoveElementSpellXp(string playerId, int ElementIndex, int spellIndex, int amount)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmRemoveStatSkillXp);

        // -1 because server doesn't use the "NONE" the client does as a selection Option
        message.AddString(playerId);
        message.AddInt(ElementIndex - 1);
        message.AddInt(spellIndex - 1);
        message.AddInt(amount);
        client.Send(message);
    }

    public void GmSetElementSpellXp(string playerId, int elementIndex, int spellIndex, int amount)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.gmSetStatSkillXp);
        message.AddString(playerId);

        // -1 because server doesn't use the "NONE" the client does as a selection Option
        message.AddInt(elementIndex - 1);
        message.AddInt(spellIndex - 1);
        message.AddInt(amount);
        client.Send(message);
    }
}
