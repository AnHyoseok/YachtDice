using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager instance;
    private PhotonView pv;
    public CupController cupController;
    public List<Player> playersInRoom = new List<Player>();
    public TextMeshProUGUI currentturnText;
    public int currentPlayerIndex = 0;
    public int currentTurnRound = 0;
    public const int MAX_ROUNDS = 12;

    public GameObject turnAlarm;
    public TextMeshProUGUI UsernameText;
    public float popupDuration = 15f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        cupController = FindAnyObjectByType<CupController>();
        currentturnText.text = currentTurnRound.ToString() + "/12";
        StartCoroutine(WaitAndInitPlayers());
    }

    private IEnumerator WaitAndInitPlayers()
    {
        while (GameSceneManager.Instance == null || PhotonNetwork.PlayerList.Length == 0)
        {
            yield return null;
        }

        while (cupController == null)
        {
            cupController = FindAnyObjectByType<CupController>();
            yield return null;
        }

        //  TurnIndex가 모든 플레이어에게 할당될 때까지 대기
        while (!AllPlayersHaveTurnIndex())
        {
            //Debug.Log("TurnIndex가 아직 안 들어온 플레이어가 있습니다. 대기 중...");
            yield return null;
        }

        playersInRoom = PhotonNetwork.PlayerList
            .OrderBy(p => (int)p.CustomProperties["TurnIndex"])
            .ToList();
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AIPlayers"))
        {
            string[] aiNames = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            foreach (string aiName in aiNames)
            {
                playersInRoom.Add(null); // AI는 null 슬롯으로 표시
            }
        }
        if (playersInRoom.Count == 0)
        {
            //Debug.LogError("플레이어 리스트를 가져오지 못했습니다.");
            yield break;
        }

        //Debug.Log("TurnManager: TurnIndex 기준으로 플레이어 정렬 완료");

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateTurn(0, 0);
            GameSceneManager.Instance.BroadcastTurn(0, 0);
        }
    }

    private bool AllPlayersHaveTurnIndex()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey("TurnIndex"))
                return false;
        }
        return true;
    }
    public bool IsAITurnNow()
    {
        if (playersInRoom == null || playersInRoom.Count == 0)
            return false;

        if (currentPlayerIndex < 0 || currentPlayerIndex >= playersInRoom.Count)
            return false;

        Player currentPlayer = playersInRoom[currentPlayerIndex];

        // AI는 PhotonNetwork에 없는 유저
        return currentPlayer == null || !PhotonNetwork.PlayerList.Contains(currentPlayer);

    }
    public bool IsMyTurn()
    {
        if (IsAITurnNow()) return false;
        if (playersInRoom == null || playersInRoom.Count == 0) return false;

        if (currentPlayerIndex < 0 || currentPlayerIndex >= playersInRoom.Count)
        {
            //Debug.LogWarning($"currentPlayerIndex({currentPlayerIndex})가 플레이어 수({playersInRoom.Count})를 벗어났습니다.");
            return false;
        }

      

        var current = playersInRoom[currentPlayerIndex];
        //Debug.Log($"내 이름: {PhotonNetwork.LocalPlayer.NickName}, 현재 턴 플레이어: {current.NickName}");

        return PhotonNetwork.LocalPlayer == current;

    }

    public Player GetCurrentPlayer()
    {
        return playersInRoom[currentPlayerIndex];
    }

    public void EndMyTurn()
    {


        //Debug.Log(" EndMyTurn() 호출됨");
        //Debug.Log($" GameSceneManager.Instance == null ? {GameSceneManager.Instance == null}");
        //if (!PhotonNetwork.IsMasterClient) return;
        int nextIndex = currentPlayerIndex + 1;
        int nextRound = currentTurnRound;
        //Debug.Log($"현재 인덱스{nextIndex-1}");
        if (nextIndex >= playersInRoom.Count)
        {
            nextIndex = 0;
            nextRound++;

            //Debug.Log($"다음 인덱스{nextIndex}");
            if (nextRound >= MAX_ROUNDS)
            {
               GameResultManager.Instance.StartResultSequence();
                //Debug.Log("게임 종료!");
                return;
            }

            //Debug.Log($"다음 라운드 시작: {nextRound + 1}턴째");
        }

        // 본인만 즉시 적용
        UpdateTurn(nextIndex, nextRound);
        // 전체에 동기화 (RPC는 GameSceneManager에서 관리)
        GameSceneManager.Instance.BroadcastTurn(nextIndex, nextRound);
        if (IsAITurnNow())
        {
            //Debug.Log("[AI] 자동 턴 시작");
            StartCoroutine(AI_TurnRoutine());
        }
    }

    public void UpdateTurn(int playerIndex, int round)
    {
        //Debug.Log("UpdateTurn() 호출됨");

        currentPlayerIndex = playerIndex;
        currentTurnRound = round;

        if (DiceManager.Instance != null && cupController != null)
        {
            DiceManager.Instance.rollsLeft = 3;
            DiceManager.Instance.UpdataRollsLeft();
            DiceManager.Instance.isDiceArray = false;
            DiceManager.Instance.isArrays = false;
            cupController.StartCupState(true);
            DiceManager.Instance.selectDice.movesThisTurn = 0;
            DiceManager.Instance.selectDice.currentTargetIndex = 0;
            DiceManager.Instance.selectDice.selectDiceObject = null;
            DiceManager.Instance.selectDice.foundEmptyPosition = false;
            for (int i = 0; i < DiceManager.Instance.selectDice.isTarget.Length; i++)
            {
                DiceManager.Instance.selectDice.isTarget[i] = false;
            }
        }
       
        currentturnText.text = $"{currentTurnRound + 1} / {MAX_ROUNDS}";
        
        if (IsAITurnNow())
        {
            string aiName = GetCurrentAIName();
            int actorNumber = aiName.GetHashCode();

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(aiName, out object rawProps)
                && rawProps is ExitGames.Client.Photon.Hashtable aiProps)
            {
                if (GameSceneManager.Instance.scoreboardEntries.TryGetValue(actorNumber, out var aiEntry))
                {
                    aiEntry.UpdateScoreData(aiProps);
                    //Debug.Log($"[AI] 점수 복원 완료: {aiName}");
                }
            }
        }
        if (IsAITurn())
        {
            //Debug.Log("AI 턴 감지! AI 자동 행동 시작");
            StartCoroutine(AI_TurnRoutine());
        }
        foreach (var entry in FindObjectsByType<ScoreboardEntry>(FindObjectsSortMode.None))
        {
            if (entry != null)
                entry.ClearHighlight();
        }

        // 알림은 RPC로 실행
        if (PhotonNetwork.LocalPlayer == TurnManager.instance.GetCurrentPlayer())
        {
            //Debug.Log($"{PhotonNetwork.LocalPlayer}=현재 유저 ");

            cupController.photonView.RequestOwnership();
            //Debug.Log($"[요청] CupController 소유권 요청: {PhotonNetwork.LocalPlayer.NickName}");

            StartCoroutine(DelayedOwnershipCheck());
        }
        //Debug.Log($"현재 턴: {round + 1}턴 - 플레이어: {GetCurrentPlayer()?.NickName}");
    }
    public bool IsAITurn()
    {
        return !PhotonNetwork.LocalPlayer.IsMasterClient && GetCurrentPlayer() == null;
    }
    private IEnumerator AI_TurnRoutine()
    {
        //Debug.Log("[AI] 턴 시작 - 컵 흔들기 시작");
        cupController.isShake = true;
        yield return new WaitForSeconds(2f);
        if(cupController.isShake)
        { 
            cupController.button.isButton = false;
            cupController.UpdateCupState();
            AudioController.instance.PlayCupShake();
        }


        yield return new WaitForSeconds(2f); // 컵 흔들기 연출

        cupController.button.isButton = true;
        cupController.UpdateCupState();

        

        yield return new WaitForSeconds(0.2f);

        //  진짜 주사위 생성 요청 (기존 플레이어와 동일)
        cupController.photonView.RPC("RPC_RequestDiceSpawn", RpcTarget.MasterClient);

        float waitTime = 0f;
        while (!DiceManager.Instance.isDiceArray && waitTime < 10f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        yield return null;
        yield return new WaitForSeconds(2f);

        //점수 텍스트
     
        AudioController.instance.PlayarrayDice(); 
        DiceManager.Instance.ShowPreviewScore();
       
        string bestCategory = FindBestScoreCategory();
    
        int score = DiceManager.Instance.CalculateScore(bestCategory, previewOnly: true);
      
        yield return new WaitForSeconds(1f);
        
        string aiName = TurnManager.instance.GetCurrentAIName();
        int actorNumber = aiName.GetHashCode();

        if (GameSceneManager.Instance.scoreboardEntries.TryGetValue(actorNumber, out var aiEntry))
        {
            yield return new WaitForSeconds(0.5f);
            
            aiEntry.UpdateScore(bestCategory, score);
            aiEntry.ClearHighlight();
            //Debug.Log($"[AI] 점수 선택 완료: {bestCategory} = {score}");
            AudioController.instance.PlayselectScore();
        }
        else
        {
            //Debug.LogWarning($"[AI_TurnRoutine] AI 점수판 못 찾음: {aiName} / hash = {actorNumber}");
        }

        TurnManager.instance.EndMyTurn();
    }

    private string FindBestScoreCategory()
    {
        var lowerCategories = new string[]
        {
        DiceScore.YAHTZEE, DiceScore.FOUR_KIND, DiceScore.FULL_HOUSE,
        DiceScore.SMALL_STRAIGHT, DiceScore.LARGE_STRAIGHT
        };

        var allCategories = new string[]
        {
        DiceScore.YAHTZEE, DiceScore.FOUR_KIND, DiceScore.FULL_HOUSE,
        DiceScore.SMALL_STRAIGHT, DiceScore.LARGE_STRAIGHT, DiceScore.Choice,
        DiceScore.ONES, DiceScore.TWOS, DiceScore.THREES, DiceScore.FOURS,
        DiceScore.FIVES, DiceScore.SIXES
        };

        string aiName = TurnManager.instance.GetCurrentAIName();
        int actorNumber = aiName.GetHashCode();

        if (!GameSceneManager.Instance.scoreboardEntries.TryGetValue(actorNumber, out var aiEntry))
        {
            //Debug.LogWarning($"[FindBestScoreCategory] AI 점수판을 찾을 수 없음: {aiName}");
            return DiceScore.Choice;
        }

        int[] values = DiceManager.Instance.GetDiceValues().Concat(DiceManager.Instance.GetDiceValue()).ToArray();
        int[] counts = new int[7];
        foreach (int v in values) counts[v]++;

        // 1. 하단 항목 중 점수가 발생한 항목 우선
        string bestLower = "";
        int maxLowerScore = -1;
        foreach (string category in lowerCategories)
        {
            int index = aiEntry.GetCategoryIndex(category);
            if (index != -1 && aiEntry.IsAlreadyScored(index)) continue;

            int score = DiceManager.Instance.CalculateScore(category, previewOnly: true);
            if (score > 0 && score > maxLowerScore)
            {
                maxLowerScore = score;
                bestLower = category;
            }
        }

        if (!string.IsNullOrEmpty(bestLower))
        {
            //Debug.Log($"[AI 전략] 하단 점수 우선 기입: {bestLower} = {maxLowerScore}");
            return bestLower;
        }

        // 2. 상단에서 가장 많이 나온 숫자
        string bestUpper = "";
        int mostCount = 0;
        for (int i = 1; i <= 6; i++)
        {
            string category = i switch
            {
                1 => DiceScore.ONES,
                2 => DiceScore.TWOS,
                3 => DiceScore.THREES,
                4 => DiceScore.FOURS,
                5 => DiceScore.FIVES,
                6 => DiceScore.SIXES,
                _ => DiceScore.ONES
            };

            int index = aiEntry.GetCategoryIndex(category);
            if (!aiEntry.IsAlreadyScored(index) && counts[i] > mostCount)
            {
                mostCount = counts[i];
                bestUpper = category;
            }
        }

        if (!string.IsNullOrEmpty(bestUpper))
        {
            //Debug.Log($"[AI 전략] 상단 최다 수 선택: {bestUpper}");
            return bestUpper;
        }

        // 3. Choice 조건
        int choiceScore = DiceManager.Instance.CalculateScore(DiceScore.Choice, previewOnly: true);
        int choiceIndex = aiEntry.GetCategoryIndex(DiceScore.Choice);
        if (!aiEntry.IsAlreadyScored(choiceIndex) && choiceScore >= 25)
        {
            //Debug.Log($"[AI 전략] Choice 점수 25 이상 선택: {choiceScore}");
            return DiceScore.Choice;
        }

        // 4. 나머지 점수 높은 항목
        string bestCategory = "";
        int maxScore = -1;
        foreach (string category in allCategories)
        {
            int index = aiEntry.GetCategoryIndex(category);
            if (index != -1 && aiEntry.IsAlreadyScored(index)) continue;

            int score = DiceManager.Instance.CalculateScore(category, previewOnly: true);
            if (score > maxScore)
            {
                maxScore = score;
                bestCategory = category;
            }
        }

        //Debug.Log($"[AI 전략] 일반 점수 최대 선택: {bestCategory} = {maxScore}");
        return bestCategory;
    }



    private IEnumerator DelayedOwnershipCheck()
    {
        yield return new WaitForSeconds(0.5f);
        //Debug.Log($"[체크] CupController의 PhotonView 소유권: IsMine = {cupController.photonView.IsMine}, Owner = {cupController.photonView.Owner.NickName}");
    }
    [PunRPC]
    private void ShowTurnPopupRPC(string playerName)
    {
        if (turnAlarm != null && UsernameText != null)
        {
            UsernameText.text = $"{playerName}'S";
            turnAlarm.SetActive(true);
            CancelInvoke(nameof(HideTurnPopup));
            Invoke(nameof(HideTurnPopup), popupDuration);
        }
    }

    private void HideTurnPopup()
    {
        if (turnAlarm != null)
        {
            turnAlarm.SetActive(false);
        }
    }
    public string GetCurrentAIName()
    {
        if (IsAITurnNow())
        {
            string[] aiNames = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            return aiNames[currentPlayerIndex - PhotonNetwork.PlayerList.Length];
        }
        return null;
    }
}
