using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // List�� ����Ϸ��� �߰�

public class ScoreboardTurnActivator : MonoBehaviour
{
    public GameObject playerA;
    public GameObject playerB;
    public bool isPlayerATurn = false; // ������ �� �ƹ��� ���� �ƴ�
    public bool isPlayerBTurn = false;

    private List<Image> playerATurnImages = new List<Image>(); // ���� ���� TurnImage�� ���� ����Ʈ
    private List<Image> playerBTurnImages = new List<Image>();

    void Start()
    {
        // Player_A�� Player_B ������ "TurnImage"�� ��� ã�� ����Ʈ�� ����
        FindTurnImages(playerA, playerATurnImages);
        FindTurnImages(playerB, playerBTurnImages);
    }

    private void FindTurnImages(GameObject player, List<Image> turnImages)
    {
        if (player == null) return;

        Image[] images = player.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img.gameObject.name == "TurnImage")
            {
                turnImages.Add(img); // TurnImage�� ����Ʈ�� �߰�
            }
        }
    }

    void Update()
    {
        // Player_A�� TurnImage Ȱ��ȭ/��Ȱ��ȭ
        foreach (var img in playerATurnImages)
        {
            img.gameObject.SetActive(isPlayerATurn); // �� TurnImage�� isPlayerATurn�� �°� Ȱ��ȭ/��Ȱ��ȭ
        }

        // Player_B�� TurnImage Ȱ��ȭ/��Ȱ��ȭ
        foreach (var img in playerBTurnImages)
        {
            img.gameObject.SetActive(isPlayerBTurn); // �� TurnImage�� isPlayerBTurn�� �°� Ȱ��ȭ/��Ȱ��ȭ
        }

        // Ű �Է��� ���� Player_A ���� ���
        if (Input.GetKeyDown(KeyCode.Alpha1))  // ���� 1�� ������ ��
        {
            isPlayerATurn = !isPlayerATurn;  // isPlayerATurn ���� ���
        }

        // Ű �Է��� ���� Player_B ���� ���
        if (Input.GetKeyDown(KeyCode.Alpha2))  // ���� 2�� ������ ��
        {
            isPlayerBTurn = !isPlayerBTurn;  // isPlayerBTurn ���� ���
        }
    }
}