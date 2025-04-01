using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class PlayerHoverEffect : MonoBehaviour
{
    public GameObject hoverEffect; // 호버 애니메이션 오브젝트
    public GameObject categories; // Select표시(주황색)할 카테고리
    public List<Image> selectImages = new List<Image>(); // SelectImage들을 담을 리스트
    public List<RectTransform> playerRectTransforms = new List<RectTransform>(); // ARectTransform(HoverAnimation위치)들을 저장해놓을 리스트
    
    private int previousHoveredIndex = -1;

    public string[] targetNames =
    {
        "Aces", "Deuces", "Threes", "Fours", "Fives", "Sixes",
        "Choice", "4 of a Kind", "Full House", "S. Straight", "L. Straight", "Yacht"
    };


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Categories 내부의 "SelectImage"를 모두 찾아 리스트에 저장
        FindSelectImage(categories, selectImages);

        

    }

    // Update is called once per frame
    void Update()
    {
        // 마우스의 위치와 겹친 RectTransform은 그 인덱스를 반환하고
        // 그 인덱스를 받아 애니메이션 위치를 결정
    }

    

    private void FindSelectImage(GameObject categories, List<Image> selectImages)
    {
        /*f (PhotonNetwork.IsMasterClient) // 본인일때 
        {

        }*/
        if (categories == null) return;

        Image[] images = categories.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            if (img.gameObject.name == "SelectImage")
            {
                selectImages.Add(img); // "SelectImage"들을 리스트에 추가
            }
        }
    }
}
