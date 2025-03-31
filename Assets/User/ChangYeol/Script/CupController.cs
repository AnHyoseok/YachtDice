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
    public float initialForce = 10f;
    public SelectDice selectDice;

    private DiceManager diceManager;
    private List<GameObject> falseDices = new List<GameObject>();
    private GameObject Dice;
    GameObject Forcedice;
    [HideInInspector] public Animator animator;
    private bool isShake = false;
    private float timer;
    public float pourOutTime = 5;

    private const string DiceCount = "DiceCount";
    private const string IsShake = "IsShake";
    private const string IsButton = "IsButton";
    #endregion
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        photonView.OwnershipTransfer = OwnershipOption.Request;
        animator = GetComponent<Animator>();
        diceManager = DiceManager.Instance;
    }
    private void Update()
    {

        if (!TurnManager.instance.IsMyTurn())
        {
            Debug.LogWarning("내 턴이 아니라서 조작 불가능!");
            return;
        }
        else
        {
            Debug.Log("내 턴입니다! 조작 가능!");
        }

        if (diceManager.isDiceArray  || diceManager.isArrays)
        {
            Debug.LogWarning($"주사위 조작 제한: isDiceArray={diceManager.isDiceArray}, rollsLeft={diceManager.rollsLeft}, isArrays={diceManager.isArrays}");
            return;
        }
        Debug.Log($"[흔들기 진입] isShake={isShake}, isButton={button.isButton}, timer={timer}");

        isShake = !Input.GetKey(KeyCode.LeftShift);
        if (!isShake )
        {
            timer += Time.deltaTime;
            Debug.Log($"[흔들림 대기] timer={timer}");
            if (timer >= pourOutTime)
            {
                isShake = true;
                timer = 0;
                Debug.Log("[흔들림 강제 시작]");
            }
        }
        else if (!button.isButton)
        {
            timer += Time.deltaTime;
            Debug.Log($"[버튼 대기] timer={timer}");
            if (timer >= pourOutTime)
            {
                button.isButton = true;
                timer = 0;
                Debug.Log("[버튼 활성화 완료]");
            }
        }
        UpdateCupState();
    }
  

    void UpdateCupState()
    {
        int ani = selectDice.turnLimit - selectDice.movesThisTurn;
        animator.SetInteger(DiceCount, ani);
        animator.SetBool(IsShake, isShake);
        animator.SetBool(IsButton, button.isButton);
      
        // 다른 유저한테 동기화
        photonView.RPC("SyncAnimatorState", RpcTarget.Others, ani, isShake, button.isButton);
    }


    [PunRPC]
void SyncAnimatorState(int ani, bool shake, bool isBtn)
{
    animator.SetInteger("DiceCount", ani);
    animator.SetBool("IsShake", shake);
    animator.SetBool("IsButton", isBtn);
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
        if (TurnManager.instance.IsMyTurn())
        {
            ObjectInstantiate();
        }
    }
    [PunRPC]
    public void DesDiceList(int diceID)
    {
        GameObject dice = PhotonView.Find(diceID).gameObject;
        if(falseDices.Contains(dice))
        {
            PhotonNetwork.Destroy(dice);
        }
    }
    [PunRPC]
 
    public void ObjectInstantiate()
    {
        if (!TurnManager.instance.IsMyTurn()) return;

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

        //  진짜 주사위 생성 및 배열 할당
        for (int i = 0; i < diceManager.dices.Length; i++)
        {
            Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x, transform.position.x + 0.01f);
            Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));

            GameObject diceObj = PhotonNetwork.Instantiate("Dice", offset, randomRot);
            diceManager.dices[i] = diceObj.GetComponent<Dice>();
            diceObj.name = $"Dice{i}";

            Rigidbody dicerb = diceObj.GetComponent<Rigidbody>();
            if (dicerb != null && wall != null)
            {
                Vector3 forceDirection = (wall.position - offset).normalized;
                dicerb.AddForce(forceDirection * initialForce, ForceMode.Impulse);
            }
        }

        // 상태 초기화
        diceManager.isArray = false;
        diceManager.isRotat = false;
        diceManager.rollsLeft--;
    }

    [PunRPC]
    void RandomPoition()
    {
        Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x, transform.position.x + 0.01f);
        Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        Dice = PhotonNetwork.Instantiate("Dice", offset, randomRot);
        Rigidbody dicerb = Dice.GetComponent<Rigidbody>();
        if (dicerb != null && wall != null)
        {
            Vector3 forceDirection = (wall.position - offset).normalized;
            dicerb.AddForce(forceDirection * initialForce, ForceMode.Impulse);
        }
        animator.SetInteger(DiceCount, 0);
    }
    [PunRPC]
    public void RandomDice()
    {
        if(TurnManager.instance.IsMyTurn())
        {
            photonView.RPC("BoxobjectActivetrue", RpcTarget.All);
            boxGroup.SetActive(true);
            GetComponent<BoxCollider>().enabled = true;
            for (int i = 0; i < selectDice.turnLimit - selectDice.movesThisTurn; i++)
            {
                Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x, transform.position.x + 0.01f);
                Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                Forcedice = PhotonNetwork.Instantiate("Forcedice", offset, randomRot);
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
    [PunRPC]
    public void AddDiceList(int diceID)
    {
        GameObject dice = PhotonView.Find(diceID).gameObject;
        if (!falseDices.Contains(dice))
        {
            falseDices.Add(dice);
        }
    }
}

