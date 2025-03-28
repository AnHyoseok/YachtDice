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
    private Player player;
    private bool isAI = false;
    private string aiName;
    private bool[] isScored = new bool[14];
    //점수 이미지 
    public Dictionary<string, TextMeshProUGUI> scoreTexts = new Dictionary<string, TextMeshProUGUI>();


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
        this.aiName = aiName;
        isAI = true;
        playerNameText.text = aiName + " [AI]";

        SetTeamColor(properties);
        SetProfileImage(properties, true);
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
            scores[index] = score;
            isScored[index] = true;
            ClearHighlight();

         

            if (scoreTexts.TryGetValue(category, out var textUI))
            {
                textUI.text = score.ToString(); // 확정 점수 다시 셋팅
                Color c = textUI.color;
                c.a = 1f;                      //  확정된 점수는 불투명
                textUI.color = c;
         
            }
            UpdateSubtotalAndBonus();
            UpdateScoreUI();
            // 모든 플레이어에게 점수 동기화
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
        if (player != null && player.CustomProperties.TryGetValue("Score", out object rawScore))
        {
            if (player != PhotonNetwork.LocalPlayer)
            {
                scores = (int[])rawScore;

                if (properties.TryGetValue("ScoredFlags", out object rawFlags))
                {
                    isScored = (bool[])rawFlags;
                }
                else
                {
                    // fallback - 점수 값 기준 추론
                    for (int i = 0; i < scores.Length; i++)
                    {
                        isScored[i] = scores[i] != 0;
                    }
                }

                UpdateScoreUI();
            }
            else
            {
                Debug.Log("내 점수판이므로 UpdateScoreData() 무시");
            }
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
                    textUI.color = new Color(textUI.color.r, textUI.color.g, textUI.color.b, 1f); // 항상 불투명
                }
                continue;
            }


            // 이미 점수가 기록된 항목은 미리보기 적용하지 않음
            if (IsAlreadyScored(index)) continue;

            if (index < upperSectionTexts.Length)
            {
                upperSectionTexts[index].text = scoreEntry.Value.ToString();
            }
            else if (index - 8 < lowerSectionTexts.Length)
            {
                lowerSectionTexts[index - 8].text = scoreEntry.Value.ToString();
            }
            else
            {
                Debug.LogWarning($"[미리보기 실패] {scoreEntry.Key}는 인덱스 범위를 초과했어요.");
            }
        }

        ShowAll();  // 알파값 적용 (이미 기록된 항목은 1f, 미기록은 0.5f)
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
        //selectedCategory = null;

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
                c.a = isScored[index] ? 1f : 0.5f;
            }

            kvp.Value.color = c;
        }
    }
}
