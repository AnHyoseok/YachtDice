using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance;

    [Header("UI References")]
    public GameObject fadein;
    public GameObject scoreboardPanel;
    public GameObject backgroundPanel;
    public Button nextButton;
    public GameObject winloseCanvas;
    public Transform resultContentParent;
    public GameObject userResultPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
       
        nextButton.onClick.AddListener(() => StartCoroutine(ShowResultsAfterDelay()));
       
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartResultSequence();
        }
    }
    public void StartResultSequence()
    {
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(RPC_StartResultSequence), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_StartResultSequence()
    {
        StartCoroutine(ResultFlow());
    }
    
    private IEnumerator ResultFlow()
    {
        yield return new WaitForSeconds(1f);

        fadein.SetActive(true);
        backgroundPanel.SetActive(true);
        scoreboardPanel.transform.localPosition = Vector3.zero;
        scoreboardPanel.SetActive(true);
        nextButton.gameObject.SetActive(true);
    }

    private IEnumerator ShowResultsAfterDelay()
    {
        scoreboardPanel.SetActive(false);
        nextButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);

        winloseCanvas.SetActive(true);
        ShowResultEntries();
    }

    private void ShowResultEntries()
    {
        List<object> redTeam = new List<object>();
        List<object> blueTeam = new List<object>();
        Dictionary<object, int> playerScores = new Dictionary<object, int>();

        // 점수 및 팀 색상 수집
        foreach (var kvp in GameSceneManager.Instance.scoreboardEntries)
        {
            var entry = kvp.Value;
            int totalScore = entry.GetScoreByCategoryIndex(6) +  // SUBTOTAL
                             entry.GetScoreByCategoryIndex(7) +  // BONUS
                             Enumerable.Range(8, 6).Sum(i => entry.GetScoreByCategoryIndex(i)); // LOWER

            playerScores[kvp.Key] = totalScore;

            if (entry.teamColor.color == Color.red)
                redTeam.Add(kvp.Key);
            else
                blueTeam.Add(kvp.Key);
        }

        int redTotal = redTeam.Sum(key => playerScores[key]);
        int blueTotal = blueTeam.Sum(key => playerScores[key]);
        string winningTeam = redTotal >= blueTotal ? "Red" : "Blue";

        var sorted = playerScores.OrderByDescending(kv => kv.Value).ToList();

        foreach (var kv in sorted)
        {
            GameObject go = Instantiate(userResultPrefab, resultContentParent);
            UserEntry entry = go.GetComponent<UserEntry>();

            object key = kv.Key;
            int score = kv.Value;

            bool isWinner = (redTeam.Contains(key) && winningTeam == "Red") ||
                            (blueTeam.Contains(key) && winningTeam == "Blue");

            int actorNumber = -1;

            if (key is Player p)
            {
                actorNumber = p.ActorNumber;
            }
            else if (key is int hash)
            {
                actorNumber = hash;
            }

            if (GameSceneManager.Instance.scoreboardEntries.TryGetValue(actorNumber, out var sbEntry))
            {
                if (sbEntry.isAI)
                {
                    entry.SetupAI(sbEntry, isWinner);
                }
                else
                {
                    entry.Setup(sbEntry.player, isWinner);
                }

                if (score == sorted.First().Value)
                {
                    entry.playerNameText.color = Color.yellow;
                    entry.playerNameText.fontStyle = TMPro.FontStyles.Bold;
                }
            }
            else
            {
                Debug.LogWarning($"[Result] ScoreboardEntry를 찾을 수 없음: {actorNumber}");
            }
        }
    }

    private int CalculateTotalScore(int[] scores)
    {
        int subtotal = 0;
        for (int i = 0; i <= 5; i++) subtotal += scores[i];
        int bonus = scores[7];
        int lower = 0;
        for (int i = 8; i <= 13; i++) lower += scores[i];
        return subtotal + bonus + lower;
    }
}
