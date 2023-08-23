using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCToGameSelectionButton : MonoBehaviour
{
    public void ClickedOnGoBack()
    {
        // TODO also remove character from SCC stuff. 
        GameManager.Instance.UI.canvasManager.TurnOnCanvas(CanvasManager.canvases.GameSelectionCanvas);
    }
}
