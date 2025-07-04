using UnityEngine;
using UnityEngine.Serialization;

public class AlterLine : MonoBehaviour
{
    [Header("------Common------")]
    public GameObject    pointCircle;
    public LayerMask     checkLayer; // 오직 충돌할 레이어
  
    private LineRenderer lineRenderer;
    private Vector2      direction;
    private RaycastHit2D ray;
    private Vector2      endPosition;
    
    public bool           isConvert;        // 좌우반전
    
    [Header("------Enemy------")]
    public EnemyAttack     enemyAttack;
    public EnemyController enemyCon;
    public EnemyHp         enemyHp;

    [Header("------Boss------")]
    public BossController bossCon;
    public bool           isRightWingMid;   // 오른 날개의 중심 얼터 라인 인지
    public bool           isLeftWingMid;    // 왼쪽 날개의 중심 얼터 라인 인지

    [Header("------Turret------")] 
    public Turret turret;
    [HideInInspector]
    public bool   isAlterLineOn;
    
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        // 일반적(스나이퍼/드론)
        if (enemyCon)
        {
            if (enemyHp.liveState && (enemyAttack.isRotation || enemyAttack.isAttackPause) && enemyCon.enemyAnimStateInfo.IsName("Shoot") && !enemyHp.isStun)
            {
                lineRenderer.enabled = true;    // 활성화
                pointCircle.SetActive(true);    
                
                int bodyDirection    = enemyCon.bodyObject.transform.localScale.x == 1 ? 1 : -1;                                                     // 레이방향
                Vector2 rayDirection = (bodyDirection == -1) ? -enemyAttack.laserPointTrans.right : enemyAttack.laserPointTrans.right;      // 
                ray = Physics2D.Raycast(enemyAttack.laserPointTrans.position, rayDirection, 20, checkLayer);                   //
                
                if (ray.collider != null)
                {
                    endPosition = new Vector2(ray.point.x, ray.point.y);
                    pointCircle.transform.position = endPosition;
                }
                else
                {
                    endPosition = new Vector2(enemyAttack.laserPointTrans.position.x  + rayDirection.x * 20,enemyAttack.laserPointTrans.position.y  + rayDirection.y * 20);
                    pointCircle.transform.position = endPosition;
                }
                
                lineRenderer.SetPosition(0, enemyAttack.laserPointTrans.transform.position);  // 월드공간 사용 !!
                lineRenderer.SetPosition(1, endPosition);                              // 월드공간 사용 !!
            }
            else
            {
                lineRenderer.enabled = false;
                pointCircle.SetActive(false);
            }
        }
        // 보스
        else if (bossCon)
        {
            // 추적 중(라인이 켜졌고, 플레이어 에이밍 상태가 아님)
            if (bossCon.isLaserPatten && bossCon.isPlayerAiming)
            {
                lineRenderer.enabled = true; // 활성화   
                pointCircle.SetActive(true);

                int bodyDirection = bossCon.bodyObject.transform.localScale.x == 1 ? 1 : -1;
                if (isConvert)
                    bodyDirection *= -1;

                Vector2 rayDirection = (bodyDirection == -1) ? -transform.right : transform.right;
                ray = Physics2D.Raycast(transform.position, rayDirection, 20, checkLayer);

                if (ray.collider != null)
                {
                    endPosition = new Vector2(ray.point.x, ray.point.y);
                    pointCircle.transform.position = endPosition;
                }
                else
                {
                    endPosition = new Vector2(transform.position.x + rayDirection.x * 20,
                        transform.position.y + rayDirection.y * 20);
                    pointCircle.transform.position = endPosition;
                }
                
                lineRenderer.SetPosition(0, transform.position); // 월드공간 사용 !!
                lineRenderer.SetPosition(1, endPosition); // 월드공간 사용 !!
                
                if(ray.collider != null)
                {
                    // 오른쪽 날개의 중심 라인
                    if (isRightWingMid)
                    {
                        // 플레이어 닿음(+ 엑셀상태 아닐 때)
                        if (ray.collider.CompareTag("Player") && !PlayerAcceleration.instance.isAcceleration)
                            bossCon.rightGunAimingCompleted = true;
                        else
                            bossCon.leftGunAimingCompleted = false;
                    }
                    // 왼쪽 날개의 중심 라인
                    else if (isLeftWingMid)
                    {
                        // 플레이어 닿음(+ 엑셀상태 아닐 때)
                        if (ray.collider.CompareTag("Player") && !PlayerAcceleration.instance.isAcceleration)
                            bossCon.leftGunAimingCompleted = true;
                        else
                            bossCon.leftGunAimingCompleted = false;
                    }
                }
            }
            else
            {
                lineRenderer.enabled = false;
                pointCircle.SetActive(false);

                // 오른쪽 날개의 중심 라인    
                if (isRightWingMid)
                    bossCon.rightGunAimingCompleted = false;
                // 왼쪽 날개의 중심 라인
                else if (isLeftWingMid)
                    bossCon.leftGunAimingCompleted = false;
            }
        }
        // 회전형 포탑
        else if (turret)
        {
            if (isAlterLineOn)
            {
                lineRenderer.enabled = true;    
                pointCircle.SetActive(true);    
                
                Vector2 rayDirection = turret.attackPauseEffectMakeTrans.right;
                if (isConvert)
                    rayDirection *= -1f;
                
                ray = Physics2D.Raycast(turret.attackPauseEffectMakeTrans.position, rayDirection, 20, checkLayer);                 

                if (ray.collider != null)
                {
                    endPosition = new Vector2(ray.point.x, ray.point.y);
                    pointCircle.transform.position = endPosition;
                }
                else
                {
                    endPosition = new Vector2(turret.attackPauseEffectMakeTrans.position.x  + rayDirection.x * 20,turret.attackPauseEffectMakeTrans.position.y  + rayDirection.y * 20);
                    pointCircle.transform.position = endPosition;
                }
                
                lineRenderer.SetPosition(0 ,turret.attackPauseEffectMakeTrans.transform.position);  // 월드공간 사용 !!
                lineRenderer.SetPosition(1, endPosition);                              // 월드공간 사용 !!
            }
            else
            {
                lineRenderer.enabled = false;
                pointCircle.SetActive(false);
            }
        }
    }
}
