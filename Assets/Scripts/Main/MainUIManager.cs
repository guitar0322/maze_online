using BackEnd.Tcp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    private static MainUIManager _instance;
    public GameObject rankingPanel;
    public GameObject matchWaitPanel;
    public GameObject matchCompletePanel;
    public Text log;

    public static MainUIManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(MainUIManager)) as MainUIManager;
            }
            return _instance;
        }
    }
    public void RequestMatching()
    {
        bool isSuccess = MatchingManager.instance.RequestMatch();
        if (isSuccess == true)
        {
            matchWaitPanel.SetActive(true);
        }
    }
    public void CreateLog(string text)
    {
        StartCoroutine("LogCoroutine", text);
    }

    IEnumerable LogCoroutine(string text)
    {
        log.text = text;
        log.gameObject.SetActive(true);
        log.GetComponent<Animator>().SetTrigger("start");
        yield return new WaitForSeconds(1.5f);
        log.gameObject.SetActive(false);
    }
    public void CancelMatching()
    {
        MatchingManager.instance.CancelMatch();
        matchWaitPanel.SetActive(false);
    }
    public void ShowRankingPanel()
    {
        CommonFunc.instance.SetActiveTarget(rankingPanel);
    }
    public void CloseMatchingCompletePanel()
    {
        matchCompletePanel.SetActive(false);
    }
    public void MatchingComplete()
    {
        matchCompletePanel.SetActive(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        MatchingManager.instance.JoinMatchMakingServer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
