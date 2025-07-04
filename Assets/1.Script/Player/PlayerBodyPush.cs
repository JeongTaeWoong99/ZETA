using UnityEngine;

public class PlayerBodyPush : MonoBehaviour
{
    public static PlayerBodyPush instance;

    [HideInInspector]
    public BoxCollider2D box2D;
    
    [HideInInspector] 
    public bool      isBodyPushActive;     // 밀어내기 작동
    [HideInInspector]
    public bool      isBodyRightPush;      // 밀어내기 R(공격이동 밀어내기 작동 여부)
    [HideInInspector]
    public bool      isBodyLeftPush;       // 밀어내기 L(공격이동 밀어내기 작동 여부)
    
    public LayerMask pushLayer;
    public float     pushCubeSize;
    public float     pushSpeed;
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        box2D = GetComponent<BoxCollider2D>();
    }
    
    void FixedUpdate()
    {
        // 박스가 켜져 있고(생존), 라이트 트레일이 켜져 있고(공격 중 앞으로 이동), 땅이고(바닥에서만 밀리기), 달리고 있을 때(이동)
        // 밀어내기 작동
        if (box2D.enabled && PlayerFloorCollider.instance.isGrounded     
            && !PlayerController.instance.playerAnimStateInfo.IsName("Dash") && !PlayerController.instance.playerAnimStateInfo.IsName("Run")
            && !PlayerController.instance.playerAnimStateInfo.IsName("Walk") && !PlayerController.instance.playerAnimStateInfo.IsName("IdleToRun")
            && !PlayerController.instance.playerAnimStateInfo.IsName("Hit")  && !PlayerController.instance.playerAnimStateInfo.IsName("Jump"))
        {
            // 좌우 밀리기 체크
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position,new Vector2(pushCubeSize,pushCubeSize),0f,pushLayer);

            int rightCount = 0;
            int leftCount  = 0;

            foreach (Collider2D collider in colliders)
            {
                // 자기자신 제외
                if(collider.gameObject == gameObject)
                    continue;
                
                // 밀리지 않기 상태 체크
                // 일반 적 = EnemyBodyPush 컴포넌트 있음 + 애니메이션 상태
                if (collider.gameObject.GetComponent<EnemyBodyPush>() &&
                    (collider.gameObject.GetComponent<EnemyBodyPush>().enemyCon.enemyAnimStateInfo.IsName("Run") || collider.gameObject.GetComponent<EnemyBodyPush>().enemyCon.enemyAnimStateInfo.IsName("Walk")))
                    continue;

                Vector2 otherObjectPosition = collider.transform.position;
                float   relativePosition    = otherObjectPosition.x - transform.position.x;

                if (relativePosition > 0)
                    rightCount++;
                else if (relativePosition < 0)
                    leftCount++;
            }

            // R 한쪽만 or L 한쪽만
            if ((rightCount > 0 && leftCount == 0) || (leftCount > 0 && rightCount == 0))
            {
                isBodyPushActive = true;
                float xVelocity  = 0f;
                if (rightCount > 0 && leftCount == 0)
                {
                    xVelocity       = pushSpeed * -1f;     // 이동방향(오른쪽에 있으니, 왼쪽으로 이동)
                    isBodyLeftPush  = true;                // 왼쪽 이동
                    isBodyRightPush = false;
                }
                else if (leftCount > 0 && rightCount == 0)
                {
                    xVelocity = pushSpeed;                  // 이동방향(왼쪽에 있으니, 오른쪽으로 이동)
                    isBodyRightPush = true;                 // 오른쪽 이동
                    isBodyLeftPush  = false;
                }
                
                // 이동
                if (PlayerController.instance.isSlope)
                    PlayerController.instance.rb2D.velocity = PlayerController.instance.perpendicular * xVelocity * -1;
                else
                    PlayerController.instance.rb2D.velocity = new Vector2(xVelocity,  PlayerController.instance.rb2D.velocity.y);
            }
            // R-L 양쪽
            else if (rightCount > 0 && leftCount > 0)
            {
                isBodyPushActive = false;
                isBodyLeftPush   = false;
                isBodyRightPush  = false;
            }
            // 없음
            else
            {
                isBodyPushActive = false;
                isBodyLeftPush   = false;
                isBodyRightPush  = false;
            }
        }
        else
        {
            isBodyPushActive = false;
            isBodyLeftPush   = false;
            isBodyRightPush  = false;
        }
    }
    
    private void OnDrawGizmos()
    {
        // 푸쉬 범위
        Gizmos.color = new Color(0f, 0f, 1f);
        Gizmos.DrawWireCube(transform.position,new Vector3(pushCubeSize,pushCubeSize,pushCubeSize));
    }
}