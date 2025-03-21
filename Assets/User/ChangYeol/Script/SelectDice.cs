using NUnit.Framework;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SelectDice : MonoBehaviour
{
    #region Variables
    public RectTransform SelectUI;
    public Transform[] targetPositions;
    public float moveSpeed = 2f;
    public int turnLimit = 5;

    private Camera mainCamera;
    private GameObject currentActiveUI = null;
    private Animator currentAnimator = null;
    private GameObject selectDiceObject = null;
    private Vector3 originalPosition; // ���� ��ġ ����
    private int currentTargetIndex = 0;
    public int movesThisTurn = 0;
    private bool isGoMove = false;
    public bool isPut = false;
    #endregion

    private void Start()
    {
        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (currentTargetIndex >= 0)
        {
            isPut = Input.GetKey(KeyCode.Escape);
            if (isPut)
            {
                //���������߉� 

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
        if(DiceManager.Instance.isDiceArray)
        {
            MoveUItoMousePosition();
        }
    }
    void DiceSelect()
    {
        if(movesThisTurn >= turnLimit)
        {
            Debug.Log("�̵� Ƚ�� �ʰ�");
            OnTurnEnd();
            return;
        }
        if (isGoMove || DiceManager.Instance.isArrays) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit))
        {
            Rigidbody rb = hit.collider.gameObject.GetComponent<Rigidbody>();
            if (hit.collider.CompareTag("Dice"))
            {
                if (rb.isKinematic)
                {
                    selectDiceObject = hit.collider.gameObject;
                    Dice selectedDice = selectDiceObject.GetComponent<Dice>();
                    if(!selectedDice.isSelected)
                    {
                        StartCoroutine(MoveDiceToNextTartget(selectDiceObject, targetPositions[currentTargetIndex].position));
                        MoveDiceBetweenArrays(selectedDice,DiceManager.Instance.dices,DiceManager.Instance.newdicelist);
                        currentTargetIndex = (currentTargetIndex + 1) % targetPositions.Length;
                        movesThisTurn++;
                        selectedDice.isSelected = true;
                    }
                    else if(selectedDice.isSelected)
                    {
                        StartCoroutine(MoveDiceToNextTartget(selectDiceObject, selectedDice.originPos));
                        MoveDiceBetweenArrays(selectedDice, DiceManager.Instance.newdicelist, DiceManager.Instance.dices);
                        currentTargetIndex = Mathf.Max(0,currentTargetIndex - 1);
                        movesThisTurn--;
                        selectedDice.isSelected = false;
                    }
                    DiceManager.Instance.DiceArrays();
                }
            }
        }
    }
    IEnumerator MoveDiceToNextTartget(GameObject dice, Vector3 destination)
    {
        if (dice == null || targetPositions.Length == 0) yield break;
        isGoMove = true;

        float elapsedTime = 0f;
        Vector3 initialPosition = dice.transform.position;

        while (elapsedTime < 1f)
        {
            dice.transform.position = Vector3.Lerp(initialPosition, destination, elapsedTime);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }
        dice.transform.position = destination;

        if(movesThisTurn >= turnLimit)
        {
            OnTurnEnd();
            yield break;
        }
        isGoMove = false;
    }

    void MoveDiceBetweenArrays(Dice selectedDice,Dice[] dices, Dice[] newdicelist)
    {
        int index = System.Array.FindIndex(dices, d => d == selectedDice);
        if (index != -1)
        {
            Dice[] newArray = new Dice[dices.Length - 1];
            for (int i = 0, j = 0; i < dices.Length; i++)
            {
                if (i != index)
                {
                    newArray[j++] = dices[i];
                }   
            }
            Dice[] newTargetArray = new Dice[newdicelist.Length + 1];
            System.Array.Copy(newdicelist, newTargetArray, newdicelist.Length);
            newTargetArray[newdicelist.Length] = selectedDice;
            if (dices == DiceManager.Instance.dices)
            {
                DiceManager.Instance.dices = newArray;
                DiceManager.Instance.newdicelist = newTargetArray;
            }
            else
            {
                DiceManager.Instance.newdicelist = newArray;
                DiceManager.Instance.dices = newTargetArray;
            }
        }
    }
    void MoveUItoMousePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Dice") && DiceManager.Instance.isDiceArray)
            {
                Dice dice = hit.collider.gameObject.GetComponent<Dice>();
                SelectDiceUISwich(dice);
                playDiceAnime();
                return;
            }
        }
        if (currentActiveUI != null && currentAnimator != null)
        {
            currentActiveUI.SetActive(false);
            currentAnimator.SetBool("IsSelect", false);
            currentAnimator = null;
            currentActiveUI = null;
        }
    }

    void SelectDiceUISwich(Dice dice)
    {
        if(dice != null)
        {
            if (currentActiveUI != null)
            {
                currentActiveUI.SetActive(false);
            }

            // 주사위의 윗면 값을 가져옵니다
            int diceValue = GetDiceTopValue(dice.gameObject);
            
            switch (diceValue)
            {
                case 1:
                    currentActiveUI = dice.SelectUICavas[0];
                    currentAnimator = currentActiveUI.GetComponent<Animator>();
                    break;
                case 2:
                    currentActiveUI = dice.SelectUICavas[1];
                    currentAnimator = currentActiveUI.GetComponent<Animator>();
                    break;
                case 3:
                    currentActiveUI = dice.SelectUICavas[2];
                    currentAnimator = currentActiveUI.GetComponent<Animator>();
                    break;
                case 4:
                    currentActiveUI = dice.SelectUICavas[1];
                    currentAnimator = currentActiveUI.GetComponent<Animator>();
                    break;
                case 5:
                    currentActiveUI = dice.SelectUICavas[2];
                    currentAnimator = currentActiveUI.GetComponent<Animator>();
                    break;
                case 6:
                    currentActiveUI = dice.SelectUICavas[0];
                    currentAnimator = currentActiveUI.GetComponent<Animator>();
                    break;
            }
            if (currentActiveUI != null)
            {
                currentActiveUI.SetActive(true);
            }
        }
    }

    // 주사위의 윗면 값을 가져오는 함수
    private int GetDiceTopValue(GameObject diceObject)
    {
        // 주사위의 모든 TriggerDice 컴포넌트를 가져옵니다
        TriggerDice[] triggerDices = diceObject.GetComponentsInChildren<TriggerDice>();
        
        // 각 면을 확인하여 현재 위를 향하고 있는 면의 값을 찾습니다
        foreach (TriggerDice trigger in triggerDices)
        {
            if (trigger.diceValue > 0)
            {
                return trigger.diceValue;
            }
        }
        
        return 1; // 기본값 반환
    }

    void playDiceAnime()
    {
        if (currentActiveUI != null)
        {
            Animator diceAnimator = currentActiveUI.GetComponent<Animator>();
            if(diceAnimator != null)
            {
                if(currentAnimator != null && currentAnimator != diceAnimator)
                {
                    currentAnimator.SetBool("IsSelect", false);
                }
                diceAnimator.SetBool("IsSelect", true);
                currentAnimator = diceAnimator;
            }
        }
    }
    private void OnTurnEnd()
    {
        movesThisTurn = 0;
        currentTargetIndex = 0;
        DiceManager.Instance.rollsLeft = 3;
        //�߰� ���: ���� �÷��̾� ������ �Ѿ�� ������ ���⿡ �߰� ����
    }
}
