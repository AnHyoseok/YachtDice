using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Photon.Pun;
using System.Collections;


public class ScoreboardEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Image profileImage;
    public Image teamColor;

    public TextMeshProUGUI[] upperSectionTexts;
    public TextMeshProUGUI[] lowerSectionTexts;
    public TextMeshProUGUI totalScoreText;
    //private string selectedCategory = null; // 현재 선택 중인 카테고리
    private int[] scores = new int[14]; // 점수 저장 배열
    public Player player;
    public bool isAI = false;
    public string aiName;
    private bool[] isScored = new bool[14];
    //점수 이미지 
    public Dictionary<string, TextMeshProUGUI> scoreTexts = new Dictionary<string, TextMeshProUGUI>();
    public string AIName => aiName;

    void Awake()
    {
        string[] categories = {
    "ONES", "TWOS", "THREES", "FOURS", "FIVES", "SIXES",
    "SUBTOTAL", "BONUS",
    "Choice", "4 of a Kind", "FULL_HOUSE", "SMALL_STRAIGHT", "LARGE_STRAIGHT", "YAHTZEE"
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
        if (player == PhotonNetwork.LocalPlayer)
        {
            Debug.Log(" 내 점수판 생성됨 - 클릭 허용");
            EnableScoreButtons();  //  점수 칸 클릭 가능하게 설정
        }
        else
        {
            Debug.Log(" 다른 플레이어 점수판 - 클릭 차단");
            DisableScoreButtons(); //  클릭 불가능하게 설정
        }
    }
    //버튼활성화
    void EnableScoreButtons()
    {
        foreach (var kvp in scoreTexts)
        {
            Button btn = kvp.Value.GetComponentInParent<Button>();
            if (btn != null)
            {
                btn.interactable = true;
                string category = kvp.Key; 
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => HighlightScore(category));
            }
        }
    }
    //버튼비활성화
    void DisableScoreButtons()
    {
        foreach (var kvp in scoreTexts)
        {
            Button btn = kvp.Value.GetComponentInParent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.interactable = false;
            }
        }
    }
    public void SetAIData(string aiName, ExitGames.Client.Photon.Hashtable properties)
    {
        Debug.Log($"[SetAIData] 호출됨: aiName={aiName}");
        this.aiName = aiName;
        isAI = true;
        playerNameText.text = aiName + " [AI]";
        SetTeamColor(properties);

        SetProfileImage(properties, true);
        if (!properties.ContainsKey("ScoredFlags"))
        {
            isScored = new bool[14];
        }
        UpdateScoreData(properties);
    }

    private void SetTeamColor(ExitGames.Client.Photon.Hashtable properties)
    {
        if (properties.ContainsKey("Team"))
        {
            string team = (string)properties["Team"];
            teamColor.color = (team == "Red") ? Color.red : Color.blue;
        }
    }

    private void SetProfileImage(ExitGames.Client.Photon.Hashtable properties, bool isAI)
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
            //  점수 중복 방지
            if (IsAlreadyScored(index)) return;

            scores[index] = score;
            isScored[index] = true;

            if (scoreTexts.TryGetValue(category, out var textUI))
            {
                textUI.text = score.ToString();
                SetAlpha(textUI, 1f);  //  해당 텍스트만 알파 적용
            }

            UpdateSubtotalAndBonus();
            UpdateScoreUI();

          
            if (player == PhotonNetwork.LocalPlayer)
            {
                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
                hash["Score"] = scores;
                hash["ScoredFlags"] = isScored;
                player.SetCustomProperties(hash);
            }
        }
    }


    private void UpdateSubtotalAndBonus()
    {
        int subtotal = 0;
        for (int i = 0; i <= 5; i++) // ONES ~ SIXES
        {
            subtotal += scores[i];
        }

        int bonus = subtotal >= 63 ? 35 : 0;

        scores[6] = subtotal; // SUBTOTAL
        scores[7] = bonus;    // BONUS

        if (scoreTexts.TryGetValue(DiceScore.SUBTOTAL, out var subtotalText))
        {
            subtotalText.text = subtotal.ToString();
            Color c = subtotalText.color;
            c.a = 1f;
            subtotalText.color = c;
        }

        if (scoreTexts.TryGetValue(DiceScore.BONUS, out var bonusText))
        {
            bonusText.text = bonus.ToString();
            Color c = bonusText.color;
            c.a = 1f;
            bonusText.color = c;
        }
    }

    public void UpdateAIScore(ExitGames.Client.Photon.Hashtable properties)
    {
        if (isAI)
        {
            UpdateScoreData(properties);
        }
    }

    public void UpdateScoreData(ExitGames.Client.Photon.Hashtable properties)
    {
        if (player != null && !properties.ContainsKey("Score")) return;

        if (isAI)
        {
            if (properties.TryGetValue("Score", out object rawScore))
                scores = (int[])rawScore;

            if (properties.TryGetValue("ScoredFlags", out object rawFlags))
            {
                isScored = (bool[])rawFlags;
            }
            else
            {
                // fallback 제거하고, 명시적 값 없으면 모든 항목 false
                isScored = new bool[scores.Length];
            }

            UpdateScoreUI();
        }
        else if (player != PhotonNetwork.LocalPlayer)
        {
            if (player.CustomProperties.TryGetValue("Score", out object rawScore))
                scores = (int[])rawScore;

            if (player.CustomProperties.TryGetValue("ScoredFlags", out object rawFlags))
            {
                isScored = (bool[])rawFlags;
            }
            else
            {
                isScored = new bool[scores.Length];
            }

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
            7 => "BONUS",
            8 => "Choice",
            9 => "4 of a Kind",
            10 => "FULL_HOUSE",
            11 => "SMALL_STRAIGHT",
            12 => "LARGE_STRAIGHT",
            13 => "YAHTZEE",
            _ => ""
        };
    }



    public void ShowPreview(Dictionary<string, int> previewScores)
    {
        Debug.Log($"[ShowPreview] 호출됨 - isAI={isAI}, aiName={aiName}");
        ClearHighlight();

        foreach (var scoreEntry in previewScores)
        {
            int index = GetCategoryIndex(scoreEntry.Key);
            if (index == -1) continue;

            if (scoreEntry.Key == DiceScore.SUBTOTAL || scoreEntry.Key == DiceScore.BONUS)
            {
                if (scoreTexts.TryGetValue(scoreEntry.Key, out var textUI))
                {
                    textUI.text = scoreEntry.Value.ToString();
                    SetAlpha(textUI, 1f);
                }
                continue;
            }

            if (scoreTexts.TryGetValue(scoreEntry.Key, out var text))
            {
                text.text = scoreEntry.Value.ToString();

                if (isScored != null && index < isScored.Length && isScored[index])
                {
                    SetAlpha(text, 1f);
                }
                else
                {
                    SetAlpha(text, 0.5f); 
                }
            }


        }
    }




    public void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        Color c = text.color;
        c.a = alpha;
        text.color = c;
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
            case "BONUS": return 7;
            case "Choice": return 8;
            case "4 of a Kind": return 9;
            case "FULL_HOUSE": return 10;
            case "SMALL_STRAIGHT": return 11;
            case "LARGE_STRAIGHT": return 12;
            case "YAHTZEE": return 13;
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
        foreach (var kvp in scoreTexts)
        {
            int index = GetCategoryIndex(kvp.Key);
            if (index == -1) continue;

            Color c = kvp.Value.color;

            if (kvp.Key == DiceScore.SUBTOTAL || kvp.Key == DiceScore.BONUS)
            {
                c.a = 1f;
            }
            else
            {
                c.a = isScored[index] ? 1f : 0.5f; //  기입 여부로 알파 결정
            }

            kvp.Value.color = c;
        }
    }


    public void HighlightScore(string category)
    {
        StartCoroutine(DelayedScoreConfirm(category));
    }

    private IEnumerator DelayedScoreConfirm(string category)
    {
        yield return new WaitForSeconds(0.2f); //  살짝 기다린 후 실행

        if (player != PhotonNetwork.LocalPlayer)
        {
            Debug.LogWarning(" 내 점수판 아님. 차단.");
            yield break;
        }

        int index = GetCategoryIndex(category);
        if (IsAlreadyScored(index))
        {
            Debug.LogWarning(" 이미 기록된 점수입니다.");
            yield break;
        }

        if (!TurnManager.instance.IsMyTurn())
        {
            Debug.LogWarning(" 내 턴 아님.");
            yield break;
        }
        if (!DiceManager.Instance.isDiceArray)
        {
            Debug.Log(" 주사위 정렬이 끝나지 않아 점수 선택 불가!");
            yield break;
        }

        int score = DiceManager.Instance.CalculateScore(category);
      
        UpdateScore(category, score);


       
        TurnManager.instance.EndMyTurn();
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
        return isScored[index];
    }

    //선택 초기화
    public void ClearHighlight()
    {
       
        foreach (var kvp in scoreTexts)
        {
            string key = kvp.Key;
            int index = GetCategoryIndex(key);
            Color c = kvp.Value.color;

            if (key == DiceScore.SUBTOTAL || key == DiceScore.BONUS)
            {
                c.a = 1f;
            }
            else
            {
                c.a = isScored[index] ? 1f : 0f; //  선택 안된 건 완전 투명
            }

            kvp.Value.color = c;
        }
    }
    public int GetScoreByCategoryIndex(int index)
    {
        if (index < 0 || index >= scores.Length) return 0;
        return scores[index];
    }
}
