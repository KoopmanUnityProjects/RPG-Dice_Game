using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public SendManager sendManager;
    [SerializeField] private ReceiveManager receiveManager;
    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    public Server Server { get; private set; }

    private void Start()
    {
        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server();
        Server.Start(port, maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
        sendManager.SetServer(Server);
    }

    private void FixedUpdate()
    {
        Server.Tick();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        GameManager.Instance.PlayerLeft(e.Id);
    }
}
