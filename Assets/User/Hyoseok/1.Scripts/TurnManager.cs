using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager instance;
    private PhotonView pv;
    public List<Player> playersInRoom = new List<Player>();
    public TextMeshProUGUI currentturnText;
    public int currentPlayerIndex = 0;
    public int currentTurnRound = 0;
    public const int MAX_ROUNDS = 12;

    public GameObject turnAlarm;
    public TextMeshProUGUI UsernameText;
    public float popupDuration = 15f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        currentturnText.text = currentTurnRound.ToString() + "/12";
        StartCoroutine(WaitAndInitPlayers());
    }

    private IEnumerator WaitAndInitPlayers()
    {
        while (GameSceneManager.Instance == null || GameSceneManager.Instance.scoreboardEntries.Count == 0)
        {
            yield return null;
        }

        playersInRoom = GameSceneManager.Instance.GetSortedPlayers();

        if (playersInRoom == null || playersInRoom.Count == 0)
        {
            Debug.LogError("플레이어 리스트를 가져오지 못했습니다.");
            yield break;
        }

        Debug.Log("TurnManager: GameSceneManager로부터 플레이어 리스트 수신 완료");

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateTurn(0, 0);  // 첫 턴 시작
            GameSceneManager.Instance.BroadcastTurn(0, 0);
        }
    }

    public bool IsMyTurn()
    {
        if (playersInRoom == null || playersInRoom.Count == 0)
        {
            Debug.LogWarning("아직 플레이어 목록이 준비되지 않았습니다.");
            return false;
        }

        if (currentPlayerIndex < 0 || currentPlayerIndex >= playersInRoom.Count)
        {
            Debug.LogWarning($"currentPlayerIndex({currentPlayerIndex})가 플레이어 수({playersInRoom.Count})를 벗어났습니다.");
            return false;
        }

        var current = playersInRoom[currentPlayerIndex];
        Debug.Log($"내 이름: {PhotonNetwork.LocalPlayer.NickName}, 현재 턴 플레이어: {current.NickName}");

        return PhotonNetwork.LocalPlayer == current;
    }

    public Player GetCurrentPlayer()
    {
        return playersInRoom[currentPlayerIndex];
    }

    public void EndMyTurn()
    {
        Debug.Log(" EndMyTurn() 호출됨");
        Debug.Log($" GameSceneManager.Instance == null ? {GameSceneManager.Instance == null}");
        if (!PhotonNetwork.IsMasterClient) return;

        int nextIndex = currentPlayerIndex + 1;
        int nextRound = currentTurnRound;

        if (nextIndex >= playersInRoom.Count)
        {
            nextIndex = 0;
            nextRound++;

            if (nextRound >= MAX_ROUNDS)
            {
                Debug.Log("게임 종료!");
                return;
            }

            Debug.Log($"다음 라운드 시작: {nextRound + 1}턴째");
        }

        // 본인만 즉시 적용
        UpdateTurn(nextIndex, nextRound);
        // 전체에 동기화 (RPC는 GameSceneManager에서 관리)
        GameSceneManager.Instance.BroadcastTurn(nextIndex, nextRound);
    }

    public void UpdateTurn(int playerIndex, int round)
    {
        Debug.Log("UpdateTurn() 호출됨");

        currentPlayerIndex = playerIndex;
        currentTurnRound = round;

        if (DiceManager.Instance != null)
            DiceManager.Instance.rollsLeft = 3;
        else
            Debug.LogWarning("DiceManager.Instance가 null입니다.");

        currentturnText.text = $"{currentTurnRound + 1} / {MAX_ROUNDS}";

        foreach (var entry in FindObjectsByType<ScoreboardEntry>(FindObjectsSortMode.None))
        {
            if (entry != null)
                entry.ClearHighlight();
        }

        // 알림은 RPC로 실행
        if (PhotonNetwork.IsMasterClient && PhotonView.Get(this) != null)
        {
            string playerName = GetCurrentPlayer()?.NickName ?? "???";
            PhotonView.Get(this).RPC(nameof(ShowTurnPopupRPC), RpcTarget.All, playerName);
        }

        Debug.Log($"현재 턴: {round + 1}턴 - 플레이어: {GetCurrentPlayer()?.NickName}");
    }


    [PunRPC]
    private void ShowTurnPopupRPC(string playerName)
    {
        if (turnAlarm != null && UsernameText != null)
        {
            UsernameText.text = $"{playerName}'S";
            turnAlarm.SetActive(true);
            CancelInvoke(nameof(HideTurnPopup));
            Invoke(nameof(HideTurnPopup), popupDuration);
        }
    }

    private void HideTurnPopup()
    {
        if (turnAlarm != null)
        {
            turnAlarm.SetActive(false);
        }
    }
}
