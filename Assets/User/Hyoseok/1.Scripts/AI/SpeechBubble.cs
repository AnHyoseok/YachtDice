using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    public TextMeshProUGUI speechText;
    public GameObject bubbleObject;
    [SerializeField] private float defaultMessageTimer = 5f;

    public void ShowMessage(string message, float duration = -1f )
    {
        if (bubbleObject == null)
        {
            Debug.LogError("[ERROR] bubbleObject is not assigned!");
            return;
        }

        //  말풍선 활성화 후 메시지 표시
        bubbleObject.SetActive(true);
        speechText.text = message;

        float messageDuration = (duration > 0) ? duration : defaultMessageTimer;

        // 일정 시간 후 숨기기 
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), messageDuration);
    }

    private void HideMessage()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(false);
        }
    }
}
