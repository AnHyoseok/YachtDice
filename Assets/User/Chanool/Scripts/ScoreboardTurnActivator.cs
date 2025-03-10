using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // List를 사용하려면 추가

public class ScoreboardTurnActivator : MonoBehaviour
{
    public GameObject playerA;
    public GameObject playerB;
    public bool isPlayerATurn = false; // 시작할 때 아무도 턴이 아님
    public bool isPlayerBTurn = false;

    private List<Image> playerATurnImages = new List<Image>(); // 여러 개의 TurnImage를 담을 리스트
    private List<Image> playerBTurnImages = new List<Image>();

    void Start()
    {
        // Player_A와 Player_B 내부의 "TurnImage"를 모두 찾아 리스트에 저장
        FindTurnImages(playerA, playerATurnImages);
        FindTurnImages(playerB, playerBTurnImages);
    }

    private void FindTurnImages(GameObject player, List<Image> turnImages)
    {
        if (player == null) return;

        Image[] images = player.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img.gameObject.name == "TurnImage")
            {
                turnImages.Add(img); // TurnImage를 리스트에 추가
            }
        }
    }

    void Update()
    {
        // Player_A의 TurnImage 활성화/비활성화
        foreach (var img in playerATurnImages)
        {
            img.gameObject.SetActive(isPlayerATurn); // 각 TurnImage를 isPlayerATurn에 맞게 활성화/비활성화
        }

        // Player_B의 TurnImage 활성화/비활성화
        foreach (var img in playerBTurnImages)
        {
            img.gameObject.SetActive(isPlayerBTurn); // 각 TurnImage를 isPlayerBTurn에 맞게 활성화/비활성화
        }

        // 키 입력을 통해 Player_A 턴을 토글
        if (Input.GetKeyDown(KeyCode.Alpha1))  // 숫자 1을 눌렀을 때
        {
            isPlayerATurn = !isPlayerATurn;  // isPlayerATurn 값을 토글
        }

        // 키 입력을 통해 Player_B 턴을 토글
        if (Input.GetKeyDown(KeyCode.Alpha2))  // 숫자 2를 눌렀을 때
        {
            isPlayerBTurn = !isPlayerBTurn;  // isPlayerBTurn 값을 토글
        }
    }
}