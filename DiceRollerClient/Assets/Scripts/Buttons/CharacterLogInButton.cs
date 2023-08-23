using TMPro;
using UnityEngine;

public class CharacterLogInButton : MonoBehaviour
{
    public string characterName;
    public int characterNumber;
    public int index;
    public TMP_Text nameInBox;

    public void CreateCharacterName(string name, int number, int indexNumber)
    {
        characterName = name;
        characterNumber = number;
        index = indexNumber;
        PutNameInTextbox();
    }

    public void PutNameInTextbox()
    {
        nameInBox.text = characterName;
    }

    public void CharacterLoginButtonClicked()
    {
        GameManager.Instance.SetConnectionStatusAndConnect(NetworkManager.ConnectionState.LoggingCharacterIn, characterName, characterNumber);
    }
}
