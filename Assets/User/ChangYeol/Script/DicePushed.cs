using UnityEngine;

/// <summary> </summary>
public class DicePushed : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Dice")
        {
            other.transform.position = transform.position;
        }
    }
}
