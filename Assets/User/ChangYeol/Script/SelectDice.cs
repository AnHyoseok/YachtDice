using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class SelectDice : MonoBehaviour
{
    #region Variables
    private PhotonView photon;
    public Button escbutton;
    public Transform[] targetPositions;
    public bool[] isTarget;
    public float moveSpeed = 2f;
    public int turnLimit = 5;

    private Camera mainCamera;
    private GameObject currentActiveUI = null;
    private Animator currentAnimator = null;
    private GameObject selectDiceObject = null;
    [HideInInspector]public int currentTargetIndex = 0;
    public int movesThisTurn = 0;
    private bool isGoMove = false;
    public bool isPut = false;
    #endregion

    private void Start()
    {
        photon = GetComponent<PhotonView>();
        mainCamera = Camera.main;
    }
    private void Update()
    {
        if (currentTargetIndex >= 0 && TurnManager.instance.IsMyTurn())
        {
            photon.RPC("EscKeySetActive", RpcTarget.All, DiceManager.Instance.isDiceArray);
            escbutton.onClick.AddListener(() => OnClickEscButton());
            isPut = Input.GetKey(KeyCode.Escape);
            if (isPut)
            {
                //점수판 ui 알파값 0 
                ScoreboardManager.instance.HideLocalScore();

                foreach (Dice dice in DiceManager.Instance.dices)
                {
                    if(dice != null)
                    {
                        PhotonNetwork.Destroy(dice.gameObject);
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
                photon.RPC("EscKeySetActive", RpcTarget.All, DiceManager.Instance.isDiceArray);
            }
        }
        if (Input.GetMouseButtonDown(0) && TurnManager.instance.IsMyTurn())
        {
            DiceSelect();
        }
        if(DiceManager.Instance.isDiceArray && TurnManager.instance.IsMyTurn())
        {
            MoveUItoMousePosition();
        }
    }
    #region SelectDice
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
                if (rb.isKinematic)
                {
                    selectDiceObject = hit.collider.gameObject;
                    Dice selectedDice = selectDiceObject.GetComponent<Dice>();

                    if (!selectedDice.isSelected)
                    {
                        // false인 위치 찾기
                        bool foundEmptyPosition = false;
                        for (int i = 0; i < isTarget.Length; i++)
                        {
                            if (!isTarget[i])
                            {
                                currentTargetIndex = i;
                                foundEmptyPosition = true;
                                break;
                            }
                        }

                        // 빈 위치를 찾았을 때만 이동
                        if (foundEmptyPosition)
                        {
                            StartCoroutine(MoveDiceToNextTartget(selectDiceObject, targetPositions[currentTargetIndex].position));
                            MoveDiceBetweenArrays(selectedDice,DiceManager.Instance.dices,DiceManager.Instance.newdicelist);
                            movesThisTurn++;
                            selectedDice.isSelected = true;
                            selectedDice.index = currentTargetIndex;
                            isTarget[currentTargetIndex] = true;
                        }
                    }
                    else if(selectedDice.isSelected)
                    {
                        StartCoroutine(MoveDiceToNextTartget(selectDiceObject, selectedDice.originPos));
                        MoveDiceBetweenArrays(selectedDice, DiceManager.Instance.newdicelist, DiceManager.Instance.dices);
                        movesThisTurn--;
                        selectedDice.isSelected = false;
                        isTarget[selectedDice.index] = false;
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

    public void MoveDiceBetweenArrays(Dice selectedDice,Dice[] dices, Dice[] newdicelist)
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
    #endregion
    #region SelectedDiceUI
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
                return;
            }
        }
        
        // 마우스가 주사위를 벗어났을 때 UI 비활성화
        if (currentActiveUI != null)
        {
            RPC_SelectUI();
        }
    }
    [PunRPC]
    void SelectDiceUISwich(Dice dice)
    {
        if(dice != null)
        {
            // 이전 UI 비활성화 및 애니메이션 리셋
            if (currentActiveUI != null)
            {
                currentActiveUI.SetActive(false);
                if (currentAnimator != null)
                {
                    currentAnimator.SetBool("IsSelect", false);
                }
            }

            int diceValue = dice.GetDiceValue();
            
            // UI 선택 및 애니메이터 설정
            switch (diceValue)
            {
                case 1:
                case 6:
                    currentActiveUI = dice.SelectUICavas[0];
                    break;
                case 2:
                case 4:
                    currentActiveUI = dice.SelectUICavas[1];
                    break;
                case 3:
                case 5:
                    currentActiveUI = dice.SelectUICavas[2];
                    break;
            }

            if (currentActiveUI != null)
            {
                currentActiveUI.SetActive(true);
                currentAnimator = currentActiveUI.GetComponent<Animator>();
                if (currentAnimator != null)
                {
                    currentAnimator.SetBool("IsSelect", true);
                }
            }
        }
    }
    [PunRPC]
    void RPC_SelectUI()
    {
        currentActiveUI.SetActive(false);
        if (currentAnimator != null)
        {
            currentAnimator.SetBool("IsSelect", false);
            currentAnimator = null;
        }
        currentActiveUI = null;
    }
    #endregion
    public void OnTurnEnd()
    {
        movesThisTurn = 0;
        currentTargetIndex = 0;
        DiceManager.Instance.rollsLeft = 3;
        //ScoreboardEntry.HighlightScore 호출
        //ScoreboardManager.instance.HighlightLocalScore("고른카테고리"); 
    }
    void OnClickEscButton()
    {
        ScoreboardManager.instance.HideLocalScore();

        foreach (Dice dice in DiceManager.Instance.dices)
        {
            if (dice != null)
            {
                PhotonNetwork.Destroy(dice.gameObject);
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
        photon.RPC("EscKeySetActive", RpcTarget.All, DiceManager.Instance.isDiceArray); 
    }
    [PunRPC]
    void EscKeySetActive(bool isEsc)
    {
        escbutton.gameObject.SetActive(isEsc);
    }
}
