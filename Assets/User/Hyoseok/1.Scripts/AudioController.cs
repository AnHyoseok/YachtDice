using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;

    public AudioSource audioSource;
    public AudioClip cupShakeSound;
    public AudioClip diceRollSound;
    public AudioClip selectDiceSound;
    public AudioClip arrayDiceSound;
    public AudioClip selectScoreSound;
    public AudioClip[] scoreTextSound;

    private string lastPlayedCategory = "";
    //페이드 
    public AudioClip fadeinSound;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void PlayCupShake()
    {
        PlayClip(cupShakeSound);
    }

    public void PlayDiceRoll()
    {
        PlayClip(diceRollSound);
    }
    public void PlayselectDice()
    {
        PlayClip(selectDiceSound);
    }

   public void PlayselectScore()
    {
        PlayClip(selectScoreSound);
    }
    public void PlayarrayDice()
    {
        PlayClip(arrayDiceSound);
    }
    public void PlayScoreTextSound(string category)
    {
        // 점수 카테고리에 맞는 사운드 재생
        if (category == lastPlayedCategory) return;
        switch (category)
        {
            case DiceScore.YAHTZEE:
                audioSource.PlayOneShot(scoreTextSound[0]);
                break;
            case DiceScore.FOUR_KIND:
                audioSource.PlayOneShot(scoreTextSound[1]);
                break;
            case DiceScore.LARGE_STRAIGHT:
                audioSource.PlayOneShot(scoreTextSound[2]); 
                break;
            case DiceScore.SMALL_STRAIGHT:
                audioSource.PlayOneShot(scoreTextSound[3]); 
                break;
            case DiceScore.FULL_HOUSE:
                audioSource.PlayOneShot(scoreTextSound[4]); 
                break;
        }
    }

    //페이드
    public void Playfadein()
    {
        PlayClip(fadeinSound);
    }
    private void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.Stop(); 
        audioSource.clip = clip;
        audioSource.Play();
    }
}
