using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class TutorialFunc : MonoBehaviour
{
    public void UpdateTutorial()
    {
        Param param = new Param();
        param.Add("tutorial", true);
        BackendReturnObject bro = Backend.GameInfo.Update("character", UserInfo.instance.DBindate, param);
        if (bro.IsSuccess())
        {
            SceneManager.LoadScene(3);
        }
        else
        {
            Debug.Log("User inDate : " + UserInfo.instance.DBindate);
            Debug.Log("statusCode : " + bro.GetStatusCode() + " , " + bro.GetMessage());
        }
    }

    public void NextGuide(GameObject nextGuide)
    {
        CommonFunc.instance.SetActiveTarget(nextGuide);
    }

    public void RemoveGuide(GameObject curGuide)
    {
        CommonFunc.instance.UnsetActiveTarget(curGuide);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
