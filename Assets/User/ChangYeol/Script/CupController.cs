using System.Collections.Generic;
using UnityEngine;

public class CupController : MonoBehaviour
{
    #region Variables
    public ButtonController button;
    public GameObject boxGroup;
    public Transform wall;
    public float initialForce = 10f;
    public SelectDice selectDice;

    private DiceManager diceManager;
    private List<GameObject> falseDices = new List<GameObject>();
    private GameObject Dice;
    [HideInInspector]public Animator animator;
    private bool isShake = false;
    private float timer;
    public float pourOutTime = 5;

    private const string DiceCount = "DiceCount";
    private const string IsShake = "IsShake";
    private const string IsButton = "IsButton";
    #endregion
    private void Start()
    {
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
        if (!isShake )
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
        int ani = selectDice.turnLimit - SelectDice.movesThisTurn;
        animator.SetInteger(DiceCount, ani);
        animator.SetBool(IsShake, isShake);
        animator.SetBool(IsButton, button.isButton);
    }
    public void ObjectInstantiate()
    {
        for(int i = 0; i < selectDice.turnLimit - SelectDice.movesThisTurn; i++)
        {
            Destroy(falseDices[i]);
        }
        for (int i = 0; i < selectDice.turnLimit - SelectDice.movesThisTurn; i++)
        {
            falseDices.RemoveAll(x => x != null);
            RandomPoition();
            diceManager.dices[i] = Dice.GetComponent<Dice>();
            Dice.name = $"Dice{i}";
        }
        if(selectDice.turnLimit - SelectDice.movesThisTurn != 5)
        {
            diceManager.isArray = false;
            diceManager.isRotat = false;
        }
        diceManager.rollsLeft--;
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

