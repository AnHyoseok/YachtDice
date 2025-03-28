using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance;

    [Header("UI References")]
    public GameObject fadein;
    public GameObject scoreboardPanel;
    public GameObject backgroundPanel;

    public Button nextButton;
    public GameObject winloseCanvas;
 
    public Transform resultContentParent;
    public GameObject userResultPrefab;
    private Dictionary<string, int> teamScores = new Dictionary<string, int>();
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
     
        nextButton.onClick.AddListener(() => StartCoroutine(ShowResultsAfterDelay()));

        StartResultSequence();
    }
    public void StartResultSequence()
    {
        StartCoroutine(ResultFlow());
    }

    private IEnumerator ResultFlow()
    {
        yield return new WaitForSeconds(1f);
        fadein.SetActive(true);
       
        backgroundPanel.SetActive(true);
        scoreboardPanel.transform.localPosition = Vector3.zero;
        scoreboardPanel.SetActive(true);
        nextButton.gameObject.SetActive(true);
    }


    private IEnumerator ShowResultsAfterDelay()
    {
        scoreboardPanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        winloseCanvas.SetActive(true);

        GameObject go = Instantiate(userResultPrefab, resultContentParent);
    }
   
}
