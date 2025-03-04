using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPun
{
    public float moveSpeed = 5f;

    void Update()
    {
        if (!photonView.IsMine) return; // 본인 플레이어만 조작 가능

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime);
    }
}
