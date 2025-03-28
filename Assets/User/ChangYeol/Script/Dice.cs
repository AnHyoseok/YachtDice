using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Dice : MonoBehaviour
{
    #region Variables
    private PhotonView photonView;
    private Rigidbody rb;
    public Transform[] diceSides;
    public GameObject[] SelectUICavas;
    public float shakeForce = 2f;

    public Vector3 originPos;
    public bool isSelected = false;
    public float friction = 0.98f;
    public float stopThreshld = 0.05f;
    public float nudgeForce = 0.5f; // ¸ð¼­¸® ´ê¾ÒÀ» ¶§ ¾àÇÑ Èû Ãß°¡

    private bool isSliding = false;
    public int index = 0;
    #endregion

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        SetupDicePhysics(rb);
    }
    private void FixedUpdate()
    {
        //photonView.RPC("SyncDice", RpcTarget.All, transform.position, transform.rotation);
        if (isSliding && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, (1 - friction) * Time.deltaTime);
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, (1 - friction) * Time.deltaTime);
            if(rb.linearVelocity.magnitude < 0.2f && rb.linearVelocity.magnitude > stopThreshld)
            {
                rb.AddForce(Random.onUnitSphere * nudgeForce, ForceMode.Impulse);
            }
            if(rb.linearVelocity.magnitude < stopThreshld)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                isSliding = false;
                if(!photonView.IsMine)
                {
                    Debug.Log(123);
                    rb.isKinematic = true;
                }
                Debug.Log(GetDiceValue());
            }
        }
        NudgeDice();
    }
    public int GetDiceValue()
    {
        Transform upside = null;
        float maxDot = -1;
        foreach (Transform side in diceSides)
        {
            float dot = Vector3.Dot(side.up, Vector3.up);
            if (!(dot > maxDot)) continue;
            maxDot = dot;
            upside = side;
        }
        if (upside != null) return int.Parse(upside.name);
        return 1;
    }
    void SetupDicePhysics(Rigidbody rb)
    {
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Extrapolate;
        rb.linearDamping = 0.02f;
        rb.angularDamping = 0.02f;
    }
    public void NudgeDice()
    {
        if (rb.linearVelocity.magnitude > 0.05f || rb.angularVelocity.magnitude > 0.05f) return;

        float threshold = 0.85f;
        float edgeMin = 0.3f;
        float edgeMax = 0.6f;
        Vector3 up = transform.up;

        float[] dotValues =
        {
            Mathf.Abs(Vector3.Dot(up, Vector3.up)),
            Mathf.Abs(Vector3.Dot(up, Vector3.down)),
            Mathf.Abs(Vector3.Dot(up, Vector3.right)),
            Mathf.Abs(Vector3.Dot(up, Vector3.left)),
            Mathf.Abs(Vector3.Dot(up, Vector3.forward)),
            Mathf.Abs(Vector3.Dot(up, Vector3.back))
        };
        float maxDot = Mathf.Max(dotValues);
        bool isProperly = maxDot >= threshold;
        bool isEdge = maxDot >= edgeMin && maxDot <= edgeMax;

        if (isProperly) return;
        if (!isEdge) return;

        Vector3 smallForce = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
        rb.AddForce(smallForce, ForceMode.Impulse);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Dice"))
        {
            isSliding = true;
            //Debug.Log("¹Ì²ô·¯Áü");
        }
    }
}
