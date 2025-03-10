using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DiceScore
{
    public const string ONES = "ONES";
    public const string TWOS = "TWOS";
    public const string THREES = "THREES";
    public const string FOURS = "FOURS";
    public const string FIVES = "FIVES";
    public const string SIXES = "SIXES";
    public const string THREE_KIND = "THREE_KIND";
    public const string FOUR_KIND = "FOUR_KIND";
    public const string FULL_HOUSE = "FULL_HOUSE";
    public const string SMALL_STRAIGHT = "SMALL_STRAIGHT";
    public const string LARGE_STRAIGHT = "LARGE_STRAIGHT";
    public const string YAHTZEE = "YAHTZEE";
    public const string CHANCE = "CHANCE";
}
public class DiceManager : MonoBehaviour
{
    #region Vaiables
    public ShakeCup cup;
    public Dice dice;
    public List<Dice> dicelist;
    //private int rollsLeft = 3;  //최대 3번 굴릴 수 있음

    private HashSet<float> usedYValues = new HashSet<float>(); // 중복 방지용 Y값 저장
    private int upperSectionScore = 0;
    private bool boonsGiven = false;
    public Button rollDice;
    #endregion
    private void Start()
    {
        for(int i = 0; i < 5; i++)
        {
            GameObject Adddice = Instantiate(dice.gameObject,GetUniqueRandomPosition(),Quaternion.identity);
            dicelist.Add(Adddice.GetComponent<Dice>());
        }
        rollDice.onClick.AddListener(() => cup.ShakerCup());
    }
    private void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.S))
        {
            cup.ShakerCup();
            RollAllDice();
        }
    }

    public Vector3 GetUniqueRandomPosition()
    {
        float x = Random.Range(-0.6f, 0.6f);
        float z = Random.Range(-0.6f, 0.6f);
        float y;

        do
        {
            y = Random.Range(2.2f, 2.8f);
        } while (usedYValues.Contains(y)); // Y 값이 중복되지 않을 때까지 반복

        usedYValues.Add(y); // 사용한 Y 값 저장

        return new Vector3(x, y, z);
    }
    public void RollAllDice()
    {
        //if (rollsLeft <= 0) return;
        //rollsLeft--;
        foreach (Dice dice in dicelist)
        {
            dice.RollDice();
        }
    }
    public int[] GetDiceValues()
    {
        int[] values = new int[dicelist.Count];
        for (int i = 0; i < dicelist.Count; i++)
        {
            values[i] = dicelist[i].GetDiceValue();
        }
        return values;
    }
    public int CalculateScore(string category)
    {
        int[] values = GetDiceValues();
        Debug.Log(GetDiceValues());
        values = values.OrderBy(v => v).ToArray();

        int score = 0;

        switch(category)
        {
            case "ONES": score = values.Count(v => v == 1) * 1;
                break;
            case "TWOS": score = values.Count(v => v == 2) * 2;
                break;
            case "THREES":score = values.Count(v => v == 3) * 3;
                break;
            case "FOURS": score = values.Count(v => v == 4) * 4;
                break;
            case "FIVES": score = values.Count(v => v == 5) * 5;
                break;
            case "SIXES": score = values.Count(v => v == 6) * 6;
                break;
            case "THREE_KIND": score = values.GroupBy(v => v).Any(g => g.Count() >= 3) ? values.Sum() : 0;
                break;
            case "FOUR_KIND": score = values.GroupBy(v => v).Any(g => g.Count() >= 4) ? values.Sum() : 0;
                break;
            case "FULL_HOUSE": score = (values.Distinct().Count() == 2 && values.GroupBy(v => v).Any(g => g.Count() == 3)) ? 25 : 0;
                break;
            case "SMALL_STRAIGHT": score = values.Distinct().SequenceEqual(new int[] { 1, 2, 3, 4 }) || values.Distinct().SequenceEqual(new int[] { 1, 2, 3, 4, 5 }) || values.Distinct().SequenceEqual(new int[] { 3, 4, 5, 6 }) ? 30 : 0;
                break;
            case "LARGE_STRAIGHT": score = values.Distinct().SequenceEqual(new int[] { 1, 2, 3, 4, 5 }) || values.Distinct().SequenceEqual(new int[] { 2, 3, 4, 5, 6 }) ? 40 : 0;
                break;
            case "YAHTZEE": score = values.Distinct().Count() == 1 ? 50 : 0;
                break;
            case "CHANCE": score = values.Sum();
                break;
        }
        if (category == DiceScore.ONES || category == DiceScore.TWOS || category == DiceScore.THREES
            || category == DiceScore.FOURS || category == DiceScore.FIVES || category == DiceScore.SIXES)
        {
            upperSectionScore += score;
            CheckForBoonus();
        }
        return score;
    }
    private void CheckForBoonus()
    {
        if (! boonsGiven && upperSectionScore >= 63)
        {
            Debug.Log("보너스 점수 획득");
            upperSectionScore += 35;
            boonsGiven = true;
        }
    }
    public int GetUpperSectionScore()
    {
        return upperSectionScore;
    }
}
