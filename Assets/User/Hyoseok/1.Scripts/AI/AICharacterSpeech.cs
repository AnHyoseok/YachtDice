using UnityEngine;
using TMPro;
using System.Collections;

public class AICharacterSpeech : MonoBehaviour
{
    public SpeechBubble speechBubble; //  인스펙터에서 직접 할당

    private string[] dialogueLines = new string[]
    {
        "Hello! Have a great day!",
        "Are you looking for treasure here?",
        "This place might be dangerous. Be careful!",
        "Let me know if you need any help.",
        "I heard something strange nearby.",
        "I think I saw something shiny!",
        "Teamwork is the key to success!",
        "Follow this path, there might be something ahead.",
        "Wow, your gear looks cool!",
        "Is this your first time here? Nice to meet you!"
    };

    private void Start()
    {
     

        StartCoroutine(ChangeDialogueRoutine());
    }

    IEnumerator ChangeDialogueRoutine()
    {
        while (true)
        {
            SpeakRandom();
            yield return new WaitForSeconds(5f);
        }
    }

    private void SpeakRandom()
    {
        if (speechBubble != null)
        {
            //  말풍선이 비활성화되었으면 다시 활성화
            if (!speechBubble.gameObject.activeSelf)
            {
                speechBubble.gameObject.SetActive(true);
            }

            string randomDialogue = dialogueLines[Random.Range(0, dialogueLines.Length)];
            speechBubble.ShowMessage(randomDialogue,3f);
        }
     
    }
}
