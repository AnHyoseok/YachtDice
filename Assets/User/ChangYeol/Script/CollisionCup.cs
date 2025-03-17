using UnityEngine;

public class CollisionCup : MonoBehaviour
{
    public float maxForce = 5f;
    public float minForce = 2f;
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null && collision.gameObject.CompareTag("Dice"))
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 bounceDirection = Vector3.Reflect(rb.linearVelocity, contact.normal);
            float impactForce = collision.relativeVelocity.magnitude;
            float dynamicBounceForce = Mathf.Clamp(impactForce * 2f, minForce, maxForce);
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(bounceDirection.normalized * dynamicBounceForce, ForceMode.Impulse);

            float maxVelocity = 5f;
            if(rb.linearVelocity.magnitude > maxVelocity)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
            }

            //Debug.Log("º®¿¡ ºÎ‹HÈû");
        }
    }
}
