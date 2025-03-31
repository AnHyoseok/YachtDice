using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class ScoreboardManager : MonoBehaviourPunCallbacks
{
    public RectTransform scoreboardBackground;  
    public RectTransform scoreboardOutline;

    public RectTransform scoreboardPanel;
    public Vector3 targetPosition;
    private Vector3 originalPosition;
    public Button scoreboardButton;
    private bool isAtTarget = false; // 현재 목표 위치 여부
    public float moveSpeed = 5f; // 이동 속도
    private bool isMoving = false; // 이동 중인지 체크

    private int gameMode;
    public static ScoreboardManager instance;

    private Dictionary<Player, ScoreboardEntry> playerEntries = new Dictionary<Player, ScoreboardEntry>();
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        originalPosition = scoreboardPanel.anchoredPosition;
        if (PhotonNetwork.InRoom)
        {
            gameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"];
            AdjustScoreboardSize();
        }
    }

    //사이즈조절
    void AdjustScoreboardSize()
    {
        if (gameMode == 1) // 1:1 모드
        {
            scoreboardButton.gameObject.SetActive(false);
            scoreboardBackground.anchoredPosition = new Vector2(0, scoreboardBackground.anchoredPosition.y);
            scoreboardBackground.sizeDelta = new Vector2(540, scoreboardBackground.sizeDelta.y);

            scoreboardOutline.anchoredPosition = new Vector2(0, scoreboardOutline.anchoredPosition.y);
            scoreboardOutline.sizeDelta = new Vector2(520, scoreboardOutline.sizeDelta.y);
        }
        else if (gameMode == 2) // 2:2 모드 
        {
            scoreboardButton.gameObject.SetActive(true);
            scoreboardButton.onClick.AddListener(() => ToggleMoveUI());
            scoreboardBackground.anchoredPosition = new Vector2(123, scoreboardBackground.anchoredPosition.y);
            scoreboardBackground.sizeDelta = new Vector2(782, scoreboardBackground.sizeDelta.y); 

            scoreboardOutline.anchoredPosition = new Vector2(123, scoreboardOutline.anchoredPosition.y);
            scoreboardOutline.sizeDelta = new Vector2(752, scoreboardOutline.sizeDelta.y);
        }
    }
    public void Register(Player player, ScoreboardEntry entry)
    {
        if (!playerEntries.ContainsKey(player))
        {
            playerEntries.Add(player, entry);
        }
    }

    public ScoreboardEntry GetEntry(Player player)
    {
        if (playerEntries.ContainsKey(player))
            return playerEntries[player];
        return null;
    }

    public ScoreboardEntry GetLocalPlayerEntry()
    {
        return GetEntry(PhotonNetwork.LocalPlayer);
    }

    public void HideLocalScore()
    {
        GetLocalPlayerEntry()?.HideAll();
    }

    public void ShowAllScores()
    {
        foreach (var entry in playerEntries.Values)
        {
            entry.ShowAll();  // 알파값 0.5 처리 (ShowPreview용)
        }
    }

    public void ShowLocalScore()
    {
        GetLocalPlayerEntry()?.ShowAll();
    }

    public void HighlightLocalScore(string category)
    {
        GetLocalPlayerEntry()?.HighlightScore(category);
    }

    //점수동기화
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Score"))
        {
            ScoreboardEntry entry = GetEntry(targetPlayer);
            if (entry != null)
            {
                entry.UpdateScoreData(targetPlayer.CustomProperties);
                Debug.Log($"[동기화] {targetPlayer.NickName} 점수 업데이트됨!");
            }
            else
            {
                Debug.LogWarning($"[동기화 실패] {targetPlayer.NickName}의 ScoreboardEntry를 찾지 못함");
            }
        }
    }

    void ToggleMoveUI()
    {
        if (isMoving) return;
        isAtTarget = !isAtTarget;
        Coroutine coroutine = null;
        Vector3 destination = isAtTarget ? targetPosition : originalPosition;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(MoveUI(destination));
    }
    IEnumerator MoveUI(Vector3 destination)
    {
        isMoving = true;
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startPos = scoreboardPanel.anchoredPosition;
        while(elapsedTime < duration)
        {
            scoreboardPanel.anchoredPosition = Vector3.Lerp(startPos, destination, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        scoreboardPanel.anchoredPosition = destination;
        isMoving = false; // 이동 완료
    }

}
