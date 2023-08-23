using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSelectionToSCCButton : MonoBehaviour
{
    public void ClickedOnSccGame()
    {
        GameManager.Instance.networkManager.sendManager.LoadPlayersSccBetPage();
    }
}
