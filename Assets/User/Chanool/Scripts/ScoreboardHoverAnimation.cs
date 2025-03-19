using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ScoreboardHoverAnimation : MonoBehaviour, IPointerEnterHandler //, IPointerExitHandler
{
    public GameObject hoverAnimation; // Hover 애니메이션 효과
    public ScoreboardTurnActivator scoreboardTurnActivator; // ScoreboardTurnActivator.cs의 PlayerA, PlayerB 참조
    public List<RectTransform> playerARectTransforms = new List<RectTransform>(); // ARectTransform(HoverAnimation위치)들을 저장해놓을 리스트
    public List<RectTransform> playerBRectTransforms = new List<RectTransform>(); // BRectTransform(HoverAnimation위치)들을 저장해놓을 리스트

    public GameObject categories; // Select표시(주황색)할 카테고리
    private List<Image> selectImages = new List<Image>(); // SelectImage들을 담을 리스트

    public bool isPlayerTurn = true;

    private void Start()
    {
        scoreboardTurnActivator = GetComponent<ScoreboardTurnActivator>(); // ScoreboardTurnActivator.cs 참조
        // Player_A와 Player_B 내부의 "Line_{i}"를 찾아 리스트에 저장
        FindRectTransforms(scoreboardTurnActivator.playerA, playerARectTransforms);
        FindRectTransforms(scoreboardTurnActivator.playerB, playerBRectTransforms);
        // Categories 내부의 "SelectImage" 를 모두 찾아 리스트에 저장
        FindSelectImage(categories, selectImages);
    }

    private void Update()
    {
        // 플레이어 턴일때
        if (isPlayerTurn)
        {
            hoverAnimation.SetActive(true); // 플레이어 턴일때 애니메이션 효과 On

            if (scoreboardTurnActivator.isPlayerATurn) // PlayerA 턴이면
            {
                RectTransform hoveredRect = GetHoveredRectA();
                int hoveredIndex = GetHoveredRectAIndex();

                // hoveredRect 또는 hoveredIndex가 유효한지 체크
                if (hoveredRect != null && hoveredIndex != -1)
                {
                    // 빠르게 위치 이동
                    hoverAnimation.transform.position = hoveredRect.position;

                    // 모든 selectImages를 비활성화
                    for (int i = 0; i < selectImages.Count; i++)
                    {
                        if (i == hoveredIndex)
                        {
                            // 현재 마우스가 위치한 이미지만 활성화
                            selectImages[hoveredIndex].gameObject.SetActive(true);
                            continue;
                        }
                        selectImages[i].gameObject.SetActive(false);
                    }
                }
            }
            else if (scoreboardTurnActivator.isPlayerBTurn) // PlayerB 턴이면
            {

            }
        }
        /*else
        {
            hoverAnimation.SetActive(false); // 플레이어 턴이 아니면 애니메이션 효과 Off
        }*/
            
    }

    private void FindSelectImage(GameObject categories, List<Image> selectImages)
    {
        if (categories == null) return;

        Image[] images = categories.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img.gameObject.name == "SelectImage")
            {
                selectImages.Add(img); // TurnImage를 리스트에 추가
            }
        }
    }

    private void FindRectTransforms(GameObject player, List<RectTransform> rectTransforms)
    {
        if (player == null) return;

        RectTransform[] rects = player.GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rect in rects)
        {
            for (int i = 1; i <= 15; i++)
            {
                if (i == 7 || i == 8 || i == 15) continue;

                if (rect.gameObject.name == $"Line_{i}")
                {
                    rectTransforms.Add(rect);
                    break;
                }
            }
        }
    }

    // OnPointerEnter는 마우스가 RectTransform에 들어왔을 때 호출되는 메서드
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 올라간 RectTransform을 찾아 그 위치를 hoverEffect에 적용
        RectTransform hoveredRect = GetHoveredRectA();
        if (hoveredRect != null)
        {
            hoverAnimation.transform.position = hoveredRect.position; // HoverEffect의 위치를 마우스가 올라간 RectTransform의 위치로 설정
        }
    }

    /*// OnPointerExit는 마우스가 RectTransform을 벗어났을 때 호출되는 메서드
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false; // 마우스가 벗어났다는 것을 표시

        // HoverEffect 비활성화
        if (hoverEffect != null)
        {
            hoverEffect.SetActive(false); // HoverEffect 비활성화
        }
    }*/

    private RectTransform GetHoveredRectA()
    {
        // playerBRectTransforms 리스트에서 마우스가 포함된 RectTransform을 찾기
        foreach (RectTransform rect in playerARectTransforms)
        {
            // RectTransform이 마우스 커서와 겹치는지 확인
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
            {
                return rect; // 겹친 RectTransform을 반환
            }
        }
        return null; // 해당하는 RectTransform이 없으면 null 반환
    }

    private RectTransform GetHoveredRectB()
    {
        // playerBRectTransforms 리스트에서 마우스가 포함된 RectTransform을 찾기
        foreach (RectTransform rect in playerBRectTransforms)
        {
            // RectTransform이 마우스 커서와 겹치는지 확인
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
            {
                return rect; // 겹친 RectTransform을 반환
            }
        }
        return null; // 해당하는 RectTransform이 없으면 null 반환
    }

    private int GetHoveredRectAIndex()
    {
        for (int i = 0; i < playerARectTransforms.Count; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(playerARectTransforms[i], Input.mousePosition, null))
            {
                return i; // 해당 RectTransform의 인덱스 반환
            }
        }
        return -1; // 없을 경우 -1 반환
    }

}