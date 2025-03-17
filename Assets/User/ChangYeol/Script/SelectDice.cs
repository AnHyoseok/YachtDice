using System.Collections;
using UnityEngine;

public class SelectDice : MonoBehaviour
{
    #region Variables
    public ShakeCup shakeCup;
    public Transform[] targetPositions;
    public float moveSpeed = 2f;
    public int turnLimit = 5;

    private Camera mainCamera;
    private GameObject selectDice = null;
    private int currentTargetIndex = 0;
    private static int movesThisTurn = 0;
    private bool isGoMove = false;
    #endregion

    private void Start()
    {
        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (currentTargetIndex > 1)
        {
            if(Input.GetKey(KeyCode.Escape))
            {
                foreach (Dice dice in DiceManager.Instance.dicelist)
                {
                    dice.gameObject.SetActive(false);
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            DiceSelect();
        }
    }
    void DiceSelect()
    {
        if(movesThisTurn >= turnLimit)
        {
            Debug.Log("이동 횟수 초과");
            OnTurnEnd();
            return;
        }
        if (isGoMove || DiceManager.Instance.isArrays) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit))
        {
            Rigidbody rb = hit.collider.gameObject.GetComponent<Rigidbody>();
            if(hit.collider.CompareTag("Dice") && rb.isKinematic && rb.useGravity)
            {
                selectDice = hit.collider.gameObject;
                DiceManager.Instance.dicelist.Remove(selectDice.GetComponent<Dice>());
                DiceManager.Instance.newdicelist.Add(selectDice.GetComponent<Dice>());
                StartCoroutine(MoveDiceToNextTartget());
                DiceManager.Instance.DiceArrays();
            }
        }
    }
    IEnumerator MoveDiceToNextTartget()
    {
        if (selectDice == null || targetPositions.Length == 0) yield break;
        isGoMove = true;
        Transform targetPosition = targetPositions[currentTargetIndex];
        float elapsedTime = 0f;
        Vector3 initialPosition = selectDice.transform.position;

        while (elapsedTime < 1f)
        {
            selectDice.transform.position = Vector3.Lerp(initialPosition, targetPosition.position, elapsedTime);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }
        selectDice.transform.position = targetPosition.position;
        Debug.Log($"{selectDice.name}이 도착");

        movesThisTurn++;

        if(movesThisTurn >= turnLimit)
        {
            Debug.Log("턴 종료");
            OnTurnEnd();
            yield break;
        }
        currentTargetIndex = (currentTargetIndex + 1) % targetPositions.Length;
        isGoMove = false;
    }

    private void OnTurnEnd()
    {
        Debug.Log("플레이어 턴 종료");
        movesThisTurn = 0;
        currentTargetIndex = 0;

        //추가 기능: 다음 플레이어 턴으로 넘어가는 로직을 여기에 추가 가능
    }
}
