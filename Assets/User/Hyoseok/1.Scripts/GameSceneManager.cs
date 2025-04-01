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

    public SelectDice selectDice;
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

            bool hasTurnIndex = PhotonNetwork.PlayerList.All(p => p.CustomProperties.ContainsKey("TurnIndex"));
            if (!hasTurnIndex)
            {
                AssignTurnIndices();
            }

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AIPlayers"))
            {
                OnRoomPropertiesUpdate(PhotonNetwork.CurrentRoom.CustomProperties);
            } 
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
            
            if (IsAIPlayer(player)) continue;

            AddPlayerToScoreboard(player);
        }

        // 2. AI는 별도로 등록
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
    private bool IsAIPlayer(Player player)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("AIPlayers", out object aiRaw) && aiRaw is string[] aiNames)
        {
            return aiNames.Contains(player.NickName);
        }
        return false;
    }

    void AddPlayerToScoreboard(Player player)
    {
        ScoreboardEntry entry = CreateScoreboardEntry();
        entry.SetPlayerData(player);
        scoreboardEntries[player.ActorNumber] = entry;
    }

    void AddAIToScoreboard(string aiName, ExitGames.Client.Photon.Hashtable properties)
    {
        Debug.Log($"[AI 등록] {aiName} → 해시: {aiName.GetHashCode()}");
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
        if (propertiesThatChanged.ContainsKey("AIPlayers"))
        {
            string[] aiNames = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            foreach (string aiName in aiNames)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties[aiName] is ExitGames.Client.Photon.Hashtable aiProps)
                {
                    if (!scoreboardEntries.ContainsKey(aiName.GetHashCode()))
                    {
                        AddAIToScoreboard(aiName, aiProps);
                    }
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
    public void AssignTurnIndices()
    {
        var sortedPlayers = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToList();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            Player p = sortedPlayers[i];
            p.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "TurnIndex", i } });
        }
    }
    public void BroadcastTurn(int playerIndex, int round)
    {
        Debug.Log("BroadcastTurn 호출됨");

        if (photonView != null)
        {
            photonView.RPC("RPC_UpdateTurn", RpcTarget.All, playerIndex, round);
            foreach(Dice dice in DiceManager.Instance.newdicelist)
            {
                if(dice != null)
                {
                    selectDice.MoveDiceBetweenArrays(dice, DiceManager.Instance.newdicelist, DiceManager.Instance.dices);
                }
            }
            for(int i =0;i < DiceManager.Instance.dices.Length; i++)
            {
                if(DiceManager.Instance.dices[i] != null)
                {
                    PhotonNetwork.Destroy(DiceManager.Instance.dices[i].gameObject);
                    DiceManager.Instance.dices[i] = null;
                }
            }
            DiceManager.Instance.isDiceArray = false;
            selectDice.escbutton.gameObject.SetActive(DiceManager.Instance.isDiceArray);
            selectDice.OnTurnEnd();
        }
        else
        {
            Debug.LogError("photonView가 null입니다. BroadcastTurn 실패");
        }
    }
}
