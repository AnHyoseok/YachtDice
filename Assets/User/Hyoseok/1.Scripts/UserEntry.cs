using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image profileImage;
    public TextMeshProUGUI scoreText;
    public Image teamColor;
    public Animator animator;
    public void Setup(Player player, bool isWinner)
    {
        // 이름
        playerNameText.text = player.NickName;

        // 점수
        int totalScore = 0;
        if (player.CustomProperties.TryGetValue("Score", out object rawScore) && rawScore is int[] scores)
        {
            totalScore = CalculateTotalScore(scores);
        }
        scoreText.text = totalScore.ToString();

        // 팀 색상
        if (player.CustomProperties.TryGetValue("Team", out object team))
        {
            teamColor.color = (string)team == "Red" ? Color.red : Color.blue;
        }

        // 프로필 이미지
        if (player.CustomProperties.TryGetValue("ProfileImageIndex", out object profileIndex))
        {
            int index = (int)profileIndex;
            if (index >= 0 && index < PlayerPrefab.ProfileSprites.Length)
            {
                profileImage.sprite = PlayerPrefab.ProfileSprites[index];
            }
        }

        // 승리 여부 (임시)
        animator.SetTrigger(isWinner ? "IsWin" : "IsLose");
    
    }

    private int CalculateTotalScore(int[] scores)
    {
        int subtotal = 0;
        for (int i = 0; i <= 5; i++) subtotal += scores[i];
        int bonus = scores[7];
        int lower = 0;
        for (int i = 8; i <= 13; i++) lower += scores[i];
        return subtotal + bonus + lower;
    }

    private bool CheckIfWinner(int myScore)
    {
        // 현재 플레이어보다 높은 점수의 플레이어가 없으면 승리
        foreach (var other in PhotonNetwork.PlayerList)
        {
            if (other == PhotonNetwork.LocalPlayer) continue;

            if (other.CustomProperties.TryGetValue("Score", out object rawScore) && rawScore is int[] otherScores)
            {
                int otherTotal = CalculateTotalScore(otherScores);
                if (otherTotal > myScore) return false;
            }
        }
        return true;
    }
}
