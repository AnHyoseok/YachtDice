using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    #region Variables
    public GameObject menuCanvas;
    public GameObject optionCanvas;

    public Button newGameButton;
    public Button tutorialButton;
    public Button optionsButton;
    public Button quitGameButton;


    #endregion

    private void Start()
    {
        newGameButton.onClick.AddListener(OnNewGame);
        tutorialButton.onClick.AddListener(OnTutorial);
        optionsButton.onClick.AddListener(OnOptions);
        quitGameButton.onClick.AddListener(OnQuitGame);
    }
    void OnNewGame()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    void OnTutorial()
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
