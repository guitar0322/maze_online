using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManagement : MonoBehaviour
{
    public GameObject commonWall;
    public GameObject specialWall;
    public GameObject startWall;
    public GameObject[] startWallList;
    public GameObject[] specialWallList;
    public GameObject[] commonWallList;

    public static int MAX_START = 16;
    public static int MIN_START = 8;
    public static int MAX_COMMON = 20;
    public static int MIN_COMMON = 8;
    public static int MAX_SPECIAL = 2;
    public void InitWallList()
    {
        commonWallList = new GameObject[MAX_COMMON];
        for (int i = 0; i < MAX_COMMON; i++)
        {
            commonWallList[i] = Instantiate(commonWall, Vector3.zero, Quaternion.Euler(90f, 0, 0));
            commonWallList[i].SetActive(false);
        }

        specialWallList = new GameObject[MAX_SPECIAL];
        for (int i = 0; i < MAX_SPECIAL; i++)
        {
            specialWallList[i] = Instantiate(specialWall, Vector3.zero, Quaternion.Euler(90f, 0, 0));
            specialWallList[i].SetActive(false);
        }

        startWallList = new GameObject[MAX_START];
        for (int i = 0; i < MAX_START; i++)
        {
            startWallList[i] = Instantiate(startWall, Vector3.zero, Quaternion.Euler(90f, 0, 0));
            startWallList[i].SetActive(false);
        }
    }
    public void CreateFieldInfo()
    {
        int wallNum = 0;
        while (wallNum < roundInfo.startWall)
        {
            int ver = UnityEngine.Random.Range(1, VER-2);
            int hor = UnityEngine.Random.Range(1, HOR - 2);
            if (IsValidField(ver, hor) == false)
            {
                continue;
            }
            else
            {
                SetField(ver, hor, START_WALL);
                if (Astar.instance.PathFind() == false)
                {
                    SetField(ver, hor, EMPTY);
                }
                else
                {
                    roundInfo.startIdxList[wallNum].idx[0] = ver;
                    roundInfo.startIdxList[wallNum].idx[1] = hor;
                    wallNum++;
                }
            }
        }
        DebugField();
    }
    public void DebugField()
    {
        string str = "";
        for(int i = 0; i < VER; i++)
        {
            for(int j = 0; j < HOR; j++)
            {
                str += roundInfo.field[i].fieldRow[j];
            }
            str += "\n";
        }
        Debug.Log(str);
    }
    public void CreateStartWall()
    {
        for(int i = 0; i < roundInfo.startWall; i++)
        {
            int wallIdx = IsValidStartWall();
            startWallList[wallIdx].SetActive(true);
            startWallList[wallIdx].transform.position = new Vector3(roundInfo.startIdxList[i].idx[1] + 0.5f, 1, roundInfo.startIdxList[i].idx[0] + 0.5f);
        }
        Astar.instance.PathFind();
    }
    
    public int IsValidStartWall()
    {
        for (int i = 0; i < startWallList.Length; i++)
        {
            if (!startWallList[i].activeSelf)
            {
                return i;
            }
        }
        return -1;
    }
    public int IsValidSpecialWall()
    {
        for (int i = 0; i < specialWallList.Length; i++)
        {
            if (!specialWallList[i].activeSelf)
            {
                return i;
            }
        }
        return -1;
    }

    public int IsValidCommonWall()
    {
        for (int i = 0; i < commonWallList.Length; i++)
        {
            if (!commonWallList[i].activeSelf)
            {
                return i;
            }
        }
        return -1;
    }

    public int ConvertFieldVerIdx(float pos)
    {
        return (int)Math.Round(pos - 0.5f);
    }

    public int ConvertFieldHorIdx(float pos)
    {
        return (int)Math.Round(pos - 0.5f);
    }

    public void CreateWall(bool isDouble, Vector3 wallPos, Ray ray)
    {
        int fieldVerIdx = ConvertFieldVerIdx(wallPos.z);
        int fieldHorIdx = ConvertFieldHorIdx(wallPos.x);
        //Debug.Log(string.Format("Touch : {0} , {1}", wallPos.z, wallPos.x));
        //Debug.Log(string.Format("TouchFieldIdx : {0} , {1} , {2}", fieldVerIdx, fieldHorIdx, isDouble));
        RaycastHit hitInfo;
        if (wallPos.z <= VER - 2 && wallPos.x <= HOR - 2 && wallPos.z > 1 && wallPos.x > 1)
        {
            if(IsValidField(fieldVerIdx, fieldHorIdx) == false){
                Debug.Log("invalid field");
                if(Physics.Raycast(ray, out hitInfo))
                {
                    //play destroy sound
                    GameObject hitWall = hitInfo.transform.gameObject;
                    int hitWallVer = ConvertFieldVerIdx(hitWall.transform.position.z);
                    int hitWallHor = ConvertFieldHorIdx(hitWall.transform.position.x);
                    Vector3 hitWallPos = hitWall.transform.position;
                    Debug.Log(string.Format("hitinfo : {0} , {1} , {2}", 
                        hitWall.tag, hitWall.transform.position.z , hitWall.transform.position.x));
                    if (!hitWall.CompareTag("Start"))
                    {
                        hitWall.SetActive(false);
                        SetField(hitWallVer, hitWallHor, EMPTY);
                        if (hitWall.CompareTag("Common"))
                        {
                            roundInfo.commonWall++;
                            InGameUIManager.instance.ChangeCommonWallText(roundInfo.commonWall);
                        }
                        else if (hitWall.CompareTag("Special"))
                        {
                            roundInfo.specialWall++;
                            InGameUIManager.instance.ChangeSpecialWallText(roundInfo.specialWall);
                        }
                        Astar.instance.PathFind();
                    }
                }
                else
                {
                    //play wrong sound
                    //create wrong icon
                }
            }
            else
            {
                Debug.Log("valid field");
                SetField(fieldVerIdx, fieldHorIdx, COMMON_WALL);
                if (Astar.instance.PathFind())
                {
                    if(roundInfo.commonWall > 0 && isDouble == false)
                    {
                        //play create sound
                        int wallIdx = IsValidCommonWall();
                        commonWallList[wallIdx].transform.position = wallPos;
                        commonWallList[wallIdx].SetActive(true);
                        roundInfo.commonWall--;
                        InGameUIManager.instance.ChangeCommonWallText(roundInfo.commonWall);
                    }
                    else if(isDouble == true && roundInfo.specialWall > 0)
                    {
                        //play create sound
                        SetField(fieldVerIdx, fieldHorIdx, SPECIAL_WALL);
                        int wallIdx = IsValidSpecialWall();
                        specialWallList[wallIdx].transform.position = wallPos;
                        specialWallList[wallIdx].SetActive(true);
                        roundInfo.specialWall--;
                        InGameUIManager.instance.ChangeSpecialWallText(roundInfo.specialWall);

                    }
                    else
                    {
                        //play wrong sound
                        SetField(fieldVerIdx, fieldHorIdx, EMPTY);
                        Astar.instance.PathFind();
                        //create assetempty icon
                    }
                }
                else
                {
                    //play wrong sound
                    SetField(fieldVerIdx, fieldHorIdx, EMPTY);
                    Astar.instance.PathFind();
                    //create wrong icon
                }
            }
        }
    }

}
