using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CupState
{
    Idle,
    Shake,
    PourOut
}
public class ShakeCup : MonoBehaviour
{
    #region Variables
    public CupState cupState = CupState.Idle;
    public GameObject boxGroup;
    public DiceManager diceManager;
    public Transform wall;
    public float initialForce = 10f;
    public SelectDice selectDice;

    private List<GameObject> falseDices = new List<GameObject>();
    private GameObject Dice;
    [HideInInspector]public Animator animator;
    private bool isShake = false;

    private const string DiceCount = "DiceCount";
    private const string IsShake = "IsShake";
    #endregion
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        isShake = !Input.GetKey(KeyCode.LeftShift);
        UpdateCupState();
    }
    void UpdateCupState()
    {
        switch(cupState)
        {
            case CupState.Idle :
                IdleState();
                break;
            case CupState.Shake :
                ShakeState();
                break;
            case CupState.PourOut :
                PourOutState();
                break;
        }
    }
    void IdleState()
    {
        int ani = diceManager.dices.Length <= 0 ? selectDice.turnLimit : diceManager.dices.Length;
        animator.SetInteger(DiceCount, ani);
        animator.SetBool(IsShake, isShake);
    }
    void ShakeState()
    {
        if(isShake)
        {
            cupState = CupState.PourOut;
        }
    }
    void PourOutState()
    {

    }
    public void ObjectInstantiate()
    {
        for(int i = 0; i < selectDice.turnLimit - SelectDice.movesThisTurn; i++)
        {
            Debug.Log(i);
            Destroy(falseDices[i]);
        }
        for (int i = 0; i < selectDice.turnLimit - SelectDice.movesThisTurn; i++)
        {
            falseDices.RemoveAll(x => x != null);
            RandomPoition();
            DiceManager.Instance.dices[i] = Dice.GetComponent<Dice>();
            Dice.name = $"Dice{i}";
        }
        if(selectDice.turnLimit - SelectDice.movesThisTurn != 5)
        {
            DiceManager.Instance.isArray = false;
        }
    }
    void RandomPoition()
    {
        boxGroup.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
        Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x,transform.position.x + 0.01f);
        Quaternion randomRot = Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
        Dice = Instantiate(diceManager.dice.gameObject, offset, randomRot);
        Rigidbody dicerb = Dice.GetComponent<Rigidbody>();
        if(dicerb != null && wall != null)
        {
            Vector3 forceDirection = (wall.position - offset).normalized;
            dicerb.AddForce(forceDirection * initialForce, ForceMode.Impulse);
        }
        animator.SetInteger(DiceCount, 0);
        cupState = CupState.Idle;
    }
    public void RandomDice()
    {
        boxGroup.SetActive(true);
        GetComponent<BoxCollider>().enabled = true;
        for(int i = 0; i < selectDice.turnLimit - SelectDice.movesThisTurn; i++)
        {
            Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x, transform.position.x + 0.01f);
            Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            GameObject Forcedice = Instantiate(diceManager.dice.gameObject, offset, randomRot);
            falseDices.Add(Forcedice);
        }
    }
}

