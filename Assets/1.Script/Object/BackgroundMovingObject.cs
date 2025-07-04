using System.Collections.Generic;
using UnityEngine;

public class BackgroundMovingObject : MonoBehaviour
{
    public List<GameObject> moveObjectList = new List<GameObject>(); // 이동 할 오브젝트 리스트
    
    public float            moveSpeedX;                              // 이동 스피드 X
    private float           previousPlayerX;
    
    public float            moveSpeedY;                              // 이동 스피드 Y
    private float           previousPlayerY;

    private void Update()
    {
        // x값 이동
        if (CameraController.instance.gameObject != null)
        {
            float playerXChange = CameraController.instance.transform.position.x - previousPlayerX;        
            
            foreach (var moveObject in moveObjectList)
            {
                Vector3 objectPosition        = moveObject.transform.position;
                objectPosition.x             -= playerXChange * moveSpeedX; 
                moveObject.transform.position = objectPosition;
            }

            previousPlayerX = CameraController.instance.transform.position.x;
        }
        
        // y값 이동
        if (CameraController.instance.gameObject != null)
        {
            float playerYChange = CameraController.instance.transform.position.y - previousPlayerY;        
            
            foreach (var moveObject in moveObjectList)
            {
                Vector3 objectPosition        = moveObject.transform.position;
                objectPosition.y             -= playerYChange * moveSpeedY; 
                moveObject.transform.position = objectPosition;
            }

            previousPlayerY = CameraController.instance.transform.position.y;
        }
    }
}
