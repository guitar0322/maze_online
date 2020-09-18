using BackEnd.Tcp;
using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using Protocol;

public class MatchingManager : MonoBehaviour
{
    public static int USER_NUM = 2;
    private static MatchingManager _instance;
    private TcpEndPoint inGameServerEndPoint;
    private string inGameRoomToken;
    private bool isConnectMatchServer = false;
    private bool isConnectInGameServer = false;
    public bool isGameStart = false;
    private int retryCount = 0;
    public ErrorInfo errorInfo;
    public MatchInfo matchInfo;
    // Start is called before the first frame update

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public static MatchingManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType(typeof(MatchingManager)) as MatchingManager; 
                if(_instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "MatchingManager";
                    _instance = container.AddComponent(typeof(MatchingManager)) as MatchingManager;
                }
            }
            return _instance;
        }
    }
    
    void Start()
    {
        MatchMakingHandler();
        InGameEventHandler();
        ExceptionHandler();
    }

    public void JoinMatchMakingServer()
    {
        if(isConnectMatchServer == false)
            Backend.Match.JoinMatchMakingServer(out errorInfo);
    }

    void OnApplicationQuit()
    {
        Backend.Match.LeaveMatchMakingServer();
    }

    public bool RequestMatch()
    {
        if (isConnectMatchServer == true)
        {
            Backend.Match.RequestMatchMaking(MatchType.Random, MatchModeType.OneOnOne);
            return true;
        }
        else
        {
            Backend.Match.JoinMatchMakingServer(out errorInfo);
            return false;
        }
    }

    public void CancelMatch()
    {
        Backend.Match.CancelMatchMaking();
    }
    private void MatchMakingHandler()
    {
        Backend.Match.OnJoinMatchMakingServer += (JoinChannelEventArgs args) =>
        {
            Debug.Log("OnJoinMatchMakingServer : " + args.ErrInfo);
            if(args.ErrInfo == ErrorInfo.Success)
            {
                isConnectMatchServer = true;
                retryCount = 0;
            }
            else
            {
                Debug.Log("Connecting Matchserver was failed : " + args.ErrInfo.Reason);
                retryCount++;
                if (retryCount < 4)
                {
                    JoinMatchMakingServer();
                }
                else
                {
                    CommonFunc.instance.MakePopup("매칭 서버에 접속할 수 없습니다\n잠시후 시도해주세요");
                    retryCount = 0;
                }
            }
        };

        Backend.Match.OnLeaveMatchMakingServer += (LeaveChannelEventArgs args) =>
        {
            switch (args.ErrInfo.Category)
            {
                case ErrorCode.Success:
                    Debug.Log("Client success disconnecting server");
                    break;
                case ErrorCode.Exception:
                    Debug.Log("Client was disconnected by server for some reason");
                    break;
                case ErrorCode.DisconnectFromRemote:
                    Debug.Log("Leave matchserver DisconnectFromRemote");
                    break;
                default:
                    isConnectMatchServer = false;
                    break;
            }
        };

        Backend.Match.OnMatchMakingResponse += (MatchMakingResponseEventArgs args) =>
        {
            switch (args.ErrInfo)
            {
                case ErrorCode.Match_InProgress:
                    Debug.Log("Reqesting match was successed");
                    break;
                case ErrorCode.Success:
                    Debug.Log("Matching is successed");
                    inGameServerEndPoint = args.RoomInfo.m_inGameServerEndPoint;
                    inGameRoomToken = args.RoomInfo.m_inGameRoomToken;
                    Backend.Match.JoinGameServer(inGameServerEndPoint.m_address, inGameServerEndPoint.m_port,
                                                false, out errorInfo);

                    break;
                case ErrorCode.Match_MatchMakingCanceled:
                    Debug.Log("Matching is failed for some reason" + " Reason : " + args.Reason);

                    break;
                case ErrorCode.Match_InvalidMatchType:
                    Debug.Log("Invalid MatchType");
                    break;
                case ErrorCode.InvalidOperation:
                    Debug.Log("Request match error : InvalidOperation");
                    break;

            }
        };
    }
    public void AcceptMatch()
    {
        if(isConnectInGameServer)
            Backend.Match.JoinGameRoom(inGameRoomToken);
        else
        {
            Debug.Log("fail accept matching...");
        }
    }

    public void DenyMatch()
    {
        Backend.Match.LeaveGameServer();
    }
    public void InGameEventHandler()
    {
        Backend.Match.OnSessionJoinInServer += (JoinChannelEventArgs args) =>
        {
            ErrorInfo errInfo = args.ErrInfo;
            if(errInfo == ErrorInfo.Success)
            {
                Debug.Log("any player join inGameServer");
                if (args.Session.NickName.Equals(UserInfo.instance.nickname))
                {
                    isConnectInGameServer = true;
                    AcceptMatch();
                    //MainUIManager.instance.MatchingComplete();
                }
            }
            else if(errInfo.Category == ErrorCode.Exception)
            {
                Debug.Log("any player fail join inGameServer for some reason : " + errInfo.Reason);
            }
            else if(errInfo.Category == ErrorCode.Success)
            {
                Debug.Log("success reJoin inGameServer");
            }
        };

        Backend.Match.OnSessionOnline += (MatchInGameSessionEventArgs args) =>
        {
            Debug.Log("another player reJoin Ingame");
        };

        Backend.Match.OnSessionListInServer += (MatchInGameSessionListEventArgs args) =>
        {
            Debug.Log("join ingame room");
            List<InGameUserInfo> inGameUserInfoList = new List<InGameUserInfo>();
            for(int i = 0; i < args.GameRecords.Count; i++)
            {
                inGameUserInfoList.Add(new InGameUserInfo(args.GameRecords[i]));
            }
            matchInfo = new MatchInfo(inGameUserInfoList);
            for(int i = 0; i < matchInfo.userlist.Count; i++)
            {
                MatchUserGameRecord curUser = matchInfo.userlist[i].matchUserGameRecord;
                if (curUser.m_nickname.Equals(UserInfo.instance.nickname))
                {
                    if(curUser.m_isSuperGamer == true)
                    {
                        matchInfo.isSuperGamer = true;
                    }
                }
            }
            if (matchInfo.userlist.Count == USER_NUM)
                CommonFunc.instance.ChangeScene(CommonFunc.GAME_SCENE);
        };

        Backend.Match.OnMatchInGameAccess += (MatchInGameSessionEventArgs args) =>
        {
            if (args.GameRecord.m_nickname.Equals(UserInfo.instance.nickname) == false)
            {
                Debug.Log("any player join ingame room");
                if (isGameStart == false)
                {
                    matchInfo.userlist.Add(new InGameUserInfo(args.GameRecord));
                    if(matchInfo.userlist.Count == USER_NUM)
                        CommonFunc.instance.ChangeScene(CommonFunc.GAME_SCENE);
                }
            }

        };

        Backend.Match.OnMatchInGameStart += () =>
        {
            Debug.Log("game start");
            isGameStart = true;
            InGameUIManager.instance.CloseMatchInfoPanel();
        };

        Backend.Match.OnMatchRelay += (MatchRelayEventArgs args) =>
        {
            if (GameManagement.instance == null)
            {
                // 월드 매니저가 존재하지 않으면 바로 리턴
                return;
            }
            GameManagement.instance.OnRecieve(args);
        };

        Backend.Match.OnLeaveInGameServer += (MatchInGameSessionEventArgs args) =>
        {
            Debug.Log("leave ingame server");
            matchInfo = null;
            isConnectInGameServer = false;
        };

        Backend.Match.OnSessionOffline += (MatchInGameSessionEventArgs args) =>
        {
            int quitUserIdx = -1;
            for(int i = 0; i < matchInfo.userlist.Count; i++)
            {
                MatchUserGameRecord curUser = matchInfo.userlist[i].matchUserGameRecord;
                if(curUser.m_sessionId == args.GameRecord.m_sessionId)
                {
                    quitUserIdx = i;
                    break;
                }
            }
            if(quitUserIdx != -1)
            {
                matchInfo.userlist[quitUserIdx].isConnect = false;
                string debugStr = String.Format("quitUserResult : {0} disconnect", 
                    matchInfo.userlist[quitUserIdx].matchUserGameRecord.m_nickname);
            }
            else
            {
                Debug.Log("나간 플레이어의 정보를 찾을 수 없습니다");
            }
        };

        Backend.Match.OnChangeSuperGamer += (MatchInGameChangeSuperGamerEventArgs args) =>
        {
            MatchUserGameRecord newSuperGamer = args.NewSuperUserRecord;
            if (newSuperGamer.m_nickname.Equals(UserInfo.instance.nickname))
            {
                matchInfo.isSuperGamer = true;
            }
        };
    }

    public void SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        Backend.Match.SendDataToInGameRoom(byteArray);
    }
    private void ExceptionHandler()
    {
        Backend.Match.OnException += (Exception e) => {
            Debug.Log("Exception on Match : " + e);
        };
    }
    // Update is called once per frame
    void Update()
    {
        Backend.Match.poll();
    }

    public class MatchInfo
    {
        public bool isSuperGamer;
        public List<InGameUserInfo> userlist;

        public MatchInfo(List<InGameUserInfo> _userlist)
        {
            isSuperGamer = false;
            userlist = _userlist;
        }
    }

    public class InGameUserInfo
    {
        public float time;
        public float totalTime;
        public bool isConnect;
        public List<FieldRow> field;
        public MatchUserGameRecord matchUserGameRecord;
        public InGameUserInfo(MatchUserGameRecord _matchUserGameRecord)
        {
            isConnect = true;
            matchUserGameRecord = _matchUserGameRecord;
        }
    }
}
