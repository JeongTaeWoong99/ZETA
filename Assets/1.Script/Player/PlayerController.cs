using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
       
    [Header("------Common------")]
    public  GameObject       bodyGameObject;
    public  Animator         playerAnim;            // 몸 오브젝트
    [HideInInspector]
    public Rigidbody2D       rb2D;                  
    [HideInInspector]
    public CapsuleCollider2D cap2D;

    [HideInInspector] 
    public float originColliderSizeY;
    [HideInInspector] 
    public float originColliderOffsetY;
    
    [HideInInspector]
    public AnimatorStateInfo playerAnimStateInfo;   // 애니메이션 정보
    
    public SortingGroup      sortingGroup;
    public Light2D           bodyHighlightLight;
    
    [Header("------Move------")]
    public  float    runSpeed;
    public  float    walkSpeed;
    [HideInInspector]
    public  Vector2  moveVector;
    [HideInInspector] 
    public float     activeMoveSpeed;
    
    [Header("------Jump------")]
    public GameObject jumpEffect;
    private bool      jumpAction;
    public float      jumpForce;
    public LayerMask  floorLayer;
    [HideInInspector] 
    public  int       currentJumpCount = 2;
    private float     nomalGravity;
    public  int       longJumpGravity;
    
    [HideInInspector] 
    public bool       floorJumpState;    // 점프 상태 판별

    private float inDashInertiaSpeed;    // 대쉬 중, 관성 스피드
    public  float jumpAnimSpeed;         // 관성 점프 normalized에서 사용.
    private float jumpAnimLength;        // 관성 점프 normalized에서 사용.
    
    [HideInInspector] 
    public  bool  isJumpMacro;       // 벽에서 점프 후 강제 이동
    
    [HideInInspector] 
    public int tutorialJumpCheck;    // 성능실험실 인풋 체크
    
    [Header("------Slop------")]
    public float       slopeCheckDistance;          // 표시해줄 선 거리
    [HideInInspector]
    public bool        isSlope;                     // 평지판단
    [HideInInspector]
    public float       angle;
    [HideInInspector]
    public Vector2     perpendicular;
    
    [Header("------Hang------")]
    public  GameObject cornerTrans;
    public  GameObject legEndTrans;
    [HideInInspector] 
    public  int        hangWallSensorCount;
    [HideInInspector]
    public   bool      isHangWall;
    [HideInInspector]
    public  bool       isHangUp;              // 올라가기 키 
    [HideInInspector]
    public  bool       isHangDown;            // 내려가기 키
    
    [HideInInspector]
    public bool       isCornerClimb;         // 코너 클림프 상태인지 체크
    
    [Header("------Sound------")]
    public  AudioSource[] playerLoopSoundList;
    private List<float>   originVolumeValueList = new List<float>();
    public  List<float>   volumeUpSpeed         = new List<float>();

    private void Awake()
    {
        instance = this;
        
        cap2D = GetComponent<CapsuleCollider2D>();
        rb2D  = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        nomalGravity = rb2D.gravityScale;
        
        originColliderSizeY   = cap2D.size.y;
        originColliderOffsetY = cap2D.offset.y;
        
        // 오리지널 볼륨 길이 저장 및 볼륨 초기화
        foreach (var playerLoopSoundLists in playerLoopSoundList)
        {
            originVolumeValueList.Add(playerLoopSoundLists.volume); // 오리지널값 넣기
            playerLoopSoundLists.Stop();                            // 멈추기
            playerLoopSoundLists.volume = 0f;                       // 볼륨값 없애기
        }
        
        // 점프 길이 저장
        AnimationClip[] clips = playerAnim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            // 공통
            if (clip.name.Equals("Jump"))
                jumpAnimLength = clip.length * (1/jumpAnimSpeed);       // 원래길이  * 가속길이(2)
        }
    }

    private void Update()
    {
        if (PlayerHp.instance.liveState          && !PlayerHacking.instance.isHacking  && !PlayerScan.instance.isScan && !PlayerHp.instance.isRecovery && 
            !EventController.instance.eventState && !MenuManager.instance.isNormalMenu && !PlayerHp.instance.isHit    && !isJumpMacro                  &&
            !PlayerAttack.instance.isAttackState)
        {
            // 점프(입력)
            Jump();
            
            if (!PlayerDash.instance.isDash)
            {
                // 이동(입력) + 좌우반전
                Move();
                // 사다리(입력)
                HangMove();
            }
        }
        else
            moveVector = Vector2.zero;
    }

    private void FixedUpdate()
    {
        // 언덕(물리 체크)
        Slop();
        // 벽체크(물리 체크)
        HangCheck();
        // 기타
        Other();
        // 사운드
        LoopSound();
    }
    
    private void HandleLoopSound(int soundIndex, string animStateName)
    {
        if (playerAnimStateInfo.IsName(animStateName))
        {
            if (!playerLoopSoundList[soundIndex].isPlaying)
                playerLoopSoundList[soundIndex].Play();
            
            if(originVolumeValueList[soundIndex] > playerLoopSoundList[soundIndex].volume)
                playerLoopSoundList[soundIndex].volume += volumeUpSpeed[soundIndex] * Time.fixedDeltaTime;
        }
        else
        {
            playerLoopSoundList[soundIndex].volume -= volumeUpSpeed[soundIndex] * Time.fixedDeltaTime;
            if (playerLoopSoundList[soundIndex].volume == 0f && playerLoopSoundList[soundIndex].isPlaying)
            {
                playerLoopSoundList[soundIndex].time = 0f; // 초기화
                playerLoopSoundList[soundIndex].Stop();
            }
        }
    }

    private void LoopSound()
    {
        HandleLoopSound(0, "Run");
        HandleLoopSound(1, "Walk");
        HandleLoopSound(2, "UpLadder");
        HandleLoopSound(3, "DownLadder");
    }

    private IEnumerator JumpCoroutine()
    {
        jumpAction = false;
        
        Instantiate(jumpEffect, PlayerFloorCollider.instance.transform.position, Quaternion.Euler(-90f, 0f, 0f)); //이펙트
        
        AudioManager.instance.PlayerSfxCreate(9,true);                                                        // 사운드 생성       
        
        floorJumpState = true;         // 이동시 y값 오르막 제거용.(Move에서 update에서 제거.)
        rb2D.velocity  = Vector2.zero; // 점프의 
        moveVector     = Vector2.zero; // 일정한 값 유지
        
        playerAnim.SetTrigger("normalJumpOn");
        
        currentJumpCount -= 1; 
        
        // 튜토리얼 2단 점프 체크
        if (currentJumpCount == 0)
            tutorialJumpCheck += 1; 
            
        yield return new WaitForFixedUpdate();  // Y값을 0으로 만들어서, 일정한 점프값을 유지하는데, rb2D.AddForce에 영향을 주지 않도록 대기
        
        rb2D.AddForce(new Vector2(0f, jumpForce));
        
        jumpAction = false;
    }

    private void Jump()
    {
        // 점프 입력
        if (Input.GetKeyDown(KeyCode.Space)    && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.A) && 
            !EventController.instance.jumpLock && !isHangWall                      && currentJumpCount is 2 or 1   && !isJumpMacro)
        {
            // Attack도 Dash 중간에 가능하기 때문에, Dash 초기화
            // 대쉬 중, 0.8 이하 구간이면, 점프 코루틴 나가도록 함.
            if (PlayerDash.instance.isDash && playerAnimStateInfo.normalizedTime <= 0.8)
            {
                PlayerDash.instance.isDash = false;
                
                currentJumpCount           = 0;
                isJumpMacro                = true;
                
                inDashInertiaSpeed = PlayerDash.instance.decelerationValue * PlayerDash.instance.dashSpeed;      // 시작 최대 스피드(대쉬를 눌렀을 때, 감속과 대쉬의 스피드를 이용해서 결정)
                StartCoroutine(JumpMoveMacro(bodyGameObject.transform.localScale.x,true,true));
            }
            else if (!jumpAction)
            {
                PlayerDash.instance.isDash = false;
                
                jumpAction = true;
                
                StartCoroutine(JumpCoroutine());
            }
        }
    }
    
    private IEnumerator JumpMoveMacro(float direction, bool isJump, bool isInertia)
    {
        if(isInertia)
            EventController.instance.tutorialDashJumpCheckCount++;  // 성능실험실 대쉬 점프 체크(가속을 했다는 건... 대쉬 중, 점프를 했다는 것)
    
        // 상태값 제거
        isHangWall = false;
        playerAnim.SetBool("isHangWall",isHangWall);
        
        // 사운드 생성
        if(isJump)
            AudioManager.instance.PlayerSfxCreate(9,true);    
        
        // 땅(대쉬 중)
        if(PlayerFloorCollider.instance.isGrounded)
            Instantiate(jumpEffect, PlayerFloorCollider.instance.transform.position, Quaternion.Euler(-90f, 0f, 0f));
        // 점프 이펙트(벽 / 사다리)
        else
            Instantiate(jumpEffect, legEndTrans.transform.position + new Vector3(0f,0.5f,0f), 
                        bodyGameObject.transform.localScale.x == 1 ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, 90f, 0f));
        
        floorJumpState = true;         // 이동시 y값 오르막 제거용.(Move에서 update에서 제거.)
        rb2D.velocity  = Vector2.zero; // 점프의 
        moveVector     = Vector2.zero; // 일정한 값 유지
         
        // 트리거 및 좌우반전
        if (PlayerFloorCollider.instance.isGrounded) // 대쉬 점프
            playerAnim.SetTrigger("dashToJumpOn");
        else
            playerAnim.SetTrigger("normalJumpOn");

        currentJumpCount = 0;
        
        bodyGameObject.transform.localScale = new Vector3(direction, 1, 1); // 좌우반전
        
        yield return new WaitForFixedUpdate();              // Y값을 0으로 만들어서, 일정한 점프값을 유지하는데, rb2D.AddForce에 영향을 주지 않도록 대기
        
        // 물리 점프
        if (isJump)
            rb2D.AddForce(new Vector2(0f, jumpForce));
            
        //관성 적용시
        if(isInertia) 
            StartCoroutine(InertiaCo());    // 관성적용

        // 공중 벽 점프 강제이동 시간 = Y값이 -로 가면, 상태전환
        float minWaitTime      = 0.1f;
        float minWaitTimeCount = 0f; // 사다리 떨어지기의 자연스러움을 위해서, 최소 시간 설정
        while (true)
        {
            // 메크로 나가기(+ 공중 유효타가 터진 경우, 나가기.)
            minWaitTimeCount += Time.fixedDeltaTime;
            if (minWaitTimeCount > minWaitTime && (playerAnim.GetFloat("airSpeedY") < -2 || PlayerFloorCollider.instance.isGrounded || 
                                                   PlayerHp.instance.isHit                    || PlayerAttack.instance.isEffectHitJump))
            {
                activeMoveSpeed = runSpeed;
                isJumpMacro     = false;
                
                break;
            }
            
            // moveVector을 이용하지 않고, 직접 조정함.
            rb2D.velocity = new Vector2(bodyGameObject.transform.localScale.x * activeMoveSpeed, rb2D.velocity.y); // y값은 중력에 따라서 이동 유지
        
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator InertiaCo()
    {
        // 관성 적용.
        // runSpeed + inDashInertiaSpeed 를 최고값으로 시작하여, Jump 애니메이션에 맞춰서 서서히 감속되어 runSpeed로 돌아옴.
        float animTimeCount = 0f;    // 점프 애니메이션 진행 카운트 초기화
        while (true)
        {
            animTimeCount += Time.fixedDeltaTime * jumpAnimSpeed;
            
            // 애니메이션 진행량 (0 -> 1)
            float progress = animTimeCount / jumpAnimLength;
            // 1이 넘어가면, 관성 종료.
            if (progress > 1)
            {
                // 속도 복구
                activeMoveSpeed = runSpeed;
                break;
            }
            
            // 감속값 (1 -> 0)
            // 0에 가까워 질 수록, dashSpeed * productValue으로 대쉬 속도가 작아짐.
            float decelerationValue = 1f - progress;
    
            // 이속 변경
            if (runSpeed + (inDashInertiaSpeed * decelerationValue) > PlayerDash.instance.dashSpeed) // 최고 속도는 dashSpeed를 넘을 수 없도록 함.
                activeMoveSpeed = PlayerDash.instance.dashSpeed;
            else
                activeMoveSpeed = runSpeed + (inDashInertiaSpeed * decelerationValue);               // dashSpeed보다 낮은 경우.
            
            yield return new WaitForFixedUpdate();
        }
    }

    private void HangCheck()
    {
        // 벽잡기 체크(물리 체크 wallSensorCount)
        if (hangWallSensorCount == 2 && !PlayerFloorCollider.instance.isGrounded && !isCornerClimb && !isJumpMacro &&
            (playerAnimStateInfo.IsName("Fall")        || playerAnimStateInfo.IsName("Jump")             || playerAnimStateInfo.IsName("Hang"))            ||
             playerAnimStateInfo.IsName("HangToScan2") || playerAnimStateInfo.IsName("Scan2")            || playerAnimStateInfo.IsName("Scan2ToHang")      ||
             playerAnimStateInfo.IsName("UpLadder")    || playerAnimStateInfo.IsName("HangToUpLadder")   || playerAnimStateInfo.IsName("UpLadderToHang")   ||
             playerAnimStateInfo.IsName("DownLadder")  || playerAnimStateInfo.IsName("HangToDownLadder") || playerAnimStateInfo.IsName("DownLadderToHang"))
        {
            isHangWall        = true;
            rb2D.gravityScale = 0f;
            // 움직이는 플랫폼 X -> 미끄러짐 방지
            // 움직이는 플랫폼 O -> 움직이는 플랫폼의 Velocity를 따라갈 수 있도록, rb2D.velocity 초기화 X
            if (transform.parent == null)
                rb2D.velocity = Vector2.zero;
            moveVector        = Vector2.zero; // 방향키 입력값 초기화
            currentJumpCount  = 0;
            playerAnim.SetBool("isHangWall",isHangWall);
        }
        // 벽 벗어나기
        else
        {
            isHangWall = false;
            playerAnim.SetBool("isHangWall",isHangWall);
        }
    }

    private void HangMove()
    {
        if (isHangWall && !isCornerClimb && !isJumpMacro)
        {
            // 위에 키
            if (Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
            {
                isHangUp   = true;
                isHangDown = false;
            }
            // 아래 키
            else if (Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.UpArrow))
            {
                isHangDown = true;
                isHangUp   = false;
            }
            else
            {
                isHangUp   = false;
                isHangDown = false;
        
                playerAnim.SetBool("upLadder",false);
                playerAnim.SetBool("downLadder",false);   
            }
            
            // 벽 점프(오르고 내려가기 일 때, 가능)
            if(Input.GetKeyDown(KeyCode.Space))
            {
                isJumpMacro = true;
                
                isHangUp   = false;
                isHangDown = false;
        
                playerAnim.SetBool("upLadder",false);
                playerAnim.SetBool("downLadder",false);   

                StartCoroutine(bodyGameObject.transform.localScale.x == 1 ? JumpMoveMacro(-1,true,false) : JumpMoveMacro(1,true,false));
            }
        }
        else
        {
            isHangUp   = false;
            isHangDown = false;
        
            playerAnim.SetBool("upLadder",false);
            playerAnim.SetBool("downLadder",false);   
        }
    }
    
    private IEnumerator CornerClimb()
    {
        Transform objectTransform = gameObject.transform;         // 부모에서 벗어나기
        objectTransform.SetParent(null);                                                  // 부모에서 벗어나기
    
        // cornerClimbOn 트리거가 실행 되었는데, 바로 전환되지가 않아, while문을 바로 건너뛰고, isCornerClimb가 false가 되어, 트리거가 잔여하게 되는 문제를 해결하기 위해서
        // cornerClimbOn 트리거를 Any State에서 바로 실행되도록 변경
        playerAnim.SetTrigger("cornerClimbOn");
        AudioManager.instance.PlayerSfxCreate(8,true); // 사운드 생성
        
        while (true)
        {
            if (playerAnimStateInfo.IsName("CornerClimb"))
                break;
            yield return new WaitForFixedUpdate();
        }
        
        while (playerAnimStateInfo.IsName("CornerClimb"))
        {
            if(hangWallSensorCount != 0) // hangWallSensorCount 2개다 벗어나기 전까지 올라가기
                rb2D.MovePosition(rb2D.position + (Vector2.up * 8f * Time.fixedDeltaTime));
            else                         // 벗어나고 나서는 앞으로 가기
                rb2D.velocity = new Vector2(bodyGameObject.transform.localScale.x * 3f, rb2D.velocity.y);
            yield return new WaitForFixedUpdate();
        }
        currentJumpCount     = 2;                       // 점프 카운트
        
        isCornerClimb        = false;                   // 상태변경
    }
    
    private void Move()
    {
        if (!EventController.instance.moveLock)
        {
            // 이동(입력)
            // 평지
            if (!isSlope && PlayerFloorCollider.instance.isGrounded)
            {
                moveVector = new Vector2(Input.GetAxisRaw("Horizontal") * activeMoveSpeed, rb2D.velocity.y); // y값은 중력에 따라서 이동 유지
                moveVector += new Vector2(0f, -1f);
            }
            // 오르막
            else if (isSlope && PlayerFloorCollider.instance.isGrounded)
            {
                moveVector = perpendicular * activeMoveSpeed * Input.GetAxisRaw("Horizontal") * -1;
                if (floorJumpState)
                    moveVector.y = 0f;
            }
            // 공중
            else if (!PlayerFloorCollider.instance.isGrounded)
                moveVector = new Vector2(Input.GetAxisRaw("Horizontal") * activeMoveSpeed, rb2D.velocity.y); // y값은 중력에 따라서 이동 유지
        }
        
    }

    private void Slop()
    {
        // 슬로프 판단 레이 위치(플레이어 몸 기준으로 아래로 레이를 쏴서, 닿은 플로어 반사각 판단)
        RaycastHit2D hit = Physics2D.Raycast(PlayerFloorCollider.instance.transform.position + new Vector3(bodyGameObject.transform.localScale.x / 10f, 0f, 0f), 
                                    Vector2.down, slopeCheckDistance, floorLayer);
        // hit O (상황에 따라 Slop 판단)
        if (hit)
        {
            angle         = Vector2.Angle(hit.normal, Vector2.up);        // angle판단(Slop 판단)                               
            perpendicular = Vector2.Perpendicular(hit.normal).normalized; // 경사 각도 판단(표시용)
            
            if (angle != 0)
                isSlope = true;
            else
            {
                angle   = 0f;
                isSlope = false;
            }
        }
        // hit X (상태 초기화)
        else
        {
            angle   = 0f;
            isSlope = false;
        }

        // 미끄러짐 방지
        if (PlayerFloorCollider.instance.isGrounded && !PlayerBodyPush.instance.isBodyPushActive && 
                 (playerAnimStateInfo.IsName("Idle")       || playerAnimStateInfo.IsName("IdleSide") || playerAnimStateInfo.IsName("Death")        || playerAnimStateInfo.IsName("Stasis") ||
                  playerAnimStateInfo.IsName("FallToIdle") || PlayerHp.instance.isRecovery           || playerAnimStateInfo.IsName("Interaction1") || PlayerScan.instance.isScan))
        {
            //if(isSlope)
                rb2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            // else
            //     rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        // 공격 상황에 따라
        else if (PlayerFloorCollider.instance.isGrounded && PlayerAttack.instance.isAttackState && !PlayerAttack.instance.isAttackMove)
        {
            if (!PlayerBodyPush.instance.isBodyPushActive)
                rb2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            else if (PlayerBodyPush.instance.isBodyPushActive)
                rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        // 이외
        else
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        Debug.DrawLine(hit.point, hit.point + hit.normal,       Color.cyan);                     // 수직
        Debug.DrawLine(hit.point, hit.point + perpendicular,    new Color(0.78f, 0f, 1f)); // 반대
        Debug.DrawLine(hit.point, hit.point + (-perpendicular), new Color(1f, 0.96f, 0f)); // 정면
    }

    private void Other()
    {
        // moveVector 애니메이션 따라 조정 구간
        if (playerAnimStateInfo.IsName("IdleToRun") || playerAnimStateInfo.IsName("IdleSideToWalk"))
        {
            // 애니메이션 진행량 (0 -> 1)
            float progress = playerAnimStateInfo.normalizedTime;
            moveVector = new Vector2(moveVector.x * progress ,rb2D.velocity.y);
        }
        // moveVector 없애는 구간
        else if (playerAnimStateInfo.IsName("RunToIdle") || playerAnimStateInfo.IsName("WalkToIdleSide"))
        {
            moveVector = rb2D.velocity;
        }
    
        //이동(물리)
        if (PlayerHp.instance.liveState          && !PlayerHacking.instance.isHacking  && !PlayerScan.instance.isScan && !PlayerHp.instance.isRecovery && 
            !EventController.instance.eventState && !MenuManager.instance.isNormalMenu && !PlayerHp.instance.isHit    && !isJumpMacro                  &&
            !PlayerAttack.instance.isAttackState && !PlayerDash.instance.isDash        && !isCornerClimb)
        {
            // run 상태 체크는 상시 될 수 있게, IF 문 밖으로 뺌
            if (Input.GetAxisRaw("Horizontal") != 0 && !EventController.instance.moveLock)
            {
                rb2D.velocity = moveVector; // Move()함수에서 받아온 moveVector로 이동.
                playerAnim.SetBool("run", true);
                // 벽점프후무브와 벽잡기 중 이면 좌우반전 안되게
                if (!isHangWall && !isCornerClimb)
                    bodyGameObject.transform.localScale = new Vector3(Input.GetAxisRaw("Horizontal"), 1, 1); // 좌우반전
            }
            else
            {
                moveVector = Vector2.zero;                      // 입력값은 초기화
                playerAnim.SetBool("run", false);     // 입력값은 초기화
                if(!PlayerBodyPush.instance.isBodyPushActive)   // 바디 푸쉬 중이 아니면, 멈추기(-> 바디푸쉬 중 이면, 푸쉬 velocity 적용)
                    rb2D.velocity = new Vector2(0f,rb2D.velocity.y);
            }
        }
        else
        {
            moveVector = Vector2.zero; // 다른 함수들에서 이동하기 때문에, rb2D.velocity는 제어하지 않고, moveVector만 초기화 함.
            if(!NextScene.instance.isNextSeenMacro) // 다음씬 메크로가 작동 중 이지 않으면, false // 작동 중 이면, 메크로에 따라서, bool값이 변경 되도록
                playerAnim.SetBool("run", false);
        }

        // 사다리 이동(물리)
        if (isHangWall && !isCornerClimb && !isJumpMacro)
        {
            // 사다리 오르기 물리 실행
            if (isHangUp)
            {
                var hangLayerMask= 1 << LayerMask.NameToLayer("HangPlatform");
                var isCorner            = !Physics2D.OverlapCircle(cornerTrans.transform.position,    0.05f, hangLayerMask);
                
                switch (isCorner)
                {
                    // 코너 오르기
                    case true:
                        isCornerClimb = true;
                        StartCoroutine(CornerClimb());
                        break;
                    // 사다리 오르기
                    case false:
                    {
                        playerAnim.SetBool("upLadder",true);
                        playerAnim.SetBool("downLadder",false);
                        
                        // 모션 변경중에는 절반 속도만 이동하기
                        if(playerAnimStateInfo.IsName("HangToUpLadder") || playerAnimStateInfo.IsName("UpLadderToHang"))
                            rb2D.MovePosition(rb2D.position + (Vector2.up * Time.fixedDeltaTime * 2.5f));
                        else
                            rb2D.MovePosition(rb2D.position + (Vector2.up * Time.fixedDeltaTime * 5f));
                        break;
                    }
                }
            }
            // 사다리 내려가기 물리 실행
            // 사다리 내려가기 물리 실행
            else if (isHangDown)
            {
                var hangLayerMask= 1 << LayerMask.NameToLayer("HangPlatform");
                var isLegEnd            = !Physics2D.OverlapCircle(legEndTrans.transform.position,    0.05f, hangLayerMask);
                    
                switch (isLegEnd)
                {
                    // 레그엔드 떨어지기
                    case true:
                        isJumpMacro = true;
                        StartCoroutine(bodyGameObject.transform.localScale.x == 1 ? JumpMoveMacro(-1,false,false) : JumpMoveMacro(1,false,false));
                        break;
                    // 사다리 내려가기
                    case false:
                    {
                        playerAnim.SetBool("upLadder",false);
                        playerAnim.SetBool("downLadder",true);
                        
                        // 모션 변경중에는 절반 속도만 이동하기
                        if(playerAnimStateInfo.IsName("HangToDownLadder") || playerAnimStateInfo.IsName("DownLadderToHang"))
                            rb2D.MovePosition(rb2D.position - (Vector2.up * Time.fixedDeltaTime * 5f));
                        else
                            rb2D.MovePosition(rb2D.position - (Vector2.up * Time.fixedDeltaTime * 10f));
                        break;
                    }
                }
            }
            else
            {
                isHangUp   = false;
                isHangDown = false;
        
                playerAnim.SetBool("upLadder",false);
                playerAnim.SetBool("downLadder",false);   
            }
        }
        
        // 애니메이션 정보 갱신
        playerAnimStateInfo = playerAnim.GetCurrentAnimatorStateInfo(0);

        // rigidbody velocity Y 값 판단
        playerAnim.SetFloat("airSpeedY", rb2D.velocity.y);
        if (PlayerFloorCollider.instance.isGrounded)
            playerAnim.SetFloat("airSpeedY", 0f);
            
        // 바로 전환되는 문제를 해결하기 위해서
        if (playerAnimStateInfo.IsName("Jump"))
        {
            float progress = playerAnimStateInfo.normalizedTime;
            playerAnim.SetFloat("jumpToNext",progress);
        }
        else
            playerAnim.SetFloat("jumpToNext",0f);

        // 중력 적용(Hang상태 중력은 따로 적용됨.)
        if (!isHangWall)
        {
            // 공중
            if (Input.GetKey(KeyCode.Space) && !PlayerFloorCollider.instance.isGrounded && !playerAnimStateInfo.IsName("AirAttack1") && !playerAnimStateInfo.IsName("AirAttack2"))
                rb2D.gravityScale = longJumpGravity;
            // 바닥
            else
                rb2D.gravityScale = nomalGravity;
        }
    }

    private void OnDrawGizmos()
    {
        // 적 활성화 범위
        // Gizmos.color = new Color(0f, 1f, 0f);
        // Gizmos.DrawWireSphere(transform.position,DistanceActive.instance.activeDistance);
    }
}