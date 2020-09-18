using BackEnd;
using BackEnd.RealTime;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
    public string uuid;
    public bool rankingLoadFlag;
    public Text buttonText;
    public GameObject[] rankList = new GameObject[10];
    private bool rankingToggle;
    private BackendReturnObject allRanking;
    private BackendReturnObject myRanking;

    // Start is called before the first frame update
    void Start()
    {
        rankingLoadFlag = true;
        rankingToggle = true;
        UpdateRanking();
        allRanking = Backend.RTRank.GetRTRankByUuid(uuid);
        if(allRanking.IsSuccess() == false)
        {
            rankingLoadFlag = false;
            Debug.Log("allranking fail Message : " + allRanking.GetMessage());
        }
        else
        {
            Debug.Log(allRanking.GetReturnValue());
        }

        myRanking = Backend.RTRank.GetMyRTRank(uuid, 5);
        if (myRanking.IsSuccess() == false)
        {
            Debug.Log("myranking fail Message : " + myRanking.GetMessage());
            rankingLoadFlag = false;
        }
        else
        {
            Debug.Log(myRanking.GetReturnValue());
        }
    }
    public void UpdateRanking()
    {
        Backend.GameInfo.UpdateRTRankTable("character", "score", Convert.ToInt32(UserInfo.instance.score), UserInfo.instance.DBindate);
    }
    public void ParseAllRanking()
    {
        JsonData rankJson = allRanking.GetReturnValuetoJSON()["rows"];
        for(int i = 0; i < rankJson.Count; i++)
        {
            string rank = rankJson[i]["rank"]["N"].ToString() + "등";
            string nickname = rankJson[i]["nickname"].ToString();
            string score = rankJson[i]["score"]["N"].ToString() + "점";
            rankList[i].transform.GetChild(0).GetComponent<Text>().text = rank;
            rankList[i].transform.GetChild(1).GetComponent<Text>().text = nickname;
            rankList[i].transform.GetChild(2).GetComponent<Text>().text = score;
            Debug.Log(rank + " , " + nickname + " , " + score);
        }
        buttonText.text = "내 순위 보기";
    }

    public void ParseMyRanking()
    {
        JsonData rankJson = myRanking.GetReturnValuetoJSON()["rows"];
        for (int i = 0; i < rankJson.Count; i++)
        {
            string rank = rankJson[i]["rank"]["N"].ToString() + "등";
            string nickname = rankJson[i]["nickname"].ToString();
            string score = rankJson[i]["score"]["N"].ToString() + "점";
            rankList[i].transform.GetChild(0).GetComponent<Text>().text = rank;
            rankList[i].transform.GetChild(1).GetComponent<Text>().text = nickname;
            rankList[i].transform.GetChild(2).GetComponent<Text>().text = score;
            Debug.Log(rank + " , " + nickname + " , " + score);
        }
        buttonText.text = "전체 순위 보기";
    }
    public void RankingToggle()
    {
        if(rankingToggle == true)
        {
            ParseMyRanking();
        }
        else
        {
            ParseAllRanking();
        }
        rankingToggle = !rankingToggle;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
