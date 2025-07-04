using System.Collections.Generic;
using Calcatz.MeshPathfinding;
using SAP2D;
using UnityEngine;
using Node   = Calcatz.MeshPathfinding.Node;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour 
{
    [Header("------Common------")] 
    public EnemyHp                         enemyHp;
    [HideInInspector]              
    public Rigidbody2D                     rb2D;
    [HideInInspector]              
    public CapsuleCollider2D               cap2D;
    public EnemyBodyPush                   enemyBodyPush;
    public EnemyAttack                     enemyAttack;
    public EnemyLightController            enemyLightCon;
    
    public GameObject        bodyObject;
    [HideInInspector]
    public AnimatorStateInfo enemyAnimStateInfo;    // 애니메이션 정보
    public Animator	         bodyAnim;
    
    [Header("------Move------")]
    public  float            runSpeed;

    public  bool             isHold;                        // 주위를 돌아다니지 않음.
    
    public  float	         wanderSpeed;
    public  LayerMask        wallLayer;
    public  Transform        wallSensorTrans;
    public  float            wanderLength,  pauseLength;     
    private float            wanderCounter, pauseCounter;
    private bool             iswall;
    
    private Vector2	         moveDirection;
    
    [Header("------Sound------")]
    public  AudioSource[] enemyLoopSoundList;
    private List<float>   originVolumeValueList = new List<float>();
    public  List<float>   volumeUpSpeed         = new List<float>();
    [HideInInspector]
    public bool           isAimingSound;
    
    [Header("------Chase------")]
    public   float    chaseTime;
    [HideInInspector]
    public   bool     isChasePlayer;                 // 플레이어 추적 상태
    [HideInInspector]
    public   float    chaseTimeCount;
    private  float    chaseTurnDelayCount;

    private float     originGravityScale;
    
    [Header("------Alter------")]
    public  LayerMask alterEnemyLayer;               // 적이 적에게 알림
    public  float     alterRadiusDistance;
    
    [Header("------RunAway------")]
    public  LayerMask runAwayLayer;                   // 적이 적에게 알림
    [HideInInspector]
    public  bool      isRunAway;
    public  float     runAwayCheckRadiusDistance;
    public  float     runAwayMoveDistance;
    
    [Header("------Found------")]
    public  LayerMask    foundLayerMask;
    [HideInInspector]
    public bool         longDistanceAttackPossible; // 레이에 닿고 있는지 -> 투사체가 생성 후 닿을 수 있는지 여부
    public  float        maxDistance;
    public  float        layInterval;
    public  int          rayUpDownNum;
    [HideInInspector]
    public List<Vector2> rayDirectionsRight;
    [HideInInspector]
    public List<Vector2> rayDirectionsLeft;

    [HideInInspector]
    public List<float>   gizmoX = new List<float>();     // 찾기 레이의 선의 x값 리스트     
    [HideInInspector]
    public List<float>   gizmoY = new List<float>();     // 찾기 레이의 선의 y값 리스트

    [Header("------Slope------")]
    public EnemyFloorCollider  enemyFloor;
    public  float              slopeCheckDistance; // 표시해줄 선 거리
    [HideInInspector]
    public  bool               isSlope;            // 평지판단
    [HideInInspector]
    public  float              angle;
    [HideInInspector]          
    public  Vector2            perpendicular;
    public  EnemyFloorCollider enemyFloorCollider;
    public  LayerMask          floorLayer;

    [HideInInspector]
    public RaycastHit2D frontFloorHit;
    [HideInInspector] 
    public bool         isFrontCliff;
    [HideInInspector]
    public RaycastHit2D backFloorHit;
    [HideInInspector] 
    public bool         isBackCliff;
    
    [Header("------PathCommon------")]
    public  float             pathFindCheckTime;
    [HideInInspector]
    public  float             pathFindTimeCount;
    [HideInInspector]
    public  bool              isLastNode;
    [HideInInspector] 
    public  bool              hasPath;               // 길이 있는지 여부
    [HideInInspector]
    public bool               isSpawnLocationArrive; // 스폰 위치 도착했는지 여부
    [HideInInspector]
    public GameObject         spawnPointGameObject;  // 스폰위치값

    [Header("------GroundPath------")] 
    public  Pathfinding pathfinding;
    [HideInInspector]       
    public  Node[]      pathResultGrounds;
    [HideInInspector]
    public  int         currentNodeNum;
    private float       maxHight;                           // 판단 높이
    
    [Header("------FlyPath------")]
    public  bool                   isFly;
    private bool                   isFlyMoving;            // 플라이의 경우, 움직임을 update에서 transform 이동해야, 부드럽게 이동.
    public  SAP2DPathfindingConfig Config;                 // 세팅데이터
    [HideInInspector]
    public  Vector3                posInGrid;              // 이동 레이 그리드
    [HideInInspector]
    public Vector2[]               pathResultFlys;          // array of path tiles
    private SAP2DPathfinder        pathfinder;
    private SAP_GridSource         grid;
    public  bool                   ShowGraphic;

    private void Awake()
    {
        rb2D	 = GetComponent<Rigidbody2D>();
        cap2D    = GetComponent<CapsuleCollider2D>();
    }

    private void Start()
    {
        // 오리지널 그래비티 <-> 엑셀 그래피비
        originGravityScale = rb2D.gravityScale;
        
        // 스폰위치 저장 (및 시작상태 true)
        isSpawnLocationArrive          = false;                                                   // 스폰 위치 = 거리판단 위치
        Vector3 currentPosition        = transform.position;                                      // 돌아갈 위치는 floor이 없는 공중 때문에, transform.position 사용
        GameObject emptyObject         = new GameObject(gameObject.name + " spawnPoint");    
        emptyObject.transform.position = currentPosition;                                         
        emptyObject.transform.parent   = EnemyDistanceActive.instance.spawnGatherTrans.transform; // 스폰 위치 모아놓는 상자에 넣기. 
        spawnPointGameObject           = emptyObject;                                             // 스폰 위치 오브젝트로 등록.
        spawnPointGameObject.transform.localScale = bodyObject.transform.localScale;              // 보는 방향 저장.
                                                                                                    
        // 그라운드
        maxHight   = cap2D.bounds.max.y - transform.position.y;                                   // GroundPath 판단
        pathfinder = SAP2DPathfinder.singleton;                                                   // Fly 맵 정보
        
        // Found레이 GizmoDraw
        for (int i = -rayUpDownNum; i <= rayUpDownNum; i++)
        {
            rayDirectionsRight.Add(new Vector2( 1 - layInterval * Mathf.Abs(i), layInterval * -i));
            rayDirectionsLeft .Add(new Vector2(-1 + layInterval * Mathf.Abs(i), layInterval * -i));
            
            gizmoX.Add(layInterval * Mathf.Abs(i));
            gizmoY.Add(layInterval * -i);
        }
        
        // Sound
        // 오리지널 볼륨 길이 저장 및 볼륨 초기화
        foreach (var enemyLoopSoundLists in enemyLoopSoundList)
        {
            if (enemyLoopSoundLists != null)
            {
                originVolumeValueList.Add(enemyLoopSoundLists.volume); // 오리지널값 넣기
                enemyLoopSoundLists.Stop();                            // 멈추기
                enemyLoopSoundLists.volume = 0f;                       // 볼륨값 없애기
            }
            else
            {
                originVolumeValueList.Add(0f);
            }
        }
    }

    private void Update()
    {
        // 부드러운 이동 공중 
        if (isFly)
        {
            if (isFlyMoving && hasPath)
            {
                // 이동
                Vector2 currentTargetVector = grid.GetTileDataAtWorldPosition(pathResultFlys[currentNodeNum]).WorldPosition;
                transform.position          = Vector2.MoveTowards(transform.position, currentTargetVector,Time.deltaTime * runSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue);
            }
        }
    }

    private void FixedUpdate()
    {
        if (enemyLightCon.isAppear && PlayerHp.instance.liveState)
        {
            Action();
            Found();
        }
        else
        {
            rb2D.velocity = Vector2.zero;
            bodyAnim.SetBool("walk", false);
            bodyAnim.SetBool("run", false);
        }
        
        Slop();
        
        Other();
        
        if(enemyLoopSoundList.Length != 0)
            LoopSound();
    }

    public void Alter()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(enemyBodyPush.transform.position, alterRadiusDistance, Vector2.zero, 0f, alterEnemyLayer);   // 범위의 적 저장
        foreach (var hit in hits)
        {
            // 중간에 벽이 있는지 여부
            RaycastHit2D obstaclePlatform = Physics2D.Raycast(enemyBodyPush.transform.position, hit.transform.position - enemyBodyPush.transform.position, 
                                                              Vector2.Distance(enemyBodyPush.transform.position,hit.transform.position), wallLayer);
            // false면 벽이 없음 -> 추격실행
            if(!obstaclePlatform)
                hit.collider.gameObject.GetComponent<EnemyController>().chaseTimeCount = chaseTime;
        }
    }
    
    private void HandleLoopSound(int soundIndex, string animStateName, bool specificSound)
    {
        if (enemyAnimStateInfo.IsName(animStateName) || specificSound)
        {
            // 슛의 경우, 애니메이션은 Shoot인데, 해당 조건들을 만족하지 않으면, 조준하고 있지 않은 상태이다.
            // 그러니, 오히려 소리를 줄여야 한다.
            if ((enemyAnimStateInfo.IsName("Shoot") && (enemyAttack.isRotationRecovery || enemyHp.isStun || !isAimingSound)) ||
                (enemyAnimStateInfo.IsName("Run") || enemyAnimStateInfo.IsName("Walk")) && enemyHp.isStun)
            {
                enemyLoopSoundList[soundIndex].volume -= volumeUpSpeed[soundIndex] * Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
                if (enemyLoopSoundList[soundIndex].volume == 0f && enemyLoopSoundList[soundIndex].isPlaying)
                {
                    enemyLoopSoundList[soundIndex].time = 0f; // 초기화
                    enemyLoopSoundList[soundIndex].Stop();
                }
                return;
            }
            
            // 소리 높히기
            if (!enemyLoopSoundList[soundIndex].isPlaying)
                enemyLoopSoundList[soundIndex].Play();
            
            if(originVolumeValueList[soundIndex] > enemyLoopSoundList[soundIndex].volume)
                enemyLoopSoundList[soundIndex].volume += volumeUpSpeed[soundIndex] * Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
        }
        else
        {
            // 소리 줄이기
            enemyLoopSoundList[soundIndex].volume -= volumeUpSpeed[soundIndex] * Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (enemyLoopSoundList[soundIndex].volume == 0f && enemyLoopSoundList[soundIndex].isPlaying)
            {
                enemyLoopSoundList[soundIndex].time = 0f; // 초기화
                enemyLoopSoundList[soundIndex].Stop();
            }
        }
    }

    private void LoopSound()
    {
        if(enemyLoopSoundList[0] != null)
            HandleLoopSound(0, "Run",false);
        
        if(enemyLoopSoundList[1] != null)
            HandleLoopSound(1, "Walk",false);
        
        if (enemyLoopSoundList[2] != null)
            HandleLoopSound(2, "Shoot",false);
            
        if (enemyLoopSoundList[3] != null)
            HandleLoopSound(3,null,enemyHp.isStun); // 특정 상태 사운드
    }
    
    private void Found()
    {
        if (enemyHp.liveState)
        {
            List<Vector2> rayDirections;                                                                     // 찾기 방향
            rayDirections = bodyObject.transform.localScale.x == 1 ? rayDirectionsRight : rayDirectionsLeft; // 오른쪽 왼쪽 결정
            
            longDistanceAttackPossible = false;                                                              // 원거리 공격 가능성 초기화
            foreach (Vector2 direction in rayDirections)
            {
                var rayHit = Physics2D.Raycast(enemyBodyPush.transform.position + new Vector3(bodyObject.transform.localScale.x * 0.2f, 0f, 0f), 
                                                            direction, maxDistance, foundLayerMask);
                
                if (rayHit.collider != null && rayHit.collider.gameObject == PlayerController.instance.gameObject)
                {
                    if (!enemyHp.isStun && !PlayerAcceleration.instance.isAcceleration)
                    {
                        chaseTimeCount             = chaseTime;
                        longDistanceAttackPossible = true;
                    }
                }
            }
            
            if (chaseTimeCount > 0)
            {
                // 추적 X ->추적 O 으로 처음 진입시, 발견모션(1회)
                if (!isChasePlayer)
                {
                    bodyAnim.SetTrigger("found");
                    AudioManager.instance.EnemySfxCreate(4, true, gameObject);
                    Alter();
                }
                
                isChasePlayer = true;
                bodyAnim.SetBool("isChasePlayer", isChasePlayer);
            }
            else
            {
                //추적 O ->추적 X 으로 처음 진입시, 복귀 판단(1회 먼저 실행, 이후 잘 가고 있는지 계속 실행)
                if (isChasePlayer)
                {
                    isSpawnLocationArrive = false;
                    
                    EnemyFindPath(spawnPointGameObject.transform);
                }
            
                isChasePlayer = false;
                bodyAnim.SetBool("isChasePlayer", isChasePlayer);
            }
        }
        
        // 쫒기 시간 감소
        if(!PlayerAcceleration.instance.isAcceleration)
            chaseTimeCount -= Time.fixedDeltaTime;
    }

    private void Action()
    {
        if (enemyHp.liveState               && PlayerHp.instance.liveState         && !enemyAnimStateInfo.IsName("Hit1")    && !enemyAnimStateInfo.IsName("Hit2")    && !enemyAnimStateInfo.IsName("Hit3") && 
        !enemyAnimStateInfo.IsName("Death") && !enemyAnimStateInfo.IsName("Found") && !enemyAnimStateInfo.IsName("Attack1") && !enemyAnimStateInfo.IsName("Attack2") && !enemyAnimStateInfo.IsName("Shoot"))
        {
            // 경계
            if (PlayerController.instance.gameObject.activeInHierarchy && chaseTimeCount < 0 && isSpawnLocationArrive && !isHold)
            {
                // 원덜 카운트 작동(랜덤할당 wanderDirection = moveDirection으로 걸어다님), 끝나면 pauseCounter
                if (wanderCounter >= 0)
                {
                    wanderCounter -= Time.fixedDeltaTime;
                    // 1회 작동
                    if (wanderCounter <= 0)
                    {
                        pauseCounter = Random.Range(pauseLength * 0.75f, pauseLength * 1.25f); // 퍼즈시간
                    }
                }

                // 퍼즈 카운터 작동 및 카운터
                if (pauseCounter >= 0)
                {
                    pauseCounter    -= Time.fixedDeltaTime;
                    moveDirection.x  = 0f;
                    // 1회 작동
                    if (pauseCounter <= 0)
                    {
                        wanderCounter   = Random.Range(wanderLength * 0.75f, wanderLength * 1.25f); // 이동시간
                        moveDirection.x = Random.Range(-1, 2);                                      // 이동방향(-1 0 1)
                    }
                }
                
                // 벽 충돌 방향전환
                iswall = (Physics2D.OverlapCircle(wallSensorTrans.transform.position, 0.05f, wallLayer));
                if (iswall)
                    moveDirection.x *= -1;
                    
                // 이동
                GroundEnemyMove(moveDirection.x,wanderSpeed);
                // 상태체크(walk)
                StateCheck(true);
            }
            // 복귀 (// <--------계속 걸어갈 것 인데, Idle -> IdleToWalk -> Walk 일 때,이동하도록)
            else if (PlayerController.instance.gameObject.activeInHierarchy && chaseTimeCount < 0 && !isSpawnLocationArrive)
            {
                // 재설정 카운트(길을 찾는게 추적보다는 느리게)
                if (!PlayerAcceleration.instance.isAcceleration)
                    pathFindTimeCount -= Time.fixedDeltaTime / 2f;

                // 길찾기
                if (pathFindTimeCount <= 0)
                {
                    // 스폰위치와 현재 위치 판단
                    // 도착 O 
                    if (Vector2.Distance(spawnPointGameObject.transform.position, transform.position) < 2f)
                        isSpawnLocationArrive           = true;                                      // 상태전환
                    // 도착 X
                    else
                    {
                        isSpawnLocationArrive = false;
                        EnemyFindPath(spawnPointGameObject.transform);
                    }
                }

                // 무브
                if(enemyAnimStateInfo.IsName("Idle") || enemyAnimStateInfo.IsName("IdleToWalk") || enemyAnimStateInfo.IsName("Walk"))
                    EnemyPathMove(false);
                
                // 도착 완료하면, 원래 바라보던 방향 보기
                if(isSpawnLocationArrive)
                    bodyObject.transform.localScale = spawnPointGameObject.transform.localScale;
            }
            // 추격
            else if (PlayerController.instance.gameObject.activeInHierarchy && chaseTimeCount > 0)
            {
                // 재설정 카운트
                if (!PlayerAcceleration.instance.isAcceleration)
                    pathFindTimeCount -= Time.fixedDeltaTime;
                
                // 길찾기
                if (pathFindTimeCount <= 0 && !isRunAway)
                {
                    // 멈추기
                    hasPath                       = false;              // 멈춤
                    pathResultGrounds             = null;               // 길초기화
                    pathResultFlys                = null;               // 길초기화
                    bodyAnim.SetBool("walk", false);
                    bodyAnim.SetBool("run", false);
                    moveDirection.x               = 0f;
                    
                    // Keep이 되면, 다시 길찾기
                    if(enemyAnimStateInfo.IsName("Keep"))
                        EnemyFindPath(PlayerFloorCollider.instance.transform);
                }

                // 공격 범위 안(근정공격 가능 or 원거리 공격 가능)
                if (!isRunAway && ((!enemyAttack.rotationMain     && enemyAttack.closeRangeAttackPossible) || 
                                   (enemyAttack.rotationMain && (enemyAttack.closeRangeAttackPossible || longDistanceAttackPossible))))
                {
                    // 멈추기
                    hasPath            = false;                // 길없음  초기화(멈추기)
                    pathResultGrounds  = null;                 // 길초기화
                    pathResultFlys     = null;                 // 길초기화
                    bodyAnim.SetBool("walk", false);
                    bodyAnim.SetBool("run", false);
                    moveDirection.x    = 0f;
                    
                    // 공격
                    // Keep일 때 + 공격 쿨타임 끝남
                    if (enemyAttack.attackCoolTimeCount < 0 && enemyAnimStateInfo.IsName("Keep"))
                    {
                        // 근접 공격 (원거리 공격 + 근접 공격을 가지고 있는 캐릭터의 경우 근접일 경우, 근접 공격을 우선순위로 부여한다)
                        if (enemyAttack.closeRangeAttackPossible)
                        {
                            // 모션락(공격 시작부터 히트가 꺼지거나, 미사일이 날라가지 전 까지 모션락 시작)
                            enemyAttack.attackMotionLock = true;      
                            bodyAnim.SetTrigger("attack1");

                            pathFindTimeCount = 0f;
                        }
                        //  원거리 공격
                        else if (enemyAttack.rotationMain && longDistanceAttackPossible)
                        {
                            RaycastHit2D[] hits = Physics2D.CircleCastAll(enemyBodyPush.transform.position, 
                                                                                      runAwayCheckRadiusDistance, Vector2.zero, 0f, runAwayLayer);   // 범위 안에 플레이어가이 있는지 체크
                            // 공격(공격 사거리와 도망 범위의 사이에 플레이어가 위치하고 있다면, 공격)
                            if (hits.Length == 0)
                            {
                                ShootFunction();
                            }
                            // 도망(도망가능이면 도망, 아니면 맞써 싸우기)
                            else
                            {
                                // 왼쪽 도망 판단(플레이어가 더 오른쪽에 있음)
                                if (PlayerController.instance.transform.position.x > enemyBodyPush.transform.position.x)
                                {
                                    // 평지 정면 왼쪽 체크
                                    if (angle == 0f)
                                    {
                                        RaycastHit2D checkFront = Physics2D.Raycast(enemyBodyPush.transform.position, Vector2.left, runAwayMoveDistance, wallLayer);
                                        // 벽없음(-> 도망)
                                        if (!checkFront)
                                        {
                                            isRunAway = true;
                                            // x값에 runAwayMoveDistance를 넣을 값으로, 도망갈 위치 노드 길 찾기
                                            Vector3 newVector3     = new Vector3(enemyBodyPush.transform.position.x - runAwayMoveDistance, 
                                                                                   enemyBodyPush.transform.position.y, enemyBodyPush.transform.position.z);
                                            Transform newTransform = new GameObject().transform;
                                            newTransform.position  = newVector3;
                                            EnemyFindPath(newTransform);
                                        }
                                        // 벽있음(막다른 길-> 공격)
                                        else
                                        {
                                            ShootFunction();
                                        }
                                    }
                                    // 경사 체크
                                    else if (angle != 0f)
                                    {
                                        // 왼쪽 위 체크(앵글 크고 \, 왼쪽 도망)
                                        if (perpendicular.y > 0)
                                        {   
                                            RaycastHit2D checkSlope = Physics2D.Raycast(enemyBodyPush.transform.position, new Vector2(-1,0.4f), runAwayMoveDistance, wallLayer);
                                            if (!checkSlope) // 왼쪽 위 O
                                            {
                                                isRunAway = true;
                                                // x값에 runAwayMoveDistance를 넣을 값으로, 도망갈 위치 노드 길 찾기
                                                Vector3 newVector3     = new Vector3(enemyBodyPush.transform.position.x - runAwayMoveDistance, 
                                                                                     enemyBodyPush.transform.position.y + runAwayMoveDistance, enemyBodyPush.transform.position.z);
                                                Transform newTransform = new GameObject().transform;
                                                newTransform.position  = newVector3;
                                                EnemyFindPath(newTransform);
                                            }
                                            else  // 왼쪽 위 X
                                            {
                                                ShootFunction();
                                            }
                                        }
                                        // 왼쪽 아래 체크(앵글 작고 /, 왼쪽 도망)
                                        else if (perpendicular.y < 0)
                                        {
                                            RaycastHit2D checkSlope = Physics2D.Raycast(enemyBodyPush.transform.position, new Vector2(-1,-0.5f), runAwayMoveDistance, wallLayer);
                                            if (!checkSlope) // 왼쪽 아래 O
                                            {
                                                isRunAway = true;
                                                // x값에 runAwayMoveDistance를 넣을 값으로, 도망갈 위치 노드 길 찾기
                                                Vector3 newVector3     = new Vector3(enemyBodyPush.transform.position.x - runAwayMoveDistance, 
                                                                                     enemyBodyPush.transform.position.y - runAwayMoveDistance, enemyBodyPush.transform.position.z);
                                                Transform newTransform = new GameObject().transform;
                                                newTransform.position  = newVector3;
                                                EnemyFindPath(newTransform);
                                            }
                                            else  // 왼쪽 아래 X
                                            {
                                                ShootFunction();
                                            }
                                        }
                                    }
                                }
                                // 오른쪽 도망
                                else
                                {
                                    // 평지 정면 오른쪽 체크
                                    if (angle == 0f)
                                    {
                                        RaycastHit2D checkFront = Physics2D.Raycast(enemyBodyPush.transform.position, Vector2.right, runAwayMoveDistance, wallLayer);
                                        // 벽없음(-> 도망)
                                        if (!checkFront)
                                        {
                                            isRunAway = true;
                                            // x값에 runAwayMoveDistance를 넣을 값으로, 도망갈 위치 노드 길 찾기
                                            Vector3 newVector3     = new Vector3(enemyBodyPush.transform.position.x + runAwayMoveDistance, 
                                                                                   enemyBodyPush.transform.position.y, enemyBodyPush.transform.position.z);
                                            Transform newTransform = new GameObject().transform;
                                            newTransform.position  = newVector3;
                                            EnemyFindPath(newTransform);
                                        }
                                        // 벽있음(막다른 길-> 공격)
                                        else
                                        {
                                            ShootFunction();
                                        }
                                    }
                                    // 경사 체크
                                    else if (angle != 0f)
                                    {
                                        // 오른쪽 아래 체크(앵글 크고 \, 오른쪽 도망)
                                        if (perpendicular.y > 0)
                                        {
                                            RaycastHit2D checkSlope = Physics2D.Raycast(enemyBodyPush.transform.position, new Vector2(1,-0.5f), runAwayMoveDistance, wallLayer);
                                            if (!checkSlope) // 오른쪽 아래 O
                                            {
                                                isRunAway = true;
                                                // x값에 runAwayMoveDistance를 넣을 값으로, 도망갈 위치 노드 길 찾기
                                                Vector3 newVector3     = new Vector3(enemyBodyPush.transform.position.x + runAwayMoveDistance, 
                                                                                     enemyBodyPush.transform.position.y - runAwayMoveDistance, enemyBodyPush.transform.position.z);
                                                Transform newTransform = new GameObject().transform;
                                                newTransform.position  = newVector3;
                                                EnemyFindPath(newTransform);
                                            }
                                            else // 오른쪽 아래 X
                                            {
                                                ShootFunction();
                                            }
                                        }
                                        // 오른쪽 위 체크(앵글 작고 /, 오른쪽 도망)
                                        else if (perpendicular.y < 0)
                                        {
                                            RaycastHit2D checkSlope = Physics2D.Raycast(enemyBodyPush.transform.position, new Vector2(1,0.4f), runAwayMoveDistance, wallLayer);
                                            if (!checkSlope) // 오른쪽 위 O
                                            {
                                                isRunAway = true;
                                                // x값에 runAwayMoveDistance를 넣을 값으로, 도망갈 위치 노드 길 찾기
                                                Vector3 newVector3     = new Vector3(enemyBodyPush.transform.position.x + runAwayMoveDistance, 
                                                                                     enemyBodyPush.transform.position.y + runAwayMoveDistance, enemyBodyPush.transform.position.z);
                                                Transform newTransform = new GameObject().transform;
                                                newTransform.position  = newVector3;
                                                EnemyFindPath(newTransform);
                                            }
                                            else // 오른쪽 위  X
                                            {
                                                ShootFunction();
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
                // 공격 가능 상태 X
                else
                {
                    // 무브
                    EnemyPathMove(true);
                }
            }
            else
            {
                bodyAnim.SetBool("walk", false);
                bodyAnim.SetBool("run", false);
            }
        }
        else
        {
            if (!enemyHp.isStun)
            {
                bodyAnim.SetBool("walk", false);
                bodyAnim.SetBool("run", false);
            }
        }
    }

    private void ShootFunction()
    {
        enemyAttack.attackMotionLock = true;
        isRunAway                    = false; 
        bodyAnim.SetTrigger("shoot");
                                                
        pathFindTimeCount = 0f;
    }
    private void EnemyPathMove(bool isTrace)
    {
        // 길 있음
        if (hasPath)
        {
            // 지상
            if (!isFly)
            {
                // 다음 노드의 x값과 포지션 x값의 비교를 통해, 근접하게 노드로 이동
                float distanceDifference = 0;
                if (enemyFloor.transform.position.x < pathResultGrounds[currentNodeNum].transform.position.x)
                {
                    distanceDifference = pathResultGrounds[currentNodeNum].transform.position.x - enemyFloor.transform.position.x;
                }
                else if (enemyFloor.transform.position.x > pathResultGrounds[currentNodeNum].transform.position.x)
                {
                    distanceDifference = enemyFloor.transform.position.x - pathResultGrounds[currentNodeNum].transform.position.x;
                }
                
                // 다음노드 
                if (!isLastNode && distanceDifference < 0.25f)
                {
                    // 마지막 노드가 아닐 시(번호 증가)
                    if ((currentNodeNum < pathResultGrounds.Length - 1))
                    {
                        currentNodeNum++;
                    }
                    // 마지막 노드일 시(상태 변경)
                    else
                    {
                        isLastNode = true;
                    }
                }

                // 마지막 노드 도착
                if (isLastNode)
                {
                    moveDirection.x = 0f;
                    isRunAway       = false;

                    // 길을 찾았는데, 바로 옆에 있는 경우 좌우반전
                    if (currentNodeNum == 0)
                    {
                        Vector2 scale                   = PlayerController.instance.transform.position.x > enemyBodyPush.transform.position.x ? new Vector2(1f, 1f) : new Vector2(-1f, 1f);
                        bodyObject.transform.localScale = scale;
                    }
                }
                // 마지막 노드 아님
                else if (!isLastNode)
                {
                    // 이동방향 체크
                    if (transform.position.x != pathResultGrounds[currentNodeNum].transform.position.x)
                    {
                        var derection = transform.position.x < pathResultGrounds[currentNodeNum].transform.position.x ? 1 : -1;
                        moveDirection.x  = derection;
                    }
                    
                    // 이동
                    GroundEnemyMove(moveDirection.x, isTrace ? runSpeed : wanderSpeed);
                    
                    // 다운 플랫폼 무시(지상이고, 현재 가는 노드가 다운플랫폼을 무시해야 하는지 판단)
                    enemyFloor.CompareNodes();
                }
                
            }
            // 공중
            else
            {
                // 다음노드
                // 바닥과 공중의 block거리 때문에, x값만 판단
                if (isLastNode == false && (Vector2.Distance(transform.position,pathResultFlys[currentNodeNum]) < 0.25f))
                {
                    // 마지막 노드가 아닐 시(번호 증가)
                    if ((currentNodeNum < pathResultFlys.Length - 1))
                        currentNodeNum++;
                    // 마지막 노드일 시(상태 변경)
                    else
                        isLastNode = true; 
                }
                        
                // 마지막 노드 도착
                if (isLastNode)
                {
                    moveDirection.x = 0f;
                    isFlyMoving     = false; // 공중 이동 false
                    
                    // 길을 찾았는데, 바로 옆에 있는 경우 좌우반전을 통해, 공격하도록 문제 해결!
                    if (currentNodeNum == 0 && !enemyHp.isStun)
                    {
                        Vector2 scale                   = PlayerController.instance.transform.position.x > enemyBodyPush.transform.position.x ? new Vector2(1f, 1f) : new Vector2(-1f, 1f);
                        bodyObject.transform.localScale = scale;
                    }
                }
                // 마지막 노드 아님(공중 이동)
                else
                {
                    isFlyMoving = true; // 공중 이동 true
                    
                    // 좌우반전(1 -1 때 좌우반전 / 0은 할필요 없음)
                    moveDirection.x = pathResultFlys[currentNodeNum].x - transform.position.x;   //  이동방향 체크(좌우반전, 상태전환)
                    if (moveDirection.x != 0)
                    {
                        float xDirection                = Mathf.Sign(moveDirection.x);
                        bodyObject.transform.localScale = new Vector2(xDirection, 1f);
                    }
                }
            }
            
            // 상태체크(Run)
            if(isTrace)
                StateCheck(false);
            else
                StateCheck(true);
        }
        // 길 없음
        else if (!hasPath)
        {
            bodyAnim.SetBool("walk", false);
            bodyAnim.SetBool("run", false);
            
            moveDirection.x = 0f;
            isRunAway     = false;
        }
    }

    private void EnemyFindPath(Transform findTrans)
    {
        pathFindTimeCount = pathFindCheckTime;                                             // 길찾기 시간 초기화
        
        // 지상
        if (!isFly)
        {
            pathfinding.pathResult  = null;                                                // 들어있는 지상 길값 초기화
            pathfinding.StartFindPath(findTrans.transform,maxHight, false);         // 현재위치에서 스폰위치까지의 길찾기
            pathResultGrounds       = pathfinding.GetPathResult();                         // 길찾기 결과받아오기
            
        }
        // 공중
        else
        {
            pathResultFlys = null;                                 // 들어있는 공중 길값 초기화
            grid           = pathfinder.GetGrid(Config.GridIndex);
            posInGrid      = grid.GetTileDataAtWorldPosition(bodyObject.transform.position).WorldPosition;
            
            // 플레이어 길찾기
            // 갈 수 있는 가장 높은 위치를 찾아서 이동
            if (findTrans.name == PlayerFloorCollider.instance.transform.gameObject.name)
            {
                for (int i = 2; i >= 0; i--)
                {
                    if (grid.GetTileDataAtWorldPosition(bodyObject.transform.position).WorldPosition != grid.GetTileDataAtWorldPosition(PlayerController.instance.transform.position + i * Vector3.up).WorldPosition)
                    {
                        pathResultFlys = pathfinder.FindPath(bodyObject.transform.position, PlayerController.instance.transform.position + i * Vector3.up, Config);
                    }           
                    if (pathResultFlys != null)
                        break;
                }
            }
            // 복귀위치 길찾기
            else if(findTrans.name == spawnPointGameObject.name)
            {
                if (grid.GetTileDataAtWorldPosition(bodyObject.transform.position).WorldPosition != grid.GetTileDataAtWorldPosition(spawnPointGameObject.transform.position).WorldPosition)
                {
                    pathResultFlys = pathfinder.FindPath(bodyObject.transform.position, spawnPointGameObject.transform.position, Config);
                }
            }
        }
        
        // 공중 장애물 판단
        // 길이 2개이상일 때, 가는길에 벽이 있는지 확인(예, 트리거 게이트 등등)
        bool isObstacle = false;
        if (pathResultFlys != null && pathResultFlys.Length > 1)
        {
            for (int i = 0; i < pathResultFlys.Length-1; i++)
            {
                // 가는길 벽 체크(-> 트리거 GATE)
                RaycastHit2D obstaclePlatform = Physics2D.Raycast(pathResultFlys[i], pathResultFlys[i + 1] - pathResultFlys[i], 
                    Vector2.Distance(pathResultFlys[i], pathResultFlys[i + 1]), wallLayer);
                        
                if(obstaclePlatform)
                    isObstacle = true;
            }
        }
        
        // 지상 불필요 노드 제거(좌우반전 2번 연속으로 일어나는 것을 방지)
        // (3보다 크고 즉, 다운노드를 가기 전 거쳐야 하는 노드가 1개이상 있고,그 노드가 필요없는 노드라면 삭제)
        if (pathResultGrounds != null)
        {
            if (pathResultGrounds.Length > 1)
            {
                bool isIgnoreNode = false;
                Node[] beforeDownNodes = pathfinding.waypoints.beforeDownNodes.ToArray(); // 다운플렛폼 닿게 되기 전 거쳐가는 노드(무시노드 체크에 필요)
                
                // 노드 2번이 다운플렛폼 닿게 되기 전 거쳐가는 노드인지 체크
                foreach (var beforeDownNode in beforeDownNodes)
                {
                    if (pathResultGrounds[0].name == beforeDownNode.name)
                    {
                        isIgnoreNode = true;
                    }
                }
            
                // 무시 X
                // (2번째 노드가 다운노드이기 때문에, 무시하지 않는다.)
                if (isIgnoreNode)
                    currentNodeNum = 0; // 기본 숫자 초기화
                // 무시 O
                // (2번째 노드가 다운노드가 아니기 때문에, 무시한다.)
                else
                {
                    currentNodeNum = 1; // 0번 무시하고, currentNodeNum을 1로 초기화
                }
            }
            else
            {
                currentNodeNum = 0; // 기본 숫자 초기화
            }
        }
        else
        {
            currentNodeNum  = 0; // 기본 숫자 초기화
        }
        
        // 길 여부 판단     // 지상                                                      // 공중
        hasPath         = (pathResultGrounds != null && pathResultGrounds.Length > 0) || (pathResultFlys != null && pathResultFlys.Length > 0 && !isObstacle);
        isLastNode      = false; // 라스트 노드 false
    }
    
    private void GroundEnemyMove(float directionX,float autoSpeed)
    {
        if (!enemyHp.isStun)
        {
            if (!PlayerAcceleration.instance.isAcceleration)
            {
                //평지 이동
                if (isSlope == false)
                    rb2D.velocity = new Vector2(directionX * autoSpeed, rb2D.velocity.y); // y값은 중력에 따라서 이동 유지
                // 오르막 이동
                else if (isSlope)
                    rb2D.velocity = perpendicular * autoSpeed * directionX * -1;
            }
            else
            {
                //평지 이동
                if (isSlope == false)
                    rb2D.velocity = new Vector2(directionX * autoSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue, rb2D.velocity.y * PlayerAcceleration.instance.accelerationChangedTimeValue); // y값은 중력에 따라서 이동 유지
                // 오르막 이동
                else if (isSlope)
                    rb2D.velocity = perpendicular * autoSpeed * directionX * -1 * PlayerAcceleration.instance.accelerationChangedTimeValue;
            }
        }
    }

    private void StateCheck(bool isWalkMove)
    {
        if (!enemyHp.isStun)
        {
            // 좌우반전(1 -1 때 좌우반전 / 0은 할필요 없음)
            if (moveDirection.x != 0)
            {
                float xDirection                = Mathf.Sign(moveDirection.x);
                bodyObject.transform.localScale = new Vector2(xDirection, 1f);
            }
            
            // 상태전환
            if (moveDirection.x != 0)
            {
                if (isWalkMove)
                {
                    bodyAnim.SetBool("walk", true);
                    bodyAnim.SetBool("run", false);
                }
                else
                {
                    bodyAnim.SetBool("run", true);
                    bodyAnim.SetBool("walk", false);
                }
            
            }
            else
            {
                bodyAnim.SetBool("walk", false);
                bodyAnim.SetBool("run", false);
            }
        }
    }

    private void Slop()
    {
        if (!isFly)
        {
            // 정면
            frontFloorHit = Physics2D.Raycast(enemyFloorCollider.transform.position + new Vector3(bodyObject.transform.localScale.x / 10f, 0.2f, 0f), Vector2.down, slopeCheckDistance, floorLayer); // 플레이어 몸 기준으로 아래로 레이를 쏴서, 닿은 플로어 반사각 판단
            if (frontFloorHit)
            {
                isFrontCliff  = false; // 정면 절벽 X
                perpendicular = Vector2.Perpendicular(frontFloorHit.normal).normalized; // 경사판단
                angle         = Vector2.Angle(frontFloorHit.normal, Vector2.up);        // 경사 angle판단                                                    
                
                if (angle != 0)       // 언덕 판단
                    isSlope = true;
                else
                    isSlope = false;
            }
            else
            {
                isFrontCliff = true;   // 정면 절벽 O
                isSlope      = false;
            }
            
            Debug.DrawLine(frontFloorHit.point, frontFloorHit.point + frontFloorHit.normal, new Color(0f, 1f, 1f)); // 수직
            Debug.DrawLine(frontFloorHit.point, frontFloorHit.point + perpendicular       , new Color(1f, 0f, 1f)); // 반대
            Debug.DrawLine(frontFloorHit.point, frontFloorHit.point + (-perpendicular)    , new Color(1f, 1f, 0f)); // 정면
            
            // 뒷통수
            backFloorHit = Physics2D.Raycast(enemyFloorCollider.transform.position + new Vector3(-bodyObject.transform.localScale.x / 10f, 0.2f, 0f), Vector2.down, slopeCheckDistance, floorLayer); // 플레이어 몸 기준으로 아래로 레이를 쏴서, 닿은 플로어 반사각 판단
            if (backFloorHit)
                isBackCliff = false; // 뒷통수 절벽 X                                                
            else
                isBackCliff = true;   // 튓통수 절벽 O

            Debug.DrawLine(backFloorHit.point, backFloorHit.point + backFloorHit.normal, new Color(1f, 0f, 0f)); // 수직
        }

        // 워프 중, 위치 고정
        // if (!enemyLightCon.isAppear)
        // {
        //     rb2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        // }
        // 미끄러짐 방지
        if (!enemyBodyPush.isBodyPushActive &&
            (enemyHp.isStun                    || enemyAnimStateInfo.IsName("Found")      || 
             enemyAnimStateInfo.IsName("Keep") || enemyAnimStateInfo.IsName("KeepToRun")  || enemyAnimStateInfo.IsName("RunToKeep") || enemyAnimStateInfo.IsName("KeepToIdle") || 
             enemyAnimStateInfo.IsName("Idle") || enemyAnimStateInfo.IsName("IdleToWalk") || enemyAnimStateInfo.IsName("WalkToIdle")))
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        // 공격 자세 및 공격 끝나고 이동 방지(히트시간 있을시에도 제외)
        else if (!enemyBodyPush.isBodyPushActive && !(enemyHp.hitTimeCount / enemyHp.hitAnimLength < 1f) && (((enemyAnimStateInfo.IsName("Attack1") || enemyAnimStateInfo.IsName("Attack2")) && !enemyAttack.meleeWeapon.enemyBladeCollider.enabled) ||
                                                                                                              (enemyAnimStateInfo.IsName("Shoot")                                            && !enemyAttack.isFirearmRecoil)))
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        // 죽음 애니메이션 시, 위치 고정
        else if (enemyAnimStateInfo.IsName("Death"))
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        // 이외
        else
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Other()
    {
        // 애니메이션 정보 갱신
        enemyAnimStateInfo = bodyAnim.GetCurrentAnimatorStateInfo(0);
        
        // 스턴에 따른 시간감속 체크(스턴이거나 / 공격 중 멈추기 시간이 남아있거나)
        if ((enemyHp.isStun || enemyAttack.isAttackPause) && !enemyAnimStateInfo.IsName("Death"))
            bodyAnim.speed = 0f;
        else
            bodyAnim.speed = PlayerAcceleration.instance.accelerationChangedTimeValue;
        
        // 무게 변화
        // (엑세레이션일 때, 속도가 느려졌는데, 무게가 그대로면, 제대로 이동하지 않는 문제가 발생하기 때문에, 중력을 바꿔 줘야 함.)
        rb2D.gravityScale = originGravityScale * PlayerAcceleration.instance.accelerationChangedTimeValue;
    }

    // private void OnDrawGizmos()
    // {
    //     // 공격레이
    //     Gizmos.color = Color.red;
    //
    //     if (bodyObject.transform.localScale.x == 1)
    //     {
    //         foreach (Vector2 direction in rayDirectionsRight)
    //         {
    //             Gizmos.DrawRay(enemyBodyPush.transform.position + new Vector3(bodyObject.transform.localScale.x * 0.2f, 0f, 0f),
    //                 direction.normalized * maxDistance  // Normalize the direction vector
    //             );
    //         }
    //     }
    //     else
    //     {
    //         foreach (Vector2 direction in rayDirectionsLeft)
    //         {
    //             Gizmos.DrawRay(enemyBodyPush.transform.position + new Vector3(bodyObject.transform.localScale.x * 0.2f, 0f, 0f),
    //                 direction.normalized * maxDistance  // Normalize the direction vector
    //             );
    //         }
    //     }
    //     // 알람범위        
    //     Gizmos.color = new Color(1f, 1f, 0f);
    //     Gizmos.DrawWireSphere(transform.position,alterRadiusDistance);
    //     
    //     // 도망범위  
    //     Gizmos.color = new Color(1f, 0.35f, 0f);
    //     Gizmos.DrawWireSphere(transform.position,runAwayCheckRadiusDistance);
    //     
    //     // 플라이 길찾기
    //     if (ShowGraphic)
    //     {       
    //         Gizmos.color = new Color(1f, 0.4f, 1f);      
    //         if (pathResultFlys != null)
    //         {
    //             for (int i = 0; i < pathResultFlys.Length; i++)
    //             {
    //                 if (i + 1 < pathResultFlys.Length)
    //                     if (pathResultFlys[i] != Vector2.zero)
    //                         Gizmos.DrawLine(pathResultFlys[i], pathResultFlys[i + 1]);
    //             }
    //         }
    //         // else
    //         // {
    //         //     Gizmos.DrawLine(transform.position, grid.GetTileDataAtWorldPosition(PlayerController.instance.transform.position).WorldPosition);
    //         // }       
    //     }
    // }
    
}
