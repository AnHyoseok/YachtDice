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
        if (diceManager.isDiceArray || diceManager.rollsLeft == 0 || diceManager.isArrays)
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
    }
    public void TryRollDice()
    {

        if (TurnManager.instance.IsMyTurn() && DiceManager.Instance.rollsLeft > 0)
        {
            photonView.RPC("RPC_RequestDiceSpawn", RpcTarget.MasterClient);
        }
    }
    [PunRPC]
    public void RPC_RequestDiceSpawn()
    {
        if (!TurnManager.instance.IsMyTurn())
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
        if(TurnManager.instance.IsMyTurn())
        {
            boxGroup.SetActive(false);
            GetComponent<BoxCollider>().enabled = false;
            for (int i = 0; i < diceManager.dices.Length; i++)
            {
                if (i <= diceManager.dices.Length)
                {
                    photonView.RPC("DesDiceList", RpcTarget.MasterClient, falseDices[i].GetComponent<PhotonView>().ViewID);
                    falseDices[i] = null;
                }
                photonView.RPC("RandomPoition", RpcTarget.MasterClient);
                diceManager.dices[i] = Dice.GetComponent<Dice>();
                Dice.name = $"Dice{i}";
            }
            falseDices.RemoveAll(x => x == null);
            diceManager.isArray = false;
            diceManager.isRotat = false;
            diceManager.rollsLeft--;
        }
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
                photonView.RPC("AddDiceList", RpcTarget.MasterClient, Forcedice.GetComponent<PhotonView>().ViewID);
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

