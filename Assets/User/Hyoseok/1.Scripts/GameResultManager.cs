using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager Instance;

    [Header("UI References")]
    public GameObject fadein;
    public GameObject scoreboardPanel;
    public GameObject backgroundPanel;

    public GameObject nextButton;
    public GameObject winloseCanvas;
 
    public Transform resultContentParent;
    public GameObject resultEntryPrefab;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
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
        nextButton.SetActive(true);
    }

    public void OnClickNext()
    {
        StartCoroutine(ShowResultsAfterDelay());
    }

    private IEnumerator ShowResultsAfterDelay()
    {
        scoreboardPanel.SetActive(false);
        nextButton.SetActive(false);

        yield return new WaitForSeconds(1f);

        winloseCanvas.SetActive(true);

       
    }
   
}
