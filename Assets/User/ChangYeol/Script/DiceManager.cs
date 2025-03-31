using Photon.Pun;
using Photon.Realtime;
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
    private PhotonView photonView;
    public GameObject[] boxobject;
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
    public bool isDiceArray = false;
    #endregion
    protected override void Awake()
    {
        base.Awake();
        photonView = GetComponent<PhotonView>();
    }
    private void Start()
    {
        scoreboardEntry = FindAnyObjectByType<ScoreboardEntry>();
    }
    private void Update()
    {
        CheckDiceStopped();
        //스코어 기록 
        rollsLeftText.text = rollsLeft.ToString() + " left";
        //Debug.Log($"[Check] isArray={isArray}, dices length={dices.Length}");
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
            //Debug.LogError(" GetDiceValues() - newdicelist가 비어 있음! 주사위가 추가되지 않음.");
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
            //Debug.LogError(" GetDiceValues() - dices가 비어 있음! 주사위가 추가되지 않음.");
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

        //Debug.Log($"주사위 값: {string.Join(", ", values)}");

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
                int bonusSubtotal = 0;
                for (int i = 1; i <= 6; i++)
                    bonusSubtotal += counts[i] * i;
                score = bonusSubtotal >= 63 ? 35 : 0;
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

        int subtotal = 0;
        int bonus = 0;

Player currentPlayer = TurnManager.instance.GetCurrentPlayer();
if (currentPlayer.CustomProperties.TryGetValue("Score", out object rawScore))
{
    int[] confirmedScores = (int[])rawScore;
    for (int i = 0; i <= 5; i++) subtotal += confirmedScores[i];
    bonus = subtotal >= 63 ? 35 : 0;
}


        UpdateScoreboard(DiceScore.SUBTOTAL, subtotal);
        UpdateScoreboard(DiceScore.BONUS, bonus);

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

   
    // 점수 미리보기 호출 (내가 주사위 던졌을 때 실행)
    public void ShowPreviewScore()
    {
        if (!isDiceArray ) return;
        
       
        int[] values = GetDiceValues().Concat(GetDiceValue()).ToArray();
        int[] counts = new int[7];
        foreach (int v in values) counts[v]++;

        Dictionary<string, int> previewScores = new Dictionary<string, int>();
        previewScores[DiceScore.ONES] = counts[1] * 1;
        previewScores[DiceScore.TWOS] = counts[2] * 2;
        previewScores[DiceScore.THREES] = counts[3] * 3;
        previewScores[DiceScore.FOURS] = counts[4] * 4;
        previewScores[DiceScore.FIVES] = counts[5] * 5;
        previewScores[DiceScore.SIXES] = counts[6] * 6;
        // 확정된 점수 가져오기
        int subtotal = 0;
        var currentPlayer = TurnManager.instance.GetCurrentPlayer();
        if (currentPlayer.CustomProperties.TryGetValue("Score", out object rawScore))
        {
            int[] confirmedScores = (int[])rawScore;

            // 0~5: ONES~SIXES
            for (int i = 0; i <= 5; i++)
            {
                subtotal += confirmedScores[i];
            }
        }
        previewScores[DiceScore.SUBTOTAL] = subtotal;

        previewScores[DiceScore.BONUS] = subtotal >= 63 ? 35 : 0;
     
        previewScores[DiceScore.Choice] = values.Sum();
        previewScores[DiceScore.FOUR_KIND] = counts.Any(c => c >= 4) ? values.Sum() : 0;

        bool hasThree = counts.Any(c => c == 3);
        bool hasTwo = counts.Any(c => c == 2);
        previewScores[DiceScore.FULL_HOUSE] = (hasThree && hasTwo) ? values.Sum() : 0;
        previewScores[DiceScore.SMALL_STRAIGHT] = HasStraight(values, 4) ? 15 : 0;
        previewScores[DiceScore.LARGE_STRAIGHT] = HasStraight(values, 5) ? 30 : 0;
        previewScores[DiceScore.YAHTZEE] = counts.Any(c => c == 5) ? 50 : 0;

        // 딕셔너리 -> 배열로 변환 (RPC 전송용)
        string[] keys = previewScores.Keys.ToArray();
        int[] vals = previewScores.Values.ToArray();

        // 모든 유저에게 A의 점수 미리보기 전송

        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("RPC_ShowPreviewScore", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, keys, vals);

        //Debug.Log($"[Preview 전송] {PhotonNetwork.LocalPlayer.NickName}: {string.Join(", ", previewScores.Select(kv => $"{kv.Key}:{kv.Value}"))}");
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

    [PunRPC]
    public void RPC_ShowPreviewScore(int actorNumber, string[] keys, int[] values)
    {
    
        Dictionary<string, int> previewScores = new Dictionary<string, int>();
        for (int i = 0; i < keys.Length; i++)
        {
            previewScores[keys[i]] = values[i];
        }

        if (GameSceneManager.Instance.scoreboardEntries.TryGetValue(actorNumber, out var entry))
        {

            entry.ShowPreview(previewScores);
            //Debug.Log($"[모두에게 미리보기 적용됨] 플레이어 {actorNumber}: {string.Join(", ", previewScores.Select(kv => $"{kv.Key}:{kv.Value}"))}");
        }
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
        Player currentPlayer = TurnManager.instance.GetCurrentPlayer();
        int actorNumber = currentPlayer.ActorNumber;

        if (GameSceneManager.Instance.scoreboardEntries.TryGetValue(actorNumber, out ScoreboardEntry entry))
        {
            entry.UpdateScore(category, score);
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
            if (dices[i] == null)
            {
                Debug.LogWarning($"[CheckDiceStopped] dice {i} is null");
                return;
            }

            if (!isArray)
            {
                bool allStopped = System.Array.TrueForAll(dices, dice => dice != null && dice.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.2f
                && dice.GetComponent<Rigidbody>().angularVelocity.magnitude < 0.2f);

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
                dice.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, elapsedTime);
                elapsedTime += Time.deltaTime * moveSpeed;

                yield return null;
            }
            dice.transform.position = targetPosition;
            dice.transform.rotation = targetRotation;
            dice.GetComponent<Dice>().originPos = dice.transform.position;
        
            if (rb != null)
            {
                rb.isKinematic = true; // 완전히 멈추도록 설정
            }
        }
        photonView.RPC("BoxobjectActiveFalse", RpcTarget.All);
        isArrays = false;
        isDiceArray = true;

        //점수 알파 0.5
        photonView.RPC("RPC_ShowAllScoreboards", RpcTarget.All);


    }
    [PunRPC]
    public void RPC_ShowAllScoreboards()
    {
        foreach (var entry in GameSceneManager.Instance.scoreboardEntries.Values)
        {
            entry.ShowAll();
        }
    }
    [PunRPC]
    void BoxobjectActiveFalse()
    {
        for (int i = 0; i < boxobject.Length; i++)
        {
            boxobject[i].SetActive(false);
        }
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
