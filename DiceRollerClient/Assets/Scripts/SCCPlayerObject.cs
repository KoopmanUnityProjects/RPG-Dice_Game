using TMPro;
using UnityEngine;

public class SCCPlayerObject : MonoBehaviour
{
    [SerializeField]
    TMP_Text playerName;

    [SerializeField]
    TMP_Text playerBet;

    public void SetPlayerNameAndBet(string name, string bet)
    {
        playerName.text = name;
        playerBet.text = bet;
    }
}
