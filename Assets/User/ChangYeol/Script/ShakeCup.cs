using System.Collections;
using UnityEngine;

public class ShakeCup : MonoBehaviour
{
    #region Variables
    public GameObject boxGroup;
    public DiceManager diceManager;
    private GameObject Dice;
    public Transform wall;
    public float initialForce = 10f;
    #endregion
    public void ObjectInstantiate(int index)
    {
        for (int i = 0; i < index; i++)
        {
            RandomPoition();
        }
    }
    void RandomPoition()
    {
        boxGroup.SetActive(false);
        GetComponent<BoxCollider>().enabled = false;
        Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x,transform.position.x + 0.01f);
        Quaternion randomRot = Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
        Dice = Instantiate(diceManager.dice.gameObject, offset, randomRot);
        diceManager.dicelist.Add(Dice.GetComponent<Dice>());
        Rigidbody dicerb = Dice.GetComponent<Rigidbody>();
        if(dicerb != null && wall != null)
        {
            Vector3 forceDirection = (wall.position - offset).normalized;
            dicerb.AddForce(forceDirection * initialForce, ForceMode.Impulse);
            //Debug.Log("wall방향으로");
        }
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
        }
    }
}

