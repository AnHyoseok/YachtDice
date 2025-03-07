using UnityEngine;
using UnityEngine.UI;

public class TooltipSpriteSwitcher : MonoBehaviour
{
    public Image tooltipImage;
    public Sprite sprite1;
    public Sprite sprite2;
    public Button rightButton;
    public Button leftButton;

    private bool isSprite1Active = true;

    void Start()
    {
        rightButton.onClick.AddListener(SwitchToSprite2);
        leftButton.onClick.AddListener(SwitchToSprite1);
    }

    void SwitchToSprite2()
    {
        if (isSprite1Active)
        {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(false);
            tooltipImage.sprite = sprite2;
            isSprite1Active = false;
        }
    }

    void SwitchToSprite1()
    {
        if (!isSprite1Active)
        {
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(true);
            tooltipImage.sprite = sprite1;
            isSprite1Active = true;
        }
    }

    
}
