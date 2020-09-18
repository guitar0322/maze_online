using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Protocol
{
    public enum Type : sbyte
    {
        Key = 0,        // 키(가상 조이스틱) 입력
        //GamerInfo,
        RoundStart,     // 시작 카운트
        RoundResult,
        BestPlayer,
        GameEnd,        // 게임 종료
        GameSync,       // 플레이어 재접속 시 게임 현재 상황 싱크
        Max
    }

    public class Message
    {
        public Type type;

        public Message(Type type)
        {
            this.type = type;
        }
    }

    public class RoundInfoMessage : Message
    {
        public List<FieldRow> fieldInfo;
        public int round;
        public int startNum;
        public int commonNum;
        public int specialNum;
        public List<IdxRow> startIdxList;
        public RoundInfoMessage(List<FieldRow> _fieldInfo, int _round, int _startNum, int _commonNum, int _specialNum, List<IdxRow> _startIdxList) : base(Type.RoundStart)
        {
            fieldInfo = _fieldInfo;
            round = _round;
            startNum = _startNum;
            commonNum = _commonNum;
            specialNum = _specialNum;
            startIdxList = _startIdxList;
        }
    }

    public class RoundResultMessage : Message
    {
        public List<FieldRow> fieldInfo;
        public float time;
        public string nickname;
        public RoundResultMessage(List<FieldRow> _fieldInfo, float _time, string _nickname) : base(Type.RoundResult)
        {
            fieldInfo = _fieldInfo;
            time = _time;
            nickname = _nickname;
        }
    }

    public class BestPlayerMessage : Message
    {
        public List<FieldRow> fieldInfo;
        public string nickname;
        public BestPlayerMessage(List<FieldRow> _fieldInfo, string _nickname) : base(Type.BestPlayer)
        {
            fieldInfo = _fieldInfo;
            nickname = _nickname;
        }
    }

    [System.Serializable]
    public class IdxRow
    {
        public int[] idx;
        public IdxRow(int[] _idx)
        {
            idx = _idx;
        }
    }
    [System.Serializable]
    public class FieldRow
    {
        public int[] fieldRow;
        public FieldRow(int [] _fieldRow)
        {
            fieldRow = _fieldRow;
        }
    }
}
