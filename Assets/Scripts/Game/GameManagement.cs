using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using BackEnd.Tcp;
using UnityEngine.Networking.Match;

public partial class GameManagement : MonoBehaviour
{
    private static GameManagement _instance;
    public bool debugMode;
    private static int TILE_SIZE = 1;
    public static int VER = 19;
    public static int HOR = 17;
    public static int EMPTY = 0;
    public static int OUTLINE = 1;
    public static int START_WALL = 2;
    public static int COMMON_WALL = 3;
    public static int SPECIAL_WALL = 4;
    public static int FINAL_ROUND = 4;

    public int[,] mazeMatrix = new int[VER, HOR];
    public RoundInfo roundInfo;
    public GameObject timeManager;
    public GameObject touchManager;
    public GameObject runner;
    public bool isHighlight;
    public static GameManagement instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(GameManagement)) as GameManagement;
                if (_instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "GameManagement";
                    _instance = container.AddComponent(typeof(GameManagement)) as GameManagement;
                }
            }
            return _instance;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        InitWallList();
        isHighlight = false;
    }
    
    public bool IsValidField(int ver, int hor)
    {
        if (roundInfo.field[ver].fieldRow[hor] == EMPTY &&
            roundInfo.field[ver+1].fieldRow[hor] == EMPTY &&
            roundInfo.field[ver].fieldRow[hor+1] == EMPTY &&
            roundInfo.field[ver+1].fieldRow[hor+1] == EMPTY)
        {
            return true;
        }
        else
            return false;
    }

    public void SetField(int ver, int hor, int info)
    {
        roundInfo.field[ver].fieldRow[hor] = info;
        roundInfo.field[ver+1].fieldRow[hor] = info;
        roundInfo.field[ver].fieldRow[hor+1] = info;
        roundInfo.field[ver+1].fieldRow[hor+1] = info;

    }
    public void InitRound()
    {
        if(debugMode == true)
        {
            if (roundInfo == null)
                roundInfo = new RoundInfo(0);
            else
                roundInfo = new RoundInfo(roundInfo.round + 1);
            CreateFieldInfo();
            InGameUIManager.instance.StartRound();
        }
        else if(MatchingManager.instance.matchInfo.isSuperGamer == true)
        {
            Debug.Log("isSuperGamer");
            if (roundInfo == null)
                roundInfo = new RoundInfo(0);
            else
                roundInfo = new RoundInfo(roundInfo.round + 1);
            CreateFieldInfo();
            for(int i = 0; i < MatchingManager.USER_NUM; i++)
            {
                MatchingManager.instance.matchInfo.userlist[i].time = 0;
            }
            RoundInfoMessage roundInfoMessage;
            roundInfoMessage = new RoundInfoMessage(roundInfo.field, roundInfo.round, roundInfo.startWall, roundInfo.commonWall, 
                                        roundInfo.specialWall, roundInfo.startIdxList);
            MatchingManager.instance.SendDataToInGame<RoundInfoMessage>(roundInfoMessage);
        }
    }

    public void OnRecieve(MatchRelayEventArgs args)
    {
        if (args.BinaryUserData == null)
        {
            Debug.LogWarning(string.Format("빈 데이터가 브로드캐스팅 되었습니다.\n{0} - {1}", args.From, args.ErrInfo));
            // 데이터가 없으면 그냥 리턴
            return;
        }
        Message msg = DataParser.ReadJsonData<Message>(args.BinaryUserData);
        if (msg == null)
        {
            return;
        }
        //if (args.From.NickName.Equals(UserInfo.instance.nickname))
        //{
        //    return;
        //}
        switch (msg.type)
        {
            case Protocol.Type.RoundStart:
                Debug.Log("receive round start msg");
                RoundInfoMessage roundInfoMessage = DataParser.ReadJsonData<RoundInfoMessage>(args.BinaryUserData);
                Debug.Log("roundInfoMessage's field");
                string debugStr = "";
                for(int i = 0; i < VER; i++)
                {
                    for(int j = 0; j < HOR; j++)
                    {
                        debugStr += roundInfoMessage.fieldInfo[i].fieldRow[j];
                    }
                    debugStr += "\n";
                }

                Debug.Log(debugStr);
                roundInfo = new RoundInfo(roundInfoMessage.round, roundInfoMessage.startNum, roundInfoMessage.commonNum, 
                    roundInfoMessage.specialNum, roundInfoMessage.fieldInfo, roundInfoMessage.startIdxList);
                Debug.Log(string.Format("roundInfo : {0}, {1}, {2}", roundInfo.startWall, roundInfo.commonWall, roundInfo.specialWall));
                InGameUIManager.instance.StartRound();
                break;
            case Protocol.Type.RoundResult:
                RoundResultMessage roundResultMessage = DataParser.ReadJsonData<RoundResultMessage>(args.BinaryUserData);
                for(int i = 0; i < MatchingManager.USER_NUM; i++)
                {
                    MatchUserGameRecord curUser = MatchingManager.instance.matchInfo.userlist[i].matchUserGameRecord;
                    if (curUser.m_nickname.Equals(roundResultMessage.nickname))
                    {
                        MatchingManager.instance.matchInfo.userlist[i].time = roundResultMessage.time;
                        MatchingManager.instance.matchInfo.userlist[i].totalTime += roundResultMessage.time;
                        MatchingManager.instance.matchInfo.userlist[i].field = roundResultMessage.fieldInfo;
                        Debug.Log(string.Format("round record : {0}, {1}", 
                            roundResultMessage.nickname, roundResultMessage.time));
                    }
                }

                if(MatchingManager.instance.matchInfo.isSuperGamer == true)
                {
                    bool roundEnd = true;
                    for (int i = 0; i < MatchingManager.USER_NUM; i++)
                    {
                        if(MatchingManager.instance.matchInfo.userlist[i].isConnect == false)
                            continue;
                        else if(MatchingManager.instance.matchInfo.userlist[i].time == 0)
                            roundEnd = false;
                    }
                    if(roundEnd == true)
                    {
                        int bestPlayIdx = CalcBestPlayer();
                        if (bestPlayIdx == -1)
                        {
                            Debug.Log("1등 플레이어를 찾지 못했습니다");
                        }
                        else
                        {
                            BestPlayerMessage bestPlayerMessage =
                                new BestPlayerMessage(MatchingManager.instance.matchInfo.userlist[bestPlayIdx].field,
                                                    MatchingManager.instance.matchInfo.userlist[bestPlayIdx].matchUserGameRecord.m_nickname);
                            MatchingManager.instance.SendDataToInGame<BestPlayerMessage>(bestPlayerMessage);
                        }
                    }
                }
                break;

            case Protocol.Type.BestPlayer:
                InGameUIManager.instance.CloseWaitPanel();
                InGameUIManager.instance.OpenRoundInfoPanel();
                break;

        }
    }

    public int CalcBestPlayer()
    {
        float time = 0;
        int result = -1;
        for(int i  = 0; i < MatchingManager.USER_NUM; i++)
        {
            if (MatchingManager.instance.matchInfo.userlist[i].isConnect == false)
                continue;
            else if (time < MatchingManager.instance.matchInfo.userlist[i].time)
            {
                result = i;
                time = MatchingManager.instance.matchInfo.userlist[i].time;
            }
        }
        return result;
    }

    public class RoundInfo
    {
        public int round;
        public int startWall;
        public int commonWall;
        public int specialWall;
        public List<FieldRow> field;
        public List<IdxRow> startIdxList;
        public RoundInfo(int _round)
        {
            round = _round;
            startWall = UnityEngine.Random.Range(MIN_START, MAX_START);
            commonWall = UnityEngine.Random.Range(MIN_COMMON, MAX_COMMON);
            specialWall = UnityEngine.Random.Range(0, MAX_SPECIAL);
            startIdxList = new List<IdxRow>();
            for(int i = 0; i < MAX_START; i++)
            {
                startIdxList.Add(new IdxRow(new int[2]));
            }
            field = new List<FieldRow>();
            for(int i = 0; i < VER; i++)
            {
                field.Add(new FieldRow(new int[HOR]));
            }

            for(int i = 0; i < VER; i++)
            {
                for(int j = 0; j < HOR; j++)
                {
                    if(i == 0 || j == 0 || i == VER-1 || j == HOR - 1)
                    {
                        field[i].fieldRow[j] = OUTLINE;
                    }
                }
            }
            field[VER - 1].fieldRow[HOR / 2] = EMPTY;
            field[VER - 1].fieldRow[HOR / 2 - 1] = EMPTY;
            field[VER - 1].fieldRow[HOR / 2 + 1] = EMPTY;

            field[0].fieldRow[HOR / 2] = EMPTY;
            field[0].fieldRow[HOR / 2 - 1] = EMPTY;
            field[0].fieldRow[HOR / 2 + 1] = EMPTY;
        }
        public RoundInfo(int _round, int _startWall, int _commonWall, int _specialWall, List<FieldRow> _field, List<IdxRow> _startIdxList)
        {
            round = _round;
            startWall = _startWall;
            commonWall = _commonWall;
            specialWall = _specialWall;
            field = _field;
            startIdxList = _startIdxList;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
