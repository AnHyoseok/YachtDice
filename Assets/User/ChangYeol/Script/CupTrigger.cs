using UnityEngine;

public class CupTrigger : MonoBehaviour
{
    public float forcePower = 5;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dice"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 pushDirection = (other.transform.position - transform.position).normalized;
                rb.AddForce(pushDirection * forcePower, ForceMode.Impulse); // 주사위를 멀리 보내기
            }
        }
    }
}
