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

        // �� ���� �� ���� ����
        if (player.CustomProperties.ContainsKey("Team"))
        {
            string team = (string)player.CustomProperties["Team"];
            teamColor.color = (team == "Red") ? Color.red : Color.blue;  //  �� ���� ����
            teamName.text = team;  //  ���� ǥ��
        }

        // �غ� ���� Ȯ��
        if (player.CustomProperties.ContainsKey("Ready"))
        {
            bool isReady = (bool)player.CustomProperties["Ready"];
            readyIcon.gameObject.SetActive(isReady);  //  Ready ������ Ȱ��ȭ/��Ȱ��ȭ
        }
        else
        {
            readyIcon.gameObject.SetActive(false);
        }
    }
}
