using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image profileImage;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI Win_LoseText;
    public Image teamColor;
    public Animator animator;
    public void Setup(Player player, bool isWinner)
    {
        // AI가 아닌 유저만 처리
        string name = player.NickName;

        // AI 이름 리스트에서 유저 이름이 포함되어 있어도 제외 (유저는 PhotonNetwork.PlayerList에 포함됨)
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("AIPlayers", out object aiObj)
            && aiObj is string[] aiNames)
        {
            foreach (string aiName in aiNames)
            {
                if (aiName == name && !IsAIPlayer(player))  // 유저면 그냥 이름만
                {
                    break; // 유저일 땐 [AI] 안 붙임
                }
            }
        }

        playerNameText.text = name;

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

        // 승리 여부
        animator.SetTrigger(isWinner ? "IsWin" : "IsLose");
        Win_LoseText.text = isWinner ? "Winner!" : "Too bad...";
    }

    // 유저가 AI인지 정확히 판별
    private bool IsAIPlayer(Player player)
    {
        // AI는 PhotonNetwork.PlayerList에 존재하지 않음
        return !PhotonNetwork.PlayerList.Contains(player);
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
    public void SetupAI(ScoreboardEntry sbEntry, bool isWinner)
    {
        // AI 이름 표시
        playerNameText.text = sbEntry.AIName + " [AI]";

        // 점수
        scoreText.text = sbEntry.totalScoreText.text;

        // 프로필 이미지 및 팀 색상
        profileImage.sprite = sbEntry.profileImage.sprite;
        teamColor.color = sbEntry.teamColor.color;

        // 승리 애니메이션
        animator.SetTrigger(isWinner ? "IsWin" : "IsLose");
        Win_LoseText.text = isWinner ? "Winner!" : "Too bad...";
    }


}
