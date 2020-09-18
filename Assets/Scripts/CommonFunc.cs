using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CommonFunc : MonoBehaviour
{
    private static CommonFunc _instance;

    public static int LOGIN_SCENE = 0;
    public static int STORY_SCENE = 1;
    public static int TUTORIAL_SCENE = 2;
    public static int MAIN_SCENE = 3;
    public static int GAME_SCENE = 4;

    // Start is called before the first frame update
    public GameObject Popup;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public static CommonFunc instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(CommonFunc)) as CommonFunc;
                if (_instance == null)
                {
                    GameObject container = new GameObject();
                    container.name = "CommonFunc";
                    _instance = container.AddComponent(typeof(CommonFunc)) as CommonFunc;
                }
            }
            return _instance;
        }
    }

    public void SetActiveTarget(GameObject targetObject)
    {
        targetObject.SetActive(true);
    }
    
    public void UnsetActiveTarget(GameObject targetObject)
    {
        targetObject.SetActive(false);
    }

    public void ChangeScene(int sceneNum)
    {
        SceneManager.LoadScene(sceneNum);
    }

    public void MakePopup(string guide)
    {
        Popup.SetActive(true);
        Popup.GetComponentInChildren<Text>().text = guide;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
