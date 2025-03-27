using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System.Collections;
using System.Linq;

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

    void Update()
    {
        // 매 프레임 미리보기 계산하지 않도록 조건 검사
        if (DiceManager.Instance != null && DiceManager.Instance.isDiceArray)
        {
            DiceManager.Instance.ShowPreviewScore();
        }
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
                if (PhotonNetwork.CurrentRoom.CustomProperties[aiName] is System.Collections.Hashtable aiRawProps)
                {
                    ExitGames.Client.Photon.Hashtable aiProps = new ExitGames.Client.Photon.Hashtable();
                    foreach (DictionaryEntry entry in aiRawProps)
                    {
                        aiProps[entry.Key] = entry.Value;
                    }
                    AddAIToScoreboard(aiName, aiProps);
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

    void AddAIToScoreboard(string aiName, ExitGames.Client.Photon.Hashtable properties)
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

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "Score", newScores } };
        player.SetCustomProperties(playerProperties);
        scoreboardEntries[player.ActorNumber].UpdateScoreData(playerProperties);
    }

    public void UpdateAIScore(string aiName, int[] newScores)
    {
        if (!scoreboardEntries.ContainsKey(aiName.GetHashCode())) return;

        ExitGames.Client.Photon.Hashtable aiProperties = new ExitGames.Client.Photon.Hashtable { { "Score", newScores } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { aiName, aiProperties } });

        scoreboardEntries[aiName.GetHashCode()].UpdateAIScore(aiProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log($"[변경 감지] {targetPlayer.NickName} → {string.Join(", ", changedProps.Keys.Cast<object>())}");
        if (changedProps.ContainsKey("PreviewScore") && changedProps["PreviewScore"] is ExitGames.Client.Photon.Hashtable previewTable)
        {
            Dictionary<string, int> previewScores = new Dictionary<string, int>();

            foreach (DictionaryEntry entry in previewTable)
            {
                string key = entry.Key as string;
                if (entry.Value is int intVal)
                {
                    previewScores[key] = intVal;
                }
                else if (entry.Value is long longVal) // Photon sometimes uses long
                {
                    previewScores[key] = (int)longVal;
                }
                else
                {
                    Debug.LogWarning($"PreviewScore 변환 실패: {entry.Key} = {entry.Value} ({entry.Value?.GetType()})");
                }
            }

            if (scoreboardEntries.ContainsKey(targetPlayer.ActorNumber))
            {
                scoreboardEntries[targetPlayer.ActorNumber].ShowPreview(previewScores);
                Debug.Log($"[Preview 수신] {targetPlayer.NickName}: {string.Join(", ", previewScores.Select(kv => $"{kv.Key}:{kv.Value}"))}");
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        foreach (var key in propertiesThatChanged.Keys)
        {
            if (key is string aiName && propertiesThatChanged[key] is ExitGames.Client.Photon.Hashtable aiProperties)
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
        if (TurnManager.instance != null)
        {
            TurnManager.instance.UpdateTurn(playerIndex, round);
        }
        else
        {
            Debug.LogError("TurnManager.instance가 null입니다.");
        }
    }

    public void BroadcastTurn(int playerIndex, int round)
    {
        Debug.Log("BroadcastTurn 호출됨");

        if (photonView != null)
        {

            photonView.RPC("RPC_UpdateTurn", RpcTarget.All, playerIndex, round);
        }
        else
        {
            Debug.LogError("photonView가 null입니다. BroadcastTurn 실패");
        }
    }
}
