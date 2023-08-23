using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public List<Canvas> canvasList;
    public Canvas FirstCanvasToLoad;

    public enum canvases
    {
        StartScreenCanvas = 0,
        CreateCharacterCanvas,
        ClaimCharacterCanvas,
        MessageCanvas,
        GameSelectionCanvas,
        CharacterSheetCanvas,
        GameMasterCanvas,
        ChatBoxCanvas,
        DiceCanvas,
        ShipCaptainCrewBetCanvas,
        ShipCaptainCrewGameCanvas,
        ShipCaptainCrewStatisticsCanvas,
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Canvas canvas in canvasList)
        {
            canvas.enabled = false;
        }

        FirstCanvasToLoad.enabled = true;
    }

    public void TurnOnCanvas(canvases newCanvas)
    {
        foreach (Canvas canvas in canvasList)
        {
            canvas.enabled = false;
        }

        canvasList[(int)newCanvas].enabled = true;
    }

    public void TurnOnAdditionalCanvas(canvases newCanvas)
    {
        canvasList[(int)newCanvas].enabled = true;
    }

    public void TurnOffAdditionalCanvas(canvases newCanvas)
    {
        canvasList[(int)newCanvas].enabled = false;
    }

    public bool IsCanvasEnabled(canvases canvas)
    {
        return canvasList[(int)canvas].enabled;
    }
}
