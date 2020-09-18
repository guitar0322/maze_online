using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryUIManager : MonoBehaviour
{
    public void NextStory(GameObject nextStory)
    {
        CommonFunc.instance.SetActiveTarget(nextStory);
    }

    public void EndStory()
    {
        CommonFunc.instance.ChangeScene(2);
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
