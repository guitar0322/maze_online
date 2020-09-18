using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveRunner : MonoBehaviour
{
    private float runTime;
    private int pathIdx;
    private Vector3 curPosition;
    private List<Vector3> path;
    private float runSpeed;
    // Start is called before the first frame update

    void Start()
    {
        runSpeed = 5;
    }

    public void OnEnable()
    {
        InitRunner();
    }
    public void InitRunner()
    {
        runTime = 0;
        pathIdx = 0;
        gameObject.transform.position = new Vector3(8, 1f, 0);
        path = new List<Vector3>();
        for (int i = Astar.instance.resultPath.Count - 1; i >= 0; i--)
        {
            path.Add(new Vector3(Astar.instance.resultPath[i].hor, 1f, Astar.instance.resultPath[i].ver));

        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        curPosition = gameObject.transform.position;
        if(pathIdx < path.Count)
        {
            runTime += Time.fixedDeltaTime;
            gameObject.transform.position = Vector3.MoveTowards(curPosition, path[pathIdx], runSpeed * Time.deltaTime);
            if (pathIdx >= 1)
            {
                if (path[pathIdx].x - path[pathIdx - 1].x == 1)
                {
                    gameObject.GetComponent<Animator>().SetInteger("direction", 2);
                }
                else if (path[pathIdx].x - path[pathIdx - 1].x == -1)
                {
                    gameObject.GetComponent<Animator>().SetInteger("direction", 1);
                }
                else if (path[pathIdx].z - path[pathIdx - 1].z == 1)
                {
                    gameObject.GetComponent<Animator>().SetInteger("direction", 0);
                }
                else if (path[pathIdx].z - path[pathIdx - 1].z == -1)
                {
                    gameObject.GetComponent<Animator>().SetInteger("direction", 3);
                }
            }

            if (Vector3.Distance(curPosition, path[pathIdx]) == 0f)
            {
                pathIdx++;
            }
        }
        else
        {
            if(SceneManager.GetActiveScene().buildIndex == CommonFunc.GAME_SCENE)
            {
                if (GameManagement.instance.isHighlight == true)
                {

                }
                else
                {
                    if (GameManagement.instance.roundInfo.round == GameManagement.FINAL_ROUND)
                    {

                    }
                    else
                    {
                        InGameUIManager.instance.OpenWaitPanel();
                        RoundResultMessage roundResultMessage = 
                            new RoundResultMessage(GameManagement.instance.roundInfo.field, runTime, UserInfo.instance.nickname);
                        MatchingManager.instance.SendDataToInGame<RoundResultMessage>(roundResultMessage);
                    }
                }
            }
            else if(SceneManager.GetActiveScene().buildIndex == CommonFunc.TUTORIAL_SCENE)
            {

            }
            else
            {

            }
            Debug.Log(string.Format("runtime : {0}", runTime));
            gameObject.SetActive(false);
        }
    }


}
