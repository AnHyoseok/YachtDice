using System.Collections;
using UnityEngine;

public class SelectDice : MonoBehaviour
{
    #region Variables
    public Transform[] targetPositions;
    public float moveSpeed = 2f;
    public int turnLimit = 5;

    private Camera mainCamera;
    private GameObject selectDice = null;
    private Vector3 originalPosition; // 원래 위치 저장
    private int currentTargetIndex = 0;
    public static int movesThisTurn = 0;
    private bool isGoMove = false;
    public bool isPut = false;
    #endregion

    private void Start()
    {
        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (currentTargetIndex > 1)
        {
            isPut = Input.GetKey(KeyCode.Escape);
            if (isPut)
            {
                foreach (Dice dice in DiceManager.Instance.dices)
                {
                    if(dice != null)
                    {
                        Destroy(dice.gameObject);
                    }
                }
                for (int i = 0; i < DiceManager.Instance.dices.Length; i++)
                {
                    if (DiceManager.Instance.dices[i] != null)
                    {
                        DiceManager.Instance.dices[i] = null;
                    }
                }
                DiceManager.Instance.isDiceArray = false;
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
                Dice selectedDice = selectDice.GetComponent<Dice>();
                MoveDiceBetweenArrays(selectedDice);
                StartCoroutine(MoveDiceToNextTartget(selectDice, targetPositions[currentTargetIndex]));
                DiceManager.Instance.DiceArrays();
            }
        }
    }
    IEnumerator MoveDiceToNextTartget(GameObject dice, Transform destination)
    {
        if (selectDice == null || targetPositions.Length == 0) yield break;
        isGoMove = true;

        float elapsedTime = 0f;
        Vector3 initialPosition = dice.transform.position;

        while (elapsedTime < 1f)
        {
            dice.transform.position = Vector3.Lerp(initialPosition, destination.position, elapsedTime);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }
        dice.transform.position = destination.position;

        movesThisTurn++;

        if(movesThisTurn >= turnLimit)
        {
            OnTurnEnd();
            yield break;
        }
        currentTargetIndex = (currentTargetIndex + 1) % targetPositions.Length;
        isGoMove = false;
    }

    void MoveDiceBetweenArrays(Dice selectedDice)
    {
        Dice[] diceArray = DiceManager.Instance.dices;
        int index = System.Array.FindIndex(diceArray, d => d == selectedDice);
        if (index != -1)
        {
            Dice[] newArray = new Dice[diceArray.Length - 1];
            for (int i = 0, j = 0; i < diceArray.Length; i++)
            {
                if (i != index)
                {
                    newArray[j++] = diceArray[i];
                }
            }
            DiceManager.Instance.dices = newArray;
        }
        Dice[] newDiceArray = DiceManager.Instance.newdicelist;
        System.Array.Resize(ref newDiceArray, newDiceArray.Length + 1);
        newDiceArray[newDiceArray.Length - 1] = selectedDice;
        DiceManager.Instance.newdicelist = newDiceArray;
    }

    void ReturnDiceToOriginalList(Dice selectedDice)
    {
        Dice[] newDiceArray = DiceManager.Instance.newdicelist;
        int index = System.Array.FindIndex(newDiceArray, d => d == selectedDice);

        if (index != -1)
        {
            Dice[] newArray = new Dice[newDiceArray.Length - 1];
            for (int i = 0, j = 0; i < newDiceArray.Length; i++)
            {
                if (i != index)
                {
                    newArray[j++] = newDiceArray[i];
                }
            }
            DiceManager.Instance.newdicelist = newArray;
        }

        Dice[] diceArray = DiceManager.Instance.dices;
        System.Array.Resize(ref diceArray, diceArray.Length + 1);
        diceArray[diceArray.Length - 1] = selectedDice;
        DiceManager.Instance.dices = diceArray;
    }

    private void OnTurnEnd()
    {
        movesThisTurn = 0;
        currentTargetIndex = 0;
        DiceManager.Instance.rollsLeft = 3;
        //추가 기능: 다음 플레이어 턴으로 넘어가는 로직을 여기에 추가 가능
    }
}
