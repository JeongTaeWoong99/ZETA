using UnityEngine;

public class EnemyBodyPush : MonoBehaviour
{
    public EnemyController enemyCon;
    
    [HideInInspector]
    public BoxCollider2D   box2D;

    [HideInInspector] 
    public  bool           isBodyPushActive;
    public  LayerMask      pushLayer;
    public  float          pushCubeSize;
    public  float          pushSpeed;

    private void Start()
    {
        box2D = GetComponent<BoxCollider2D>();
    }
    
    void FixedUpdate()
    {
        // 공중 지상 공통
        // box2D가 켜져 있으면(생존), 스턴이 걸려 있고(스턴일 때는 무조건 밀치기)
        if (box2D.enabled &&
             (enemyCon.enemyHp.isStun                    || enemyCon.enemyAnimStateInfo.IsName("Found") || 
              enemyCon.enemyAnimStateInfo.IsName("Keep") || enemyCon.enemyAnimStateInfo.IsName("Idle")  || 
              (((enemyCon.enemyAnimStateInfo.IsName("Attack1") || enemyCon.enemyAnimStateInfo.IsName("Attack2")) && !enemyCon.enemyAttack.meleeWeapon.enemyBladeCollider.enabled) ||
                (enemyCon.enemyAnimStateInfo.IsName("Shoot")                                                     && !enemyCon.enemyAttack.isFirearmRecoil))))
        {
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position,new Vector2(pushCubeSize,pushCubeSize),0f,pushLayer);

            int rightCount = 0;
            int leftCount  = 0;

            foreach (Collider2D collider in colliders)
            {
                // 자기자신 제외
                if(collider.gameObject == gameObject)
                    continue;
                
                EnemyBodyPush colliderBodyPush = collider.gameObject.GetComponent<EnemyBodyPush>();
                // 닿은 적이 Run Or Walk면, 밀리지 않기
                // 자신 + 닿은적 서로 뛰고 있는 중 이면, 밀리도록 하기.
                if(colliderBodyPush && 
                    (colliderBodyPush.enemyCon.enemyAnimStateInfo.IsName("Run") || colliderBodyPush.enemyCon.enemyAnimStateInfo.IsName("Walk")) && 
                   !(enemyCon.enemyAnimStateInfo.IsName("Run")                  || enemyCon.enemyAnimStateInfo.IsName("Walk")))
                    continue;
                
                Vector2 otherObjectPosition = collider.transform.position;
                float   relativePosition    = otherObjectPosition.x - transform.position.x;
                
                if (relativePosition > 0)
                {
                    rightCount++;
                }
                else if (relativePosition < 0)
                {
                    leftCount++;
                }
            }

            // R 한쪽만 or L 한쪽만
            if ((rightCount > 0 && leftCount == 0) || (leftCount > 0 && rightCount == 0))
            {
                isBodyPushActive = true;

                float xVelocity = 0;
                if(rightCount > 0 && leftCount == 0)
                    xVelocity     = enemyCon.transform.localScale.x * pushSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue * -1; // 이동방향(오른쪽에 있으니, 왼쪽으로 이동)
                else if(leftCount > 0 && rightCount == 0)
                    xVelocity     = enemyCon.transform.localScale.x * pushSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;      // 이동방향(왼쪽에 있으니, 오른쪽으로 이동)
                
                if (enemyCon.isSlope)
                    enemyCon.rb2D.velocity = enemyCon.perpendicular * xVelocity * -1;
                else
                    enemyCon.rb2D.velocity = new Vector2(xVelocity,  enemyCon.rb2D.velocity.y);
            }
            // R-L 양쪽
            else if ((rightCount > 0 && leftCount > 0) || (rightCount == 0 && leftCount == 0))
                isBodyPushActive = false;
            // 없음
            else
                isBodyPushActive = false;
        }
        else
            isBodyPushActive   = false;
    }
    
    // private void OnDrawGizmos()
    // {
    //     // 푸쉬 범위
    //     Gizmos.color = new Color(0f, 0f, 1f);
    //     Gizmos.DrawWireCube(transform.position,new Vector3(pushCubeSize,pushCubeSize,pushCubeSize));
    // }
}