using System.Collections.Generic;
using UnityEngine;

public class FreightGenerator : MonoBehaviour
{
    //[HideInInspector]
    public bool isOperation;

    public  GameObject      freightPrefabs;
    public  List<Transform> createTransList = new List<Transform>();    // 생성위치 리스트
    public  List<bool>      isMoveUpList    = new List<bool>();         // 이동방향 리스트
    private int             createNum       = 0;

    public  float createInterval;
    private float createIntervalCount;
    
    private void Start()
    {
        createIntervalCount = createInterval;   // 작동 시, 바로 1개가 생성될 수 있도록.
    }

    private void FixedUpdate()
    {
        if (isOperation)
        {
            createIntervalCount += Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;

            if (createIntervalCount > createInterval)
            {
                GameObject freightClone = Instantiate(freightPrefabs, createTransList[createNum].position,Quaternion.identity,transform);
                freightClone.GetComponentInChildren<FlyBodyFloating>().movingUp = isMoveUpList[createNum]; // 이동방향 결정(자식 오브젝트에서 찾기!)
                createNum++;
                if (createNum >= createTransList.Count)
                    createNum = 0;
                createIntervalCount = 0;
            }
        }
    }
}