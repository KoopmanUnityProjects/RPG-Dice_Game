using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public NetworkManager networkManager;
    public SaveManager saveManager;
    public ShipCaptainCrewManager sccManager;
    public RollManager rollManager;
    public ExperienceManager experienceManager;
    public GmManager gmManager;

    private Dictionary<ushort, Player> playerList = new Dictionary<ushort, Player>();

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

    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        Application.runInBackground = true;
        Instance = this;
    }

    private void Update()
    {

    }

  

    public void InstantiatePlayer(ushort clientID, string playerName)
    {
        Player player = Instantiate(PlayerPrefab).GetComponent<Player>();
        player.gameObject.name = playerName;
        player.ClientId = clientID;
        AddPlayerToList(player);
    }

    public void CreateCharacter(ushort clientID, string playerName, int number)
    {
        InstantiatePlayer(clientID, playerName);
        if (playerList.TryGetValue(clientID, out Player player))
        {
            player.CreateCharacter(playerName, number);
            saveManager.Save(player);
            networkManager.sendManager.RegisterClientsCharacter(clientID, playerName, number);
            LogPlayerIn(clientID);
        }
    }

    public void LogIntoCharacter(ushort clientID, string playerName, int number, bool claimCharacter)
    {
        InstantiatePlayer(clientID, playerName);
        if (playerList.TryGetValue(clientID, out Player player))
        {
            player.CreateCharacter(playerName, number);

            if (saveManager.LoadCharacter(player))
            {
                LogPlayerIn(clientID);
                if (claimCharacter)
                {
                    networkManager.sendManager.RegisterClientsCharacter(clientID, playerName, number);
                }
            }
            else
            {
                networkManager.sendManager.SendErrorToClient(clientID, "Character Not Found");
                networkManager.Server.DisconnectClient(clientID);
            }
        }
    }

    public void LogPlayerIn(ushort clientID)
    {
        networkManager.sendManager.LogPlayerIn(clientID);
    }

    public void RemovePlayerFromList(Player player)
    {
        playerList.Remove(player.ClientId);
        Destroy(player.gameObject);
    }

    public void AddPlayerToList(Player player)
    {
        playerList.Add(player.ClientId, player);
    }

    public void PlayerLeft(ushort Id)
    {
        if (playerList.TryGetValue(Id, out Player player))
        {
            RemovePlayerFromList(player);
        }
    }

    public Player GetPlayer(ushort id)
    {
        if (playerList.TryGetValue(id, out Player player))
        {
            return player;
        }

        Debug.Log($"Player not found - id:{id}");
        return null;
    }

    public List<Player> GetAllPlayersOnline()
    {
        List<Player> players = new List<Player>();
        foreach (KeyValuePair<ushort, Player> player in playerList)
        {
            players.Add(player.Value);
        }
        return players;
    }

    public void SendPlayerToSSCBetPage(ushort id)
    {
        Player player = GetPlayer(id);
        sccManager.PlayerJoinsBetScreen(player);
    }

    public void PlayerPlacedSccbet(ushort id, int bet)
    {
        Player player = GetPlayer(id);
        sccManager.PlayerPlacedBet(player, bet);
    }

    public void LoadPlayerStats(ushort id)
    {
        Player player = GetPlayer(id);
        player.SendPlayerSccStats();
    }


    // THIS IS JUST FOR TESTING
    public ushort GetfirstPlayer()
    {
        return playerList.FirstOrDefault().Key;
    }
}
