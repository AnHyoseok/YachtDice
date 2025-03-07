using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    #region Variables
    public GameObject menuCanvas;
    public GameObject optionCanvas;
    public GameObject TutorialCanvas;

    public Button newGameButton;
    public Button tutorialButton;
    public Button optionsButton;
    public Button quitGameButton;

    //뒤로가기
    public Button optionBackbutton;
    public Button tutorialBackButton;

    //사운드
    public Slider masterSlider;
    public Slider soundSlider;
    public Slider musicSlider;
    public Button resetDefaultButton;
    #endregion

    private void Start()
    {
        newGameButton.onClick.AddListener(OnNewGame);
        tutorialButton.onClick.AddListener(OnTutorial);
        optionsButton.onClick.AddListener(OnOptions);
        quitGameButton.onClick.AddListener(OnQuitGame);
        optionBackbutton.onClick.AddListener(OnOptionBack);
        tutorialBackButton.onClick.AddListener(OnTutorialBack);

        resetDefaultButton.onClick.AddListener(OnResetDefaultButtonClick);

    }
    void OnNewGame()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    void OnTutorial()
    {
        TutorialCanvas.SetActive(true);
    }

    void OnOptions()
    {
        optionCanvas.SetActive(true);
    }

    void OnQuitGame()
    {

    }

    void OnOptionBack()
    {
        optionCanvas.SetActive(false);
    }

    void OnTutorialBack()
    {
        TutorialCanvas.SetActive(false);
    }

    // Reset Default 버튼 클릭 시 호출되는 함수
    public void OnResetDefaultButtonClick()
    {
        if (masterSlider != null)
        {
            masterSlider.value = 1; // MasterSlider의 값을 1로 설정
            soundSlider.value = 1;
            musicSlider.value = 1;

        }
    }
}
