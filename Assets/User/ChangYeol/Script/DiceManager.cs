using System.Collections;
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
    public const string FOUR_KIND = "FOUR_KIND";
    public const string FULL_HOUSE = "FULL_HOUSE";
    public const string SMALL_STRAIGHT = "SMALL_STRAIGHT";
    public const string LARGE_STRAIGHT = "LARGE_STRAIGHT";
    public const string YAHTZEE = "YAHTZEE";
    public const string CHANCE = "CHANCE";
}
public class DiceManager : Singleton<DiceManager>
{
    #region Vaiables
    private ScoreboardEntry scoreboardEntry;

    public Transform dicetrans;
    public float spacing = 0.8f;
    public float moveSpeed = 2f;

    public Dice dice;
    public Dice[] dices = new Dice[5];
    public Dice[] newdicelist;
    public float minRange = -0.5f;
    public float maxRange = 0.5f;
    public float minRangeY = 1f;
    public float maxRangeY = 2f;
    public int rollsLeft = 3;  //최대 3번 굴릴 수 있음

    private HashSet<float> usedYValues = new HashSet<float>(); // 중복 방지용 Y값 저장
    private int upperSectionScore = 0;
    private bool boonsGiven = false;
    public Button rollDice;
    [HideInInspector] public bool isArray = false;
    [HideInInspector] public bool isArrays = false;
    [HideInInspector] public bool isRotat = false;
    public bool isDiceArray = false;
    #endregion

    private void Start()
    {
        scoreboardEntry = FindAnyObjectByType<ScoreboardEntry>();
    }
    private void Update()
    {
        CheckDiceStopped();
    }

    public Vector3 GetUniqueRandomPosition(float minRangeX, float maxRangeX)
    {
        float y = Random.Range(minRangeY, maxRangeY);
        float z = Random.Range(minRange, maxRange);
        float x;
        do
        {
            x = Random.Range(minRangeX, maxRangeX);
        } while (usedYValues.Contains(y)); // Y 값이 중복되지 않을 때까지 반복

        usedYValues.Add(y); // 사용한 Y 값 저장

        return new Vector3(x, y, z);
    }
    public int[] GetDiceValues()
    {
        int[] values = new int[newdicelist.Length];
        for (int i = 0; i < newdicelist.Length; i++)
        {
            values[i] = newdicelist[i].GetDiceValue();
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
        UpdateScoreboard(category, score);
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
    private void UpdateScoreboard(string category, int score)
    {
        if (scoreboardEntry != null)
        {
            scoreboardEntry.UpdateScore(category, score);
        }
    }
    public int GetUpperSectionScore()
    {
        return upperSectionScore;
    }
    void CheckDiceStopped()
    {
        for(int i = 0; i < dices.Length;i++)
        {
            if (!isArray && dices[i] != null)
            {
                bool allStopped = System.Array.TrueForAll(dices, dice => dice.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.1f
                && dice.GetComponent<Rigidbody>().angularVelocity.magnitude < 0.1f);

                if (allStopped && dices.Length > 0)
                {
                    Dicearray();
                }
            }
        }
    }
    public void DiceArrays()
    {
        if (isArrays) return;
        isArrays = true;
        System.Array.Sort(dices,(a, b) => a.GetDiceValue().CompareTo(b.GetDiceValue()));
        StartCoroutine(MoveDiceToSortedPosition());
    }
    void Dicearray()
    {
        if (isArray || isArrays) return;
        isArray = true;
        isArrays = true;
        System.Array.Sort(dices,(a, b) => a.GetDiceValue().CompareTo(b.GetDiceValue()));
        StartCoroutine(MoveDiceToSortedPosition());
        //점수 보이게 하기

    }
    private IEnumerator MoveDiceToSortedPosition()
    {
        int diceCount = dices.Length;
        if (diceCount == 0) yield break;

        float totalWidth = (diceCount - 1) * spacing;
        Vector3 startPosition = dicetrans.position - new Vector3(totalWidth / 2, 0, 0);

        for (int i = 0; i < diceCount; i++)
        {
            GameObject dice = dices[i].gameObject;
            Rigidbody rb = dice.GetComponent<Rigidbody>(); // Rigidbody 가져오기

            int faceValue = dice.GetComponent<Dice>().GetDiceValue();
            Quaternion targetRotation = GetTargetRotation(faceValue);

            Vector3 targetPosition = startPosition + new Vector3(i * spacing, 0, 0);
            Vector3 initaialPosition = dice.transform.position;
            Quaternion initialRotation = dice.transform.rotation;
            float elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                dice.transform.position = Vector3.Lerp(initaialPosition, targetPosition, elapsedTime);
                if(!isRotat)
                {
                    dice.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, elapsedTime);
                    isRotat = true;
                }
                elapsedTime += Time.deltaTime * moveSpeed;

                yield return null;
            }
            dice.transform.position = targetPosition;
            dice.transform.rotation = targetRotation;
            for(int j= 0;j < dice.GetComponent<Dice>().diceList.Count; j++)
            {
                BoxCollider box = dice.GetComponent<Dice>().diceList[j].GetComponent<BoxCollider>();
                box.enabled = false;
            }
            if (rb != null)
            {
                rb.isKinematic = true; // 완전히 멈추도록 설정
            }
        }
        isArrays = false;
        isDiceArray = true;

    }
    private Quaternion GetTargetRotation(int faceValue)
    {
        switch(faceValue)
        {
            case 1: return Quaternion.Euler(-180f, 0f, 0f);
            case 2: return Quaternion.Euler(0f, 0f, -90f);
            case 3: return Quaternion.Euler(-90f, 0f, 0f);
            case 4: return Quaternion.Euler(0f, 0f, 90f);
            case 5: return Quaternion.Euler(90f, 0f, 0f);
            case 6: return Quaternion.Euler(0f, 0f, 0f);
        }
        return Quaternion.identity;
    }
}
