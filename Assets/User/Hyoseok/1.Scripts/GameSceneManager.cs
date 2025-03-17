using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameSceneManager : MonoBehaviourPunCallbacks
{
    public Transform scoreboardPanel; //  점수판 UI 부모
    public GameObject scoreboardPrefab; //  점수판 프리팹

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log($" 현재 방의 총 플레이어 수: {PhotonNetwork.PlayerList.Length}");

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                Debug.Log($" 플레이어 확인 - 닉네임: {player.NickName}, ID: {player.ActorNumber}, 방장 여부: {player.IsMasterClient}");
            }

            InitializeScoreboard();
        }
        else
        {
            Debug.LogError(" PhotonNetwork.InRoom이 false입니다. 방에 입장한 상태인지 확인하세요.");
        }
    }

    void InitializeScoreboard()
    {
        // 네트워크 플레이어 추가
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AddPlayerToScoreboard(player.NickName, player);
        }

        // AI 플레이어 추가
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AIPlayers"))
        {
            string[] aiPlayers = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            foreach (string aiName in aiPlayers)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(aiName))
                {
                    ExitGames.Client.Photon.Hashtable aiProperties =
                        (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[aiName];

                    AddAIToScoreboard(aiName, aiProperties);
                }
            }
        }
    }
    // 플레이어 추가 함수 (실제 네트워크 유저)
    void AddPlayerToScoreboard(string name, Player player)
    {
        GameObject scoreboardEntry = Instantiate(scoreboardPrefab, scoreboardPanel);
        ScoreboardEntry entryScript = scoreboardEntry.GetComponent<ScoreboardEntry>();

        if (entryScript != null)
        {
            entryScript.SetPlayerData(player);
            Debug.Log($" ScoreboardEntry 생성 완료: {name}");
        }
    }
    // AI 추가 함수
    void AddAIToScoreboard(string aiName, ExitGames.Client.Photon.Hashtable properties)
    {
        GameObject scoreboardEntry = Instantiate(scoreboardPrefab, scoreboardPanel);
        ScoreboardEntry entryScript = scoreboardEntry.GetComponent<ScoreboardEntry>();

        if (entryScript != null)
        {
            entryScript.SetAIData(aiName, properties);
            Debug.Log($" AI ScoreboardEntry 생성 완료: {aiName}");
        }
    }
}
