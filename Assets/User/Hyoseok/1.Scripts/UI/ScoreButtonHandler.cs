using UnityEngine;
using UnityEngine.UI;

public class ScoreButtonHandler : MonoBehaviour
{
    public string category;

    private Button button;
    private ScoreboardEntry entry;

    void Start()
    {
        button = GetComponent<Button>();
        entry = GetComponentInParent<ScoreboardEntry>();

        if (button != null && entry != null)
        {
            button.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.LogWarning("ScoreButtonHandler 초기화 실패: Button 또는 ScoreboardEntry를 찾을 수 없습니다.");
        }
    }

    public void OnClick()
    {
        if (TurnManager.instance == null)
        {
            Debug.LogWarning("TurnManager 인스턴스가 존재하지 않습니다.");
            return;
        }

        if (!TurnManager.instance.IsMyTurn())
        {
            Debug.Log(" 지금은 내 턴이 아닙니다!");
            return;
        }

        if (entry == null)
        {
            Debug.LogWarning("ScoreboardEntry를 찾을 수 없습니다.");
            return;
        }

        int index = entry.GetCategoryIndex(category);
        if (index == -1)
        {
            Debug.LogWarning($" 잘못된 카테고리: {category}");
            return;
        }

        if (entry.IsAlreadyScored(index))
        {
            Debug.Log(" 이미 기록된 점수입니다!");
            return;
        }

        // 점수 기록 및 턴 넘기기
        entry.HighlightScore(category);
        Debug.Log($"{category} 선택됨 (점수 확정됨)");
    }
}
