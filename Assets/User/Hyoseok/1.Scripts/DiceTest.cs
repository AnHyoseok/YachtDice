using UnityEngine;
using System.Collections;

public class DiceTest : MonoBehaviour
{
    private DiceManager diceManager;
    private ScoreboardEntry scoreboardEntry;

    void Start()
    {
        diceManager = DiceManager.Instance;
        StartCoroutine(FindScoreboardAndStartTest());
    }

    IEnumerator FindScoreboardAndStartTest()
    {
        while (scoreboardEntry == null)
        {
            scoreboardEntry = FindAnyObjectByType<ScoreboardEntry>(); 
            if (scoreboardEntry == null)
            {
                Debug.LogWarning("ScoreboardEntry 찾는 중...");
                yield return new WaitForSeconds(0.5f); // 0.5초 대기 후 다시 찾기
            }
        }

        if (diceManager == null)
        {
            Debug.LogError("DiceManagerTest가 설정되지 않았습니다.");
            yield break;
        }

        Debug.Log("ScoreboardEntry 찾음! 점수 테스트 시작.");
        TestCalculateScore();
    }

    void TestCalculateScore()
    {
        string[] testCategories = { "ONES", "TWOS", "THREES", "FOURS", "FIVES", "SIXES", "CHANCE" ,"FOUR_KIND", "FULL_HOUSE", "SMALL_STRAIGHT", "LARGE_STRAIGHT", "YAHTZEE" };

        foreach (string category in testCategories)
        {
            int score = diceManager.CalculateScore(category);
            Debug.Log($"{category}: {score}점");
            scoreboardEntry.UpdateScore(category, score);
        }
    }
}
