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
        List<Player> redTeam = new List<Player>();
        List<Player> blueTeam = new List<Player>();

        Dictionary<Player, int> playerScores = new Dictionary<Player, int>();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("Score", out object rawScore) && rawScore is int[] scores)
            {
                int totalScore = CalculateTotalScore(scores);
                playerScores[player] = totalScore;

                if (player.CustomProperties.TryGetValue("Team", out object team))
                {
                    if ((string)team == "Red") redTeam.Add(player);
                    else blueTeam.Add(player);
                }
            }
        }

        int redTotal = redTeam.Sum(p => playerScores[p]);
        int blueTotal = blueTeam.Sum(p => playerScores[p]);
        string winningTeam = redTotal >= blueTotal ? "Red" : "Blue";

        // 정렬: 팀 먼저, 점수 내림차순
        var allPlayers = redTeam.Concat(blueTeam)
                                .OrderByDescending(p => playerScores[p])
                                .ToList();

        foreach (var player in allPlayers)
        {
            GameObject go = Instantiate(userResultPrefab, resultContentParent);
            UserEntry entry = go.GetComponent<UserEntry>();

            bool isWinner = (string)player.CustomProperties["Team"] == winningTeam;
            entry.Setup(player, isWinner);

            // 최고점수 플레이어 강조 예시
            if (playerScores[player] == playerScores.Values.Max())
            {
                entry.playerNameText.color = Color.yellow; // 예시: 이름을 노란색으로
                entry.playerNameText.fontStyle = TMPro.FontStyles.Bold;
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
