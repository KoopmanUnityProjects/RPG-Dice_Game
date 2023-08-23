using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDie : MonoBehaviour
{
    [SerializeField]
    private int dieIndex;

    [SerializeField]
    private Image dieImage;

    [SerializeField]
    private TextMeshProUGUI dieText;

    UIDieManager dieManager;

    private int value;
    private int colorIndex;

    private void Start()
    {
        dieManager = GameManager.Instance.UI.dieManager;
    }

    public void UIDieClicked()
    {
        Debug.Log($"die:{dieIndex} clicked");
        dieManager.DieClicked(dieIndex);
    }

    public void HideUIDie()
    {

        dieImage.color = Color.clear;
        dieText.color = Color.clear;
        this.GetComponent<Button>().enabled = false;

        // TODO add selected and such to here too;
    }

    public void ShowUIDie()
    {
        this.GetComponent<Button>().enabled = true;
        dieImage.color = Color.white;
        if (colorIndex == (int)Enums.DieColor.Black)
        {
            dieText.color = Color.white;
        }
        else
        {
            dieText.color = Color.black;
        }

        // TODO add selected and such to here too;
    }

    public void UpdateDie(Enums.DieColor color, int number)
    {
        ShowUIDie();
        colorIndex = (int)color;
        value = number;
        UpdateColorAndText();
    }

    private void UpdateColorAndText()
    {
        dieImage.sprite = GameManager.Instance.UI.dieManager.ColoredDice[colorIndex];
        if (colorIndex == (int)Enums.DieColor.Black)
        {
            dieText.color = Color.white;
        }
        else
        {
            dieText.color = Color.black;
        }

        dieText.text = value.ToString();
    }

    public int GetDieIndex()
    {
        return colorIndex;
    }
}
