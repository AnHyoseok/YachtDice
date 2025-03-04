using UnityEngine;
using TMPro;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;  //  화면 좌측 상단에 닉네임 표시
    public GameObject canvasMain;  // 방 목록 UI
    public GameObject canvasTeam;  // 팀 UI
    public GameObject modeSelectionPanel;  // 모드 선택 UI
    public GameObject playerNamePanel;  //  플레이어 이름 설정 UI 패널
    public TMP_InputField nameInputField;  //  플레이어 이름 입력 필드

    public static UIManager instance;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        //기본 이름 설정값
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "User" + Random.Range(100000, 999999);
        }

        UpdatePlayerNameUI(); 
    }

    void UpdatePlayerNameUI()
    {
        if (playerNameText != null)
        {
            playerNameText.text = PhotonNetwork.NickName;
        }
    }

    //  팀 UI 표시 (방에 입장했을 때)
    public void ShowTeamUI()
    {
        canvasMain.SetActive(false);
        canvasTeam.SetActive(true);
    }

    //  방 목록 UI 표시 (방을 나갔을 때)
    public void ShowMainUI()
    {
        canvasMain.SetActive(true);
        canvasTeam.SetActive(false);
        modeSelectionPanel.SetActive(false);
    }

    //  모드 선택 UI 열기
    public void ShowModeSelection()
    {
        modeSelectionPanel.SetActive(true);
    }

    //  모드 선택 UI 닫기
    public void HideModeSelection()
    {
        modeSelectionPanel.SetActive(false);
    }

    //  플레이어 이름 입력 UI 표시
    public void ShowPlayerNamePanel()
    {
        playerNamePanel.SetActive(true);
    }

    //  플레이어 이름 설정
    public void SetPlayerName()
    {
        string playerName = nameInputField.text.Trim();
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.NickName = playerName;
            playerNamePanel.SetActive(false);  
            UpdatePlayerNameUI();  
            Debug.Log($" 플레이어 이름 설정됨: {PhotonNetwork.NickName}");
        }
        else
        {
            Debug.LogError(" 플레이어 이름을 입력하세요!");
        }
    }
}
