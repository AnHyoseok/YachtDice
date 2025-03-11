using UnityEngine;

public class CollisionCup : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (collision.gameObject.CompareTag("Dice"))
        {
            Debug.Log("¡÷ªÁ¿ß");

            Vector3 normal = collision.contacts[0].normal;
            Vector3 oppsiteForce = -normal * 2;

            rb.AddForce(oppsiteForce, ForceMode.Impulse);
        }
    }
}
