using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class PlayerPrefab : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image playerProfile;
    public Image readyIcon;
    public Image teamColor;
    public TextMeshProUGUI teamName;

    public void Setup(Player player)
    {
        if (playerNameText != null)
        {
            playerNameText.text = player.NickName;
        }

        // 팀 색상 및 팀명 변경
        if (player.CustomProperties.ContainsKey("Team"))
        {
            string team = (string)player.CustomProperties["Team"];
            teamColor.color = (team == "Red") ? Color.red : Color.blue;  //  팀 색상 변경
            teamName.text = team;  //  팀명 표시
        }

        // 준비 상태 확인
        if (player.CustomProperties.ContainsKey("Ready"))
        {
            bool isReady = (bool)player.CustomProperties["Ready"];
            readyIcon.gameObject.SetActive(isReady);  //  Ready 아이콘 활성화/비활성화
        }
        else
        {
            readyIcon.gameObject.SetActive(false);
        }
    }
}
