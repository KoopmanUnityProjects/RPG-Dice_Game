using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GmOptionsManager : MonoBehaviour
{
    [SerializeField]
    GameObject MainOptions;

    [SerializeField]
    GameObject MoneyOptions;

    [SerializeField]
    GameObject MessageOptions;

    [SerializeField]
    GameObject StatsAndSkillsOptions;

    [SerializeField]
    GameObject CampeignOptions;

    [SerializeField]
    GameObject GmPowerOptions;

    [SerializeField]
    TMP_InputField MessageInput;

    [SerializeField]
    GameObject onlineCharacterPrefab;

    [SerializeField]
    GameObject onlineListParent;

    SendManager sendManager;
    CanvasManager canvasManager;

    [SerializeField]
    TMP_InputField GoldInput;

    [SerializeField]
    TMP_InputField SilverInput;

    [SerializeField]
    TMP_InputField CopperInput;

    [SerializeField]
    TMP_InputField XpAmountInput;

    [SerializeField]
    TMP_Dropdown AttributeDropdown;

    [SerializeField]
    TMP_Dropdown SkillDropdown;

    [SerializeField]
    TMP_Dropdown ElementDropdown;

    [SerializeField]
    TMP_Dropdown SpellDropdown;

    [SerializeField]
    TMP_InputField XpAmount;


    List<GmPlayerButton> selectedPlayers = new List<GmPlayerButton>();
    List<GameObject> playersLoggedInObjects = new List<GameObject>();

    //TODO figure out a better way of doing this, this feels kinda hacky.
    bool resettingFormerDropDownSettings = false;

    private void Start()
    {
        sendManager = GameManager.Instance.networkManager.sendManager;
        canvasManager = GameManager.Instance.UI.canvasManager;

        FillDropDowns();
    }

    public void MoveCharacterToGmOptionScreen()
    {
        canvasManager.TurnOnAdditionalCanvas(CanvasManager.canvases.GameMasterCanvas);
        LoadMainOptions();
    }

    public void MessageButtonClicked()
    {
        LoadMessageOptions();
    }

    public void MoneyButtonClicked()
    {
        LoadMoneyOptions();
    }

    public void StatsAndSkillsButtonClicked()
    {
        LoadStatsAndSkillsOptions();
    }

    public void CampeignButtonClicked()
    {
        LoadCampeignOptions();
    }

    public void GmPowerOptionsButtonClicked()
    {
        LoadGmPowerOptions();
    }

    public void SendMessageButtonClicked()
    {
        string message = MessageInput.text;
        if (string.IsNullOrEmpty(message))
        {
            Debug.Log("Attempted to send a empty or null string");
            return;
        }

        SendMessageToPlayers(message);
    }

    public void BackToMainOptionsButtonClicked()
    {
        LoadMainOptions();
    }

    public void UpdateCharacterList(Dictionary<string, string> characters)
    {
        playersLoggedInObjects.Clear();
        selectedPlayers.Clear();

        foreach (var character in characters)
        {
            GameObject gmCharacter = Instantiate(onlineCharacterPrefab, onlineListParent.transform);
            GmPlayerButton gmCharScript = gmCharacter.GetComponent<GmPlayerButton>();
            string id = character.Key;
            string name = character.Value;
            gmCharScript.SetButtonInfo(id, name);
            playersLoggedInObjects.Add(gmCharacter);
        }
    }

    public void PlayerButtonClicked(GmPlayerButton playerButton, bool selected)
    {
        if (selected)
        {
            selectedPlayers.Add(playerButton);
        }
        else
        {
            selectedPlayers.Remove(playerButton);
        }
    }

    public void AddMoneyButtonClicked()
    {
        int money = ParseMoneyInput();

        if (money > 0)
        {
            foreach (GmPlayerButton player in selectedPlayers)
            {
                sendManager.GmAddMoneyToPlayer(player.getCharacterId(), money);
            }
        }
    }

    public void RemoveMoneyButtonClicked()
    {
        int money = ParseMoneyInput();

        if (money > 0)
        {
            foreach (GmPlayerButton player in selectedPlayers)
            {
                sendManager.GmRemoveMoneyFromPlayer(player.getCharacterId(), money);
            }
        }
    }

    public void SetMoneyButtonClicked()
    {
        int money = ParseMoneyInput();

        if (money > 0)
        {
            foreach (GmPlayerButton player in selectedPlayers)
            {
                sendManager.GmSetMoneyOnPlayer(player.getCharacterId(), money);
            }
        }
    }

    public void GrantGmPowersButtonClicked()
    {
        if (selectedPlayers.Count != 1)
        {
            Debug.Log("Please select 1 player to grant Gm powers");
            return;
        }
        GmPlayerButton player = selectedPlayers[0];
        sendManager.GmGrantGmPower(player.getCharacterId());
    }

    public void RevokeGmPowersButtonClicked()
    {
        if (selectedPlayers.Count != 1)
        {
            Debug.Log("Please select 1 player to remove Gm powers");
            return;
        }
        GmPlayerButton player = selectedPlayers[0];
        sendManager.GmRevokeGmPower(player.getCharacterId());
    }

    public void AddXpButtonClicked()
    {
        int statSelectedIndex = AttributeDropdown.value;
        int skillSelectedIndex = SkillDropdown.value;
        int elementSelectedIndex = AttributeDropdown.value;
        int spellSelectedIndex = SkillDropdown.value;
        if (int.TryParse(XpAmount.text, out int xp))
        {
            if (xp <= 0)
            {
                Debug.Log("Attempting to grant 0 xp");
            }

            if (statSelectedIndex > 0 || skillSelectedIndex > 0)
            {
                foreach (GmPlayerButton player in selectedPlayers)
                {
                    sendManager.GmAddStatSkillXp(player.getCharacterId(), statSelectedIndex, skillSelectedIndex, xp);
                }
            }
            else if (elementSelectedIndex > 0 || spellSelectedIndex > 0)
            {
                foreach (GmPlayerButton player in selectedPlayers)
                {
                    sendManager.GmAddStatSkillXp(player.getCharacterId(), elementSelectedIndex, spellSelectedIndex, xp);
                }
            }
        }
    }

    public void RemoveXpButtonClicked()
    {
        int statSelectedIndex = AttributeDropdown.value;
        int skillSelectedIndex = SkillDropdown.value;
        int elementSelectedIndex = AttributeDropdown.value;
        int spellSelectedIndex = SkillDropdown.value;
        if (int.TryParse(XpAmount.text, out int xp))
        {
            if (xp <= 0)
            {
                Debug.Log("Attempting to grant 0 xp");
            }

            if (statSelectedIndex > 0 || skillSelectedIndex > 0)
            {
                foreach (GmPlayerButton player in selectedPlayers)
                {
                    sendManager.GmRemoveStatSkillXp(player.getCharacterId(), statSelectedIndex, skillSelectedIndex, xp);
                }
            }
            else if (elementSelectedIndex > 0 || spellSelectedIndex > 0)
            {
                foreach (GmPlayerButton player in selectedPlayers)
                {
                    sendManager.GmRemoveStatSkillXp(player.getCharacterId(), elementSelectedIndex, spellSelectedIndex, xp);
                }
            }
        }
    }

    public void SetXpButtonClicked()
    {
        int statSelectedIndex = AttributeDropdown.value;
        int skillSelectedIndex = SkillDropdown.value;
        int elementSelectedIndex = AttributeDropdown.value;
        int spellSelectedIndex = SkillDropdown.value;
        if (int.TryParse(XpAmount.text, out int xp))
        {
            if (statSelectedIndex > 0 || skillSelectedIndex > 0)
            {
                foreach (GmPlayerButton player in selectedPlayers)
                {
                    sendManager.GmSetStatSkillXp(player.getCharacterId(), statSelectedIndex, skillSelectedIndex, xp);
                }
            }
            else if (elementSelectedIndex > 0 || spellSelectedIndex > 0)
            {
                foreach (GmPlayerButton player in selectedPlayers)
                {
                    sendManager.GmSetStatSkillXp(player.getCharacterId(), elementSelectedIndex, spellSelectedIndex, xp);
                }
            }
        }
    }

    public void ClearXpButtonClicked()
    {
        AttributeDropdown.value = 0;
        SkillDropdown.value = 0;
        AttributeDropdown.value = 0;
        SkillDropdown.value = 0;
        XpAmount.text = "";
    }
    
    public void StatOrSkillChanged()
    {
        int statSelectedIndex = AttributeDropdown.value;
        int skillSelectedIndex = SkillDropdown.value;

        if ((statSelectedIndex != 0 || skillSelectedIndex != 0) && !resettingFormerDropDownSettings)
        {
            resettingFormerDropDownSettings = true;
            ElementDropdown.value = 0;
            SpellDropdown.value = 0;
            resettingFormerDropDownSettings = false;
        }       
    }

    public void ElementOrSpellChanged()
    {
        int elementSelectedIndex = AttributeDropdown.value;
        int spellSelectedIndex = SkillDropdown.value;

        if ((elementSelectedIndex != 0 || spellSelectedIndex != 0) && !resettingFormerDropDownSettings)
        {
            resettingFormerDropDownSettings = true;
            AttributeDropdown.value = 0;
            SkillDropdown.value = 0;
            resettingFormerDropDownSettings = false;
        }
    }

    public void SpellValueChanged(int value)
    {
        if (value > 0)
        {
            SpellDropdown.value = value;
            AttributeDropdown.value = 0;
            SkillDropdown.value = 0;
        }
    }


    int ParseMoneyInput()
    {
        int.TryParse(GoldInput.text, out int gold);
        int.TryParse(SilverInput.text, out int silver);
        int.TryParse(CopperInput.text, out int copper);

        // validate each field
        if (gold < 0 || silver < 0 || copper < 0)
        {
            GameManager.Instance.DisplayMessage("All currency fields must be positive.");
            return - 1;
        }

        // validate total
        return copper + (silver * 100) + (gold * 10000);
    }

    void LoadMainOptions()
    {
        MainOptions.SetActive(true);
        MoneyOptions.SetActive(false);
        MessageOptions.SetActive(false);
        StatsAndSkillsOptions.SetActive(false);
        CampeignOptions.SetActive(false);
        GmPowerOptions.SetActive(false);
    }

    void LoadMoneyOptions()
    {
        MainOptions.SetActive(false);
        MoneyOptions.SetActive(true);
        GoldInput.text = "";
        SilverInput.text = "";
        CopperInput.text = "";
    }

    void LoadMessageOptions()
    {
        MainOptions.SetActive(false);
        MessageOptions.SetActive(true);
        MessageInput.text = "";
    }

    void LoadStatsAndSkillsOptions()
    {
        MainOptions.SetActive(false);
        StatsAndSkillsOptions.SetActive(true);
    }

    void LoadCampeignOptions()
    {
        MainOptions.SetActive(false);
        CampeignOptions.SetActive(true);
    }

    void LoadGmPowerOptions()
    {
        MainOptions.SetActive(false);
        GmPowerOptions.SetActive(true);
    }

    void FillDropDowns()
    {
        List<string> m_DropOptions = new List<string>();
        m_DropOptions.Add("None");
        for (int i = 0; i < (int)Enums.Statistics.NumberOfStats; i++)
        {
            m_DropOptions.Add(((Enums.Statistics)i).ToString());
        }
        AttributeDropdown.AddOptions(m_DropOptions);


        m_DropOptions.Clear();
        m_DropOptions.Add("None");
        for (int i = 0; i < (int)Enums.Skills.NumberOfSkills; i++)
        {
            m_DropOptions.Add(((Enums.Skills)i).ToString());
        }
        SkillDropdown.AddOptions(m_DropOptions);

        m_DropOptions.Clear();
        m_DropOptions.Add("None");
        for (int i = 0; i < (int)Enums.Elements.NumberOfElements; i++)
        {
            m_DropOptions.Add(((Enums.Elements)i).ToString());
        }
        ElementDropdown.AddOptions(m_DropOptions);

        m_DropOptions.Clear();
        m_DropOptions.Add("None");
        for (int i = 0; i < (int)Enums.Spells.NumberOfSpells; i++)
        {
            m_DropOptions.Add(((Enums.Spells)i).ToString());
        }
        SpellDropdown.AddOptions(m_DropOptions);
    }

    void SendMessageToPlayers(string message)
    {
        foreach (GmPlayerButton player in selectedPlayers)
        {
            sendManager.SendGmMessageToPlayer(player.getCharacterId(), message);
        }
    }
}
