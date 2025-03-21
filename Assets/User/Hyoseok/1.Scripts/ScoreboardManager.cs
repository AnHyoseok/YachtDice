using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
public class ScoreboardManager : MonoBehaviour
{
    public RectTransform scoreboardBackground;  
    public RectTransform scoreboardOutline;   

    private int gameMode;
    public static ScoreboardManager instance;

    private Dictionary<Player, ScoreboardEntry> playerEntries = new Dictionary<Player, ScoreboardEntry>();
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
           
            gameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"];
            AdjustScoreboardSize();
        }
    }

    //사이즈조절
    void AdjustScoreboardSize()
    {
        if (gameMode == 1) // 1:1 모드
        {
            scoreboardBackground.anchoredPosition = new Vector2(0, scoreboardBackground.anchoredPosition.y);
            scoreboardBackground.sizeDelta = new Vector2(540, scoreboardBackground.sizeDelta.y);

            scoreboardOutline.anchoredPosition = new Vector2(0, scoreboardOutline.anchoredPosition.y);
            scoreboardOutline.sizeDelta = new Vector2(520, scoreboardOutline.sizeDelta.y);
        }
        else if (gameMode == 2) // 2:2 모드 
        {
            scoreboardBackground.anchoredPosition = new Vector2(123, scoreboardBackground.anchoredPosition.y);
            scoreboardBackground.sizeDelta = new Vector2(782, scoreboardBackground.sizeDelta.y); 

            scoreboardOutline.anchoredPosition = new Vector2(123, scoreboardOutline.anchoredPosition.y);
            scoreboardOutline.sizeDelta = new Vector2(752, scoreboardOutline.sizeDelta.y);
        }
    }
    public void Register(Player player, ScoreboardEntry entry)
    {
        if (!playerEntries.ContainsKey(player))
        {
            playerEntries.Add(player, entry);
        }
    }

    public ScoreboardEntry GetEntry(Player player)
    {
        if (playerEntries.ContainsKey(player))
            return playerEntries[player];
        return null;
    }

    public ScoreboardEntry GetLocalPlayerEntry()
    {
        return GetEntry(PhotonNetwork.LocalPlayer);
    }

    public void HideLocalScore()
    {
        GetLocalPlayerEntry()?.HideAll();
    }

    public void ShowLocalScore()
    {
        GetLocalPlayerEntry()?.ShowAll();
    }

    public void HighlightLocalScore(string category)
    {
        GetLocalPlayerEntry()?.HighlightScore(category);
    }
}
