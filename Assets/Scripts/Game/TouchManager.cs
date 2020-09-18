using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TouchManager : MonoBehaviour
{
    private float touchDeltaTime; //터치를 한시점부터 측정된 시간.
    private float doubleTouchMaxTime = 0.2f;
    private float createWallYPosition = 1f;
    private bool onTouch; //true일경우 시간측정.

    public Ray ray;
    public RaycastHit hitInfo;
    private Vector3 touchPos;
    private Vector3 wallPosition;
    private bool validPosition;

    public bool isDouble;
    private int touchCount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool IsDoubleTap(Vector3 newMousePosition)
    {
        bool result = false;
        float variancePosition = 1;
        float lengthOfDeltaPosition = Vector2.Distance(touchPos, newMousePosition);
        if (touchDeltaTime < doubleTouchMaxTime && lengthOfDeltaPosition < variancePosition && touchCount == 2)
            result = true;
        //Debug.Log(string.Format("IsDoubleTap : {0} , {1} , {2}", result, lengthOfDeltaPosition, touchCount));
        return result;
    }

    // Update is called once per frame
    void Update()
    {
        if (onTouch)
            touchDeltaTime += Time.deltaTime;

        if (Input.GetMouseButtonUp(0))
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            onTouch = true;
            touchCount++;
            touchPos = ray.origin;
            isDouble = IsDoubleTap(ray.origin);
            
            touchPos.y = createWallYPosition;
            wallPosition.x = (int)touchPos.x + 0.5f;//.5좌표에 맞추기 위함
            wallPosition.z = (int)touchPos.z + 0.5f;
            wallPosition.y = touchPos.y;
        }

        if (touchDeltaTime > doubleTouchMaxTime + 0.02f)
        {
            GameManagement.instance.CreateWall(isDouble, wallPosition, ray);
            touchCount = 0;
            onTouch = false;
            touchDeltaTime = 0;
        }
    }
}
