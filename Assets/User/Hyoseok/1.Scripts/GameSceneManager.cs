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
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log($" Scoreboard 생성 중: {player.NickName} (ID: {player.ActorNumber}, 방장 여부: {player.IsMasterClient})");

            GameObject scoreboardEntry = Instantiate(scoreboardPrefab, scoreboardPanel);
            ScoreboardEntry entryScript = scoreboardEntry.GetComponent<ScoreboardEntry>();

            if (entryScript != null)
            {
                entryScript.SetPlayerData(player);
                Debug.Log($" ScoreboardEntry 생성 완료: {player.NickName}");
            }
            else
            {
                Debug.LogError(" ScoreboardEntry 스크립트를 찾을 수 없습니다.");
            }
        }
    }
}
