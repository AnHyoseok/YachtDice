using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // List를 사용하려면 추가

public class ScoreboardTurnActivator : MonoBehaviour
{
    public List<GameObject> playerList = new List<GameObject>(); // 자식 오브젝트인 Player를 담을 리스트
    public GameObject playerA; // PlayerList[0]
    public GameObject playerB; // PlayerList[1]
    public bool isPlayerATurn = false; // 시작할 때 아무도 턴이 아님
    public bool isPlayerBTurn = false;
    private List<Image> playerATurnImages = new List<Image>(); // 여러 개의 TurnImage를 담을 리스트
    private List<Image> playerBTurnImages = new List<Image>();

    void Start()
    {
        // 자식중에 Player를 모두찾아 저장하기
        FindPlayersWithTag();
        // Player_A와 Player_B 내부의 "TurnImage"를 모두 찾아 리스트에 저장
        FindTurnImages(playerA, playerATurnImages);
        FindTurnImages(playerB, playerBTurnImages);
    }

    // "Player" 태그를 가진 자식 오브젝트들을 리스트에 저장하는 메서드
    void FindPlayersWithTag()
    {
        // GetComponentsInChildren<Transform>을 사용해 모든 자식 오브젝트를 검색
        foreach (Transform child in transform)
        {
            // 자식 오브젝트가 "Player" 태그를 가졌다면 playerList에 추가
            if (child.CompareTag("Player"))
            {
                playerList.Add(child.gameObject);
            }
        }
        if (playerList[0] == null )return;
        playerA = playerList[0];
        if (playerList[1] == null) return;
        playerB = playerList[1];
    }

    // "TurnImage" 이름를 가진 자식 오브젝트들을 리스트에 저장하는 메서드
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
            if (isPlayerATurn)  // Player_A의 턴이 시작되면
            {
                isPlayerBTurn = false;  // Player_B의 턴을 강제로 false로 설정
            }
        }

        // 키 입력을 통해 Player_B 턴을 토글
        if (Input.GetKeyDown(KeyCode.Alpha2))  // 숫자 2를 눌렀을 때
        {
            isPlayerBTurn = !isPlayerBTurn;  // isPlayerBTurn 값을 토글
            if (isPlayerBTurn)  // Player_B의 턴이 시작되면
            {
                isPlayerATurn = false;  // Player_A의 턴을 강제로 false로 설정
            }
        }
    }
}