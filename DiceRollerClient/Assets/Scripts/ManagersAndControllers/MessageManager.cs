using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    [SerializeField]
    TMP_Text messageText;

    [SerializeField]
    TMP_Text buttonText;

    CanvasManager canvasManager;
    CanvasManager.canvases messageCanvas;

    private List<string> messageList = new List<string>();

    private void Start()
    {
        canvasManager = GameManager.Instance.UI.canvasManager;
        messageCanvas = CanvasManager.canvases.MessageCanvas; ;
    }

    public void AddMessage(string message)
    {
        if (!canvasManager.IsCanvasEnabled(messageCanvas))
        {
            PopUpMessage(message);
        }
        else
        {
            messageList.Add(message);
        }
    }

    public void OkButtonClicked()
    {
        if (messageList.Count > 0)
        {
            PopUpMessage(messageList[0]);
            messageList.RemoveAt(0);
        }
        else
        {
            canvasManager.TurnOffAdditionalCanvas(messageCanvas);
        }
    }

    private void PopUpMessage(string message)
    {

        messageText.text = message;
        buttonText.text = "OK";
        canvasManager.TurnOnAdditionalCanvas(messageCanvas);
    }
}
