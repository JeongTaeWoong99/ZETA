using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("------Common------")] 
    public  EnemyController enemyCon;
    public  EnemyHp         enemyHp;
    
    [HideInInspector]
    public  bool      attackMotionLock;                // 공격 히트박스 켜져있을 때, 모션 변경 락
    public  float     attackCoolTime;                  // 공격 쿨타임
    [HideInInspector]
    public  float     attackCoolTimeCount;                  

    [Header("------AttackPause------")] 
    public GameObject attackPauseEffect;               // 프리팹
    public Transform  attackPauseEffectMakeTrans;      // 생성위치
    public float      attackPauseTime;                 // 공격 퍼즈시간
    [HideInInspector] 
    public bool  isAttackPause;                        // 어택퍼즈 상태
    [HideInInspector]
    public float attackPauseTimeCount;                 // 타임카운트

    [Header("------MeleeWeapon------")]
    public MeleeWeapon  meleeWeapon;
    public GameObject   attackSlashEffect;                   // 각자 근접공격 이펙트 프리팹
    public GameObject   attackHitEffect;                     // 각자의 히트 이펙트
    [HideInInspector]  
    public GameObject   currentSlashEffect;                  // 만들어진 슬레쉬 이펙트
    public int          attackDamage;                        // 근접공격 데미지(공격 데미지)
    [HideInInspector]   
    public bool         closeRangeAttackPossible;            // 근접공격 가능여부
    public float        attackSpeed;                         // 스피드
    [HideInInspector]   
    public float        distanceToPlayer;                    // 플레이어와의 거리에 따른 곱값
    public float        distanceConstrain;                   // 곱값 최대 제한
    public float        sniperCloseRangeAttackRotationSpeed; // 스나이퍼 전용

    [Header("------RotationCommon------")]
    public float angleCorrectionValue;        // 드론의 Y값 보정
    [HideInInspector]
    public bool  isRotation;                  // 회전가능여부
    [HideInInspector]
    public bool  isRotationRecovery;          // 회전복구여부

    [Header("------RotationMain------")]            
    public  GameObject rotationMain;                // 메인으로 돌아갈 로테이션 오브젝트(★총)
    public  float      rotationMainSpeed;           // 회전 스피드
    private float      originMainRotationZ;         // 앵글 계산에 사용
    public  bool       isLimitMainRotation;         // 메인 로테이션이 앵글 제한이 있는지 여부
    public  float      mainGetBackValue;            // 복귀 때, 돌아가는 각도
    [HideInInspector]
    public Vector2     commonDirection;             // 회전 벡터(공중 공격 반동 방향에 사용)
    
    [Header("------RotationSub------")]
    public  GameObject rotationSub;                 // 서브로 돌아갈 로테이션(★머리)
    public  float      rotationSubSpeed;            // 회전 스피드
    private float      originSubRotationZ;      
    public  bool       isLimitSubRotation;
    public float       subGetBackValue;
    
    [Header("------FirearmRecoil------")] 
    public  float firearmRecoilSpeed;        // 반동스피드(뒤로 이동)
    [HideInInspector]
    public  bool isFirearmRecoil;           // 총기반동상태
    
    [Header("------LaserPrefabs------")]
    public GameObject laserPrefabs;               // 레이저 불릿 프리팹
    public GameObject laserShootEffect;           // 레이저 슛 이펙트 프리팹
    
    public Transform laserPointTrans;             // 레이저 포인트 시작위치
    
    public List <Transform> laserBulletTrans = new List<Transform>();           // 레이저 불릿 프리팹 시작위치
    public List <Transform> shootEffectTrans = new List<Transform>();           // 레이저 슛 트렌스

    private void Start()
    {
        // 시작상태
        isRotationRecovery = true;
    
        // 메인(총신 등등)
        if (rotationMain)
        {
            originMainRotationZ  = rotationMain.transform.localRotation.eulerAngles.z;   // 회전에 참조
        }
        
        // 서브(머리 등등)
        if (rotationSub)
        {
            originSubRotationZ = rotationSub.transform.localRotation.eulerAngles.z;    // 회전에 참조
        }
    }

    private void Update()
    {
        // Sniper + Drone
        Rotation();
    
        // 공격 퍼즈 시간 체크
        attackPauseTimeCount -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
        if (attackPauseTimeCount > 0f)
            isAttackPause = true;
        else
            isAttackPause = false;
            
        // 공격 쿨타임 초기화 및 감소(공격 중 일 때 OR Idle 상태일 때)
        if (enemyCon.enemyAnimStateInfo.IsName("Attack1") || enemyCon.enemyAnimStateInfo.IsName("Attack2")    || enemyCon.enemyAnimStateInfo.IsName("Shoot") ||
            enemyCon.enemyAnimStateInfo.IsName("Idle")    || enemyCon.enemyAnimStateInfo.IsName("IdleToWalk") || enemyCon.enemyAnimStateInfo.IsName("Walk")   || enemyCon.enemyAnimStateInfo.IsName("WalkToIdle"))
            attackCoolTimeCount = attackCoolTime;
        else
            attackCoolTimeCount -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            
        // 슬레쉬 이펙트 
        // (현재 만들어진 슬레쉬 이펙트가 있다면,그룹 레이어 문제로 따로 빼서 이동시킴))
        if (currentSlashEffect)
        {
            // 스나이퍼 공격 이펙트 따라가기(스나이퍼는 라이트세이버 + 슛터이기 때문에, 우선순위 enemyCon.isShooter를 먼저 if문에 걸리도록 함.)
            // (팔이 중심)
            if (rotationMain)
            {
                float xOffset = (enemyCon.bodyObject.transform.localScale.x == 1) ? 0.9f : -0.9f;
                currentSlashEffect.transform.position = rotationMain.transform.position + new Vector3(xOffset, -0.25f, 0f);
            }
            // 가드 공격 이펙트 따라가기(바디가 중심)
            else if (meleeWeapon)
            {
                currentSlashEffect.transform.position = enemyHp.bodyBoomTrans.transform.position + new Vector3(0f, 0f, 0f);
            }
        }
    }

    private void FixedUpdate()
    {
        // ALL Enemy
        AttackMove(); // 공격 앞으로 이동 + 총기반동 뒤로 이동
    }
    
    private void AttackMove()
    {
        // 공격 앞으로 이동
        if (enemyCon.enemyAnimStateInfo.IsName("Attack1"))
        {
            // 공격 모션중 히트박스 on(반동o와 움직음 가능)
            if (meleeWeapon.enemyBladeCollider.enabled && !enemyHp.isStun)
            {
                // 정면 공격 이동, 앞이 절벽이면 이동하지 않음.
                if (enemyCon.isFrontCliff)
                {
                    enemyCon.rb2D.velocity = new Vector2(0f,enemyCon.rb2D.velocity.y);
                    return;
                }
            
                // 칼이 켜질 때, 거리값 정해짐. // 거리값제한
                if (distanceToPlayer > distanceConstrain)
                    distanceToPlayer = distanceConstrain;
                
                var xVelocity = enemyCon.bodyObject.transform.localScale.x * attackSpeed * distanceToPlayer * PlayerAcceleration.instance.accelerationChangedTimeValue;     // 이동방향
                var yVelocity = enemyCon.rb2D.velocity.y * PlayerAcceleration.instance.accelerationChangedTimeValue;
                
                if (enemyCon.isSlope)
                    enemyCon.rb2D.velocity = enemyCon.perpendicular * xVelocity * -1f;
                else
                    enemyCon.rb2D.velocity = new Vector2(xVelocity, yVelocity);
            }
        }
        // 반동 뒤로 이동
        else if (enemyCon.enemyAnimStateInfo.IsName("Shoot"))
        {
            // 총기반동 구간
            if (isFirearmRecoil && !enemyHp.isStun)
            {
                // 반동 뒤로 이동, 뒤가 절벽이면 이동하지 않음.
                if (enemyCon.isBackCliff)
                {
                    enemyCon.rb2D.velocity = new Vector2(0f,enemyCon.rb2D.velocity.y);
                    return;
                }
            
                // 지상(스나이퍼)
                if (!enemyCon.isFly)
                {
                    float xVelocity = -enemyCon.bodyObject.transform.localScale.x * firearmRecoilSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue; // 이동방향
                    float yVelocity =  enemyCon.rb2D.velocity.y * PlayerAcceleration.instance.accelerationChangedTimeValue;
                    
                    if (enemyCon.isSlope)
                        enemyCon.rb2D.velocity = enemyCon.perpendicular * xVelocity * -1;
                    else
                        enemyCon.rb2D.velocity = new Vector2(xVelocity, yVelocity);
                }
                // 공중(드론)
                else if(enemyCon.isFly)
                {
                    Vector2 normalVector2 = -commonDirection.normalized;
                    enemyCon.rb2D.velocity = new Vector2(-normalVector2.x * firearmRecoilSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue, 
                        -normalVector2.y * firearmRecoilSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue);
                }
            }
        }
    }

    private void Rotation()
    {
        // 슛 회전
        if (enemyCon.enemyAnimStateInfo.IsName("Shoot") && isRotation && !isRotationRecovery && !enemyHp.isStun)
        {
            // 정면 조준 끝
            if (PlayerController.instance.gameObject)
            {
                //Vector2 direction;  // 회전 벡터
                
                // 회전이 있을 시, Main이 될 로테이션 오브젝트는 무조건 있기 때문에
                // 여기서 direction은 Main(1번)으로 Sub(2번) 또한, 통일한다.
                // 1. 가속일 때(키를 누른 위치)
                if (PlayerAcceleration.instance.isAcceleration)
                    commonDirection = new Vector2(rotationMain.transform.position.x - PlayerAcceleration.instance.inputAccelerationXtrans, rotationMain.transform.position.y - PlayerAcceleration.instance.inputAccelerationYtrans);
                // 2. 기본(플레이어 위치)
                else
                    commonDirection = new Vector2(rotationMain.transform.position.x - PlayerController.instance.transform.position.x, rotationMain.transform.position.y - PlayerController.instance.transform.position.y);

                //commonDirection *= -1;                                                                                      // 방향 반전 조정
                float      angle = Mathf.Atan2(commonDirection.normalized.y, commonDirection.normalized.x) * Mathf.Rad2Deg; // 회전하는 앵글값
                if(enemyCon.bodyObject.transform.localScale.x == 1)                                                         // 앵글값 보정
                    angle += angleCorrectionValue;
                else
                    angle -= angleCorrectionValue;
                
                // 바디 푸쉬가 닿고 있으면, 정면으로 쏘도록 하기.
                // 뒤에 실행되지 안게 하기.
                if (enemyCon.enemyBodyPush.isBodyPushActive)
                {
                    FrontAngleMove();
                    return;
                }
                
                // 방향에 따른 앵글은
                // 9시에서 시계방향으로 3시까지,      -180 --->>> 0
                // 9시에서 시계 반대 방향으로 3시에서  180 --->>> 0
                // bodyDirection이 1일 때는, 앵글 절대갓이 75보다 작으면 이동가능
                // bodyDirection이-1일 때는  앵글 절대값이 105보다 크면 이동가능
                // 메인 로테이션(조건 1 = true이고, 각도 조건이 맞을 시 / 조건 2 = false일 시)
                // 메인 로테이션
                if (rotationMain)
                {
                    // 스나같은 적들은 로테이션 제한 있음.
                    if (isLimitMainRotation)
                    {
                        if (enemyCon.longDistanceAttackPossible)
                        {
                            Quaternion angleAxis            = Quaternion.AngleAxis(angle - 90f + (originMainRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
                            Quaternion rotation             = Quaternion.Slerp(rotationMain.transform.rotation, angleAxis, rotationMainSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
                            rotationMain.transform.rotation = rotation;
                        }
                        else if (!enemyCon.longDistanceAttackPossible)
                        {
                            FrontAngleMove();
                            return;
                        }
                    }
                    // 드론같은 적은 로테이션 제한 없음.
                    else if (!isLimitMainRotation)
                    {
                        Quaternion angleAxis            = Quaternion.AngleAxis(angle - 90f + (originMainRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
                        Quaternion rotation             = Quaternion.Slerp(rotationMain.transform.rotation, angleAxis, rotationMainSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
                        rotationMain.transform.rotation = rotation;
                    }
                }

                // 서브 로테이션
                if (rotationSub)
                {
                    if (isLimitSubRotation)
                    {
                        if (enemyCon.longDistanceAttackPossible)
                        {
                            Quaternion angleAxis           = Quaternion.AngleAxis(angle - 90f + (originSubRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
                            Quaternion rotation            = Quaternion.Slerp(rotationSub.transform.rotation, angleAxis, rotationSubSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
                            rotationSub.transform.rotation = rotation;
                        }
                        else if (!enemyCon.longDistanceAttackPossible)
                        {
                            FrontAngleMove();
                            return;
                        }
                    }
                    else if (!isLimitSubRotation)
                    {
                        Quaternion angleAxis           = Quaternion.AngleAxis(angle - 90f + (originSubRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
                        Quaternion rotation            = Quaternion.Slerp(rotationSub.transform.rotation, angleAxis, rotationSubSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
                        rotationSub.transform.rotation = rotation;
                    }
                }
            }
        }
        // 스나이퍼 근접공격
        // 메인 팔 조정
        else if (enemyCon.enemyAnimStateInfo.IsName("Attack1") && isRotation && !isRotationRecovery && !enemyHp.isStun)
        {
            float angle = Mathf.Atan2(-0.2f, -enemyCon.bodyObject.transform.localScale.x) * Mathf.Rad2Deg;      // y값이 0이면 정면이다.
            if (rotationMain)
            {
                Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f + (originMainRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
                Quaternion rotation  = Quaternion.Slerp(rotationMain.transform.rotation, angleAxis, sniperCloseRangeAttackRotationSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
                rotationMain.transform.rotation = rotation;
            }
        }
        // 스나이퍼 및 드론
        // 메인 서브 복귀
        else if (!isRotation && isRotationRecovery && !enemyHp.isStun)
        {
            float      angle     = Mathf.Atan2(mainGetBackValue, -enemyCon.bodyObject.transform.localScale.x) * Mathf.Rad2Deg;     // 머리는 앞
            if (rotationMain)
            {
                Quaternion angleAxis          = Quaternion.AngleAxis(angle - 90f + (originMainRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
                Quaternion rotation           = Quaternion.Slerp(rotationMain.transform.rotation, angleAxis, rotationMainSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
                rotationMain.transform.rotation = rotation;
            }
            
            float      angle2     = Mathf.Atan2(subGetBackValue, -enemyCon.bodyObject.transform.localScale.x) * Mathf.Rad2Deg;  // 팔은 대각선
            if (rotationSub)
            {
                Quaternion angleAxis2 = Quaternion.AngleAxis(angle2 - 90f + (originSubRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
                Quaternion rotation2  = Quaternion.Slerp(rotationSub.transform.rotation, angleAxis2, rotationSubSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
                rotationSub.transform.rotation = rotation2;
            }
        }
        
    }

    
    private void FrontAngleMove()
    {
        // 회전할 수 없는 앵글이면, 복귀
        float returnAngle = Mathf.Atan2(-0.2f, -enemyCon.bodyObject.transform.localScale.x) * Mathf.Rad2Deg; // 정면 보기     
        if (rotationMain)
        {
            Quaternion angleAxis          = Quaternion.AngleAxis(returnAngle - 90f + (originMainRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
            Quaternion rotation           = Quaternion.Slerp(rotationMain.transform.rotation, angleAxis, rotationMainSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
            rotationMain.transform.rotation = rotation;
        }
                    
        if (rotationSub)
        {
            Quaternion angleAxis2 = Quaternion.AngleAxis(returnAngle - 90f + (originSubRotationZ * enemyCon.bodyObject.transform.localScale.x), Vector3.forward);
            Quaternion rotation2  = Quaternion.Slerp(rotationSub.transform.rotation, angleAxis2, rotationSubSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
            rotationSub.transform.rotation = rotation2;
        }
    }
        
    // 공격범위 안
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !enemyHp.isStun && !PlayerAcceleration.instance.isAcceleration)
        {
            closeRangeAttackPossible = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !enemyHp.isStun && !PlayerAcceleration.instance.isAcceleration)
        {
            closeRangeAttackPossible = true;
        }
    }

    // 공격범위 밖
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            closeRangeAttackPossible = false;
        }
    }
}
