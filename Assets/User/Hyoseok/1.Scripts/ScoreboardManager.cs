using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class ScoreboardManager : MonoBehaviour
{
    public RectTransform scoreboardBackground;  
    public RectTransform scoreboardOutline;   

    private int gameMode; 

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
}
