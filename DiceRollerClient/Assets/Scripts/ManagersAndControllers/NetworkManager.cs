using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using UnityEngine;


public class NetworkManager : MonoBehaviour
{
    public SendManager sendManager;
    [SerializeField] private ReceiveManager receiveManager;
    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    public enum ConnectionState
    {
        Disconnected = 0,
        CreatingCharacter,
        LoggingCharacterIn,
        ClaimingCharacter,
        LoggedIn,
    }

    private ConnectionState connectionState;

    public Client Client { get; private set; }

    private void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.Disconnected += DidDisconnect;
        sendManager.SetClient(Client);
    }

    private void FixedUpdate()
    {
        Client.Tick();
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    private void DidConnect(object sender, EventArgs e)
    {
        // TODO: Not sure I like this being here, but not sure where I want it yet.
        Player player = GameManager.Instance.GetCurrentPlayer();
        string name = player.GetUserName();
        int number = player.GetUserNumber();

        switch (connectionState)
        {
            case ConnectionState.CreatingCharacter:
                sendManager.CreateCharacter(name, number);
                break;
            case ConnectionState.LoggingCharacterIn:
                sendManager.LogIntoCharacter(name, number);
                break;
            case ConnectionState.ClaimingCharacter:
                sendManager.ClaimCharacter(name, number);
                break;
        }
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        // TODO Bring up error message
        Debug.Log("failed to connect");
        connectionState = ConnectionState.Disconnected;
        GameManager.Instance.UI.canvasManager.TurnOnCanvas(CanvasManager.canvases.StartScreenCanvas);
        GameManager.Instance.DisplayMessage("Failed to connect to server.");
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        // TODO consider trying to see if disconnect was intentional, and throwing error screen if it wasn't.
        Debug.Log("Disconnected");
        connectionState = ConnectionState.Disconnected;
        GameManager.Instance.UI.canvasManager.TurnOnCanvas(CanvasManager.canvases.StartScreenCanvas);
        GameManager.Instance.DisplayMessage("Disconnected from server.");
    }

    public void SetConnectionState(ConnectionState state)
    {
        connectionState = state;
    }
}
