using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;

public class ScoreboardEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image profileImage;
    public Image teamColor;

    public TextMeshProUGUI[] upperSectionTexts;
    public TextMeshProUGUI[] lowerSectionTexts;
    public TextMeshProUGUI totalScoreText;

    private int[] scores = new int[14]; // 점수 저장 배열
    private Player player;
    private bool isAI = false;
    private string aiName;

    public void SetPlayerData(Player player)
    {
        this.player = player;
        isAI = false;
        playerNameText.text = player.NickName;

        SetTeamColor(player.CustomProperties);
        SetProfileImage(player.CustomProperties, false);
        UpdateScoreData(player.CustomProperties);
    }

    public void SetAIData(string aiName, Hashtable properties)
    {
        this.aiName = aiName;
        isAI = true;
        playerNameText.text = aiName + " [AI]";

        SetTeamColor(properties);
        SetProfileImage(properties, true);
        UpdateScoreData(properties);
    }

    private void SetTeamColor(Hashtable properties)
    {
        if (properties.ContainsKey("Team"))
        {
            string team = (string)properties["Team"];
            teamColor.color = (team == "Red") ? Color.red : Color.blue;
        }
    }

    private void SetProfileImage(Hashtable properties, bool isAI)
    {
        if (properties.ContainsKey(isAI ? "ProfileIndex" : "ProfileImageIndex"))
        {
            int profileIndex = (int)properties[isAI ? "ProfileIndex" : "ProfileImageIndex"];
            profileImage.sprite = GetProfileSprite(profileIndex, isAI);
        }
    }

    public void UpdateScore(string category, int score)
    {
        int index = GetCategoryIndex(category);
        if (index != -1)
        {
            scores[index] = score;
            UpdateScoreUI();
        }
    }

    public void UpdateAIScore(Hashtable properties)
    {
        if (isAI)
        {
            UpdateScoreData(properties);
        }
    }

    public void UpdateScoreData(Hashtable properties)
    {
        if (properties.ContainsKey("Score"))
        {
            scores = (int[])properties["Score"];
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        for (int i = 0; i < upperSectionTexts.Length; i++)
            upperSectionTexts[i].text = scores[i].ToString();

        for (int i = 0; i < lowerSectionTexts.Length; i++)
            lowerSectionTexts[i].text = scores[i + 8].ToString();

        int total = 0;
        foreach (int score in scores) total += score;
        totalScoreText.text = total.ToString();
    }

    private int GetCategoryIndex(string category)
    {
        switch (category)
        {
            case "ONES": return 0;
            case "TWOS": return 1;
            case "THREES": return 2;
            case "FOURS": return 3;
            case "FIVES": return 4;
            case "SIXES": return 5;
            case "Subtotal": return 6;
            case "Bonus": return 7;
            case "Choice": return 8;
            case "4 of a Kind": return 9;
            case "Full House": return 10;
            case "S. Straight": return 11;
            case "L. Straight": return 12;
            case "Yacht": return 13;
            default: return -1;
        }
    }

    private Sprite GetProfileSprite(int index, bool isAI)
    {
        Sprite[] spriteArray = isAI ? PlayerPrefab.AIProfileSprites : PlayerPrefab.ProfileSprites;
        if (spriteArray != null && index >= 0 && index < spriteArray.Length)
        {
            return spriteArray[index];
        }
        return (spriteArray != null && spriteArray.Length > 0) ? spriteArray[0] : null;
    }
}
