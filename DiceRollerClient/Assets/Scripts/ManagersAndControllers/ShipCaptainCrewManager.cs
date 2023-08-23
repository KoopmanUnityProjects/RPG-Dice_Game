using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipCaptainCrewManager : MonoBehaviour
{
    [SerializeField]
    GameObject awaitingBetGroup;

    [SerializeField]
    GameObject gameInProgressGroup;

    [SerializeField]
    TMP_Text countdownTimer;

    [SerializeField]
    TMP_Text playerMoneyText;

    [SerializeField]
    TMP_InputField goldInput;

    [SerializeField]
    TMP_InputField silverInput;

    [SerializeField]
    TMP_InputField copperInput;

    [SerializeField]
    Button PlaceBetButton;

    [SerializeField]
    GameObject PlayingListParent;

    [SerializeField]
    GameObject PlayerPlayingPrefab;

    // external connections
    CanvasManager.canvases shipCaptainCrewBetCanvas;
    CanvasManager.canvases diceCanvas;
    CanvasManager.canvases shipCaptainCrewGameCanvas;
    SccUiManager sccUI;
    CanvasManager canvasManager;
    UIDieManager uiDieManager;
    SendManager sendManager;

    List <GameObject> playersPlayingList = new List<GameObject>();

    bool betPlaced = false;
    bool myTurn = false;

    private void Start()
    {
        canvasManager = GameManager.Instance.UI.canvasManager;
        sccUI = GameManager.Instance.UI.sccUiManager;
        uiDieManager = GameManager.Instance.UI.dieManager;
        sendManager = GameManager.Instance.networkManager.sendManager;

        shipCaptainCrewBetCanvas = CanvasManager.canvases.ShipCaptainCrewBetCanvas;
        diceCanvas = CanvasManager.canvases.DiceCanvas;
        shipCaptainCrewGameCanvas = CanvasManager.canvases.ShipCaptainCrewGameCanvas;
    }

    public void JoinGameScreen(List<string> playerNames)
    {
        canvasManager.TurnOnCanvas(shipCaptainCrewGameCanvas);
        canvasManager.TurnOnAdditionalCanvas(diceCanvas);
        uiDieManager.ClearDice();
        uiDieManager.HideActionButton();
        sccUI.SetupForNewGame(playerNames);
    }

    public void JoinBetScreen(string timer, bool gameInProgress, List<Tuple<string, int>> players)
    {
        canvasManager.TurnOnCanvas(shipCaptainCrewBetCanvas);
        betPlaced = false;
        ClearPlayingList();
        UpdatePageText(timer, players);

        if (gameInProgress)
        {
            SwitchToGameInProgressGroup();
        }
        else
        {
            SwitchToAwaitingBetGroup();
        }
    }

    void UpdatePageText(string timer, List<Tuple<string, int>> players)
    {
        UpdateMoneyText(GameManager.Instance.currentPlayer.character.money);
        UpdateSccTimer(timer);

        foreach (Tuple<string, int> player in players)
        {
            AddPlayerObjectToListView(player.Item1, player.Item2);
        }
    }

    public void UpdateSccTimer(string timer)
    {
        countdownTimer.text = timer;
    }

    public void AddPlayerObjectToListView(string playerName, int playerBet)
    {
        string betAsString = ConvertMoneyToString(playerBet, false);

        GameObject playerPlayingObject = Instantiate(PlayerPlayingPrefab);
        SCCPlayerObject playerPlayingScript = playerPlayingObject.GetComponent<SCCPlayerObject>();
        playerPlayingScript.SetPlayerNameAndBet(playerName, betAsString);
        playerPlayingObject.transform.SetParent(PlayingListParent.transform, false);
        playersPlayingList.Add(playerPlayingObject);
    }

    void UpdateMoneyText(int money)
    {
        string playerMoney = ConvertMoneyToString(money, true);

        playerMoneyText.text = playerMoney;
    }

    string ConvertMoneyToString(int money, bool includeZeros)
    {
        string playerMoneyString = "";
        int copper = money;
        int gold = copper / 10000;
        copper = copper % 10000;
        int silver = copper / 100;
        copper = copper % 100;

        if (includeZeros || gold > 0)
        {
            playerMoneyString = playerMoneyString + gold + "G ";
        }

        if (includeZeros || silver > 0)
        {
            playerMoneyString = playerMoneyString + silver + "S ";
        }

        if (includeZeros || copper > 0)
        {
            playerMoneyString = playerMoneyString + copper + "C";
        }

        return playerMoneyString;
    }

    void SwitchToAwaitingBetGroup()
    {
        awaitingBetGroup.SetActive(true);
        gameInProgressGroup.SetActive(false);
        copperInput.text = "";
        silverInput.text = "";
        goldInput.text = "";
    }

    public void SwitchToGameInProgressGroup()
    {
        awaitingBetGroup.SetActive(false);
        gameInProgressGroup.SetActive(true);
        countdownTimer.text = "--";
        copperInput.text = "";
        silverInput.text = "";
        goldInput.text = "";
    }

    public void UpdateCountdownTimer(int timeLeft)
    {
        countdownTimer.text = timeLeft.ToString();
    }

    public void PlaceBet()
    {
        if (betPlaced)
        {
            return;
        }

        Debug.Log("Bet Button Clicked");
        int.TryParse(goldInput.text, out int gold);
        int.TryParse(silverInput.text, out int silver);
        int.TryParse(copperInput.text, out int copper);

        // validate each field
        if (gold < 0 || silver < 0 || copper < 0)
        {
            GameManager.Instance.DisplayMessage("All currency fields must be positive.");
            copperInput.text = "";
            silverInput.text = "";
            goldInput.text = "";
            return;
        }

        // validate total
        int totalcopper = copper + (silver * 100) + (gold * 10000);

        if (totalcopper == 0)
        {
            GameManager.Instance.DisplayMessage("bet cannot be lower than 1 copper.");
            return;
        }

        if (totalcopper > GameManager.Instance.currentPlayer.character.money)
        {
            GameManager.Instance.DisplayMessage("Bet cannot be more money than you have.");
            copperInput.text = "";
            silverInput.text = "";
            goldInput.text = "";
            return;
        }

        betPlaced = true;

        sendManager.PlaceSccBet(totalcopper);
    }

    public void UpdatePlayersTurn(bool isMyTurn, int currentPlayersIndex)
    {
        sccUI.DisplayPlayersTurn(currentPlayersIndex);
        myTurn = isMyTurn;
        if (myTurn)
        {
            uiDieManager.DisplayActionButton("Roll Dice");
            uiDieManager.OnActionButtonClick -= PlayerClickedActionButton;
            uiDieManager.OnActionButtonClick += PlayerClickedActionButton;
            uiDieManager.UIDieClick -= PlayerClickedUIDie;
            uiDieManager.UIDieClick += PlayerClickedUIDie;
        }
    }

    public void PlayerClickedActionButton()
    {
        if (myTurn)
        {
            sendManager.SccActionButtonClicked();
        }
    }

    public void PlayerClickedUIDie(int index)
    {
        if (myTurn)
        {
            sendManager.ClickedSccDice(index);
        }
    }

    public void SpectateButtonClicked()
    {
        Debug.Log("Spectate Button Clicked");
        sendManager.SccSpectateButtonClicked();
    }

    public void ClearPlayingList()
    {
        foreach (GameObject playerPlaying in playersPlayingList)
        {
            Destroy(playerPlaying);
        }
        playersPlayingList.Clear();
    }
}
