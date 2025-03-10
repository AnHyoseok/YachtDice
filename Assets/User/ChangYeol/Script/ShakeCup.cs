using System.Collections;
using UnityEngine;

public class ShakeCup : MonoBehaviour
{
    #region Variables
    public BoxCollider box;
    public float minShakeAmount = 0.1f;
    public float maxShakeAmount = 0.3f;
    public float minShakeSpeed = 5f;
    public float maxShakeSpeed = 10f;
    public float shakeDurtion = 1.5f;

    private bool isShakeing = false;
    private float shakeSpeedX, shakeSpeedY, shakeSpeedZ;
    private float shakeAmountX, shakeAmountY, shakeAmountZ;
    #endregion
    public void ShakerCup()
    {
        if(!isShakeing)
        {
            StartCoroutine(ShakerRouine());
        }
    }
    private IEnumerator ShakerRouine()
    {
        isShakeing = true;
        box.isTrigger = false;
        float elapsed = 0f;
        Vector3 originalPosition = transform.position;

        // 랜덤한 속도 & 강도 생성
        shakeSpeedX = Random.Range(minShakeSpeed,maxShakeSpeed);
        shakeSpeedY = Random.Range(minShakeSpeed,maxShakeSpeed);
        shakeSpeedZ = Random.Range(minShakeSpeed, maxShakeSpeed);
        shakeAmountX = Random.Range(minShakeAmount,maxShakeAmount);
        shakeAmountY = Random.Range(minShakeAmount,maxShakeAmount);
        shakeAmountZ = Random.Range(minShakeAmount, maxShakeAmount);

        while (elapsed < shakeDurtion)
        {
            float offsetX = Mathf.Sin(Time.time * shakeSpeedX) * shakeAmountX;
            float offsetY = Mathf.Sin(Time.time * shakeSpeedY) * shakeAmountY;
            float offsetZ = Mathf.Sin(Time.time * shakeSpeedZ) * shakeAmountZ;
            transform.position = originalPosition + new Vector3(offsetX, offsetY, offsetZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        //box.isTrigger = true;
        isShakeing = false;
    }
}

