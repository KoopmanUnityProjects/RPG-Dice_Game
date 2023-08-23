using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GmPlayerButton : MonoBehaviour
{
    [SerializeField]
    Image backgroundImage;

    [SerializeField]
    TMP_Text playerNameText;

    string characterId;
    string characterName;
    bool selected;

    Color selectedColor = new Color(0, .9f, .25f, .25f);
    Color defaultColor = new Color(0, 0, 0, 0);

    public void SetButtonInfo(string id, string name)
    {
        characterId = id;

        characterName = name;
        playerNameText.text = characterName;

        selected = false;
        backgroundImage.color = defaultColor;
    }

    public void ButtonClicked()
    {
        selected = !selected;

        if (selected)
        {
            backgroundImage.color = selectedColor;
        }
        else
        {
            backgroundImage.color = defaultColor;
        }

        GameManager.Instance.gmOptionsManager.PlayerButtonClicked(this, selected);
    }

    public string getCharacterId()
    {
        return characterId;
    }
}
