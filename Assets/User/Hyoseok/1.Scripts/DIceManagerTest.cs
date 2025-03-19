using UnityEngine;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class DiceManagerTest : MonoBehaviour
{
    private ScoreboardEntry scoreboardEntry;
    private int upperSectionScore = 0;
    private bool boonsGiven = false;

    void Start()
    {
        scoreboardEntry = FindAnyObjectByType<ScoreboardEntry>(); 
        if (scoreboardEntry == null)
        {
            Debug.LogError("ScoreboardEntry를 찾을 수 없습니다!");
        }
    }

    public int CalculateScore(string category)
    {
        int[] values = GetDiceValues();
        Debug.Log(string.Join(", ", values));  // 주사위 값 디버깅

        values = values.OrderBy(v => v).ToArray();
        int score = 0;

        switch (category)
        {
            case "ONES": score = values.Count(v => v == 1) * 1; break;
            case "TWOS": score = values.Count(v => v == 2) * 2; break;
            case "THREES": score = values.Count(v => v == 3) * 3; break;
            case "FOURS": score = values.Count(v => v == 4) * 4; break;
            case "FIVES": score = values.Count(v => v == 5) * 5; break;
            case "SIXES": score = values.Count(v => v == 6) * 6; break;

            case "CHANCE": score = values.Sum(); break;

            case "FOUR_KIND": score = values.GroupBy(v => v).Any(g => g.Count() >= 4) ? values.Sum() : 0; break;
            case "FULL_HOUSE": score = (values.Distinct().Count() == 2 && values.GroupBy(v => v).Any(g => g.Count() == 3)) ? 25 : 0; break;
            case "SMALL_STRAIGHT":
                score = (values.Distinct().SequenceEqual(new int[] { 1, 2, 3, 4 }) ||
                         values.Distinct().SequenceEqual(new int[] { 1, 2, 3, 4, 5 }) ||
                         values.Distinct().SequenceEqual(new int[] { 3, 4, 5, 6 })) ? 30 : 0;
                break;
            case "LARGE_STRAIGHT":
                score = (values.Distinct().SequenceEqual(new int[] { 1, 2, 3, 4, 5 }) ||
                         values.Distinct().SequenceEqual(new int[] { 2, 3, 4, 5, 6 })) ? 40 : 0;
                break;
            case "YAHTZEE": score = values.Distinct().Count() == 1 ? 50 : 0; break;
        }

        if (IsUpperSection(category))
        {
            upperSectionScore += score;
            CheckForBonus();
        }

        UpdateScoreboard(category, score);
        return score;
    }

    private bool IsUpperSection(string category)
    {
        return category == "ONES" || category == "TWOS" || category == "THREES" ||
               category == "FOURS" || category == "FIVES" || category == "SIXES";
    }

    private void CheckForBonus()
    {
        if (!boonsGiven && upperSectionScore >= 63)
        {
            Debug.Log("보너스 점수 획득");
            upperSectionScore += 35;
            boonsGiven = true;
            UpdateScoreboard("Bonus", 35);
        }
    }

    private void UpdateScoreboard(string category, int score)
    {
        if (scoreboardEntry != null)
        {
            scoreboardEntry.UpdateScore(category, score);
        }
    }

    private int[] GetDiceValues()
    {
        // 테스트용으로 5개의 주사위 값을 랜덤으로 반환
        return new int[] { Random.Range(1, 7), Random.Range(1, 7), Random.Range(1, 7), Random.Range(1, 7), Random.Range(1, 7) };
    }
}
