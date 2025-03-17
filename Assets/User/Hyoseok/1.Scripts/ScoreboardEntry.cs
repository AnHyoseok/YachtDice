using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class ScoreboardEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText; //  닉네임 표시
    public Image profileImage; //  프로필 이미지
    public Image teamColor; //  팀 색상
    public Sprite[] profileSprites; //  프로필 이미지 배열

    public void SetPlayerData(Player player)
    {
        if (playerNameText != null)
        {
            bool isMaster = player.IsMasterClient; //  방장 여부 확인
            string displayName = isMaster ?   player.NickName+ "[M]" : player.NickName;
            playerNameText.text = displayName;
        }

        //  프로필 인덱스 예외 처리 추가
        int profileIndex = 0;
        if (player.CustomProperties.ContainsKey("ProfileImageIndex"))
        {
            profileIndex = (int)player.CustomProperties["ProfileImageIndex"];
        }

        if (profileSprites != null && profileSprites.Length > 0)
        {
            if (profileIndex >= profileSprites.Length)
            {
                Debug.LogWarning($" 프로필 인덱스 {profileIndex}가 범위를 초과했습니다. 기본 프로필(0번) 사용.");
                profileIndex = 0; //  범위 초과 시 기본값 사용
            }

            profileImage.sprite = profileSprites[profileIndex];
        }
        else
        {
            Debug.LogWarning(" profileSprites 배열이 비어 있습니다. 기본 이미지 사용.");
        }

        if (player.CustomProperties.ContainsKey("Team"))
        {
            string team = (string)player.CustomProperties["Team"];
            teamColor.color = (team == "Red") ? Color.red : Color.blue; //  팀 색상 적용
        }
    }
    public void SetAIData(string aiName, ExitGames.Client.Photon.Hashtable properties)
    {
        if (playerNameText != null)
        {
            playerNameText.text =  aiName+ "[AI]"; // AI 이름 표시
        }

        int profileIndex = properties.ContainsKey("ProfileIndex") ? (int)properties["ProfileIndex"] : 0;

        if (profileSprites != null && profileSprites.Length > 0)
        {
            if (profileIndex >= profileSprites.Length)
            {
                Debug.LogWarning($" AI 프로필 인덱스 {profileIndex}가 범위를 초과했습니다. 기본 프로필(0번) 사용.");
                profileIndex = 0;
            }

            profileImage.sprite = profileSprites[profileIndex];
        }
        else
        {
            Debug.LogWarning(" profileSprites 배열이 비어 있습니다. 기본 이미지 사용.");
        }

        if (properties.ContainsKey("Team"))
        {
            string team = (string)properties["Team"];
            teamColor.color = (team == "Red") ? Color.red : Color.blue;
        }
    }

}
