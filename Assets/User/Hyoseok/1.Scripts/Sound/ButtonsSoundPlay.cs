using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class SetButtonsSoundPlay : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    public Button[] buttons;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AddButtons();
    }

    void PlayHoverSound()
    {
        audioSource.PlayOneShot(hoverClip);
    }

    void PlayClickSound()
    {
        audioSource.PlayOneShot(clickClip);
    }

    // 모든 버튼에 AudioSource를 추가하고 클릭 이벤트를 연결하는 메서드
    void AddButtons()
    {
        // 씬 내 모든 버튼을 찾습니다.
        buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        //찾아온 모든 버튼 중 "Teleport"버튼들은 텔레포트 사운드, 다른 모든 버튼들은 클릭 사운드 AddListener로 이벤트 할당
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => PlayClickSound());

            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

            //호버사운드
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => PlayHoverSound());
            trigger.triggers.Add(entry);
        }
    }
}


