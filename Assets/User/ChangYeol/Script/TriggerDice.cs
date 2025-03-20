using System.Collections.Generic;
using UnityEngine;

public class TriggerDice : MonoBehaviour
{
    public int diceValue = 0;
    private Dictionary<int, float> faceAreas = new Dictionary<int, float>();
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == ("Ground"))
        {
            int faceNumber = GetFaceNumberFromName(gameObject.name);
            float faceArea = other.bounds.size.x * other.bounds.size.z;

            if(!faceAreas.ContainsKey(faceNumber))
            {
                faceAreas.Add(faceNumber, faceArea);
            }
            else
            {
                faceAreas[faceNumber] += faceArea;
            }
            int bestFace = diceValue;
            float maxArea = faceAreas.ContainsKey(diceValue) ? faceAreas[diceValue] : 0;

            foreach (var face in faceAreas)
            {
                if (face.Value > maxArea)
                {
                    maxArea = face.Value;
                    bestFace = face.Key;
                }
            }

            diceValue = bestFace;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == ("Ground"))
        {
            diceValue = 0;
        }
    }
    private int GetFaceNumberFromName(string faceName)
    {
        return int.Parse(faceName.Replace("DiceFace_", ""));
    }
}
