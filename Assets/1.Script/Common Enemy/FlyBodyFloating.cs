using UnityEngine;

public class FlyBodyFloating : MonoBehaviour
{
    [Header("------Common------")]
    public  Transform flyBody;
    public  float     flySpeed;
    public  float     yMoveRange;
    private float     originFlyY;      // 오리지널 Y값 기준으로 위아래로 바디가 흔들림
    public  bool      movingUp;
    
    [Header("------FixedUpdate------")]
    public  Rigidbody2D rb2D;
    
    [Header("------Enemy------")]
    public EnemyController      enemyCon;
    public EnemyLightController enemyLightCon;
    
    [Header("------Boss------")]
    public BossController bossCon;
    
    [Header("------Jump Platform------")] 
    public bool isJumpPlatform;
    
    [Header("------Freight------")]
    public  bool      isFreight;
    public  float     moveSpeed;
    private Vector3   createTrans;
    
    void Start()
    {
        // 플라이 부유 오리지널 Y값
        if(flyBody)
            originFlyY = flyBody.localPosition.y;

        createTrans = transform.position;
    }

    // 일반적
    private void Update()
    {
        // 플라이 둥둥 떠있는 연출
        // 플라이의 MoveTowards로 이동하지 않을 때, 둥둥 제어
        // 플라이 + 보스
        if (enemyCon)
        {
            if(enemyLightCon.isAppear && !enemyCon.enemyAnimStateInfo.IsName("Walk") && !enemyCon.enemyAnimStateInfo.IsName("Run") && !enemyCon.enemyAnimStateInfo.IsName("Death"))
                EnemyFloating();
        }
        else if (bossCon)
        {
            if(bossCon.isAppear && !bossCon.bossAnimStateInfo.IsName("Walk") && !bossCon.bossAnimStateInfo.IsName("Death"))
                EnemyFloating();
        }
    }

    private void FixedUpdate()
    {
        if (isJumpPlatform)
            JumpPlatformFloating();
        else if (isFreight)
            FreightMove();
    }

    // Update 트렌스폼 둥둥 떠있기
    private void EnemyFloating()
    {   
        float targetY = movingUp ? (originFlyY + yMoveRange) : (originFlyY - yMoveRange);
        
        Vector3 newPosition   = flyBody.localPosition;
        newPosition.y         = Mathf.MoveTowards(flyBody.localPosition.y, targetY, flySpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
        flyBody.localPosition = newPosition;

        // 목표 지점에 도달하면 방향 전환
        if (Mathf.Approximately(flyBody.localPosition.y, targetY))
            movingUp = !movingUp;
    }

    private void JumpPlatformFloating()
    {
        float targetY   = movingUp ? (originFlyY + yMoveRange) : (originFlyY - yMoveRange);
        
        float currentY  = flyBody.localPosition.y;
        float direction = movingUp ? 1f : -1f;

        rb2D.velocity = new Vector2(rb2D.velocity.x, direction * flySpeed * PlayerAcceleration.instance.accelerationChangedTimeValue);
        
        // 플레이어가 매달려 있으면, 플레이어의 rb2D.velocity에도 영향을 주도록 하기.
        if (transform.childCount > 0)
            transform.GetChild(0).GetComponent<PlayerController>().rb2D.velocity 
                = rb2D.velocity = new Vector2(rb2D.velocity.x, direction * flySpeed * PlayerAcceleration.instance.accelerationChangedTimeValue);
        
        // 목표 지점에 도달하면 방향 전환
        if ((movingUp && currentY >= targetY) || (!movingUp && currentY <= targetY))
        {
            movingUp = !movingUp;
            rb2D.velocity = new Vector2(rb2D.velocity.x, 0f); // 속도를 0으로 설정하여 잠시 멈춤
        }
    }

    private void FreightMove()
    {
        float direction = movingUp ? 1f : -1f;
        
        rb2D.velocity = new Vector2(rb2D.velocity.x, direction * moveSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue);
        
        // 플레이어가 매달려 있으면, 플레이어의 rb2D.velocity에도 영향을 주도록 하기.
        if (transform.childCount > 0)
            transform.GetChild(0).GetComponent<PlayerController>().rb2D.velocity 
                = rb2D.velocity = new Vector2(rb2D.velocity.x, direction * moveSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue);
        
        if(Vector2.Distance(createTrans,transform.position) > 40)
            Destroy(transform.parent.gameObject);
    }
}