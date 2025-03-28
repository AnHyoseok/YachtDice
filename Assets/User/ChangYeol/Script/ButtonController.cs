using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    #region Variables
    public bool isButton = true;
    #endregion
    public void OnPointerDown(PointerEventData eventData)
    {
        if (TurnManager.instance.IsMyTurn())
        {
            isButton = false;
        }
       
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(TurnManager.instance.IsMyTurn())
        {

            isButton = true;

        }

        
    }
}
