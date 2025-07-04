using Unity.Mathematics;
using UnityEngine;

public class PlayerSpineTrailCreate : MonoBehaviour
{
    public GameObject spineTrailGameObject; // 생성할 오브젝트
    
    public  float spineCreateTime;
    private float spineCreateTimeCount;

    private void FixedUpdate()
    {
        spineCreateTimeCount += Time.fixedDeltaTime;
        if (spineCreateTimeCount >= spineCreateTime && PlayerAcceleration.instance.isAcceleration && 
            !PlayerController.instance.playerAnimStateInfo.IsName("Idle") && !PlayerController.instance.playerAnimStateInfo.IsName("Hang"))
        {
            spineCreateTimeCount = 0f; // 카운트 초기화
            
            Instantiate(spineTrailGameObject, transform.position, quaternion.identity); // 생성
        }
    }
}
