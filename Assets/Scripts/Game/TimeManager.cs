using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public Slider timeBar;
    public Image fillArea;
    public Text time;
    public int colorChangeFlag;
    public Sprite[] timeBarColor = new Sprite[3];
    private static float ROUND_TIME = 5;
    // Start is called before the first frame update
    void Start()
    {
    }
    private void OnEnable()
    {
        timeBar.value = 1;
        colorChangeFlag = 0;
    }
    // Update is called once per frame
    void Update()
    {
        timeBar.value -= Time.deltaTime / ROUND_TIME;
        time.text = "" + (int)(timeBar.value * ROUND_TIME);
        if(timeBar.value > 0.66f && colorChangeFlag == 0)
        {
            fillArea.sprite = timeBarColor[0];
            colorChangeFlag = 1;
        }
        else if (timeBar.value <= 0.66f && colorChangeFlag == 1)
        {
            fillArea.sprite = timeBarColor[1];
            colorChangeFlag = 2;
        }
        else if(timeBar.value <= 0.3f && colorChangeFlag == 2)
        {
            fillArea.sprite = timeBarColor[2];
            colorChangeFlag = 3;
        }
        else if(timeBar.value <= 0)
        {
            GameManagement.instance.runner.SetActive(true);
            GameManagement.instance.touchManager.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
