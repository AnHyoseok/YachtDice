using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public PhotonNetworkManager networkManager;
    public Button createRoomButton;
    public Button joinRoomButton;

    void Start()
    {
        createRoomButton.onClick.AddListener(networkManager.CreateRoom);
        joinRoomButton.onClick.AddListener(networkManager.JoinRoom);
    }
}
