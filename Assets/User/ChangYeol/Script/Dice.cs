using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Dice : MonoBehaviour
{
    #region Variables
    private Rigidbody rb;
    private bool isRolling = false;
    public float shakeForce = 2f;
    public Transform cupPos;
    private float maxDistance = 0.5f;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupDicePhysics(rb);
    }
    public void RollDice()
    {
        if (isRolling) return;
        isRolling = true;

        //rb.linearVelocity = Vector3.zero;
        //rb.angularVelocity = Vector3.zero;
        if (Vector3.Distance (transform.position,cupPos.position) > maxDistance)
        {
            transform.position = cupPos.position + new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        Vector3 forceDirection = (cupPos.position - transform.position).normalized; //ÄÅ Áß¾ÓÀ» ÇâÇÏ´Â ¹æÇâ
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.3f,0.8f), Random.Range(-0.3f, 0.3f));
        rb.AddForce(forceDirection * shakeForce + randomOffset, ForceMode.Impulse);
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
        Vector3 up = transform.up;
        if (Vector3.Dot(up, Vector3.up) > 0.9f) return 6;
        if (Vector3.Dot(up, Vector3.down) > 0.9f) return 1;
        if (Vector3.Dot(up, Vector3.right) > 0.9f) return 4;
        if (Vector3.Dot(up, Vector3.left) > 0.9f) return 2;
        if (Vector3.Dot(up, Vector3.forward) > 0.9f) return 3;
        if (Vector3.Dot(up, Vector3.back) > 0.9f) return 5;
        return 0;
    }
    void SetupDicePhysics(Rigidbody rb)
    {
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
        rb.linearDamping = 1.5f;
        rb.angularDamping = 1f;
    }
}
