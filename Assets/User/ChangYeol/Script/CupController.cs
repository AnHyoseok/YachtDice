using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class CupController : MonoBehaviour
{
    #region Variables
    private PhotonView photonView;
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
        animator = GetComponent<Animator>();
        diceManager = DiceManager.Instance;
    }
    private void Update()
    {
        if (diceManager.isDiceArray || diceManager.rollsLeft == 0 || diceManager.isArrays)
        {
            return;
        }
        isShake = !Input.GetKey(KeyCode.LeftShift);
        if (!isShake)
        {
            timer += Time.deltaTime;
            if (timer >= pourOutTime)
            {
                isShake = true;
                timer = 0;
            }
        }
        else if (!button.isButton)
        {
            timer += Time.deltaTime;
            if (timer >= pourOutTime)
            {
                button.isButton = true;
                timer = 0;
            }
        }
        UpdateCupState();
    }
    void UpdateCupState()
    {
        AnimeCup();
        //photonView.RPC("AnimeCup", RpcTarget.All);
    }
    //[PunRPC]
    void AnimeCup()
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
        if (PhotonNetwork.IsMasterClient)
        {
            ObjectInstantiate();
        }
    }
    [PunRPC]
    public void DesDiceList()
    {
        for (int i = 0; i < selectDice.turnLimit - selectDice.movesThisTurn; i++)
        {
            Debug.Log("dddd");
            Destroy(falseDices[i]);
        }
        for (int i = 0; i < selectDice.turnLimit - selectDice.movesThisTurn; i++)
        {
            falseDices.RemoveAll(x => x != null);

            RandomPoition();
            Debug.Log("123132");

            diceManager.dices[i] = Dice.GetComponent<Dice>();
            Dice.name = $"Dice{i}";

        }
    }
    //[PunRPC]
    public void ObjectInstantiate()
    {
        photonView.RPC("DesDiceList", RpcTarget.MasterClient);
        
      
        diceManager.isArray = false;
        diceManager.isRotat = false;

        diceManager.rollsLeft--;

      
    }
    void RandomPoition()
    {
        boxGroup.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
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
    //[PunRPC]
    public void RandomDice()
    {
     
        if (!PhotonNetwork.IsMasterClient) return;
        boxGroup.SetActive(true);
        GetComponent<BoxCollider>().enabled = true;
        for (int i = 0; i < selectDice.turnLimit - selectDice.movesThisTurn; i++)
        {
            Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x, transform.position.x + 0.01f);
            Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            if (PhotonNetwork.IsMasterClient)
            {
                Forcedice = PhotonNetwork.Instantiate("Forcedice", offset, randomRot);
            }

            photonView.RPC("AddDiceList", RpcTarget.Others); 
        }
    }

    [PunRPC]
    public void AddDiceList()
    {
        falseDices.Add(Forcedice);
    }
}

