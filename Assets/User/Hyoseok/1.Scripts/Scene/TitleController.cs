using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace IdleGame
{
    public class TitleController : MonoBehaviour
    {
        #region Variables
        public GameObject menuCanvas;
        public GameObject optionCanvas;

        public Button newGameButton;
        public Button continewButton;
        public Button optionsButton;
        public Button quitGameButton;

     
        #endregion

        private void Start()
        {
            newGameButton.onClick.AddListener(OnNewGame);
            continewButton.onClick.AddListener(OnContiew);
            optionsButton.onClick.AddListener(OnOptions);
            quitGameButton.onClick.AddListener(OnQuitGame);
        }
        void OnNewGame()
        {
            SceneManager.LoadScene("MainBase");
        }

        void OnContiew()
        {

        }

        void OnOptions()
        { 
            optionCanvas.SetActive(true);
        }

        void OnQuitGame()
        {

        }
    }
}