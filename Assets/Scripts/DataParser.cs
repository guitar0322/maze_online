﻿using UnityEngine;
using System.Text;

public class DataParser : MonoBehaviour
{
    public static T ReadJsonData<T>(byte[] buf)
    {
        var strByte = Encoding.Default.GetString(buf);
        Debug.Log("strByte : " + strByte);
        //byte 배열을 string으로 변환
        return JsonUtility.FromJson<T>(strByte);
    }

    public static byte[] DataToJsonData<T>(T obj)
    {
        var jsonData = JsonUtility.ToJson(obj);
        Debug.Log("roundInfo json : " + jsonData.ToString());
        //string을 byte 배열로 변환
        return Encoding.UTF8.GetBytes(jsonData);
    }
}
