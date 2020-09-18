using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchingComplete : MonoBehaviour
{
    private static float TIME = 5;
    public Slider timeBar;
    public Text guide;
    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        timeBar.value = 1;
        guide.text = "매치완료!";
    }
    public void AcceptMatch()
    {
        MatchingManager.instance.AcceptMatch();
        guide.text = "수락 대기중...";
    }

    public void DenyMatch()
    {
        MatchingManager.instance.DenyMatch();
        this.gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        timeBar.value -= Time.deltaTime / TIME;
        if(timeBar.value <= 0)
        {
            Debug.Log("deny match");
            MatchingManager.instance.DenyMatch();
            MainUIManager.instance.CreateLog("매칭이 성사되지 않았습니다");
            this.gameObject.SetActive(false);
        }
    }
}
