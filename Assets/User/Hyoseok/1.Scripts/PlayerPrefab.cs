using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class PlayerPrefab : MonoBehaviour
{
    public static int ProfileCount { get; private set; } = 0;
    public TextMeshProUGUI playerNameText;
    public Image profileImage;
    public static Sprite[] ProfileSprites;
    public Image readyIcon;
    public Image teamColor;
    public Sprite[] profileSprites;

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
            string displayName = isMaster ? "[Master] " + player.NickName : player.NickName;
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
}
