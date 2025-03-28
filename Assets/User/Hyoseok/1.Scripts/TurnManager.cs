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
    public CupController cupController;
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

        cupController = FindAnyObjectByType<CupController>();
        currentturnText.text = currentTurnRound.ToString() + "/12";
        StartCoroutine(WaitAndInitPlayers());
    }

    private IEnumerator WaitAndInitPlayers()
    {
        while (GameSceneManager.Instance == null || PhotonNetwork.PlayerList.Length == 0)
        {
            yield return null;
        }

        //  TurnIndex가 모든 플레이어에게 할당될 때까지 대기
        while (!AllPlayersHaveTurnIndex())
        {
            Debug.Log("TurnIndex가 아직 안 들어온 플레이어가 있습니다. 대기 중...");
            yield return null;
        }

        playersInRoom = PhotonNetwork.PlayerList
            .OrderBy(p => (int)p.CustomProperties["TurnIndex"])
            .ToList();

        if (playersInRoom.Count == 0)
        {
            Debug.LogError("플레이어 리스트를 가져오지 못했습니다.");
            yield break;
        }

        Debug.Log("TurnManager: TurnIndex 기준으로 플레이어 정렬 완료");

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateTurn(0, 0);
            GameSceneManager.Instance.BroadcastTurn(0, 0);
        }
    }

    private bool AllPlayersHaveTurnIndex()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey("TurnIndex"))
                return false;
        }
        return true;
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
        {
            DiceManager.Instance.rollsLeft = 3;
            DiceManager.Instance.isDiceArray = false;
            DiceManager.Instance.isArrays = false;
        }
        else
            Debug.LogWarning("DiceManager.Instance가 null입니다.");

        currentturnText.text = $"{currentTurnRound + 1} / {MAX_ROUNDS}";

        foreach (var entry in FindObjectsByType<ScoreboardEntry>(FindObjectsSortMode.None))
        {
            if (entry != null)
                entry.ClearHighlight();
        }

        // 알림은 RPC로 실행
        if (PhotonNetwork.LocalPlayer == TurnManager.instance.GetCurrentPlayer())
        {
            Debug.Log($"{PhotonNetwork.LocalPlayer}=현재 유저 ");

            cupController.photonView.RequestOwnership();
            Debug.Log($"[요청] CupController 소유권 요청: {PhotonNetwork.LocalPlayer.NickName}");

            StartCoroutine(DelayedOwnershipCheck());
        }
        Debug.Log($"현재 턴: {round + 1}턴 - 플레이어: {GetCurrentPlayer()?.NickName}");
    }

    private IEnumerator DelayedOwnershipCheck()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"[체크] CupController의 PhotonView 소유권: IsMine = {cupController.photonView.IsMine}, Owner = {cupController.photonView.Owner.NickName}");
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
