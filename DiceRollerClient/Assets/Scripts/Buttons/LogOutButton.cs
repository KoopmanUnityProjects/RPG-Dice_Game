using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogOutButton : MonoBehaviour
{
    public void LogOutButtonClicked()
    {
        GameManager.Instance.DisconnectFromServer();
    }

}
