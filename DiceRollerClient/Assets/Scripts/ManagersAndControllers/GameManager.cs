using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public NetworkManager networkManager;
    public ShipCaptainCrewManager sccManager;
    public UIManager UI;
    public StatisticsManager statManager;
    public GmOptionsManager gmOptionsManager;

    public Player currentPlayer;

    public static GameManager Instance
    {
        get => instance;
        private set
        {
            if (instance == null)
            {
                instance = value;
            }
            else if (instance != value)
            {
                Debug.Log($"{nameof(GameManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Application.runInBackground = true;
        Instance = this;
    }

    public void Connect()
    {
        networkManager.Connect();
    }

    public void DisconnectFromServer()
    {
        networkManager.Client.Disconnect();
        UI.canvasManager.TurnOnAdditionalCanvas(CanvasManager.canvases.StartScreenCanvas);
    }

    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }

    private void OnApplicationQuit()
    {
        networkManager.Client.Disconnect();
    }

    public void SetConnectionStatusAndConnect(NetworkManager.ConnectionState state, string name, int number)
    {
        networkManager.SetConnectionState(state);
        currentPlayer.SetUsername(name);
        currentPlayer.SetUserNumber(number);
        Connect();
    }

    public void DisplayMessage(string message)
    {
        UI.messageManager.AddMessage(message);
    }

    public void SaveCharacterToRegistery(string characterName, int characterNumber)
    {
        UI.clientLogInManager.SaveCharacterToLogInRegistery(characterName, characterNumber);
    }
}
