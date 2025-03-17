using UnityEngine;
using System.Collections;

public class AICharacterSpeech : MonoBehaviour
{
    public SpeechBubble speechBubble;
    [SerializeField]private float nextSpeechTime=15f;

    private string[] dialogueLines = new string[]
    {   
        //인사말
        "Hello!\nHave a great day!",
        "Let me know\n if you need any help.",
        "Teamwork is the key\n to success!",
        "Is this your first time\n here? Nice to meet you!",

        //다이스 게임 팁
        "Try to aim for a\n Yahtzee early in the game!",
        "Keep high-value dice\n for better scoring chances.",
        "If you can't get a\n good roll, go for chance!",
        "Full house gives\n decent points in a pinch.",
        "Don't waste your Yahtzee!\n Save it for later turns.",
        "Straights are tricky.\n Keep a sequence if possible!",
        "Rolling again?\n Make sure it’s worth the risk!",
        "Lower scores are fine.\n Keep your options open!",
        "Three of a kind?\n Make sure it's high value!",
        "Need a bonus?\n Fill in the upper section first!"
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
            yield return new WaitForSeconds(nextSpeechTime); 
        }
    }

    private void SpeakRandom()
    {
        if (speechBubble != null)
        {
            string randomDialogue = dialogueLines[Random.Range(0, dialogueLines.Length)];
            speechBubble.ShowMessage(randomDialogue);
        }
    }
}
