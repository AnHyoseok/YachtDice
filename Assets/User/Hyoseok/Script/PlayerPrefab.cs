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

        // �� ���� ����
        if (player.CustomProperties.ContainsKey("Team"))
        {
            string team = (string)player.CustomProperties["Team"];
            background.color = (team == "Red") ? Color.red : Color.blue;
        }

        // �غ� ���� Ȯ��
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
