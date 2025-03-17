using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using Photon.Pun;

public class PlayerPrefab : MonoBehaviour
{
    public static int ProfileCount { get; private set; } = 0;
    public TextMeshProUGUI playerNameText;
    public Image profileImage;
    public static Sprite[] ProfileSprites; //플레이어 프로필
 
    public Image readyIcon;
    public Image teamColor;
    public Sprite[] profileSprites;
    public Sprite[] aiProfileSprites;     // AI 전용 프로필 
    public Button removeAIButton; // AI 삭제 버튼
    private string aiName; // 삭제할 AI 이름 저장
    void Awake()
    {
        InitializeProfiles();
    }

    //  프로필 초기화 (게임 시작 시 실행되도록 추가)
    public static void InitializeProfiles()
    {
        if (ProfileSprites == null || ProfileSprites.Length == 0)
        {
            GameObject prefabInstance = Resources.Load<GameObject>("PlayerPrefab");
            if (prefabInstance != null)
            {
                PlayerPrefab instance = prefabInstance.GetComponent<PlayerPrefab>();
                if (instance != null && instance.profileSprites.Length > 0)
                {
                    ProfileSprites = instance.profileSprites;
                    ProfileCount = instance.profileSprites.Length;
                    Debug.Log($" ProfileSprites 초기화 완료! 총 {ProfileCount}개 프로필 사용 가능");
                }
            }
        }
    }

    public void Setup(Player player)
    {
        if (playerNameText != null)
        {
            bool isMaster = player.IsMasterClient;
            string displayName = isMaster ?  player.NickName+"[M]" : player.NickName;
            playerNameText.text = displayName;
        }

        if (player.CustomProperties.ContainsKey("Team"))
        {
            string team = (string)player.CustomProperties["Team"];
            teamColor.color = (team == "Red") ? Color.red : Color.blue;
        }

        if (player.CustomProperties.ContainsKey("Ready"))
        {
            bool isReady = (bool)player.CustomProperties["Ready"];
            readyIcon.gameObject.SetActive(isReady);
        }
        else
        {
            readyIcon.gameObject.SetActive(false);
        }

        if (player.CustomProperties.ContainsKey("ProfileImageIndex"))
        {
            int profileIndex = (int)player.CustomProperties["ProfileImageIndex"];
            if (ProfileSprites != null && profileIndex < ProfileSprites.Length)
            {
                profileImage.sprite = ProfileSprites[profileIndex];
            }
            else
            {
                Debug.LogWarning("잘못된 프로필 인덱스입니다!");
            }
        }
    }

    public void SetupAI(string aiName, string team, bool isReady, int profileIndex)
    {
        this.aiName = aiName;

        if (playerNameText != null)
        {
            playerNameText.text = aiName + "(AI)";
        }

        teamColor.color = (team == "Red") ? Color.red : Color.blue;
        readyIcon.gameObject.SetActive(isReady);

        // 🔹 AI 전용 프로필 적용
        if (aiProfileSprites != null && profileIndex < aiProfileSprites.Length && profileImage != null)
        {
            profileImage.sprite = aiProfileSprites[profileIndex];
        }
        else
        {
            Debug.LogWarning("AI 프로필 인덱스가 범위를 벗어났습니다. 기본값을 사용합니다.");
        }

        // AI 삭제 버튼 활성화 및 클릭 이벤트 추가
        if (removeAIButton != null)
        {
            removeAIButton.gameObject.SetActive(true);
            removeAIButton.onClick.RemoveAllListeners();
            removeAIButton.onClick.AddListener(RemoveAI);
        }
    }


    public void RemoveAI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonRoom.instance.RemoveSpecificAI(aiName);
        }
    }

}
