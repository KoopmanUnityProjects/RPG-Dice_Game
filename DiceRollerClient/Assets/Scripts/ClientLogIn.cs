using TMPro;
using System.Collections;
using UnityEngine;



public class ClientLogIn : MonoBehaviour
{
    [SerializeField] GameObject CharacterListParent;
    [SerializeField] GameObject CharacterButtonPrefab;

    [SerializeField] TMP_InputField CreateCharacterInput;
    [SerializeField] TMP_InputField ClaimCharacterNameInput;
    [SerializeField] TMP_InputField ClaimCharacterNumberInput;

    private void Start()
    {
        FillCharacterList();
    }

    public void FillCharacterList()
    {
        Debug.Log("Filling Character Buttons");
        int numberofCharacters = PlayerPrefs.GetInt("NumberOfCharacters");
        for (int i = 0; i < numberofCharacters; i++)
        {
            Debug.Log($"Adding character number {i}");
            string characterName = PlayerPrefs.GetString("Character" + i + "Name");
            int characterNumber = PlayerPrefs.GetInt("Character" + i + "Number");
            GameObject logInButton = Instantiate(CharacterButtonPrefab);
            CharacterLogInButton characterButton = logInButton.GetComponent<CharacterLogInButton>();
            characterButton.CreateCharacterName(characterName, characterNumber, i);
            characterButton.transform.SetParent(CharacterListParent.transform, false);
        }
    }

    public void BackToMainPageButtonClicked()
    {
        GameManager.Instance.UI.canvasManager.TurnOnCanvas(CanvasManager.canvases.StartScreenCanvas);
    }

    public void GotoCharacterCreationPageButtonClicked()
    {
        GameManager.Instance.UI.canvasManager.TurnOnCanvas(CanvasManager.canvases.CreateCharacterCanvas);
    }

    public void GotoClaimCharacterPageButtonClicked()
    {
        GameManager.Instance.UI.canvasManager.TurnOnCanvas(CanvasManager.canvases.ClaimCharacterCanvas);
    }

    public void CreateCharacterButtonClicked()
    {
        string name = CreateCharacterInput.text;

        if (ValidateUsername(name))
        {
            Debug.Log($"Create Character Button Clicked: name={name}");
            int randomNumber = Random.Range(1, 1000000);
            GameManager.Instance.SetConnectionStatusAndConnect(NetworkManager.ConnectionState.CreatingCharacter, name, randomNumber);
        }
    }

    bool ValidateUsername(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            GameManager.Instance.DisplayMessage("Please Enter Username");
            Debug.Log("Error: Must enter character name");
            return false;
        }

        if (userName.Length < 3 || userName.Length > 17)
        {
            GameManager.Instance.DisplayMessage("Username must be between 3 and 17 characters");
            Debug.Log("Error: Must be between 3 and 17 chars");
            return false;
        }

        if (userName == "RemoveAll")
        {
            PlayerPrefs.DeleteAll();
            GameManager.Instance.DisplayMessage("All Characters have been removed.");
            Debug.Log("Removed all Characters");
            return false;
        }

        return true;
    }

    public void ClaimCharacterButtonClicked()
    {
        string name = ClaimCharacterNameInput.text;
        if (int.TryParse(ClaimCharacterNumberInput.text, out int number))
        {
            if (name.Length != 0 && number > 0)
            {
                Debug.Log("Claim Character Button Clicked");
                GameManager.Instance.SetConnectionStatusAndConnect(NetworkManager.ConnectionState.ClaimingCharacter, name, number);
            }
            else
            {
                // TODO throw some kinda error
            }
        }
    }

    public void SaveCharacterToLogInRegistery(string characterName, int characterNumber)
    {
        int numberOfCharacters = PlayerPrefs.GetInt("NumberOfCharacters");
        PlayerPrefs.SetString($"Character{numberOfCharacters}Name", characterName);
        PlayerPrefs.SetInt($"Character{numberOfCharacters}Number", characterNumber);
        numberOfCharacters++;
        PlayerPrefs.SetInt("NumberOfCharacters", numberOfCharacters);
        PlayerPrefs.Save();
        AddCharacterToList(characterName, characterNumber);
    }

    public void AddCharacterToList(string characterName, int characterNumber)
    {
        int numberofCharacters = PlayerPrefs.GetInt("NumberOfCharacters");
        int i = numberofCharacters - 1;
        {
            GameObject logInButton = Instantiate(CharacterButtonPrefab);
            CharacterLogInButton characterButton = logInButton.GetComponent<CharacterLogInButton>();
            characterButton.CreateCharacterName(characterName, characterNumber, i);
            characterButton.transform.SetParent(CharacterListParent.transform, false);
        }
    }
}
