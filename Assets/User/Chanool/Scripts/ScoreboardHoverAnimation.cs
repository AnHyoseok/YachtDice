using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ScoreboardHoverAnimation : MonoBehaviour
{
    public GameObject hoverAnimation; // Hover 애니메이션 효과
    public ScoreboardTurnActivator scoreboardTurnActivator; // ScoreboardTurnActivator.cs의 PlayerA, PlayerB 참조
    public List<RectTransform> playerARectTransforms = new List<RectTransform>(); // ARectTransform(HoverAnimation위치)들을 저장해놓을 리스트
    public List<RectTransform> playerBRectTransforms = new List<RectTransform>(); // BRectTransform(HoverAnimation위치)들을 저장해놓을 리스트
    public GameObject categories; // Select표시(주황색)할 카테고리
    public List<Image> selectImages = new List<Image>(); // SelectImage들을 담을 리스트
    
    public bool isPlayerTurn = true;

    private int previousHoveredIndex = -1;

    private void Start()
    {
        scoreboardTurnActivator = GetComponent<ScoreboardTurnActivator>(); // ScoreboardTurnActivator.cs 참조
        // Player_A와 Player_B 내부의 "Line_{i}"를 찾아 리스트에 저장
        FindRectTransforms(scoreboardTurnActivator.playerA, playerARectTransforms);
        FindRectTransforms(scoreboardTurnActivator.playerB, playerBRectTransforms);
        // Categories 내부의 "SelectImage" 를 모두 찾아 리스트에 저장
        FindSelectImage(categories, selectImages);
        // 버튼들 가져오기
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