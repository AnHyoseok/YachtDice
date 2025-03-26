using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;
public class GameSceneManager : MonoBehaviourPunCallbacks
{
    public static GameSceneManager Instance;

    public Transform scoreboardPanel;
    public GameObject scoreboardPrefab;

    public Dictionary<int, ScoreboardEntry> scoreboardEntries = new Dictionary<int, ScoreboardEntry>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            InitializeScoreboard();
        }
        else
        {
            Debug.LogError("PhotonNetwork.InRoom이 false입니다. 방에 입장한 상태인지 확인하세요.");
        }
    }

    private void Update()
    {
        DiceManager.Instance.ShowPreviewScore();
    }

    void InitializeScoreboard()
    {
        scoreboardEntries.Clear();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AddPlayerToScoreboard(player);
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AIPlayers"))
        {
            string[] aiPlayers = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            foreach (string aiName in aiPlayers)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(aiName))
                {
                    AddAIToScoreboard(aiName, (Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[aiName]);
                }
            }
        }
    }

    void AddPlayerToScoreboard(Player player)
    {
        ScoreboardEntry entry = CreateScoreboardEntry();
        entry.SetPlayerData(player);
        scoreboardEntries[player.ActorNumber] = entry;
    }

    void AddAIToScoreboard(string aiName, Hashtable properties)
    {
        ScoreboardEntry entry = CreateScoreboardEntry();
        entry.SetAIData(aiName, properties);
        scoreboardEntries[aiName.GetHashCode()] = entry;
    }

    private ScoreboardEntry CreateScoreboardEntry()
    {
        GameObject scoreboardEntry = Instantiate(scoreboardPrefab, scoreboardPanel);
        return scoreboardEntry.GetComponent<ScoreboardEntry>();
    }

    public void UpdatePlayerScore(Player player, int[] newScores)
    {
        if (!scoreboardEntries.ContainsKey(player.ActorNumber)) return;

        Hashtable playerProperties = new Hashtable { { "Score", newScores } };
        player.SetCustomProperties(playerProperties);
        scoreboardEntries[player.ActorNumber].UpdateScoreData(playerProperties);
    }

    public void UpdateAIScore(string aiName, int[] newScores)
    {
        if (!scoreboardEntries.ContainsKey(aiName.GetHashCode())) return;

        Hashtable aiProperties = new Hashtable { { "Score", newScores } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { aiName, aiProperties } });

        scoreboardEntries[aiName.GetHashCode()].UpdateAIScore(aiProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("PreviewScore"))
        {
            Dictionary<string, int> previewScores = (Dictionary<string, int>)changedProps["PreviewScore"];
            Debug.Log($" {targetPlayer.NickName}의 점수 업데이트 감지!");

            if (GameSceneManager.Instance != null && GameSceneManager.Instance.scoreboardEntries.ContainsKey(targetPlayer.ActorNumber))
            {
                GameSceneManager.Instance.scoreboardEntries[targetPlayer.ActorNumber].ShowPreview(previewScores);
            }
        }
    }



    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        foreach (var key in propertiesThatChanged.Keys)
        {
            if (key is string aiName && propertiesThatChanged[key] is Hashtable aiProperties)
            {
                if (scoreboardEntries.ContainsKey(aiName.GetHashCode()))
                {
                    scoreboardEntries[aiName.GetHashCode()].UpdateAIScore(aiProperties);
                }
            }
        }
    }

    public List<Player> GetSortedPlayers()
    {
        Player[] players = PhotonNetwork.PlayerList;
        System.Array.Sort(players, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));
        return new List<Player>(players);
    }

    [PunRPC]
    public void RPC_UpdateTurn(int playerIndex, int round)
    {
        Debug.Log("RPC_UpdateTurn 호출됨 ");
        TurnManager.instance.UpdateTurn(playerIndex, round);
    }

    public void BroadcastTurn(int playerIndex, int round)
    {
        Debug.Log("BroadcastTurn 호출됨");
        photonView.RPC("RPC_UpdateTurn", RpcTarget.All, playerIndex, round);
    }
}
