using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    int playerNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    playerNumber = 0;
        //    Debug.Log("Changed to house");
        //}
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    playerNumber = 1;
        //    Debug.Log("Changed to player1");
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    playerNumber = 2;
        //    Debug.Log("Changed to player2");
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    playerNumber = 3;
        //    Debug.Log("Changed to player3");
        //}
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    playerNumber = 4;
        //    Debug.Log("Changed to player4");
        //}
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    playerNumber = 5;
        //    Debug.Log("Changed to player5");
        //}
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameManager.Instance.UI.sccUiManager.ClearAllProgress();
        }

        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    GameManager.Instance.UI.sccUiManager.SetPlayerProgress(playerNumber, 1);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    GameManager.Instance.UI.sccUiManager.SetPlayerProgress(playerNumber, 2);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    GameManager.Instance.UI.sccUiManager.SetPlayerProgress(playerNumber, 3);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    GameManager.Instance.UI.sccUiManager.SetPlayerProgress(playerNumber, 4);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    GameManager.Instance.UI.sccUiManager.SetPlayerProgress(playerNumber, 5);
        //}
    }

    public void RollSpecificDie(int index)
    {
        int dieColor = Random.Range(0, 6);
        int Number = Random.Range(0, 10);
        GameManager.Instance.UI.dieManager.SetUIDie(index, (Enums.DieColor)dieColor, Number);
    }
  
    public void RollXDice(int numberOfDice)
    {
        GameManager.Instance.UI.dieManager.ClearDice();

        for (int i = 0; i < numberOfDice; i++)
        {
            int dieColor = Random.Range(0, 6);
            int Number = Random.Range(0, 10);
            GameManager.Instance.UI.dieManager.SetUIDie(i, (Enums.DieColor)dieColor, Number);
        }
    }
}
