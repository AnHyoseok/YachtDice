using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class ScoreboardEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image profileImage;
    public Image teamColor;

    public TextMeshProUGUI[] upperSectionTexts;
    public TextMeshProUGUI[] lowerSectionTexts;
    public TextMeshProUGUI totalScoreText;
    private string selectedCategory = null; // 현재 선택 중인 카테고리
    private int[] scores = new int[14]; // 점수 저장 배열
    private Player player;
    private bool isAI = false;
    private string aiName;

    //점수 이미지 
    public Dictionary<string, TextMeshProUGUI> scoreTexts = new Dictionary<string, TextMeshProUGUI>();


    void Awake()
    {
        string[] categories = {
    "ONES", "TWOS", "THREES", "FOURS", "FIVES", "SIXES",
    "Subtotal", "Bonus",
    "Choice", "4 of a Kind", "Full House", "SMALL_STRAIGHT", "LARGE_STRAIGHT", "Yacht"
};

        for (int i = 0; i < upperSectionTexts.Length; i++)
        {
            scoreTexts[categories[i]] = upperSectionTexts[i];
        }

        for (int i = 0; i < lowerSectionTexts.Length; i++)
        {
            scoreTexts[categories[i + upperSectionTexts.Length]] = lowerSectionTexts[i];
        }
    }
    public void SetPlayerData(Player player)
    {
        this.player = player;
        isAI = false;
        playerNameText.text = player.NickName;
        ScoreboardManager.instance.Register(player, this);
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

            // 모든 플레이어에게 점수 동기화
            if (player != null)
            {
                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
                hash["Score"] = scores;
                player.SetCustomProperties(hash);
            }
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
        // Upper (0~7)
        for (int i = 0; i < upperSectionTexts.Length; i++)
        {
            upperSectionTexts[i].text = scores[i].ToString();
        }

        // Lower (8~13)
        for (int i = 0; i < lowerSectionTexts.Length; i++)
        {
            int scoreIndex = i + 8;
            lowerSectionTexts[i].text = scores[scoreIndex].ToString();
        }

        // 총합 계산
        int subtotal = 0;
        for (int i = 0; i <= 5; i++) subtotal += scores[i];
        int bonus = scores[7];
        int lower = 0;
        for (int i = 8; i <= 13; i++) lower += scores[i];
        int total = subtotal + bonus + lower;
        totalScoreText.text = total.ToString();
    }

    private string GetCategoryByIndex(int index)
    {
        return index switch
        {
            0 => "ONES",
            1 => "TWOS",
            2 => "THREES",
            3 => "FOURS",
            4 => "FIVES",
            5 => "SIXES",
            6 => "SUBTOTAL",
            7 => "Bonus",
            8 => "Choice",
            9 => "4 of a Kind",
            10 => "Full House",
            11 => "SMALL_STRAIGHT",
            12 => "LARGE_STRAIGHT",
            13 => "Yacht",
            _ => ""
        };
    }



    public void ShowPreview(Dictionary<string, int> previewScores)
    {
        Debug.Log(" 점수 미리보기 업데이트 실행됨!");

        foreach (var scoreEntry in previewScores)
        {
            int index = GetCategoryIndex(scoreEntry.Key);
            if (index != -1)
            {
                if (index < upperSectionTexts.Length)
                {
                    upperSectionTexts[index].text = scoreEntry.Value.ToString();
                    Debug.Log($" {scoreEntry.Key} UI 업데이트됨: {scoreEntry.Value}");
                }
                else if (index - 8 < lowerSectionTexts.Length)
                {
                    lowerSectionTexts[index - 8].text = scoreEntry.Value.ToString();
                    Debug.Log($" {scoreEntry.Key} UI 업데이트됨: {scoreEntry.Value}");
                }
                else
                {
                    Debug.LogWarning($" {scoreEntry.Key} UI 업데이트 실패 - 인덱스 범위 초과");
                }
            }
            else
            {
                Debug.LogWarning($" {scoreEntry.Key}의 인덱스를 찾을 수 없음!");
            }
        }
    }


    public int GetCategoryIndex(string category)
    {
        switch (category)
        {
            case "ONES": return 0;
            case "TWOS": return 1;
            case "THREES": return 2;
            case "FOURS": return 3;
            case "FIVES": return 4;
            case "SIXES": return 5;
            case "SUBTOTAL": return 6;
            case "Bonus": return 7;
            case "Choice": return 8;
            case "4 of a Kind": return 9;
            case "FULL_HOUSE": return 10;
            case "SMALL_STRAIGHT": return 11;
            case "LARGE_STRAIGHT": return 12;
            case "Yacht": return 13;
            case "Total": return 14;
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


    public void HideAll()
    {
        SetAllTextAlpha(0f);
    }

    public void ShowAll()
    {
        SetAllTextAlpha(0.5f);

    }

    public void HighlightScore(string category)
    {
        int index = GetCategoryIndex(category);

        if (index < 0 || index >= scores.Length)
        {
            Debug.LogWarning($"HighlightScore(): 잘못된 카테고리 또는 인덱스 범위 초과 - {category}, index: {index}");
            return;
        }

        selectedCategory = category;

        foreach (var kvp in scoreTexts)
        {
            string key = kvp.Key;
            int keyIndex = GetCategoryIndex(key);

            if (keyIndex < 0 || keyIndex >= scores.Length) continue; // 안전 처리

            Color c = kvp.Value.color;
            c.a = (scores[keyIndex] != 0) ? 1f : (key == category ? 1f : 0f);
            kvp.Value.color = c;
        }

        Debug.Log(TurnManager.instance.IsMyTurn());
        //  점수 확정 + 턴 넘기기
        if (TurnManager.instance.IsMyTurn())
        {
            int score = DiceManager.Instance.CalculateScore(category);
            UpdateScore(category, score);

            Debug.Log($" {category} 확정됨 ({score}점), 턴 종료");

            TurnManager.instance.EndMyTurn(); // 다음 턴으로
        }
    }




    private void SetAllTextAlpha(float alpha)
    {
        foreach (var text in scoreTexts.Values)
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }
    }

    //점수기록 여부 
    public bool IsAlreadyScored(int index)
    {
        return scores[index] != 0;
    }

    //선택 초기화
    public void ClearHighlight()
    {
        selectedCategory = null;

        foreach (var kvp in scoreTexts)
        {
            string key = kvp.Key;
            int index = GetCategoryIndex(key);
            Color c = kvp.Value.color;

            c.a = (scores[index] != 0) ? 1f : 0f;
            kvp.Value.color = c;
        }
    }
}
