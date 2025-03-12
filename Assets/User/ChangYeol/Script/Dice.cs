using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.PlayerSettings;

public class Dice : MonoBehaviour
{
    #region Variables
    private Rigidbody rb;
    private bool isRolling = false;
    public float shakeForce = 2f;

    public float friction = 0.98f;
    public float stopThreshld = 0.05f;

    private bool isSliding = false;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupDicePhysics(rb);
    }
    private void FixedUpdate()
    {

        if(isSliding)
        {
            rb.linearVelocity *= friction;
            rb.angularVelocity *= friction;

            if(rb.linearVelocity.magnitude < stopThreshld)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                isSliding = false;
            }
        }
    }
    public void RollDice()
    {
        if (isRolling) return;
        isRolling = true;
        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.3f,0.8f), Random.Range(-0.5f, 0.5f));
        rb.AddForce(randomOffset * shakeForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * shakeForce, ForceMode.Impulse);
        Invoke("StopRolling", 2f);
    }
    void StopRolling()
    {
        isRolling = false;
        Debug.Log("Dice Stopped");
    }
    public int GetDiceValue()
    {
        if (rb.linearVelocity.magnitude > 0.1f) return 0;

        Vector3 up = transform.up;
        if (Vector3.Dot(up, Vector3.up) > 0.9f) return 6;
        if (Vector3.Dot(up, Vector3.down) > 0.9f) return 1;
        if (Vector3.Dot(up, Vector3.right) > 0.9f) return 2;
        if (Vector3.Dot(up, Vector3.left) > 0.9f) return 5;
        if (Vector3.Dot(up, Vector3.forward) > 0.9f) return 3;
        if (Vector3.Dot(up, Vector3.back) > 0.9f) return 4;
        return 0;
    }
    void SetupDicePhysics(Rigidbody rb)
    {
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
        rb.linearDamping = 1.5f;
        rb.angularDamping = 1f;
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground") /*&& rb.linearVelocity.magnitude < stopThreshld*/)
        {
            //isSliding = true;
            Debug.Log("afouhfo;;" + GetDiceValue());
        }
    }
}
