using System.Collections.Generic;
using UnityEngine;

public class ObjectDistanceActive : MonoBehaviour
{
    public float activeDistance;                      // 활성화 거리

    private List<GameObject> objectList = new List<GameObject>();
    
    private void Awake()
    {
        // 오브젝트 모두 넣어주기
        objectList.Clear();
        foreach (Transform child in transform)
            objectList.Add(child.gameObject);
    }

    private void FixedUpdate()
    {
        ObjectActiveCheck();
    }

    private void ObjectActiveCheck()
    {
        // 활성화
        if (PlayerHp.instance)
        {
            // 역순으로 반복하여 리스트 수정하기
            for (int i = objectList.Count - 1; i >= 0; i--)
            {
                var objectLists = objectList[i];
                if (objectLists == null) // 박스 같은 오브젝트 파괴시 리스트 제거.
                {
                    objectList.RemoveAt(i);
                    continue;
                }

                if (Vector2.Distance(PlayerController.instance.gameObject.transform.position, objectLists.transform.position) <= activeDistance)
                    objectLists.SetActive(true);
                else
                    objectLists.SetActive(false);
            }
        }
    }
}
