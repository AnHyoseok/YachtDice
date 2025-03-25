using UnityEngine;
using System.Collections;

public class DiceTest : MonoBehaviour
{
    private DiceManager diceManager;
    private ScoreboardEntry scoreboardEntry;
    private bool testExecuted = false; // 딱 1번만 실행되도록 플래그

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
                yield return new WaitForSeconds(0.5f);
            }
        }

        Debug.Log(" ScoreboardEntry 찾음! 테스트 실행");
        RunScoreTest(); // 자동 실행
    }

    void RunScoreTest()
    {
        string[] testCategories = {
            "ONES", "TWOS", "THREES", "FOURS", "FIVES", "SIXES", "SUBTOTAL", "BONUS",
            "Choice", "4 of a Kind", "FULL_HOUSE", "SMALL_STRAIGHT", "LARGE_STRAIGHT", "YAHTZEE"
        };

        foreach (string category in testCategories)
        {
            int score = diceManager.CalculateScore(category);
            Debug.Log($"{category}: {score}점");
            scoreboardEntry.UpdateScore(category, score);
        }

        testExecuted = true; // 중복 방지
    }
}
