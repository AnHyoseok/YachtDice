using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    #region Variables
    public bool isButton = true;
    #endregion
    public void OnPointerDown(PointerEventData eventData)
    {
        isButton = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isButton = true;
    }
}
