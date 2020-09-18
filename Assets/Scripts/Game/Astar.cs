using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour
{
    private static Astar _instance;
    private static int TILE_SIZE = 1;
    public GameObject[] pathTrace = new GameObject[81];
    public GameObject path;
    List<Point> openList = new List<Point>();
    List<Point> closeList = new List<Point>();
    public List<Point> resultPath = new List<Point>();
    Point startPoint = new Point(null, GameManagement.HOR/2, 0);
    Point endPoint = new Point(null, GameManagement.HOR / 2, GameManagement.VER-1);

    public static Astar instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(Astar)) as Astar;
                if (_instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "Astar";
                    _instance = container.AddComponent(typeof(Astar)) as Astar;
                }
            }
            return _instance;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        startPoint.g = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TraceInit()
    {
        for (int i = 0; i < 81; i++)
        {
            pathTrace[i] = GameObject.Instantiate(path, Vector3.zero, Quaternion.Euler(90f, 0, 0));
            pathTrace[i].SetActive(false);
        }
    }

    public void ClearTrace()
    {
        for (int i = 0; i < 81; i++)
        {
            pathTrace[i].SetActive(false);
        }
    }

    public void MakeTrace()
    {
        Vector3 tracePos;
        tracePos.y = 1f;
        for (int i = 0; i < resultPath.Count; i++)
        {
            tracePos.x = resultPath[i].hor;
            tracePos.z = resultPath[i].ver;
            pathTrace[i].transform.position = tracePos;
            pathTrace[i].SetActive(true);
        }
    }

    public bool PathFind()
    {
        if (pathTrace[0] == null && !UserInfo.instance.isBeginerMode)
            TraceInit();
        Clear();
        Point tempPoint;
        openList.Add(startPoint);
        while (true)
        {
            if (openList.Count == 0)
            {
                return false;
            }

            tempPoint = openList[0];
            //Debug.Log(tempPoint.x + " , " + tempPoint.y);
            openList.RemoveAt(0);
            if (IsSamePoint(tempPoint, endPoint))
            {
                while (tempPoint != null)
                {
                    resultPath.Add(tempPoint);
                    tempPoint = tempPoint.parent;
                }
                if (!UserInfo.instance.isBeginerMode)
                {
                    ClearTrace();
                    MakeTrace();
                }

                return true;
            }

            closeList.Add(tempPoint);
            InsertOpenList(tempPoint);
            SortOpenlist();
            //Debug.Log(openList[0].f);
        }
    }

    public void SortOpenlist()
    {
        bool flag = true;
        Point tempPoint;
        if (openList.Count == 0)
            return;
        while (flag)
        {
            flag = false;
            for (int i = 0; i < openList.Count - 1; i++)
            {
                if (openList[i].f > openList[i + 1].f)
                {
                    tempPoint = openList[i + 1];
                    openList[i + 1] = openList[i];
                    openList[i] = tempPoint;
                    flag = true;
                }
            }
        }
    }


    public bool IsExistCloselist(Point p)
    {
        for (int i = 0; i < closeList.Count; i++)
        {
            if (IsSamePoint(closeList[i], p))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsExistOpenlist(Point p)
    {
        for (int i = 0; i < openList.Count; i++)
        {
            if (IsSamePoint(openList[i], p))
            {
                CalPointInfo(p);
                if (p.g < openList[i].g)
                {
                    openList[i].g = p.g;
                    openList[i].parent = p.parent;
                }
                return true;
            }
        }
        return false;
    }

    public bool IsSamePoint(Point target, Point dest)
    {
        if (target.hor == dest.hor && target.ver == dest.ver)
            return true;
        else
        {
            return false;
        }
    }

    public void InsertOpenList(Point p)
    {
        //Debug.Log("x : " + p.x);
        Point tempPoint;
        for (int i = p.hor - 1; i <= p.hor + 1; i++)
        {
            for (int j = p.ver - 1; j <= p.ver + 1; j++)
            {
                if (i > 0 && j > 0 && j < GameManagement.VER)
                {
                    tempPoint = new Point(p, i, j);
                    if (GameManagement.instance.roundInfo.field[j].fieldRow[i] == GameManagement.EMPTY && !(i == p.hor && j == p.ver) && !IsExistOpenlist(tempPoint))
                    {
                        if (CheckValidNode(tempPoint) && !IsExistCloselist(tempPoint))
                        {
                            //GameObject.Instantiate(tracePathNode, new Vector3(i, j, 2), Quaternion.identity);
                            openList.Add(tempPoint);
                            CalPointInfo(tempPoint);
                            //Debug.Log("i : " + i + " j : " + j + " g : " + tempPoint.g + " h : " + tempPoint.h);
                        }
                    }
                }
            }
        }

    }

    public bool CheckValidNode(Point p)
    {
        bool result = true;
        int destX = p.parent.hor - p.hor;
        int destY = p.parent.ver - p.ver;
        if (GameManagement.instance.roundInfo.field[p.parent.ver - destY].fieldRow[p.parent.hor] != GameManagement.EMPTY &&
            GameManagement.instance.roundInfo.field[p.parent.ver].fieldRow[p.parent.hor - destX] != GameManagement.EMPTY)
        {
            result = false;
        }

        return result;
    }

    public void CalPointInfo(Point p)
    {
        int distX = p.hor - endPoint.hor;
        int distY = p.ver - endPoint.ver;
        if (Mathf.Abs(p.parent.hor - p.hor) + Mathf.Abs(p.parent.ver - p.ver) == 2)
        {
            p.g = p.parent.g + 1414;
        }
        else
        {
            p.g = p.parent.g + 1000;
        }
        //(9, 18)
        p.h = Mathf.Sqrt(distX * distX + distY * distY) * 1000;
        p.f = p.g + p.h;
    }

    public void Clear()
    {
        openList = null;
        closeList = null;
        openList = new List<Point>();
        closeList = new List<Point>();
        resultPath.Clear();
    }

    public class Point
    {
        public int hor;
        public int ver;
        public float f;
        public int g;
        public float h;
        public Point parent;
        public Point(Point _p, int _hor, int _ver)
        {
            parent = _p;
            hor = _hor;
            ver = _ver;
        }
    }
}
