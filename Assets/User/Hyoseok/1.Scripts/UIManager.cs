using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button goTitleButton;
    public TextMeshProUGUI playerNameText;  // 화면 좌측 상단에 닉네임 표시
    public GameObject canvasMain;  // 방 목록 UI
    public GameObject canvasTeam;  // 팀 UI
    public GameObject modeSelectionPanel;  // 모드 선택 UI
    public GameObject playerNamePanel;  // 플레이어 이름 설정 UI 패널
    public TMP_InputField nameInputField;  // 플레이어 이름 입력 필드
    public TextMeshProUGUI warningText; // 경고 메시지 UI
    public GameObject warningPanel; //경고 패널 

    public static UIManager instance;
 
    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        goTitleButton.onClick.AddListener(OnTitleGo);
        // 기본 이름 설정값
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "User" + Random.Range(100000, 999999);
        }

        UpdatePlayerNameUI();
        warningPanel.SetActive(false); // 경고 메시지 초기 비활성화
    }

    void OnTitleGo()
    {
        SceneManager.LoadScene("Title");
    }
    void UpdatePlayerNameUI()
    {
        if (playerNameText != null)
        {
            playerNameText.text = PhotonNetwork.NickName;
        }
    }

    // 팀 UI 표시 (방에 입장했을 때)
    public void ShowTeamUI()
    {
        canvasMain.SetActive(false);
        canvasTeam.SetActive(true);
    }

    // 방 목록 UI 표시 (방을 나갔을 때)
    public void ShowMainUI()
    {
        canvasMain.SetActive(true);
        canvasTeam.SetActive(false);
        modeSelectionPanel.SetActive(false);
    }

    // 모드 선택 UI 열기
    public void ShowModeSelection()
    {
        modeSelectionPanel.SetActive(true);
    }

    // 모드 선택 UI 닫기
    public void HideModeSelection()
    {
        modeSelectionPanel.SetActive(false);
    }

    // 플레이어 이름 입력 UI 표시
    public void ShowPlayerNamePanel()
    {
        playerNamePanel.SetActive(true);
        warningPanel.SetActive(false); // 경고 메시지 숨김
    }

    // 플레이어 이름 설정
    public void SetPlayerName()
    {
        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            ShowWarning("Enter your player name!");
            return;
        }

        if (playerName.Length >= 10)
        {
            ShowWarning("Player names cannot exceed 10 characters.");
            return;
        }

        PhotonNetwork.NickName = playerName;
        playerNamePanel.SetActive(false);
        UpdatePlayerNameUI();
        Debug.Log($"플레이어 이름 설정됨: {PhotonNetwork.NickName}");
    }

    // 경고 메시지 표시 (1초 후 자동 사라짐)
    void ShowWarning(string message)
    {
        warningText.text = message;
        warningPanel.SetActive(true);
        StopAllCoroutines();  
        StartCoroutine(HideWarningAfterDelay());
    }

    IEnumerator HideWarningAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        warningPanel.SetActive(false);
    }
}
