using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    private static UserInfo _instance = null;

    public bool bgm;
    public bool effectSound;
    public string nickname;
    public string win;
    public string loss;
    public string score;
    public string ticket;
    public string stage;
    public string inDate;
    public string tutorial;
    public string DBindate;
    public bool singlePanelFlag;
    public bool matching;
    public bool isBeginerMode;
    public static UserInfo instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(UserInfo)) as UserInfo;
                if (_instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "UserInfo";
                    _instance = container.AddComponent(typeof(UserInfo)) as UserInfo;
                }
            }

            return _instance;
        }
    }
    // Start is called before the first frame update


    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
