using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RawImageHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RawImage rawImage; // 대상 RawImage
    public Animator animator; // 연결된 애니메이터

    private void Start()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    // 마우스가 RawImage 위에 올려졌을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.SetBool("IsHovered", true); // 애니메이션 시작
        }
    }

    // 마우스가 RawImage에서 벗어났을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.SetBool("IsHovered", false); // 애니메이션 종료
        }
    }
}
