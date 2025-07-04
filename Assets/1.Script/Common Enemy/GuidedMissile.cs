using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GuidedMissile : MonoBehaviour
{
    [Header("------Common------")]
    public InAccelerationOrderLayer inAccelerationOrderLayer;

    private GameObject     currentTarget;
    
    private Vector2        staggerVector;
    private float          currentFlySpeed;
    private SpriteRenderer spriteRenderer; 
    private bool           isCrashZoon;                         // 충돌 범위 들어왔는지 여부
    [HideInInspector]
    public  bool           noTarget;

    public GameObject      missileExplosionEffect;
    public int             damage;
    public float           rotationSpeed;                         // 회전속도
    public float           staggerDegree;                         // 도착 위치 오차값
    public SpriteRenderer  guidedMissileLightRenderer;

    [HideInInspector] 
    public bool isHackingPossible;                                // isTracking && isHackingPossible 때, 해킹의 FindTarget에 잡히도록 하기 위함.
    
    [Header("------Create Mode------")]
    public GameObject createJet;
    public  float     createSpeed;                       // 생성 후 앞으로 날아가는 속도
    
    [Header("------Tracking Mode------")]
    public GameObject boosterJet;
    public GameObject trackingTwinkleEffect;
    [HideInInspector]
    public bool       isTracking;          // 현재 추적상태(모든 미사일이 생성되면, 워든컨트롤로에서 직접 상태를 변경함.)
    public Color      trackingColor;       // 추적 컬러
    public float      trackingSpeed;       // 기본속도
    
    [Header("------Crash Mode------")]
    public  Color  crashColor;                            // 생성 컬러
    public  float  crashSpeed;                            // 범위 안 속도
    public  float  crashRange;                            // 충돌변경 발동범위
    public  float  crashMoveTime;                         // 충돌범위 들어 온 후, 이동하다가 충돌을 안 하면, 자동으로 터짐
    private float  crashMoveTimeCount;                    // 카운트
    
    [Header("------Hacking Mode------")]
    public  Color            seizingControlColor;                            // 해킹 상태 컬러
    public  List<GameObject> newTargetPossibleList = new List<GameObject>(); // 추적 적 리스트
    private int              currentNewTargetNum;                            // 현재 추적 리스트 넘            
    public  LayerMask        hackingTrackingLayer;                           // 찾아서 공격 할 레이어
    public  LayerMask        wallLayer;                                      
    public  LayerMask        playerLayer;
    public  float            hackingTrackingDistance;                        // 찾을 적 범위(미사일 기준)
    
    private void Start()
    {
        staggerVector   = Random.insideUnitCircle * staggerDegree;  // 추적위치 랜덤 값
        currentFlySpeed = createSpeed;                              // 시작속도

        if (PlayerController.instance && PlayerHp.instance.liveState)
            currentTarget = PlayerController.instance.gameObject;   // 생성 시 기본 타켓은 플레이어
        else
            currentTarget = null;
        
        isHackingPossible = true;
    }

    private void Update()
    {
        // 이동
        Move();
    
        // 새로운 타겟 찾기
        if (!currentTarget)
        {
            // 새로운 적 있으면
            if (newTargetPossibleList.Count > currentNewTargetNum + 1)
            {
                currentNewTargetNum++;
                currentTarget = newTargetPossibleList[currentNewTargetNum];
            }
            // 새로운 적 없으면
            else
            {
                Instantiate(missileExplosionEffect, transform.position, Quaternion.identity);
                AudioManager.instance.WardenSfxCreate(13,false,gameObject); // Missile 폭파 사운드.(부모 x)
                Destroy(gameObject);
            }
        }
        
        // 플레이어 사망시 모두 터짐 + 해킹 트리거에서 근처 적 없음
        if (!PlayerHp.instance.liveState || noTarget)
        {
            Instantiate(missileExplosionEffect, transform.position, Quaternion.identity);
            AudioManager.instance.WardenSfxCreate(13,false,gameObject); // Missile 폭파 사운드.(부모 x)
            Destroy(gameObject);
        }
    }
    
    private void Move()
    {
        // 추적 이동
        if(currentTarget && isTracking && !isCrashZoon)
        {
            // Jet 바꾸기(1회)
            if (!boosterJet.activeInHierarchy)
            {
                boosterJet.SetActive(true);    
                createJet.SetActive(false);

                guidedMissileLightRenderer.color = trackingColor;   // 라이트 색변경
                            
                currentFlySpeed                  = trackingSpeed;   // 속도변경
                
                Instantiate(trackingTwinkleEffect, transform.position, Quaternion.identity,transform);  // 알림 반짝이
                
                AudioManager.instance.WardenSfxCreate(12,true,gameObject); // Missile Pick 발동 사운드.
            }
            
            // 이동
            // 가속 중(플레이어가 가속 누른 위치)
            Vector2 direction;
            if (PlayerAcceleration.instance.isAcceleration)
            {
                if(currentTarget == PlayerController.instance.gameObject)
                    direction= new Vector2(PlayerAcceleration.instance.inputAccelerationXtrans,PlayerAcceleration.instance.inputAccelerationYtrans) - (Vector2)transform.position;
                else
                    direction= currentTarget.transform.position - transform.position;
            }
            // 일반(플레이어 추적)
            else
                direction= (Vector2)currentTarget.transform.position - (Vector2)transform.position;
            
            float angle         = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle              += staggerVector.x;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward); transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue * Time.deltaTime);
            
            transform.position  += (transform.right * currentFlySpeed * PlayerAcceleration.instance.accelerationChangedTimeValue + staggerVector.y * Vector3.up) * Time.deltaTime; // 시간값은 모든계산이 끝나고 마지막에
            
            // 범위 체크
            float distanceToTarget;
            if (PlayerAcceleration.instance.isAcceleration) // 가속 중(플레이어 = 가속 누른 위치 / 다른 추적 = 원래위치)
            {
                if(currentTarget == PlayerController.instance.gameObject)
                    distanceToTarget = Vector2.Distance(transform.position, new Vector2(PlayerAcceleration.instance.inputAccelerationXtrans,PlayerAcceleration.instance.inputAccelerationYtrans));
                else
                    distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
            }
            else                                           // 일반
                distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);

            // 충돌존 판단
            if (distanceToTarget < crashRange)
            {
                // 일진선 상에 있어야, 충돌모드 활성화
                // 범위 + 일직선
                RaycastHit2D ray;
                if(currentTarget == PlayerController.instance.gameObject)   // 플레이어 따라갈 때
                    ray = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.right), 20, playerLayer);
                else                                                        // 해킹 성공 후, 적 따라갈 때
                    ray = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.right), 20, hackingTrackingLayer);

                if (ray)
                {
                    isTracking  = false;
                    isCrashZoon = true;
                }
            }
        }
        // 층돌 이동
        else if (currentTarget && !isTracking && isCrashZoon)
        {
            // Jet 바꾸기(1회)
            if (currentFlySpeed != crashSpeed)
            {
                guidedMissileLightRenderer.color = crashColor;   // 색변경
                currentFlySpeed                  = crashSpeed;   // 속도변경
            }

            // 이동
            transform.position += transform.right * currentFlySpeed * PlayerAcceleration.instance.accelerationChangedTimeValue * Time.deltaTime;
            crashMoveTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            
            // 시간초 후 자동으로 터짐(크면)
            if (crashMoveTimeCount > crashMoveTime)
            {
                Instantiate(missileExplosionEffect, transform.position, Quaternion.identity);
                AudioManager.instance.WardenSfxCreate(13,false,gameObject); // Missile 폭파 사운드.(부모 x)
                Destroy(gameObject);
            }
        }
        // 생성 이동(보스 컨트롤러에서 한번에 전환)
        else if (currentTarget && !isTracking && !isCrashZoon)
        {
            currentFlySpeed      = createSpeed;                                                                                                        // 속도변경
            transform.position  += transform.right * currentFlySpeed * PlayerAcceleration.instance.accelerationChangedTimeValue * Time.deltaTime; // 직진
        }
    }
    
    // 충돌 시
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!PlayerController.instance)
            return;
    
        // 플레이어
        if (currentTarget == PlayerController.instance.gameObject && other.CompareTag("Player") && other.GetComponent<PlayerHp>().liveState && !other.GetComponent<PlayerDash>().isDash)
        {
            other.GetComponent<PlayerHp>().DamagePlayer(transform,damage);

            Instantiate(missileExplosionEffect, transform.position, Quaternion.identity);
            AudioManager.instance.WardenSfxCreate(13,false,gameObject); // Missile 폭파 사운드.(부모 x)
            Destroy(gameObject);
        }
        
        // 플렛폼
        if (other.CompareTag("Platform") || other.CompareTag("Gate"))
        {
            Instantiate(missileExplosionEffect, transform.position, Quaternion.identity);
            AudioManager.instance.WardenSfxCreate(13,false,gameObject); // Missile 폭파 사운드.(부모 x)
            Destroy(gameObject);
        }
        
        // 적
        if (currentTarget != PlayerController.instance.gameObject && other.CompareTag("Enemy") && other.GetComponent<EnemyHp>().liveState)
        {
            other.GetComponent<EnemyHp>().hitAnimNum = 3;                          // 큰 히트 모션
            other.GetComponent<EnemyHp>().DamageEnemy(damage,transform,0f); 

            Instantiate(missileExplosionEffect, transform.position, Quaternion.identity);
            AudioManager.instance.WardenSfxCreate(13,false,gameObject); // Missile 폭파 사운드.(부모 x)
            Destroy(gameObject);
        }
        
        // 보스
        if (currentTarget != PlayerController.instance.gameObject && other.CompareTag("Boss") && other.GetComponent<BossController>().isAppear)
        {
            other.GetComponent<BossHP>().DamageBoss(damage);
        
            Instantiate(missileExplosionEffect, transform.position, Quaternion.identity);
            AudioManager.instance.WardenSfxCreate(13,false,gameObject); // Missile 폭파 사운드.(부모 x)
            Destroy(gameObject);
        }
    }

    public void SeizingControlTrigger()
    {
        //guidedMissileLightRenderer.color = seizingControlColor; // 라이트 색변경(파랑)
        isHackingPossible                = false;               // 해킹 성공하고 나면, 또 잡히지 않도록
        
        RaycastHit2D[]   hits  = Physics2D.CircleCastAll(transform.position, hackingTrackingDistance, Vector2.zero, 0f, hackingTrackingLayer);   // 범위의 추적 변경할 대상 확인
        
        foreach (var hit in hits)
        {
            // 중간에 벽이 있는지 여부
            RaycastHit2D obstaclePlatform = Physics2D.Raycast(transform.position,  hit.transform.position - transform.position, 
                                                              Vector2.Distance(transform.position,hit.transform.position), wallLayer);
            if (!obstaclePlatform)
            {
                newTargetPossibleList.Add(hit.collider.gameObject);
            }
        }
        
        // 새로운 타겟
        if (newTargetPossibleList.Count != 0)
        {
            // 가장 가까운 적
            newTargetPossibleList.Sort((a, b) => Vector2.Distance(transform.position, a.transform.position).CompareTo(Vector2.Distance(transform.position, b.transform.position)));
            currentTarget = newTargetPossibleList[0];
        }
        else
        {
            noTarget = true;
        }
    }

    public void DestroyTrigger()
    {
        noTarget = true;
    }
}