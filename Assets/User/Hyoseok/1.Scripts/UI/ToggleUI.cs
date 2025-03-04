using UnityEngine;
namespace IdleGame
{
    public class ToggleUI : MonoBehaviour
    {
        public GameObject[] uiElementsToDeactivate;
        public GameObject[] uiElementsToActivate;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                foreach (GameObject uiElement in uiElementsToDeactivate)
                {
                    uiElement.SetActive(false);
                }

                foreach (GameObject uiElement in uiElementsToActivate)
                {
                    uiElement.SetActive(true);
                }
            }
        }
    }
}