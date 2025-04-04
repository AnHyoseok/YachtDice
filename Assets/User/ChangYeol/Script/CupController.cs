using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CupController : MonoBehaviour
{
    #region Variables
    public PhotonView photonView;
    public ButtonController button;
    public GameObject boxGroup;
    public Transform wall;
    public Transform[] SpwanPos;
    public float initialForce = 10f;
    public SelectDice selectDice;

    private DiceManager diceManager;
    private List<GameObject> falseDices = new List<GameObject>();
    private GameObject Dice;
    private GameObject Forcedice;
    [HideInInspector] public Animator animator;
    public bool isShake = false;
    private float timer;
    public float pourOutTime = 5;
    private CameraMove cameraMove;

    public const string DiceCount = "DiceCount";
    public const string IsButton = "IsButton";
    #endregion
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        photonView.OwnershipTransfer = OwnershipOption.Request;
        animator = GetComponent<Animator>();
        diceManager = DiceManager.Instance;
        cameraMove = Camera.main.GetComponent<CameraMove>();
        if (photonView.IsMine && cameraMove.isStop)
        {
            UpdateCupState(); // 게임 시작 시 초기 상태 동기화
        }
    }
    private void Update()
    {
        if (diceManager.isDiceArray || diceManager.isArrays || diceManager.rollsLeft == 0 || !cameraMove.isStop) return;
        //Debug.Log($"[흔들기 진입] isShake={isShake}, isButton={button.isButton}, timer={timer}");
        if (!button.isButton)
        {
            timer += Time.deltaTime;
            //Debug.Log($"[버튼 대기] timer={timer}");
            if (timer >= pourOutTime)
            {
                button.isButton = true;
               
                timer = 0;
                //Debug.Log("[버튼 활성화 완료]");

            }
        }
        UpdateCupState();

       
           
        
    }
   public void UpdateCupState()
    {
        if (TurnManager.instance.IsMyTurn() || TurnManager.instance.IsAITurnNow())
        {
            
            int ani = selectDice.turnLimit - selectDice.movesThisTurn;
            photonView.RPC("SyncAnimatorState", RpcTarget.All, ani, button.isButton);
        }
    }
    [PunRPC]
    void SyncAnimatorState(int ani, bool isBtn)
    {
        animator.SetInteger(DiceCount, ani);
        animator.SetBool(IsButton, isBtn);
    }
    public void StartCupState(bool isStart)
    {
        if (TurnManager.instance.IsMyTurn() || TurnManager.instance.IsAITurnNow())
        {
       
            DiceManager.Instance.scoreText.text = "";
            photonView.RPC("StartSyncAnimatorState", RpcTarget.All, isStart);
        }
    }
    [PunRPC]
    void StartSyncAnimatorState(bool isStart)
    {
        animator.SetBool("IsStart", isStart);
    }
    public void TryRollDice()
    {
        if (TurnManager.instance.IsMyTurn() && DiceManager.Instance.rollsLeft >= 0)
        {
            photonView.RPC("RPC_RequestDiceSpawn", RpcTarget.MasterClient);
        }
    }
    [PunRPC]
    public void RPC_RequestDiceSpawn()
    {
        if (TurnManager.instance.IsMyTurn() )
        {
            ObjectInstantiate();
        }
    }
    [PunRPC]
    public void DesDiceList(int diceID)
    {
        GameObject dice = PhotonView.Find(diceID).gameObject;
        if (falseDices.Contains(dice))
        {
            PhotonNetwork.Destroy(dice);
        }
    }
    [PunRPC]
    public void ObjectInstantiate()
    {
        if (!TurnManager.instance.IsMyTurn() && !TurnManager.instance.IsAITurnNow()) return;
        
        //소리 스탑 
        AudioController.instance.StopCupShake();

        boxGroup.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;

        //  가짜 주사위 먼저 정리
        for (int i = 0; i < falseDices.Count; i++)
        {
            if (falseDices[i] != null)
            {
                PhotonNetwork.Destroy(falseDices[i]);
            }
        }
        falseDices.Clear();
        //주사위굴리기사운드
        AudioController.instance.PlayDiceRoll();

        //  진짜 주사위 생성 및 배열 할당
        for (int i = 0; i < diceManager.dices.Length; i++)
        {
            Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            GameObject diceObj = PhotonNetwork.Instantiate("Dice", SpwanPos[i].position, randomRot);
            diceManager.dices[i] = diceObj.GetComponent<Dice>();
            diceObj.name = $"Dice{i}";

            Rigidbody dicerb = diceObj.GetComponent<Rigidbody>();
            if (dicerb != null && wall != null)
            {
                Vector3 forceDirection = (wall.position - SpwanPos[i].position).normalized;
                dicerb.AddForce(forceDirection * initialForce, ForceMode.Impulse);
            }
        }

        // 상태 초기화
        diceManager.isArray = false;
        StartCupState(false);
        isShake = false;
        diceManager.rollsLeft-- ;
        diceManager.UpdataRollsLeft();
        
    }
    [PunRPC]
    void RandomPoition(int index)
    {
        Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        Dice = PhotonNetwork.Instantiate("Dice", SpwanPos[index].position, randomRot);
        Rigidbody dicerb = Dice.GetComponent<Rigidbody>();
        if (dicerb != null && wall != null)
        {
            Vector3 forceDirection = (wall.position - SpwanPos[index].position).normalized;
            dicerb.AddForce(forceDirection * initialForce, ForceMode.Impulse);
        }
        animator.SetInteger(DiceCount, 0);
    }
    [PunRPC]
    public void RandomDice()
    {
        if (TurnManager.instance.IsMyTurn() || TurnManager.instance.IsAITurnNow()) //  AI도 허용
        {
            photonView.RPC("BoxobjectActivetrue", RpcTarget.All);
            boxGroup.SetActive(true);
            GetComponent<BoxCollider>().enabled = true;

            for (int i = 0; i < selectDice.turnLimit - selectDice.movesThisTurn; i++)
            {
                Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                Forcedice = PhotonNetwork.Instantiate("Forcedice", SpwanPos[i].position, randomRot);
                photonView.RPC("AddDiceList", RpcTarget.All, Forcedice.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    [PunRPC]
    void BoxobjectActivetrue()
    {
        for (int i = 0; i < diceManager.boxobject.Length; i++)
        {
            diceManager.boxobject[i].SetActive(true);
        }
    }
    public void PlayShake()
    {
        if( !TurnManager.instance.IsAITurn()) AudioController.instance.PlayCupShake();

    }
    [PunRPC]
    public void AddDiceList(int diceID)
    {
        GameObject dice = PhotonView.Find(diceID).gameObject;
        if (!falseDices.Contains(dice))
        {
            falseDices.Add(dice);
        }
    }
    public void IsShaked()
    {
        isShake = true;
        
    }
}