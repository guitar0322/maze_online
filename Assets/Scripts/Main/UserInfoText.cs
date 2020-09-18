using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoText : MonoBehaviour
{
    public Text nickname;
    public Text win;
    public Text loss;
    public Text score;

    // Start is called before the first frame update
    void Start()
    {
        nickname.text = UserInfo.instance.nickname;
        win.text = UserInfo.instance.win + "승";
        loss.text = UserInfo.instance.loss + "패";
        score.text = UserInfo.instance.score + "점";
        Debug.Log("In UserInfoText : " + UserInfo.instance.nickname);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
