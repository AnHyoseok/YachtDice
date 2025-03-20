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
                //점수가려야됌 

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
            if (hit.collider.CompareTag("Dice"))
            {
                if (rb.isKinematic && rb.useGravity)
                {
                    selectDice = hit.collider.gameObject;
                    Dice selectedDice = selectDice.GetComponent<Dice>();
                    MoveDiceBetweenArrays(selectedDice);
                    StartCoroutine(MoveDiceToNextTartget(selectDice, targetPositions[currentTargetIndex]));
                    DiceManager.Instance.DiceArrays();
                }
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
    void MoveUItoMousePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray ,out hit , 5))
        {
            if(hit.collider.CompareTag("Dice") && DiceManager.Instance.isDiceArray)
            {

                Vector3 worldPosition = hit.collider.gameObject.transform.position;
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition); //화면 좌표 변환

                // 카메라의 뷰포트 안에 있는지 확인하여 UI 표시 여부 결정
                if (IsInCameraView(mainCamera, hit.collider.bounds))
                {
                    //Debug.Log("d44e");
                    if (screenPosition.z > 0) // z값이 양수일 때만 UI 표시
                    {
                        //Debug.Log("d33e");
                        SelectUI.position = screenPosition; //UI 이동
                        SelectUI.gameObject.SetActive(true); //UI 활성화
                    }
                    else
                    {
                        //Debug.Log("d22e");
                        SelectUI.gameObject.SetActive(false); // 카메라 뒤쪽이면 숨김
                    }
                }
                else
                {
                    //Debug.Log("d11e");
                    SelectUI.gameObject.SetActive(false); // 카메라 뷰포트 밖이면 숨김
                }
                return;
            }
            else
            {
                //Debug.Log("ddee");
                SelectUI.gameObject.SetActive(false);
            }
        }
        SelectUI.gameObject.SetActive(false); // 주사위가 없거나 너무 멀면 UI 숨김
    }

    bool IsInCameraView(Camera camera, Bounds bounds)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
    private void OnTurnEnd()
    {
        movesThisTurn = 0;
        currentTargetIndex = 0;
        DiceManager.Instance.rollsLeft = 3;
        //추가 기능: 다음 플레이어 턴으로 넘어가는 로직을 여기에 추가 가능
    }
}
