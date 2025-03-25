using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class DiceScore
{
    public const string ONES = "ONES";
    public const string TWOS = "TWOS";
    public const string THREES = "THREES";
    public const string FOURS = "FOURS";
    public const string FIVES = "FIVES";
    public const string SIXES = "SIXES";
    public const string SUBTOTAL = "SUBTOTAL";
    public const string BONUS = "BONUS";
    public const string Choice = "Choice";
    public const string FOUR_KIND = "4 of a Kind";
    public const string FULL_HOUSE = "FULL_HOUSE";
    public const string SMALL_STRAIGHT = "SMALL_STRAIGHT";
    public const string LARGE_STRAIGHT = "LARGE_STRAIGHT";
    public const string YAHTZEE = "YAHTZEE";
   
}
public class DiceManager : Singleton<DiceManager>
{
    #region Vaiables
    [HideInInspector]public ScoreboardEntry scoreboardEntry;
    public SelectDice selectdice;
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
    public TextMeshProUGUI rollsLeftText;

    private HashSet<float> usedYValues = new HashSet<float>(); // 중복 방지용 Y값 저장
    private int upperSectionScore = 0;
    private bool boonsGiven = false;
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
        //스코어 기록 
        rollsLeftText.text = rollsLeft.ToString() + " left";
        if (rollsLeft <= 0)
        {
            
            TurnManager.instance.EndMyTurn();
        }

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
        if (newdicelist == null || newdicelist.Length == 0)
        {
            Debug.LogError(" GetDiceValues() - newdicelist가 비어 있음! 주사위가 추가되지 않음.");
            return new int[0];  // 빈 배열 반환 (오류 방지)
        }

        int[] values = new int[newdicelist.Length];
        for (int i = 0; i < newdicelist.Length; i++)
        {
            values[i] = newdicelist[i].GetDiceValue();
        }

        Debug.Log(" 주사위 값 리스트: " + string.Join(", ", values));
        return values;
    }
    public int[] GetDiceValue()
    {
        if (dices == null || dices.Length == 0)
        {
            Debug.LogError(" GetDiceValues() - dices가 비어 있음! 주사위가 추가되지 않음.");
            return new int[0];  // 빈 배열 반환 (오류 방지)
        }

        int[] values = new int[dices.Length];
        for (int i = 0; i < dices.Length; i++)
        {
            if (dices[i] == null) 
            {
              
                values[i] = 0; 
                continue;
            }

            values[i] = dices[i].GetDiceValue();
        }

        Debug.Log(" 주사위 값 리스트: " + string.Join(", ", values));
        return values;
    }
    public int CalculateScore(string category)
    {
        int[] values = GetDiceValues().Concat(GetDiceValue()).ToArray();
        values = values.OrderBy(v => v).ToArray();

        Debug.Log($"주사위 값: {string.Join(", ", values)}");

        int score = 0;
        int[] counts = new int[7];

        foreach (int v in values)
            counts[v]++;

        switch (category)
        {
            case "ONES": score = counts[1] * 1; break;
            case "TWOS": score = counts[2] * 2; break;
            case "THREES": score = counts[3] * 3; break;
            case "FOURS": score = counts[4] * 4; break;
            case "FIVES": score = counts[5] * 5; break;
            case "SIXES": score = counts[6] * 6; break;

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
                score = values.Sum();
                break;

            case "4 of a Kind":
                if (counts.Any(c => c >= 4))
                    score = values.Sum();
                break;

           
            case "FULL_HOUSE":
                bool hasThree = counts.Any(c => c == 3);
                bool hasTwo = counts.Any(c => c == 2);
                if (hasThree && hasTwo)
                    score = values.Sum(); 
                break;

            case "SMALL_STRAIGHT":
                if (HasStraight(4)) score = 15;
                break;

            case "LARGE_STRAIGHT":
                if (HasStraight(5)) score = 30;
                break;

            case "Yacht":
                if (counts.Any(c => c == 5))
                    score = 50;
                break;
        }

        // 상단 점수 보너스 누적
        if (category == DiceScore.ONES || category == DiceScore.TWOS || category == DiceScore.THREES
            || category == DiceScore.FOURS || category == DiceScore.FIVES || category == DiceScore.SIXES)
        {
            upperSectionScore += score;
            CheckForBoonus();
        }

        UpdateScoreboard(category, score);
        return score;
    }

    private bool HasStraight(int requiredLength)
    {
        int[] allValues = GetDiceValues().Concat(GetDiceValue()).Distinct().OrderBy(v => v).ToArray();

        int maxLength = 1;
        int currentLength = 1;

        for (int i = 1; i < allValues.Length; i++)
        {
            if (allValues[i] == allValues[i - 1] + 1)
            {
                currentLength++;
                maxLength = Mathf.Max(maxLength, currentLength);
            }
            else
            {
                currentLength = 1;
            }
        }

        return maxLength >= requiredLength;
    }

    //점수 미리보여주기
    public void ShowPreviewScore()
    {
        if (!isDiceArray) return;

        int[] values = GetDiceValues().Concat(GetDiceValue()).ToArray();
        int[] counts = new int[7];
        foreach (int v in values) counts[v]++;
        HashSet<int> uniqueValues = new HashSet<int>(values);

        Dictionary<string, int> previewScores = new Dictionary<string, int>();

        previewScores[DiceScore.ONES] = counts[1] * 1;
        previewScores[DiceScore.TWOS] = counts[2] * 2;
        previewScores[DiceScore.THREES] = counts[3] * 3;
        previewScores[DiceScore.FOURS] = counts[4] * 4;
        previewScores[DiceScore.FIVES] = counts[5] * 5;
        previewScores[DiceScore.SIXES] = counts[6] * 6;

        // CHOICE
        previewScores[DiceScore.Choice] = values.Sum();

        // 4 of a Kind
        previewScores[DiceScore.FOUR_KIND] = counts.Any(c => c >= 4) ? values.Sum() : 0;

        // Full House (정확히 3개 + 2개)
        bool hasThree = counts.Any(c => c == 3);
        bool hasTwo = counts.Any(c => c == 2);
        previewScores[DiceScore.FULL_HOUSE] = (hasThree && hasTwo) ? values.Sum() : 0;

        // Small Straight
        previewScores[DiceScore.SMALL_STRAIGHT] = HasStraight(values, 4) ? 15 : 0;

        // Large Straight
        previewScores[DiceScore.LARGE_STRAIGHT] = HasStraight(values, 5) ? 30 : 0;

        // Yacht (5 of a kind)
        previewScores[DiceScore.YAHTZEE] = counts.Any(c => c == 5) ? 50 : 0;

        // 디버그 로그
        Debug.Log($"[Preview] {PhotonNetwork.LocalPlayer.NickName} → {string.Join(", ", previewScores.Select(kv => $"{kv.Key}: {kv.Value}"))}");

        // 점수 반영
        if (GameSceneManager.Instance != null && GameSceneManager.Instance.scoreboardEntries.ContainsKey(PhotonNetwork.LocalPlayer.ActorNumber))
        {
            GameSceneManager.Instance.scoreboardEntries[PhotonNetwork.LocalPlayer.ActorNumber].ShowPreview(previewScores);
        }

        // 네트워크 전송
        ExitGames.Client.Photon.Hashtable playerScores = new ExitGames.Client.Photon.Hashtable { { "PreviewScore", previewScores } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerScores);
    }

    private bool HasStraight(int[] values, int requiredLength)
    {
        int[] sorted = values.Distinct().OrderBy(v => v).ToArray();
        int currentLength = 1;
        int maxLength = 1;

        for (int i = 1; i < sorted.Length; i++)
        {
            if (sorted[i] == sorted[i - 1] + 1)
            {
                currentLength++;
                maxLength = Mathf.Max(maxLength, currentLength);
            }
            else
            {
                currentLength = 1;
            }
        }

        return maxLength >= requiredLength;
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
            if (dices[i] == null) return;
            if (!isArray)
            {
                bool allStopped = System.Array.TrueForAll(dices, dice => dice != null && dice.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.1f
                && dice.GetComponent<Rigidbody>().angularVelocity.magnitude < 0.1f);

                if (allStopped && dices.Length > 0)
                {
                    Debug.Log(" 모든 주사위 멈춤 - Dicearray() 실행");
                    Dicearray();
                    DiceManager.Instance.ShowPreviewScore();
                }
            }
        }
    }
    public void DiceArrays()
    {
        //SelectUI가 보이게 하기
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
        
        //SelectUI가 보이게 하기
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
            dice.GetComponent<Dice>().originPos = dice.transform.position;
            //Debug.Log(dice.GetComponent<Dice>().originPos);
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

        //점수 알파 0.5
        ScoreboardManager.instance.ShowLocalScore();

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
