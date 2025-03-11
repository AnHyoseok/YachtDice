using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Dice : MonoBehaviour
{
    #region Variables
    private Rigidbody rb;
    private bool isRolling = false;
    public float shakeForce = 2f;
    public Transform cupPos;
    private float bounceForce = 0.5f;
    public string cupTag = "Cup"; // 컵의 태그 설정
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
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(cupTag))
        {
            Vector3 normal = collision.contacts[0].normal;
            Vector3 oppsiteForce = -normal * bounceForce;

            rb.AddForce(oppsiteForce, ForceMode.Impulse);
        }
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
