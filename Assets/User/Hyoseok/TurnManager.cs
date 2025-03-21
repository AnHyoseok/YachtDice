using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager instance;

    public List<Player> playersInRoom = new List<Player>();
    public TextMeshProUGUI currentturnText;
    public int currentPlayerIndex = 0;
    public int currentTurnRound = 0;
    public const int MAX_ROUNDS = 12;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        currentturnText.text = currentTurnRound.ToString()+"/12";
    }
    public override void OnJoinedRoom()
    {
        playersInRoom.Clear();
        playersInRoom.AddRange(PhotonNetwork.PlayerList);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!playersInRoom.Contains(newPlayer))
            playersInRoom.Add(newPlayer);
    }

    public bool IsMyTurn()
    {
        return PhotonNetwork.LocalPlayer == playersInRoom[currentPlayerIndex];
    }

    public Player GetCurrentPlayer()
    {
        return playersInRoom[currentPlayerIndex];
    }

    public void EndMyTurn()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentPlayerIndex++;

        if (currentPlayerIndex >= playersInRoom.Count)
        {
            currentPlayerIndex = 0;
            currentTurnRound++;

            if (currentTurnRound >= MAX_ROUNDS)
            {
                Debug.Log(" 게임 종료!");
                // TODO: 게임 종료 처리 추가
                return;
            }

            Debug.Log($" 다음 라운드 시작: {currentTurnRound + 1}턴째");
        }

        photonView.RPC("UpdateTurn", RpcTarget.All, currentPlayerIndex, currentTurnRound);
    }

    [PunRPC]
    public void UpdateTurn(int playerIndex, int round)
    {
        currentPlayerIndex = playerIndex;
        currentTurnRound = round;
        DiceManager.Instance.rollsLeft = 3; //  턴 변경 시 주사위 굴림 횟수 초기화
        Debug.Log($"현재 턴: {round + 1}턴 - 플레이어: {GetCurrentPlayer().NickName}");
    }
}
