using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.UI;
using BackEnd;
using LitJson;
using UnityEngine.SceneManagement;
using System;

public class Login : MonoBehaviour
{
    // Start is called before the first frame update
    public Text log;
    public Image NicknamePanel;
    public InputField NicknameInput;
    public InputField TestID;
    public InputField TestPassword;
    public Text NicknameLog;
    public string testGoogleToken;
    public bool debug;
    public MatchingManager matchingManger;
    public string testID;
    public string testPass;
    void Start()
    {
#if UNITY_ANDROID
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
            .Builder()
            .RequestServerAuthCode(false)
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif
        Backend.Initialize(backendCallback);
        log.text = "뒤끝 서버 초기화중....";
        if (Backend.IsInitialized == true)
        {
#if UNITY_ANDROID
            GPGSLogin();
#endif
            if(debug == true)
            {
                BackendReturnObject login = Backend.BMember.LoginWithTheBackendToken();
                if (login.IsSuccess())
                    GetBackendUserInfo();
                else
                {
                    Debug.Log("test login is failed : " + login.GetErrorCode() + " , " + login.GetMessage());
                }
            }
        }
    }
    void backendCallback(BackendReturnObject bro)
    {
        if (bro.IsSuccess())
        {
            Debug.Log( "backend Init Success\n");
            log.text = "뒤끝 매치에 접속하는중...";
            //MatchingManager.instance.JoinMatchMakingServer();
        }
        else
        {
            Debug.Log("backend Init Failed\n");
        }
    }
    public void CustomLogin()
    {
        BackendReturnObject login = Backend.BMember.CustomLogin(TestID.text, TestPassword.text);
        if (login.IsSuccess())
            GetBackendUserInfo();
        else
        {
            Debug.Log("test login is failed");
        }
    }
    public void GPGSLogin()
    {
        Debug.Log("Try to login on GPGS...\n");
        log.text = "구글 서비스 로그인중....";
        BackendReturnObject BRO;
        if (Social.localUser.authenticated == true)
        {
            Debug.Log("google login already");
            log.text = "구글과 뒤끝 페더레이션 인증중...";
            BRO = Backend.BMember.AuthorizeFederation(GetTokens(), FederationType.Google, "gpgs");
            if (BRO.IsSuccess() == true)
                GetBackendUserInfo();
            else
                Debug.Log("Backend Federation Failed\n");
        }
        else
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    log.text = "구글과 뒤끝 페더레이션 인증중...";
                    Debug.Log("gpgs login success");
                    string googleToken = GetTokens();
                    BRO = Backend.BMember.AuthorizeFederation(googleToken, FederationType.Google, "gpgs");
                    if (BRO.IsSuccess() == true)
                        GetBackendUserInfo();
                    else
                        Debug.Log("Backend login federation failed");
                }
                else
                {
                    Debug.Log("gpgs login is failed for some reason");
                }

            });
        }
    }

    public void GetBackendUserInfo()
    {
        log.text = "유저 정보 로딩중...";
        BackendReturnObject Userinfo = Backend.BMember.GetUserInfo();
        Debug.Log(Userinfo.GetReturnValue());
        JsonData nickname = Userinfo.GetReturnValuetoJSON()["row"]["nickname"];
        JsonData inDate = Userinfo.GetReturnValuetoJSON()["row"]["inDate"];
        if (nickname == null)
        {
            Debug.Log("nickname is null");
            NicknamePanel.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("nickname is " + nickname);
            GetUserInfo();
        }
    }
    
    public void GetUserInfo()
    {
        BackendReturnObject getInfoReturn = Backend.GameInfo.GetPrivateContents("character");
        if (getInfoReturn.IsSuccess())
        {
            JsonData userInfo = getInfoReturn.GetReturnValuetoJSON()["rows"][0];
            string nicknameKey = "nickname";
            string winKey = "win";
            string lossKey = "loss";
            string ticketKey = "ticket";
            string scoreKey = "score";
            string tutorialKey = "tutorial";
            string stageKey = "stage";
            string indateKey = "indate";
            string dbIndateKey = "inDate";


            if (userInfo.Keys.Contains(nicknameKey) &&
                userInfo.Keys.Contains(winKey) &&
                userInfo.Keys.Contains(lossKey) &&
                userInfo.Keys.Contains(ticketKey) &&
                userInfo.Keys.Contains(scoreKey) &&
                userInfo.Keys.Contains(tutorialKey) &&
                userInfo.Keys.Contains(stageKey) &&
                userInfo.Keys.Contains(indateKey) &&
                userInfo.Keys.Contains(dbIndateKey))
            {
                UserInfo.instance.nickname = userInfo[nicknameKey]["S"].ToString();
                UserInfo.instance.win = userInfo[winKey]["N"].ToString();
                UserInfo.instance.loss = userInfo[lossKey]["N"].ToString();
                UserInfo.instance.ticket = userInfo[ticketKey]["N"].ToString();
                UserInfo.instance.score = userInfo[scoreKey]["N"].ToString();
                UserInfo.instance.tutorial = userInfo[tutorialKey]["BOOL"].ToString();
                UserInfo.instance.stage = userInfo[stageKey]["N"].ToString();
                UserInfo.instance.inDate = userInfo[indateKey]["S"].ToString();
                UserInfo.instance.DBindate = userInfo[dbIndateKey]["S"].ToString();

                Debug.Log(UserInfo.instance.nickname + " , " +
                    UserInfo.instance.win + " , " +
                    UserInfo.instance.loss + " , " +
                    UserInfo.instance.ticket + " , " +
                    UserInfo.instance.score + " , " +
                    UserInfo.instance.tutorial + " , " +
                    UserInfo.instance.stage + " , " +
                    UserInfo.instance.inDate + " , " +
                    UserInfo.instance.DBindate);

                Backend.GameInfo.UpdateRTRankTable("character", "score", Convert.ToInt32(UserInfo.instance.score), UserInfo.instance.DBindate);
            }
            else
            {
                CommonFunc.instance.MakePopup("로그인 정보를 가져오는 중 오류가 발생하였습니다");
            }

            if (UserInfo.instance.tutorial.Equals("False"))
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                SceneManager.LoadScene(3);
            }
        }
    }

    public void RequestCreateNickname()
    {
        BackendReturnObject createNicknameReturn = Backend.BMember.CreateNickname(NicknameInput.text);
        Debug.Log("Request Create Nickname is " + NicknameInput.text);
        if (createNicknameReturn.IsSuccess())
        {
            InsertUserInfo();
            GetUserInfo();
        }
        else
        {
            switch (createNicknameReturn.GetStatusCode())
            {
                case "400":
                    Debug.Log("잘못된 닉네임" + createNicknameReturn.GetStatusCode());
                    NicknameLog.gameObject.SetActive(true);
                    NicknameLog.text = "* 잘못된 닉네임 형식";
                    break;
                case "409":
                    Debug.Log("중복된 닉네임" + createNicknameReturn.GetStatusCode());
                    NicknameLog.gameObject.SetActive(true);
                    NicknameLog.text = "* 중복된 닉네임";
                    break;
            }
        }
    }
    public void InsertUserInfo()
    {
        Param Info = new Param();
        string UserInfoTableName = "character";

        Info.Add("tutorial", false);
        Info.Add("nickname", NicknameInput.text);
        Info.Add("score", 0);
        Info.Add("win", 0);
        Info.Add("loss", 0);
        Info.Add("ticket", 5);
        Info.Add("stage", 0);
        Info.Add("indate", DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss"));

        BackendReturnObject infoInsertReturn = Backend.GameInfo.Insert(UserInfoTableName, Info);
        if (infoInsertReturn.IsSuccess())
            Debug.Log("info insert success");
    }
    public string GetTokens()
    {
#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.localUser.authenticated)
        {
            // 유저 토큰 받기 첫번째 방법
            string _IDtoken = PlayGamesPlatform.Instance.GetIdToken();
            // 두번째 방법
            // string _IDtoken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
            Debug.Log(_IDtoken);
            return _IDtoken;
        }
        else
        {
            Debug.Log("접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
            return null;
        }
#endif
        return null;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
