using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ScoreboardHoverAnimation : MonoBehaviour, IPointerEnterHandler //, IPointerExitHandler
{
    public GameObject hoverEffect; // Hover효과
    public ScoreboardTurnActivator scoreboardTurnActivator; // ScoreboardTurnActivator.cs의 PlayerA, PlayerB 참조
    public List<RectTransform> playerARectTransforms = new List<RectTransform>(); // ARectTransform들을 저장해놓을 리스트
    public List<RectTransform> playerBRectTransforms = new List<RectTransform>(); // BRectTransform들을 저장해놓을 리스트

    private bool isHovered = false;

    public GameObject categories; // Select표시(주황색)할 카테고리
    private List<Image> selectImages = new List<Image>(); // SelectImage들을 담을 리스트

    private void Start()
    {
        scoreboardTurnActivator = GetComponent<ScoreboardTurnActivator>(); // ScoreboardTurnActivator.cs 참조

        // Player_A와 Player_B 내부의 "Line_{i}"를 찾아 리스트에 저장
        FindRectTransforms(scoreboardTurnActivator.playerA, playerARectTransforms);
        FindRectTransforms(scoreboardTurnActivator.playerB, playerBRectTransforms);
        // Categories 내부의 "SelectImage" 를 모두 찾아 리스트에 저장
        FindSelectImage(categories, selectImages);

        hoverEffect.SetActive(false); // 초기에는 애니메이션 비활성화
    }

    private void Update()
    {
        if(scoreboardTurnActivator.isPlayerATurn) // PlayerA 턴이면
        {
            
        }
        else if(scoreboardTurnActivator.isPlayerBTurn) // PlayerB 턴이면
        {
            
        }

        if (hoverEffect != null && isHovered)
        {
            int hoveredIndex;
            RectTransform hoveredRect = GetHoveredRectA(out hoveredIndex);

            // 모든 selectImages 비활성화 (초기화)
            foreach (var img in selectImages)
            {
                img.gameObject.SetActive(false);
            }

            // hoveredIndex가 유효한 범위 내에 있을 때만 활성화
            if (hoveredIndex >= 0 && hoveredIndex < selectImages.Count)
            {
                selectImages[hoveredIndex].gameObject.SetActive(true);
            }


            if (hoveredRect != null)
            {
                hoverEffect.transform.position = hoveredRect.position; // 빠르게 위치 이동
            }
        }
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
        isHovered = true; // 마우스가 들어왔다는 것을 표시

        int hoveredIndex;

        // 마우스가 올라간 RectTransform을 찾아 그 위치를 hoverEffect에 적용
        RectTransform hoveredRect = GetHoveredRectA(out hoveredIndex);
        if (hoveredRect != null)
        {
            hoverEffect.transform.position = hoveredRect.position; // HoverEffect의 위치를 마우스가 올라간 RectTransform의 위치로 설정
            hoverEffect.SetActive(true); // HoverEffect 활성화
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

    // GetHoveredRect는 현재 마우스가 위치한 RectTransform을 찾는 메서드
    private RectTransform GetHoveredRectA(out int index)
    {
        index = -1; // 기본적으로 인덱스를 -1로 설정 (못 찾았을 경우)

        for (int i = 0; i < playerARectTransforms.Count; i++)
        {
            // 마우스 커서가 해당 RectTransform 영역 안에 있는지 확인
            if (RectTransformUtility.RectangleContainsScreenPoint(playerARectTransforms[i], Input.mousePosition, null))
            {
                index = i; // 인덱스 저장
                return playerARectTransforms[i]; // 해당 RectTransform 반환
            }
        }

        return null; // 마우스가 겹치는 RectTransform이 없으면 null 반환
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
}