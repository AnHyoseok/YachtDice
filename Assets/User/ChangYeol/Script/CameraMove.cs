using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    #region Variables
    public Transform[] targetTransform;       //최종 도착할 위치
    public GameObject[] UICanvas;
    public float moveSpeed = 5;             //이동 속도
    public float lerpSpeed = 2f;            //목표 위치로 이동 속도
    public float triggerDistance = 2f;      //목표 지점 근처에서 변화 시작 거리
    private RectTransform moveCanvas;
    private Vector3 targetUI = new Vector3(300, 0, 0);
    public float uiMoveSpeed = 2f; // UI 이동 속도

    public bool isStop = false;
    private bool isMoveingToTarget = false;
    private bool uiMoved = false; // UI 이동 여부

    public GameObject fadeInObject;
    #endregion
    private void Start()
    {
        moveCanvas = UICanvas[1].GetComponent<RectTransform>();
        UICanvas[0].SetActive(isStop);
        UICanvas[1].SetActive(isStop);
        fadeInObject.SetActive(true); 

        StartCoroutine(DelayStartAfterFade());
    }
    private void Update()
    {
        if(!isMoveingToTarget)
        {
            Debug.Log("22");
            transform.position += transform.forward * moveSpeed * Time.deltaTime;

            //특정 거리 안에 들어오면 목표 위치로 이동 시작
            if (Vector3.Distance(transform.position, targetTransform[0].position) < triggerDistance)
            {
                Debug.Log("3");
                StartCoroutine(MoveToTarget());
            }
        }
    }

    private IEnumerator DelayStartAfterFade()
    {
        AudioController.instance.Playfadein();
        yield return new WaitForSeconds(1.5f); 
        fadeInObject.SetActive(false);     
        isMoveingToTarget = false;           
    }
    IEnumerator MoveToTarget()
    {
        isMoveingToTarget = true;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, targetTransform[1].position, elapsedTime);
            transform.rotation = Quaternion.Lerp(startRotation, targetTransform[1].rotation, elapsedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetTransform[1].position;
        transform.rotation = targetTransform[1].rotation;
        isStop = true;
        if(isStop && !uiMoved)
        {
            UICanvas[0].SetActive(isStop);
            UICanvas[1].SetActive(isStop);
            StartCoroutine(MoveUI());
            uiMoved = true;
        }
    }
    private IEnumerator MoveUI()
    {
        Vector3 startUIPosition = moveCanvas.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            moveCanvas.anchoredPosition = Vector3.Lerp(startUIPosition, targetUI, elapsedTime);
            elapsedTime += Time.deltaTime * uiMoveSpeed;
            yield return null;
        }

        moveCanvas.anchoredPosition = targetUI;
    }
}
