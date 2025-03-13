using UnityEngine;

public class DicePushed : MonoBehaviour
{
    public float forceMultiplier = 2f;
    public float maxSpeed = 5;
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Dice"))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            ContactPoint contact = collision.contacts[0];
            Vector3 forceDirection = -contact.normal;
            Vector3 force = forceDirection * forceMultiplier;

            rb.AddForce(force, ForceMode.Acceleration);

            if(rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                //Debug.Log(rb.linearVelocity);
            }
            //Debug.Log(rb.linearVelocity);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dice"))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        }
    }
}
