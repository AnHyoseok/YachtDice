using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RollDiceTest : MonoBehaviour
{
    public static RollDiceTest Instance { get; private set; }

    private int[] diceValues = new int[5]; // 주사위 5개

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 주사위를 굴리는 함수
    public void RollDice()
    {
        for (int i = 0; i < diceValues.Length; i++)
        {
            diceValues[i] = Random.Range(1, 7); // 1부터 6까지 랜덤
        }

        Debug.Log(" 주사위 결과: " + string.Join(", ", diceValues));
    }

    // 주사위 값 배열 반환
    public int[] GetDiceValues()
    {
        return diceValues;
    }

    // 카테고리 점수 계산 
    public int CalculateScore(string category)
    {
        int score = 0;
        int[] counts = new int[7]; // 인덱스 1~6 사용
        foreach (int val in diceValues) counts[val]++;

        switch (category)
        {
            case "ONES":
            case "TWOS":
            case "THREES":
            case "FOURS":
            case "FIVES":
            case "SIXES":
                int face = category switch
                {
                    "ONES" => 1,
                    "TWOS" => 2,
                    "THREES" => 3,
                    "FOURS" => 4,
                    "FIVES" => 5,
                    "SIXES" => 6,
                    _ => 0
                };
                score = counts[face] * face;
                break;
            case "SUBTOTAL":
               
                for (int i = 1; i <= 6; i++)
                    score += counts[i] * i;
                break;
            case "BONUS":
                int subtotal = 0;
                for (int i = 1; i <= 6; i++)
                    subtotal += counts[i] * i;
                score = subtotal >= 63 ? 35 : 0;
                break;
            case "Choice":
                score = diceValues.Sum();
                break;

            case "4 of a Kind":
                for (int i = 1; i <= 6; i++)
                {
                    if (counts[i] >= 4)
                    {
                        score = diceValues.Sum();
                        break;
                    }
                }
                break;

            case "FULL_HOUSE":
                bool hasThree = false, hasTwo = false;
                foreach (int count in counts)
                {
                    if (count == 3) hasThree = true;
                    if (count == 2) hasTwo = true;
                }
                if (hasThree && hasTwo)
                    score = diceValues.Sum();
                break;

            case "SMALL_STRAIGHT":
                if (HasStraight(4)) score = 15;
                break;

            case "LARGE_STRAIGHT":
                if (HasStraight(5)) score = 30;
                break;

            case "YAHTZEE":
                if (counts.Any(c => c == 5))
                    score = 50;
                break;
        }

        return score;
    }


    private bool HasStraight(int length)
    {
        var values = new HashSet<int>(diceValues);
        int[][] sequences = {
            new[] {1,2,3,4},
            new[] {2,3,4,5},
            new[] {3,4,5,6},
            new[] {1,2,3,4,5},
            new[] {2,3,4,5,6}
        };

        foreach (var seq in sequences)
            if (seq.Length >= length && new HashSet<int>(seq).IsSubsetOf(values))
                return true;
        return false;
    }
}
