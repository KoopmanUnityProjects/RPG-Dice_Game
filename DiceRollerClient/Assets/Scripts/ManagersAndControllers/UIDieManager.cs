using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDieManager : MonoBehaviour
{
    public Sprite[] ColoredDice;

    [SerializeField]
    UIDie[] UIDice = new UIDie[10];

    [SerializeField]
    Button DiceActionButton;

    TMP_Text DiceActionButtonText;

    public event Action OnActionButtonClick = delegate { };
    public event Action<int> UIDieClick = delegate { };

    private void Start()
    {
        DiceActionButtonText = DiceActionButton.GetComponentInChildren<TMP_Text>();
        if (DiceActionButtonText == null)
        {
            Debug.Log("DiceButtonTextNotFound");
        }
    }

    public void ShowUIDie(int index)
    {
        UIDice[index].ShowUIDie();
    }

    public void HideUIDie(int index)
    {
        UIDice[index].HideUIDie();
    }

    public void SetUIDie(int index, Enums.DieColor color, int value)
    {
        UIDice[index].UpdateDie(color, value);
    }
    
    public void ClearDice()
    {
        foreach (var die in UIDice)
        {
            die.HideUIDie();
        }
    }

    public void HideActionButton()
    {
        Debug.Log("Hiding Action Button");
        DiceActionButton.GetComponent<Image>().color = Color.clear;
        DiceActionButton.enabled = false;
        DiceActionButtonText.text = "";

    }

    public void DisplayActionButton(string displayText)
    {
        Debug.Log("Attempting to display action button");
        DiceActionButton.GetComponent<Image>().color = Color.white;
        DiceActionButton.enabled = true;
        DiceActionButtonText.text = displayText;
    }

    public void ActionButtonClicked()
    {
        OnActionButtonClick();
    }

    public void DieClicked(int index)
    {
        UIDieClick(index);
    }
}
