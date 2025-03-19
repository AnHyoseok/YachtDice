using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using Photon.Pun;

public class PlayerPrefab : MonoBehaviour
{
    public static int ProfileCount { get; private set; } = 0;
    public static Sprite[] ProfileSprites; // 플레이어 프로필
    public static Sprite[] AIProfileSprites; // AI 프로필

    public TextMeshProUGUI playerNameText;
    public Image profileImage;
    public Image readyIcon;
    public Image teamColor;
    public Image profileOutlineImage;
    public Sprite[] profileSprites;
    public Sprite[] aiProfileSprites; // AI 전용 프로필

    public Button removeAIButton;
    private string aiName;

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

        if (AIProfileSprites == null || AIProfileSprites.Length == 0)
        {
            GameObject prefabInstance = Resources.Load<GameObject>("PlayerPrefab");
            if (prefabInstance != null)
            {
                PlayerPrefab instance = prefabInstance.GetComponent<PlayerPrefab>();
                if (instance != null && instance.aiProfileSprites.Length > 0)
                {
                    AIProfileSprites = instance.aiProfileSprites;
                    Debug.Log($" AIProfileSprites 초기화 완료! 총 {AIProfileSprites.Length}개 AI 프로필 사용 가능");
                }
                else
                {
                    Debug.LogError("AI 프로필 스프라이트가 없습니다! AI 프로필 적용이 정상적으로 작동하지 않을 수 있습니다.");
                }
            }
        }
    }

    public void Setup(Player player)
    {
        this.aiName = null;

        if (playerNameText != null)
        {
            bool isMaster = player.IsMasterClient;
            string displayName = isMaster ? player.NickName + "[M]" : player.NickName;
            playerNameText.text = displayName;
        }

        if (player.CustomProperties.ContainsKey("Team"))
        {
            string team = (string)player.CustomProperties["Team"];
            teamColor.color = (team == "Red") ? Color.red : Color.blue;
            profileOutlineImage.color = (team == "Red") ? Color.red : Color.blue;
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
            profileImage.sprite = GetProfileSprite(profileIndex, false);
        }
    }

    public void SetupAI(string aiName, string team, bool isReady, int profileIndex)
    {
        this.aiName = aiName;

        if (playerNameText != null)
        {
            playerNameText.text = aiName + " (AI)";
        }

        teamColor.color = (team == "Red") ? Color.red : Color.blue;
        profileOutlineImage.color = (team == "Red") ? Color.red : Color.blue;
        readyIcon.gameObject.SetActive(isReady);

        // AI 전용 프로필 적용
        profileImage.sprite = GetProfileSprite(profileIndex, true);

        if (removeAIButton != null)
        {
            removeAIButton.gameObject.SetActive(true);
            removeAIButton.onClick.RemoveAllListeners();
            removeAIButton.onClick.AddListener(RemoveAI);
        }
    }

    private Sprite GetProfileSprite(int index, bool isAI)
    {
        if (isAI)
        {
            if (AIProfileSprites != null && AIProfileSprites.Length > 0)
            {
                if (index >= 0 && index < AIProfileSprites.Length)
                {
                    return AIProfileSprites[index]; // AI 프로필
                }
                else
                {
                    Debug.LogWarning($"AI 프로필 인덱스({index})가 범위를 벗어났습니다. 기본값을 사용합니다.");
                    return AIProfileSprites[0]; // 기본 AI 프로필
                }
            }
            else
            {
                Debug.LogError("AI 프로필 배열이 초기화되지 않았습니다. 기본 플레이어 프로필을 사용합니다.");
                return ProfileSprites[0];
            }
        }
        else
        {
            if (ProfileSprites != null && ProfileSprites.Length > 0)
            {
                if (index >= 0 && index < ProfileSprites.Length)
                {
                    return ProfileSprites[index]; // 플레이어 프로필
                }
                else
                {
                    Debug.LogWarning($"플레이어 프로필 인덱스({index})가 범위를 벗어났습니다. 기본값을 사용합니다.");
                    return ProfileSprites[0]; // 기본 플레이어 프로필
                }
            }
            else
            {
                Debug.LogError("플레이어 프로필 배열이 초기화되지 않았습니다.");
                return null;
            }
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
