using System.Collections;
using UnityEngine;

public class TriggerDice : MonoBehaviour
{
    public int diceValue = 0;
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == ("Ground"))
        {
            string faceName = gameObject.name;
            diceValue = int.Parse(faceName.Replace("DiceFace_", ""));

            Debug.Log(diceValue);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == ("Ground"))
        {
            diceValue = 0;
        }
    }
}
