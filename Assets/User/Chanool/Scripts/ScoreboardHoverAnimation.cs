using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Photon.Pun;

public class ScoreboardHoverAnimation : MonoBehaviour
{
    public GameObject hoverAnimation; // Hover 애니메이션 효과
    public GameObject categories; // Select표시(주황색)할 카테고리
    public List<Image> selectImages = new List<Image>(); // SelectImage들을 담을 리스트

    public ScoreboardTurnActivator scoreboardTurnActivator; // ScoreboardTurnActivator.cs의 PlayerA,B,C,D 참조
    public List<RectTransform> playerARectTransforms = new List<RectTransform>(); // ARectTransform(HoverAnimation위치)들을 저장해놓을 리스트
    public List<RectTransform> playerBRectTransforms = new List<RectTransform>(); // BRectTransform(HoverAnimation위치)들을 저장해놓을 리스트
    public List<RectTransform> playerCRectTransforms = new List<RectTransform>(); // CRectTransform(HoverAnimation위치)들을 저장해놓을 리스트
    public List<RectTransform> playerDRectTransforms = new List<RectTransform>(); // DRectTransform(HoverAnimation위치)들을 저장해놓을 리스트
    
    public bool isPlayerTurn = true;

    private int previousHoveredIndex = -1;

    private void Start()
    {
        scoreboardTurnActivator = GetComponent<ScoreboardTurnActivator>(); // ScoreboardTurnActivator.cs 참조
        // Categories 내부의 "SelectImage"를 모두 찾아 리스트에 저장
        FindSelectImage(categories, selectImages);
        // Player_A,B,C,D 내부의 RectTransform들을 찾아 리스트에 저장
        Debug.Log($"{scoreboardTurnActivator.playerA}++++++");
        if (scoreboardTurnActivator.playerA == null)
        {
            FindRectTransforms(scoreboardTurnActivator.playerA, playerARectTransforms);
            Debug.Log($"{scoreboardTurnActivator.playerA}");
            return;
        }
        Debug.Log($"{scoreboardTurnActivator.playerA}");
        if (scoreboardTurnActivator.playerB == null) return;
        FindRectTransforms(scoreboardTurnActivator.playerB, playerBRectTransforms);
        if (scoreboardTurnActivator.playerC == null) return;
        FindRectTransforms(scoreboardTurnActivator.playerC, playerCRectTransforms);
        if (scoreboardTurnActivator.playerD == null) return;
        FindRectTransforms(scoreboardTurnActivator.playerD, playerDRectTransforms);

        foreach (RectTransform rect in playerARectTransforms)
        {
            Debug.Log("저장된 RectTransform: " + rect.gameObject.name);
        }
    }

    private void Update()
    {
        // 현재 마우스가 올라간 RectTransform과 인덱스 가져오기
        RectTransform hoveredRect = GetHoveredRect(playerARectTransforms);
        int hoveredRectindex = GetHoveredRectIndex(playerARectTransforms);

        // hoverAnimation 위치 설정
        if (hoveredRect != null)
        {
            hoverAnimation.transform.position = hoveredRect.position;
            hoverAnimation.SetActive(true);
        }

        // 이전에 활성화된 selectImage를 비활성화 (단, hoveredRectindex가 -1일 때는 비활성화 안 함)
        if (previousHoveredIndex != -1 && previousHoveredIndex != hoveredRectindex && hoveredRectindex != -1)
        {
            selectImages[previousHoveredIndex].gameObject.SetActive(false);
        }

        // 새로운 hoveredRectindex가 유효하면 활성화
        if (hoveredRectindex > -1)
        {
            selectImages[hoveredRectindex].gameObject.SetActive(true);
            previousHoveredIndex = hoveredRectindex; // ✅ 마우스가 벗어나도 마지막 인덱스 기억
        }
    }

    private void FindSelectImage(GameObject categories, List<Image> selectImages)
    {
       if(PhotonNetwork.IsMasterClient) // 본인일때 
        {

        }
        if (categories == null) return;

        Image[] images = categories.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img.gameObject.name == "SelectImage")
            {
                selectImages.Add(img); // "SelectImage"들을 리스트에 추가
            }
        }
    }

    // 매개변수 player의 자식인 "Upper Section"과 "Lower Section"을 이름으로 찾는다
    // "Upper Section" 자식의 RectTransform을 "Upper Section"이 가지고있는 자식의 수 만큼 반복하여 매개변수 rectTransforms에 차례대로 저장한다
    // "Lower Section" 자식의 RectTransform을 "Lower` Section"이 가지고있는 자식의 수 만큼 반복하여 매개변수 rectTransforms에 차례대로 저장한다(위에서 저장한 rectTransforms에 이어서)
    private void FindRectTransforms(GameObject player, List<RectTransform> rectTransforms)
    {
        // player의 모든 RectTransform들을 가져오기
        RectTransform[] rects = player.GetComponentsInChildren<RectTransform>(true);

        Debug.Log($"{rects[0]},{rects.Length}");

        // Upper Section 태그를 가진 RectTransform 자식들을 먼저 처리
        foreach (RectTransform rect in rects)
        {
            if (rect.CompareTag("Upper Section"))  // 태그로 Upper Section 찾기
            {
                // Upper Section의 자식들을 rectTransforms 리스트에 추가
                foreach (RectTransform child in rect)
                {
                    rectTransforms.Add(child);
                }
                break; // Upper Section 처리 후 더 이상 반복하지 않도록 종료
            }
        }

        // Lower Section 태그를 가진 RectTransform 자식들을 처리
        foreach (RectTransform rect in rects)
        {
            if (rect.CompareTag("Lower Section"))  // 태그로 Lower Section 찾기
            {
                // Lower Section의 자식들을 rectTransforms 리스트에 추가
                foreach (RectTransform child in rect)
                {
                    rectTransforms.Add(child);
                }
                break; // Lower Section 처리 후 더 이상 반복하지 않도록 종료
            }
        }
    }

    private RectTransform GetHoveredRect(List<RectTransform> rectTransforms)
    {
        // RectTransforms 리스트에서 마우스가 올라간 RectTransform을 찾기
        foreach (RectTransform rect in rectTransforms)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
            {
                return rect; // 마우스가 올라간 RectTransform 반환
            }
        }
        return null; // 마우스가 아무 UI에도 올라가지 않으면 null 반환
    }

    private int GetHoveredRectIndex(List<RectTransform> rectTransforms)
    {
        for (int i = 0; i < rectTransforms.Count; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[i], Input.mousePosition, null))
            {
                return i; // 해당 RectTransform의 인덱스 반환
            }
        }
        return -1; // 없을 경우 -1 반환
    }
}