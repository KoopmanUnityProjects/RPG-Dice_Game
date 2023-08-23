using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SccUiManager : MonoBehaviour
{
    [SerializeField]
    List<TMP_Text> playerNames;

    [SerializeField]
    Image[] houseImages;

    [SerializeField]
    Image[] player1Images;

    [SerializeField]
    Image[] player2Images;

    [SerializeField]
    Image[] player3Images;

    [SerializeField]
    Image[] player4Images;

    [SerializeField]
    Image[] player5Images;

    List<Image[]> playersImages;

    Color currentTurnColor = Color.yellow;
    Color waitingColor = Color.black;

    private void Start()
    {
        playersImages = new List<Image[]>
        {
            houseImages,
            player1Images,
            player2Images,
            player3Images,
            player4Images,
            player5Images
        };
    }

    public void SetPlayerProgress(int playerNumber, int progress)
    {
        if (playerNumber >= 0 && playerNumber < 6 && progress > 0 && progress < 6)
        {
            playersImages[playerNumber][progress - 1].enabled = true;
        }
    }

    public void ClearAllProgress()
    {
        foreach (Image[] imageGroup in playersImages)
        {
            foreach (Image image in imageGroup)
            {
                image.enabled = false;
            }
        }
    }

    public void SetupForNewGame(List<string> players)
    {
        ClearAllProgress();

        for (int i = 0; i < 5; i++)
        {
            if (i < players.Count)
            {
                playerNames[i + 1].text = players[i];
                playerNames[i + 1].color = waitingColor;
            }
            else
            {
                playerNames[i + 1].text = "";
            }
        }
    }

    public void DisplayPlayersTurn(int playersTurn)
    {
        for (int i = 0; i < playerNames.Count; i++)
        {
            if (i == playersTurn)
            {
                playerNames[i].color = currentTurnColor;
            }
            else
            {
                playerNames[i].color = waitingColor;
            }
        }
    }
}
