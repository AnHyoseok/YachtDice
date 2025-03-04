using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class PlayerPrefab : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image background;
    public Image readyIcon;

    public void Setup(Player player)
    {
        playerNameText.text = player.NickName;

        // 팀 색상 변경
        if (player.CustomProperties.ContainsKey("Team"))
        {
            string team = (string)player.CustomProperties["Team"];
            background.color = (team == "Red") ? Color.red : Color.blue;
        }

        // 준비 상태 확인
        if (player.CustomProperties.ContainsKey("Ready"))
        {
            bool isReady = (bool)player.CustomProperties["Ready"];
            readyIcon.gameObject.SetActive(isReady);
        }
        else
        {
            readyIcon.gameObject.SetActive(false);
        }
    }
}
