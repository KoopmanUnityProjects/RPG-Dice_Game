using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    CanvasManager canvasManager;
    SendManager sendManager;

    private void Awake()
    {
        SwipeManager.OnSwipe += SwipeDetector_OnSwipe;
    }

    private void Start()
    {
        canvasManager = GameManager.Instance.UI.canvasManager;
        sendManager = GameManager.Instance.networkManager.sendManager;
    }

    private void SwipeDetector_OnSwipe(SwipeData data)
    {
        if (GameManager.Instance.networkManager.Client.IsConnected)
        {
            if (data.Direction == SwipeDirection.Left)
            {
                LeftSwipeDetected();
            }
            else if (data.Direction == SwipeDirection.Right)
            {
                RightSwipeDetected();
            }
            else if (data.Direction == SwipeDirection.Up)
            {
                UpSwipeDetected();
            }
            else if (data.Direction == SwipeDirection.Down)
            {
                DownSwipeDetected();
            }
        }
    }    

    public void UpSwipeDetected()
    {

        if (canvasManager.IsCanvasEnabled(CanvasManager.canvases.ChatBoxCanvas))
        {
            canvasManager.TurnOffAdditionalCanvas(CanvasManager.canvases.ChatBoxCanvas);
        }
    }

    public void DownSwipeDetected()
    {
        if (!canvasManager.IsCanvasEnabled(CanvasManager.canvases.ChatBoxCanvas))
        {
            canvasManager.TurnOnAdditionalCanvas(CanvasManager.canvases.ChatBoxCanvas);
        }
    }

    public void LeftSwipeDetected()
    {
        if (canvasManager.IsCanvasEnabled(CanvasManager.canvases.GameMasterCanvas))
        {
            canvasManager.TurnOffAdditionalCanvas(CanvasManager.canvases.GameMasterCanvas);
        }
        else if (!canvasManager.IsCanvasEnabled(CanvasManager.canvases.CharacterSheetCanvas))
        {
            canvasManager.TurnOnAdditionalCanvas(CanvasManager.canvases.CharacterSheetCanvas);
        }
    }

    public void RightSwipeDetected()
    {
        if (canvasManager.IsCanvasEnabled(CanvasManager.canvases.CharacterSheetCanvas))
        {
            canvasManager.TurnOffAdditionalCanvas(CanvasManager.canvases.CharacterSheetCanvas);
        }
        else if (!canvasManager.IsCanvasEnabled(CanvasManager.canvases.GameMasterCanvas))
        {
            sendManager.LoadPlayersGmScreen();
        }
    }
}
