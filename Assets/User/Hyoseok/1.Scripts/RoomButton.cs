using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class RoomButton : MonoBehaviour
{
    public TextMeshProUGUI roomNumberText;
    public TextMeshProUGUI hostNameText;
    public TextMeshProUGUI gameModeText;
    private string roomName;

    public void Setup(RoomInfo room)
    {
        roomName = room.Name;

        //  예외 방지: "_" 포함 여부 확인 후 방 번호 추출
        string[] nameParts = roomName.Split('_');
        string roomNumber = (nameParts.Length > 1) ? nameParts[1] : "Unknown";

        //  예외 방지: 방장 이름 안전하게 가져오기
        string hostName = room.CustomProperties.TryGetValue("HostName", out object host) ? (string)host : "Unknown";

        //  예외 방지: 게임 모드 안전하게 가져오기
        string gameMode = room.CustomProperties.TryGetValue("GameMode", out object mode) ? $" {mode}v{mode}" : " 1v1";

        // UI 업데이트
        roomNumberText.text = $"Room: {roomNumber}";
        hostNameText.text = $"{hostName} 's room";
        gameModeText.text = gameMode;
    }

    public void OnClick()
    {
        Debug.Log($" RoomButton 클릭됨 - 방 입장 시도: {roomName}");
        PhotonNetwork.JoinRoom(roomName);
    }

}
