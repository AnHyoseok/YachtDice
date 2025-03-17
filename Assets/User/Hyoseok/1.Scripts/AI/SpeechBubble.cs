using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI; // Image를 제어하기 위해 추가

public class SpeechBubble : MonoBehaviour
{
    public TextMeshProUGUI speechText;
    public Image bubbleImage;
   
    [SerializeField] private float typingSpeed = 0.05f; // 타이핑 속도
    [SerializeField] private float fadeDuration = 1f; // 사라지는 속도 
    [SerializeField] private float holdDuration = 10f; //  유지 시간

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (bubbleImage == null)
        {
            bubbleImage = GetComponent<Image>();
          
        }
    }

    public void ShowMessage(string message)
    {
       

        StartCoroutine(FadeIn());

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        speechText.text = "";

        foreach (char letter in message)
        {
            speechText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        //  10초 동안 유지
        yield return new WaitForSeconds(holdDuration);

        // 페이드아웃 
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float t = 0;
        Color imageColor = bubbleImage.color;
        Color textColor = speechText.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / fadeDuration);

            bubbleImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, alpha);
            speechText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);

            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float t = 0;
        Color imageColor = bubbleImage.color;
        Color textColor = speechText.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / fadeDuration);

            bubbleImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, alpha);
            speechText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);

            yield return null;
        }
    }
}
