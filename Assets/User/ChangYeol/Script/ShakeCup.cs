using System.Collections;
using UnityEngine;

public class ShakeCup : MonoBehaviour
{
    #region Variables
    public DiceManager diceManager;
    private GameObject Dice;
    public Transform wall;
    public float initialForce = 10f;

    //public float minShakeAmount = 0.1f;
    //public float maxShakeAmount = 0.3f;
    //public float minShakeSpeed = 5f;
    //public float maxShakeSpeed = 10f;
    //public float shakeDurtion = 1.5f;

    private bool isShakeing = false;

    //private float shakeSpeedX, shakeSpeedY, shakeSpeedZ;
    //private float shakeAmountX, shakeAmountY, shakeAmountZ;
    #endregion
    public void ShakerCup()
    {
        if(!isShakeing)
        {
            //StartCoroutine(ShakerRouine());
        }
    }
    /*private IEnumerator ShakerRouine()
    {
        isShakeing = true;
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
        isShakeing = false;
    }*/
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (collision.gameObject.CompareTag("Dice"))
        {
            Debug.Log("주사위");

            Vector3 normal = collision.contacts[0].normal;
            Vector3 oppsiteForce = -normal * 2;

            rb.AddForce(oppsiteForce, ForceMode.Impulse);
        }
    }
    public void ObjectInstantiate()
    {
        for (int i = 0; i < 5; i++)
        {
            RandomPoition();
        }
    }
    void RandomPoition()
    {
        Vector3 offset = diceManager.GetUniqueRandomPosition(transform.position.x,transform.position.x + 0.01f);
        Quaternion randomRot = Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
        Dice = Instantiate(diceManager.dice.gameObject, offset, randomRot);
        diceManager.dicelist.Add(Dice.GetComponent<Dice>());
        Rigidbody dicerb = Dice.GetComponent<Rigidbody>();
        if(dicerb != null && wall != null)
        {
            Vector3 forceDirection = (wall.position - offset).normalized;
            dicerb.AddForce(forceDirection * initialForce, ForceMode.Impulse);
            Debug.Log("wall방향으로");
        }
    }
}

