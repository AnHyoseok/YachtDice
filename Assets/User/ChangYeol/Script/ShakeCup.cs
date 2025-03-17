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

    private List<GameObject> falseDices = new List<GameObject>();
    private GameObject Dice;
    [HideInInspector]public Animator animator;
    private int diceCount = 5;
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
        int ani = diceManager.dicelist.Count <= 0 ? diceCount : diceManager.dicelist.Count;
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
    public void ObjectInstantiate(int index)
    {
        for (int i = 0; i < index; i++)
        {
            RandomPoition();
        }
    }
    void RandomPoition()
    {
        foreach(GameObject game in falseDices)
        {
            Destroy(game);
        }
        boxGroup.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
        Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x,transform.position.x + 0.01f);
        Quaternion randomRot = Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
        Dice = Instantiate(diceManager.dice.gameObject, offset, randomRot);
        //if(diceManager.dicelist.Count > 0)
        //{
        //    for(int i = 0; i < diceManager.dicelist.Count;i++)
        //    {
        //        Destroy(diceManager.dicelist[i].gameObject);
        //        diceManager.dicelist.RemoveAt(i);
        //    }
        //}
        diceManager.dicelist.Add(Dice.GetComponent<Dice>());
        Rigidbody dicerb = Dice.GetComponent<Rigidbody>();
        if(dicerb != null && wall != null)
        {
            Vector3 forceDirection = (wall.position - offset).normalized;
            dicerb.AddForce(forceDirection * initialForce, ForceMode.Impulse);
            //Debug.Log("wall방향으로");
        }
        animator.SetInteger(DiceCount, 0);
        cupState = CupState.Idle;
    }
    public void RandomDice(int index)
    {
        boxGroup.SetActive(true);
        GetComponent<BoxCollider>().enabled = true;
        for(int i = 0; i < index; i++)
        {
            Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x, transform.position.x + 0.01f);
            Quaternion randomRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            GameObject Forcedice = Instantiate(diceManager.dice.gameObject, offset, randomRot);
            falseDices.Add(Forcedice);
        }
    }
}

