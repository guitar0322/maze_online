using BackEnd.Tcp;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    private static InGameUIManager _instance;
    public GameObject matchInfoPanel;
    public GameObject waitPanel;
    public GameObject roundInfoPanel;
    public GameObject[] matchUserInfoList = new GameObject[MatchingManager.USER_NUM];
    public GameObject[] roundInfoList = new GameObject[MatchingManager.USER_NUM];
    public GameObject round;
    public Sprite[] roundSprite = new Sprite[5];
    public Text guide;
    public Text commWallText;
    public Text specialWallText;

    public static InGameUIManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(InGameUIManager)) as InGameUIManager;
            }
            return _instance;
        }
    }

    public void StartRound()
    {
        round.GetComponent<Image>().sprite = roundSprite[GameManagement.instance.roundInfo.round];
        round.GetComponent<Animator>().SetTrigger("Start");
        GameManagement.instance.CreateStartWall();
        ChangeCommonWallText(GameManagement.instance.roundInfo.commonWall);
        ChangeSpecialWallText(GameManagement.instance.roundInfo.specialWall);
    }
    public void ParseUserInfo()
    {
        Debug.Log("Parse UserInfo");
        if(MatchingManager.USER_NUM != MatchingManager.instance.matchInfo.userlist.Count)
        {
            guide.text = "경쟁상대의 접속을 기다립니다";
        }
        else
        {
            guide.text = "이제 곧 게임이 시작됩니다";
        }
        for(int i = 0; i < MatchingManager.instance.matchInfo.userlist.Count; i++)
        {
            MatchUserGameRecord curUser = MatchingManager.instance.matchInfo.userlist[i].matchUserGameRecord;
            matchUserInfoList[i].transform.GetChild(0).GetComponent<Text>().text = curUser.m_nickname;
            matchUserInfoList[i].transform.GetChild(1).GetComponent<Text>().text = curUser.m_numberOfWin+"승";
            matchUserInfoList[i].transform.GetChild(2).GetComponent<Text>().text = curUser.m_numberOfDefeats+"패";
        }
    }

    public void ChangeCommonWallText(int commonWallNum)
    {
        commWallText.text = ""+commonWallNum;
    }

    public void ChangeSpecialWallText(int specialWallNum)
    {
        specialWallText.text = "" + specialWallNum;
    }
    public void OpenWaitPanel()
    {
        waitPanel.SetActive(true);
    }
    public void CloseWaitPanel()
    {
        waitPanel.SetActive(false);
    }
    public SortedList SortTotalTime()
    {
        SortedList result = new SortedList();
        for(int i = 0; i < MatchingManager.USER_NUM - 1; i++)
        {
            if(MatchingManager.instance.matchInfo.userlist[i].totalTime != MatchingManager.instance.matchInfo.userlist[i+1].totalTime)
                result.Add(MatchingManager.instance.matchInfo.userlist[i].totalTime, i);
            else
            {
                result.Add(MatchingManager.instance.matchInfo.userlist[i].totalTime+0.000001f, i);
            }
        }


        return result;
    }
    public void OpenRoundInfoPanel()
    {
        SortedList sortedResult = SortTotalTime();
        for(int i = MatchingManager.USER_NUM-1; i >= 0; i--)
        {
            MatchingManager.InGameUserInfo curUserInfo = MatchingManager.instance.matchInfo.userlist[Convert.ToInt32(sortedResult.GetByIndex(i))];
            roundInfoList[i].transform.GetChild(0).GetComponent<Text>().text
                = curUserInfo.matchUserGameRecord.m_nickname;
            roundInfoList[i].transform.GetChild(1).GetComponent<Text>().text
                = curUserInfo.totalTime.ToString("N2");
            roundInfoList[i].transform.GetChild(2).GetComponent<Text>().text
                = "+"+curUserInfo.time.ToString("N2");
        }
        StartCoroutine("RoundInfoPanel");
    }
    IEnumerable RoundInfoPanel()
    {
        roundInfoPanel.SetActive(true);
        yield return new WaitForSeconds(5);
        roundInfoPanel.SetActive(false);
        GameManagement.instance.InitRound();
    }
    public void CloseMatchInfoPanel()
    {
        matchInfoPanel.SetActive(false);
        GameManagement.instance.InitRound();
    }
    // Start is called before the first frame update
    void Start()
    {
        ParseUserInfo();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
