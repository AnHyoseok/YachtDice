using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ScoreboardHoverAnimation : MonoBehaviour, IPointerEnterHandler //, IPointerExitHandler
{
    public GameObject hoverEffect; // Hover효과 오브젝트
    public ScoreboardTurnActivator scoreboardTurnActivator; // ScoreboardTurnActivator.cs의 PlayerA, PlayerB 참조
    public List<RectTransform> playerARectTransforms = new List<RectTransform>(); // 여러 개의 RectTransform을 저장해놓을 리스트
    public List<RectTransform> playerBRectTransforms = new List<RectTransform>();

    private bool isHovered = false;

    private void Start()
    {
        scoreboardTurnActivator = GetComponent<ScoreboardTurnActivator>();

        // Player_A와 Player_B 내부의 "Line_{i}"를 찾아 리스트에 저장
        FindRectTransforms(scoreboardTurnActivator.playerA, playerARectTransforms);
        FindRectTransforms(scoreboardTurnActivator.playerB, playerBRectTransforms);

        if (hoverEffect != null)
        {
            hoverEffect.SetActive(false); // 초기에는 비활성화
        }
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
            RectTransform hoveredRect = GetHoveredRectA();
            if (hoveredRect != null)
            {
                hoverEffect.transform.position = hoveredRect.position; // 빠르게 위치 이동
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

        // 마우스가 올라간 RectTransform을 찾아 그 위치를 hoverEffect에 적용
        RectTransform hoveredRect = GetHoveredRectA();
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
    private RectTransform GetHoveredRectA()
    {
        // playerARectTransforms 리스트에서 마우스가 포함된 RectTransform을 찾기
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
}