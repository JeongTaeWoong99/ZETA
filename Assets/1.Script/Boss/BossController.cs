using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public static BossController instance;

    [Header("------Common------")] 
    public EnemyGenerator enemyGenerator;
    
    [HideInInspector]
    public Rigidbody2D       rb2D;
    public Animator          bossAnim;
    [HideInInspector]
    public AnimatorStateInfo bossAnimStateInfo; // 애니메이션 정보
    public GameObject        bodyObject;

    public BoxCollider2D     pushBody;
    
    private bool         isTrigger;                           // 공격 트리거 작동 상태
    private int          currentEventNum;
    public  List<Patten> pattenList        = new List<Patten>();
    
    [HideInInspector]     
    public int loopCount = 0;                  // 루프체크

    [HideInInspector]
    public int   glowFadeID;                   // 글로우
    [HideInInspector]
    public float bodyGlowFadeValue;            // 글로우
    [HideInInspector]
    public float lanceGlowFadeValue;           // 글로우
     
    [HideInInspector]
    public int   brightFadeID;                 // 밝기
    [HideInInspector]
    public float bodyBrightFadeValue;          // 밝기
    [HideInInspector]
    public float lanceBrightFadeValue;         // 밝기

    public Material bossBodyMat;             // 바디 멧
    public Material bossBodyLightMat;        // 바디 라이트 멧
    public Material bossLanceMat;            // 렌스바디 멧
    public Material bossLanceLightMat;       // 렌스바디 라이트 멧
    
    [Header("------Sound------")]
    public  AudioSource[] enemyLoopSoundList;
    private List<float>   originVolumeValueList = new List<float>();
    public  List<float>   volumeUpSpeed         = new List<float>();
    //public bool           isAimingSound; -> 보스 컨트롤러에서는 isPlayerAiming으로 사용.
    
    [Header("------Warp------")]
    public List<Transform> warpTransList = new List<Transform>();
    [HideInInspector]
    public bool            isAppear = false;
    public float           warpSpeed;
    
    [HideInInspector]
    public int    currentWarpNum;                             // 해당 러프번호를 이용해서, BossAttack Lance1 Lerp이동에 사용 // 등등 여러군데에서 사용.
    
    [Header("------Laser------")]
    public GameObject laserPrefabs;
    public GameObject laserShootEffect;
    public float      angleCorrectionValue;     // 보정값
    
    [HideInInspector] 
    public  bool isLaserPatten;                                                                  // 레이저 페턴 상태
    
    public  float laserCoroutineTime;                                                            // 레이저 패턴 유지시간
    private float laserCoroutineTimeCount;                                                       // 카운트

    public  List<AlterLine>           alterLineList           = new List<AlterLine>();           // 각 총신 얼터라인
    public  List<Transform>           gunTranList             = new List<Transform>();           // 총신 위치
    public  List<SkeletonUtilityBone> laserBoneList           = new List<SkeletonUtilityBone>(); // 총신의 Bone
    
    private List<float>               laserBoneOriginZList    = new List<float>();               // 계산식
    private List<float>               targetRotationList      = new List<float>();               // 계산식

    [HideInInspector] 
    public bool isPlayerAiming;                                                                  // 플레이어를 플레이어를 조준하고 있는 상태인지(따라가고, 레이저를 쏘기 전까지 얼터라인이 켜져 있도록)
    
    [HideInInspector]
    public bool                       rightGunAimingCompleted;                                   // 오른쪽 날개 얼터라인 닿았는지 상태 체크
    [HideInInspector]
    public bool                       leftGunAimingCompleted;                                    // 왼쪽 날개 얼터라인 닿았는지 상태 체크
    
    private bool                      laserCoroutineState;                                       // 레이저어택 코루틴 돌아가는지 확인
    public  int                       chaseSpeed;                                                // 총신 추적 속도
    public  float                     shootPauseTime;
                                        
    public  float                     sprayWaitTime;                                              // 스프레이 전 대기시간(공격 위치 도착 후, 기다리는 시간)
    public  List<Transform>           sprayArrivalTranList = new List<Transform>();               // 스프레이 도착 위치
    public  float                     sprayIntervalTime;                                          // 슛 쏘는 간격
    public  int                       sprayMoveSpeed;
    public  float                     sprayTime;                                                  // 스프레이 유지 타임

    public  float                     returnSpeed;                                                // 패턴 끝 총신 복구 속도
    
    [Header("------Missile------")] 
    public GameObject       missilePrefabs;                           // 미사일 프리팹
    public GameObject       missileShootEffect;                       // 미사일 슛 이팩트(연기)
   
    public List<Transform>  hatchTransListR = new List<Transform>();
    public List<Transform>  hatchTransListL = new List<Transform>();
    [HideInInspector] 
    public List<GameObject> missileList     = new List<GameObject>(); // 한번에 상태변환을 위한 리스트
    
    public  float           missileWaitTime;                          // 미사일이 모두 생성되고 날라가는 시간
    private float           missileWaitTimeCount;
    
    [Header("------Lance Passing------")]
    public  float lanceFadeSpeed;
    [HideInInspector] 
    public bool isLanceAppearCoroutineRunning;
    
    // [Header("------LinearGun------")] 
    // public float linearGunChaseSpeed;
    
    // public  List<GameObject>     lanceLighteningEffectList         = new List<GameObject>();
    // private List<ParticleSystem> lanceLighteningparticleSystemList = new List<ParticleSystem>();
    
    // [Header("------Lance2------")]
    // public  float moveSpeed;                                  // 이동속도
    // public  float lanceCoroutineTime;                         // 렌스 공격 코루틴 유지시간
    // private float lanceCoroutineTimeCount;                    // 카운트

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();

        isAppear = false;

        // 글로우(등장 전 모두 초기화)
        glowFadeID = Shader.PropertyToID("_FullGlowDissolveFade");
        bodyGlowFadeValue      = 0;
        bossBodyMat.SetFloat(glowFadeID, 0f);
        bossBodyLightMat.SetFloat(glowFadeID, 0f);
        bossLanceMat.SetFloat(glowFadeID, 0f);
        bossLanceLightMat.SetFloat(glowFadeID, 0f);
        
        // 밝기(등장 전 모두 초기화)
        brightFadeID = Shader.PropertyToID("_Brightness");
        bodyBrightFadeValue      = 0;
        bossBodyMat.SetFloat(brightFadeID, 0f);
        bossBodyLightMat.SetFloat(brightFadeID, 0f);
        bossLanceMat.SetFloat(brightFadeID, 0f);
        bossLanceLightMat.SetFloat(brightFadeID, 0f);
        
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
    
    public enum Patten{LinearGunPatten,LancePassingPatten,MissilePatten,LaserPatten}
    
    private void FixedUpdate()
    {
        // 트리거 발동
        if (isAppear && !isTrigger && bossAnimStateInfo.IsName("Idle") && BossHP.instance.isLive)
        {
            // 상태 변경
            isTrigger = true;

            if (currentEventNum > pattenList.Count - 1) 
                currentEventNum = 0;
            
            // 이벤트 실행
            StartCoroutine(pattenList[currentEventNum].ToString());
            currentEventNum++;
        }

        LoopSound();
        
        Other();
    }

    private IEnumerator LinearGunPatten()
    {
        yield return StartCoroutine(Warp(6,true)); // 위치이동
        
        while (true)
        {
            if (bossAnimStateInfo.IsName("Idle"))
            {
                AudioManager.instance.WardenSfxCreate(6,true,gameObject); // IdleToLinearGun 사운드.
                bossAnim.SetTrigger("linearGunOn");                                // 애니메이션 재생
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        // 애니메이션 루프
        while (true)
        {
            if (loopCount == 4)
            {
                AudioManager.instance.WardenSfxCreate(9,true,gameObject); // LinearGunToIdle 사운드.
                bossAnim.SetTrigger("linearGunOff");
                isTrigger = false;
                loopCount = 0;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    // FixedUpdate
    private IEnumerator MissilePatten()
    {
        yield return StartCoroutine(Warp(0,true)); // 위치이동
        
        while (true)
        {
            if (bossAnimStateInfo.IsName("Idle"))
            {
                AudioManager.instance.WardenSfxCreate(10,true,gameObject); // IdleToMissile 사운드.
                bossAnim.SetTrigger("missileOn"); // 애니메이션 재생
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        bool isSoundCreate = false; // 미사일 사운드 생성 체크.
        // 애니메이션 루프
        while (true)
        {
            if (!isSoundCreate && bossAnimStateInfo.IsName("Missile")) // 1회 사운드 생성.
            {
                isSoundCreate = true;
                AudioManager.instance.WardenSfxCreate(11,true,gameObject); // Missile 사운드.
            }
        
            if (loopCount == 4)
            {
                AudioManager.instance.WardenSfxCreate(14,true,gameObject); // Missile 사운드.
                bossAnim.SetTrigger("missileOff");
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        
        // 미사일 퍼지는 시간 기다리기 초기화
        missileWaitTimeCount = missileWaitTime;
        
        while (true)
        {
            missileWaitTimeCount -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;    // 액셀시간값에 영향을 받아야 함.

            if (missileWaitTimeCount < 0)
            {
                // 미사일 모두 상태 전환(해킹가능 상태로 변경)
                foreach (var missile in missileList)
                {
                    if(missile != null)
                        missile.GetComponent<GuidedMissile>().isTracking = true;
                }
                break;
            }
            
            yield return null;
        }

        // 트리거 종료
        while (true)
        {
            // 사망 체크.
            if (!BossHP.instance.isLive)
            {
                foreach (var missile in missileList)
                {
                    if(missile != null) 
                        missile.GetComponent<GuidedMissile>().DestroyTrigger(); // 미사일 모두 파괴
                }
                break;
            }
        
            // 미사일 다 사라졌는지 체크
            bool isMissileListCheck = false; // 초기화
            foreach (var missile in missileList)
            {
                if (missile != null)
                {
                    isMissileListCheck = true; // 1개라도 남아 있으면, true
                }
            }

            // true가 아니면, 리스트의 모든 미사일들이 null이므로 종료
            if (!isMissileListCheck)
            {
                yield return new WaitForSeconds(1f);
                missileList.Clear();
                isTrigger = false;
                loopCount = 0;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator LancePassingPatten()
    {
        // 이동 위치 판단
        if (PlayerHp.instance)
        { 
            // 플레이어와 더 먼 거리로 워프
            float distanceToWarp2 = Vector2.Distance(PlayerController.instance.transform.position, warpTransList[2].position);
            float distanceToWarp3 = Vector2.Distance(PlayerController.instance.transform.position, warpTransList[3].position);
    
            currentWarpNum = distanceToWarp2 > distanceToWarp3 ? 2 : 3;
            yield return StartCoroutine(Warp(currentWarpNum,true)); // 위치이동
        }
        
        StartCoroutine(LanceAppear(true));                               // 렌스 나타나기
        while (true)
        {
            // 사망 체크.
            if (!BossHP.instance.isLive)
            {
                break;
            }
            
            // Idle상태이고 + 렌스가 다 나타났고 + 살아있다면, 공격 실행.
            if (bossAnimStateInfo.IsName("Idle") && !isLanceAppearCoroutineRunning && BossHP.instance.isLive)
            {
                AudioManager.instance.WardenSfxCreate(15,true,gameObject); // Lance 사운드 생성.
                bossAnim.SetTrigger("lance1On"); // 찌르기 공격
                break;
            }
            
            yield return new WaitForFixedUpdate();
        }
        
        yield return new WaitForFixedUpdate();  // Idle 애니메이션 전환 대기
        
        while (true)
        {
            // 사망 체크.
            if (!BossHP.instance.isLive)
            {
                break;
            }
            
            // Idle이면 종료.
            if (bossAnimStateInfo.IsName("Idle") && BossHP.instance.isLive)
            {   
                StartCoroutine(LanceAppear(false)); // 렌스 사라지기(바디와 함께 사라지기)
                
                isTrigger = false;                             // 트리거 끝내기
                
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    
    // Update
    private IEnumerator LaserPatten()
    {
        yield return StartCoroutine(Warp(1,true));   // 위치이동
        
        while (true)
        {
            if (bossAnimStateInfo.IsName("Idle"))
            {
                AudioManager.instance.WardenSfxCreate(4,true,gameObject);	// IdleToLaser 사운드.
                bossAnim.SetTrigger("laserOn");
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        // 레이저 루프로 들어오면, bone상태 전환(Laser 루프 들어오면, 1회)
        while (true)
        {
            if (bossAnimStateInfo.IsName("Laser"))
            {
                // 레이저 페턴 시작 + 조준
                isLaserPatten  = true;
                isPlayerAiming = true;
                
                foreach (var bone in laserBoneList)
                {
                    // 회전값 계산식
                    laserBoneOriginZList.Add(bone.gameObject.transform.localRotation.eulerAngles.z);
                    targetRotationList.Add(laserBoneOriginZList[laserBoneList.IndexOf(bone)]);

                    // 회전 가능하게 상태 변경
                    bone.mode      = SkeletonUtilityBone.Mode.Override;
                    bone.zPosition = false;
                    bone.position  = false;
                    bone.scale     = false;
                }
                break;
            }
            yield return null;
        }
        
        // 바깥쪽 드론 2개 생성
        StartCoroutine(enemyGenerator.CreateDrone(0));
        yield return new WaitForFixedUpdate();
        StartCoroutine(enemyGenerator.CreateDrone(3));

        // 중간 생성 상태 초기화
        bool isIntermediateMakeDrone = false;          
        
        // 모든 시작 각도는 laserBoneLists[i].gameObject.transform.localRotation.eulerAngles.z -> Z값 360에서 시작
        // 한바퀴 돌면 다시 360으로 돌아옴
        laserCoroutineTimeCount = 0f; // 코루틴타임 초기화
        while (true)
        {
            laserCoroutineTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue; // 레이저 타임 체크

            // 중간 가드 생성
            if (!isIntermediateMakeDrone && laserCoroutineTimeCount >= laserCoroutineTime / 2)
            {
                isIntermediateMakeDrone = true;
                
                // 안쪽 드론 2개 생성
                StartCoroutine(enemyGenerator.CreateDrone(1));
                yield return new WaitForFixedUpdate();
                StartCoroutine(enemyGenerator.CreateDrone(2));
            }
            
            // 총구 플레이어 추적
            if (laserCoroutineTimeCount < laserCoroutineTime && !laserCoroutineState && PlayerHp.instance.liveState)
            {
                // 공격(조준 완료)
                if (rightGunAimingCompleted && leftGunAimingCompleted)
                {
                    laserCoroutineState = true;
                    yield return StartCoroutine(LaserSalvoAttack());
                }
                // 이동(조준 비완료)
                else
                {
                    ProcessLaserBoneList(chaseSpeed,true);
                }
            }
            // 흩뿌기리 공격 및 총구 원래 위치 복구
            else if (laserCoroutineTimeCount >= laserCoroutineTime && !laserCoroutineState)
            {
                // 흩뿌리기 공격
                yield return StartCoroutine(LaserSprayAttack());
                
                // 패턴 끝
                isLaserPatten  = false;
                isPlayerAiming = false;
                
                // 원래자리 복구
                while (true)
                {
                    bool allGunBack = true; // 모든 총구가 원래 자리로 돌아 왔는지(모든 총구가 if 문에 걸리지 않는다면, true로 break됨.)
                    
                    for (int i = 0; i < laserBoneList.Count; i++)
                    {
                        // 사용값
                        float currentZRotation  = laserBoneList[i].gameObject.transform.localRotation.eulerAngles.z;    // 현재
                        float originalZRotation = targetRotationList[i];                                                // 복구방향         
                        // 앵글차이 계산
                        float angleDifference   = Mathf.DeltaAngle(currentZRotation, originalZRotation);           // 현재 차이 계산
                        float rotationThreshold = 0.01f;                                                                // 허용 오차값                                           
                        if (Mathf.Abs(angleDifference) > rotationThreshold)
                        {
                            // 이동방향
                            float rotationDirection = Mathf.Sign(angleDifference);
                            currentZRotation       += rotationDirection * returnSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;          
                            // 오차값 복구
                            if ((rotationDirection > 0 && currentZRotation >= originalZRotation) || 
                                (rotationDirection < 0 && currentZRotation <= originalZRotation))
                            {
                                currentZRotation = originalZRotation;
                            }           
                            // 이동
                            var newRotation = Quaternion.Euler(0, 0, currentZRotation);
                            laserBoneList[i].gameObject.transform.localRotation = newRotation;
                            
                            allGunBack  = false;
                        }
                    }
                    
                    // 트리거 종료
                    if (allGunBack)
                    {
                        isTrigger = false;
                        loopCount = 0;
                        AudioManager.instance.WardenSfxCreate(5,true,gameObject);	// IdleToLaser 사운드.
                        bossAnim.SetTrigger("laserOff");      
                        foreach (var bone in laserBoneList)
                        {
                            bone.mode      = SkeletonUtilityBone.Mode.Follow;
                            bone.zPosition = true;
                            bone.position  = true;
                            bone.scale     = true;
                        }
                    
                        laserBoneOriginZList.Clear();
                        targetRotationList.Clear();
                        break;
                    }
                    yield return null;
                }
                break;
            }
            yield return null;
        }
    }
    
    // 일반 조준 설정
    private void ProcessLaserBoneList(int activeSpeed, bool isUseLerp)
    {
        for (int i = 0; i < laserBoneList.Count; i++)
        {
            Vector2 personalDirection;
            if (PlayerAcceleration.instance.isAcceleration)
            {
                personalDirection = new Vector2(laserBoneList[i].gameObject.transform.position.x - PlayerAcceleration.instance.inputAccelerationXtrans, 
                                                laserBoneList[i].gameObject.transform.position.y - PlayerAcceleration.instance.inputAccelerationYtrans);
            }
            else
            {
                personalDirection = new Vector2(laserBoneList[i].gameObject.transform.position.x - PlayerController.instance.transform.position.x, 
                                                laserBoneList[i].gameObject.transform.position.y - PlayerController.instance.transform.position.y);
            }
            personalDirection *= -1;

            CalculateAndApplyRotation(personalDirection, i, angleCorrectionValue,activeSpeed, isUseLerp);
        }
    }
    
    // 흩뿌리기 조준 설정
    private void ProcessLaserBoneListAlternative(int aimNum,int activeSpeed, bool isUseLerp) 
    {
        // 시작 위치
        if (aimNum == 0)
        {
            for (int i = 0; i < laserBoneList.Count; i++)
            {
                Vector2 personalDirection = new Vector2(laserBoneList[i].gameObject.transform.position.x - sprayArrivalTranList[aimNum].transform.position.x, 
                                                        laserBoneList[i].gameObject.transform.position.y - sprayArrivalTranList[aimNum].transform.position.y);
                personalDirection *= -1;

                CalculateAndApplyRotation(personalDirection, i, angleCorrectionValue,activeSpeed,isUseLerp);
            }
        }
        // 도착 위치
        else if (aimNum == 1)
        {
            for (int i = 0; i < laserBoneList.Count; i++)
            {
                Vector2 personalDirection = new Vector2(laserBoneList[i].gameObject.transform.position.x - sprayArrivalTranList[aimNum].transform.position.x, 
                                                        laserBoneList[i].gameObject.transform.position.y - sprayArrivalTranList[aimNum].transform.position.y);
                personalDirection *= -1;
                
                CalculateAndApplyRotation(personalDirection, i, angleCorrectionValue,activeSpeed,isUseLerp);
            }
        }
    }
    
    // 메인 날개 변경.
    private void CalculateAndApplyRotation(Vector2 direction, int i, float angleCorrectionValue,int activeSpeed,bool isUseLerp)
    {
        float angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg;
        
        // 기본 각도 설정
        float baseAngle = (i < 3) ? 65 : 70;
    
        // 이동
        // 오른쪽 날개
        if (i == 1)
        {
            Quaternion angleAxis = Quaternion.AngleAxis(angle + baseAngle + (laserBoneOriginZList[i] * bodyObject.transform.localScale.x), Vector3.forward);
            Quaternion rotation;
            if(isUseLerp)
                rotation  = Quaternion.Lerp(laserBoneList[i].gameObject.transform.rotation, angleAxis, activeSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
            else
                rotation  = Quaternion.RotateTowards(laserBoneList[i].gameObject.transform.rotation, angleAxis, activeSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
            laserBoneList[i].gameObject.transform.rotation = rotation;
    
            // 보정
            float correctedAnglePlus  = angle + angleCorrectionValue;
            float correctedAngleMinus = angle - angleCorrectionValue;
    
            // 종속 날개 변경.
            ApplyRotation(0, correctedAngleMinus, baseAngle,activeSpeed,isUseLerp);
            ApplyRotation(2, correctedAnglePlus, baseAngle,activeSpeed,isUseLerp);
        }
        // 왼쪽 날개
        else if (i == 4)
        {
            Quaternion angleAxis = Quaternion.AngleAxis(angle + baseAngle + (laserBoneOriginZList[i] * bodyObject.transform.localScale.x), Vector3.forward);
            Quaternion rotation;
            if(isUseLerp)
                rotation  = Quaternion.Lerp(laserBoneList[i].gameObject.transform.rotation, angleAxis, activeSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
            else
                rotation  = Quaternion.RotateTowards(laserBoneList[i].gameObject.transform.rotation, angleAxis, activeSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
            laserBoneList[i].gameObject.transform.rotation = rotation;
            
            // 보정(왼쪽은 어째선지 더 적게 벌어져서, 2를 곱해줌.)
            float correctedAnglePlus  = angle + angleCorrectionValue * 2;
            float correctedAngleMinus = angle - angleCorrectionValue * 2;
    
            // 종속 날개 변경.
            ApplyRotation(3, correctedAnglePlus, baseAngle,activeSpeed,isUseLerp);
            ApplyRotation(5, correctedAngleMinus, baseAngle,activeSpeed,isUseLerp);
        }
    }
    
    // 종속 날개 변경.
    private void ApplyRotation(int index, float correctedAngle, float baseAngle,int activeSpeed, bool isUseLerp)
    {
        Quaternion angleAxis = Quaternion.AngleAxis(correctedAngle + baseAngle + (laserBoneOriginZList[index] * bodyObject.transform.localScale.x), Vector3.forward);
        Quaternion rotation;
        if(isUseLerp)
            rotation = Quaternion.Lerp(laserBoneList[index].gameObject.transform.rotation, angleAxis, activeSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
        else
            rotation = Quaternion.RotateTowards(laserBoneList[index].gameObject.transform.rotation, angleAxis, activeSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
        laserBoneList[index].gameObject.transform.rotation = rotation;
    }         
              
    private IEnumerator LaserSalvoAttack()  // 조준 공격 루틴
    {
        for (int i = 0; i < laserBoneList.Count; i++)
            Instantiate(BossAttack.instance.attackPauseEffect, gunTranList[i].gameObject.transform.position, Quaternion.identity); // 얼터 트윈클 생성
        float shootPauseTimeCount = 0f;                                                                                   // 퍼즈타임 초기화
         
        while (true)
        {
            shootPauseTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;     // 타임 카운트(해당 0번 3번만 사용)
            
            laserCoroutineTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue; // 레이저 타임 체크(레이저 패턴 전체 타임 체크)
            
            // 레이저 생성(해당 0번 3번만 사용)
            if (shootPauseTimeCount > shootPauseTime)
            {
                shootPauseTimeCount = 0f;
            
                isPlayerAiming = false;     // 얼터 라인 끄기
                
                // 슛 사운드 독립 생성(각 왼쪽 오른쪽 날계의 메인 날계인 경우에만 해당 위치에, 사운드 생성 - 총 2개)                                           
                foreach (var alterLineLists in alterLineList)
                    if(alterLineLists.isRightWingMid || alterLineLists.isLeftWingMid)                         
                        AudioManager.instance.WardenSfxCreate(0,true,alterLineLists.gameObject); // 사운드 생성
                
                // 총알생성
                for (int i = 0; i < laserBoneList.Count; i++)
                {
                    Instantiate(laserPrefabs, gunTranList[i].gameObject.transform.position , laserBoneList[i].gameObject.transform.rotation);
                    
                    // AlterLine의 pointCircle의 위치를 보고 슛 이펙트의 앵글을 구하고, 만든다
                    Vector2 direction = new Vector2(gunTranList[i].transform.position.x - alterLineList[i].pointCircle.transform.position.x, 
                        gunTranList[i].transform.position.y - alterLineList[i].pointCircle.transform.position.y);
                                    
                    direction *= -1;                                                                                    // 방향 반전 조정
                    float      angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg;     // 회전하는 앵글값
                    
                    Instantiate(laserShootEffect, gunTranList[i].position, Quaternion.Euler(-angle , 90f, 0f));
                }
    
                // 모든 미사일 파괴확인 및 종료
                while (true)
                {
                    shootPauseTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;     // 타임 카운트(해당 0번 3번만 사용)
    
                    if (shootPauseTimeCount > shootPauseTime)
                    {
                        laserCoroutineState = false; 
                        isPlayerAiming      = true;
                        break;
                    }
                    yield return null;
                }
                break;
            }
            yield return null;
        }
    }
    
    private IEnumerator LaserSprayAttack()  // 흩뿌리기 공격 루틴
    {
        isPlayerAiming = false; // 얼터 라인 끄기
        
        currentWarpNum = 4;
        yield return StartCoroutine(Warp(currentWarpNum,false));    // 위치이동
        
        isPlayerAiming = true; // 얼터 라인 켜기
        
        // 총구 지정 위치로 이동
        float sprayWaitTimeCount = 0;
        while (true)
        {
            // 흩뿌리기 시작 위치 이동
            ProcessLaserBoneListAlternative(0,chaseSpeed,true);
            
            sprayWaitTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (sprayWaitTimeCount > sprayWaitTime)
            {
                for (int i = 0; i < laserBoneList.Count; i++)
                    Instantiate(BossAttack.instance.attackPauseEffect, gunTranList[i].gameObject.transform.position, Quaternion.identity); // 얼터 트윈클 생성
                break;
            }
            
            yield return null;
        }
        
        // 뿌리기(조준 위치 -> 도착 위치)  
        float sprayIntervalTimeCount = 0f;    // 총알 인터벌 체크 카운트 초기화
        float sprayTimeCount         = 0f;    // 스프레이 타임 카운트 초기화
        bool  isRightWingSoot        = false; // 오른쪽 왼쪽 번갈아가며 쏘기(촘촘하게 뿌리도록) 
        
        while (true)
        {
            // 흩뿌리기 도착 위치 이동
            ProcessLaserBoneListAlternative(1,sprayMoveSpeed,false);
            
            // 총알 뿌리기
            sprayIntervalTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (sprayIntervalTimeCount > sprayIntervalTime)
            {
                sprayIntervalTimeCount = 0f; // 초기화
                
                // 왼쪽 날개 쏘기
                if (!isRightWingSoot)
                {
                    // 왼쪽 사운드 생성
                    foreach (var alterLineLists in alterLineList)
                    {
                        if(alterLineLists.isLeftWingMid)                         // ↓ 카메라와의 위치를 위한 Z값 -10
                            AudioManager.instance.WardenSfxCreate(0,true,alterLineLists.gameObject); // 사운드 생성
                    }
                    
                    // 왼쪽 총알생성(3~5)
                    for (int i = 3; i < laserBoneList.Count; i++)
                    {
                        Instantiate(laserPrefabs, gunTranList[i].gameObject.transform.position, laserBoneList[i].gameObject.transform.rotation);
                        
                        // 슛 이펙트
                        // 3D 이펙트 angle에 따라 x값 회전
                        // AlterLine의 pointCircle의 위치를 보고 슛 이펙트의 앵글을 구하고, 만든다
                        Vector2 direction = new Vector2(gunTranList[i].transform.position.x - alterLineList[i].pointCircle.transform.position.x, 
                            gunTranList[i].transform.position.y - alterLineList[i].pointCircle.transform.position.y);
                        direction *= -1; // 방향 반전 조정
                        float angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg; // 회전하는 앵글값
                        
                        Instantiate(laserShootEffect, gunTranList[i].position, Quaternion.Euler(-angle , 90f, 0f));
                    }

                    isRightWingSoot = true;
                }
                // 오른쪽 날개 쏘기
                else if (isRightWingSoot)
                {
                    // 오른쪽 사운드 생성
                    foreach (var alterLineLists in alterLineList)
                    {
                        if(alterLineLists.isRightWingMid)                         // ↓ 카메라와의 위치를 위한 Z값 -10
                            AudioManager.instance.WardenSfxCreate(0,true,alterLineLists.gameObject); // 사운드 생성
                    }
                    
                    // 오른쪽 총알생성(0~2)
                    for (int i = 0; i < 3; i++)
                    {
                        Instantiate(laserPrefabs, gunTranList[i].gameObject.transform.position, laserBoneList[i].gameObject.transform.rotation);
                        
                        // 슛 이펙트
                        // 3D 이펙트 angle에 따라 x값 회전
                        // AlterLine의 pointCircle의 위치를 보고 슛 이펙트의 앵글을 구하고, 만든다
                        Vector2 direction = new Vector2(gunTranList[i].transform.position.x - alterLineList[i].pointCircle.transform.position.x, 
                            gunTranList[i].transform.position.y - alterLineList[i].pointCircle.transform.position.y);
                        direction *= -1; // 방향 반전 조정
                        float angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg; // 회전하는 앵글값
                        
                        Instantiate(laserShootEffect, gunTranList[i].position, Quaternion.Euler(-angle , 90f, 0f));
                    }

                    isRightWingSoot = false;
                }
            }
            
            // 패턴 끝내기
            sprayTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (sprayTimeCount > sprayTime)
                break;
            
            yield return null;
        }
        
    }
    
    public IEnumerator LanceAppear(bool onOff)
    {
        isLanceAppearCoroutineRunning = true;
        while (true)
        {
            // On
            if (onOff)
            {
                // 랜스 등장
                if (lanceGlowFadeValue < 1)
                {
                    lanceGlowFadeValue          += Time.deltaTime * lanceFadeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                    bossLanceMat.SetFloat(glowFadeID, lanceGlowFadeValue);
                    bossLanceLightMat.SetFloat(glowFadeID, lanceGlowFadeValue);
                }
                // 라이트 밝히기
                else if (lanceGlowFadeValue >= 1)
                {
                    // 값 픽스
                    lanceGlowFadeValue = 1f;
                    bossLanceMat.SetFloat(glowFadeID, lanceGlowFadeValue);
                    bossLanceLightMat.SetFloat(glowFadeID, lanceGlowFadeValue);
                    
                    if (lanceBrightFadeValue < 4)
                    {
                        lanceBrightFadeValue          += Time.deltaTime * lanceFadeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                        bossLanceLightMat.SetFloat(brightFadeID, lanceBrightFadeValue);

                        if (lanceBrightFadeValue >= 4)
                        {
                            // 랜스가 나타나고, 후 켜기
                            // 렌스 이펙트가 켜져있지 않을 때 켜기(1회)
                            if (!BossAttack.instance.lanceLighteningParticleSystemList[0].isPlaying)
                            {
                                for (int i = 0; i < BossAttack.instance.lanceLighteningParticleSystemList.Count; i++)
                                    BossAttack.instance.lanceLighteningParticleSystemList[i].Play();
                            }
                        
                            // 값 픽스
                            lanceBrightFadeValue = 4f;
                            bossLanceLightMat.SetFloat(brightFadeID, lanceBrightFadeValue);
                            break;
                        }                       
                    }
                }
            }
            // off
            else
            {
                // 랜스가 사라지기 전에, 선 끄기
                // 회복이펙트가 켜져있을 때 끄기(1회)
                if (BossAttack.instance.lanceLighteningParticleSystemList[0].isPlaying)
                {
                    foreach (var t in BossAttack.instance.lanceLighteningParticleSystemList)
                        t.Stop();
                }
            
                // 바디 사라지기
                lanceGlowFadeValue          -= Time.deltaTime * lanceFadeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                bossLanceMat.SetFloat(glowFadeID, lanceGlowFadeValue);
                bossLanceLightMat.SetFloat(glowFadeID, lanceGlowFadeValue);
                
                // 라이트 끄기
                lanceBrightFadeValue          -= Time.deltaTime * lanceFadeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                bossLanceLightMat.SetFloat(brightFadeID, lanceBrightFadeValue);
                
                if (lanceGlowFadeValue <= 0 && lanceBrightFadeValue <= 0)
                {
                    // 값픽스(바디)
                    lanceGlowFadeValue = 0f;
                    bossLanceMat.SetFloat(glowFadeID, lanceGlowFadeValue);
                    bossLanceLightMat.SetFloat(glowFadeID, lanceGlowFadeValue);
                    
                    // 값 픽스(라이트)
                    lanceBrightFadeValue = 0f;
                    bossLanceLightMat.SetFloat(brightFadeID, lanceBrightFadeValue);
                    
                    break;
                }
            }
            yield return null;
        }
        isLanceAppearCoroutineRunning = false;
    }
    
    // Update
    private IEnumerator Warp(int transNum,bool isAnimTrigger)
    {   
        // 순서 1 : 바디 사라지기
        isAppear = false;                       // 사라진 상태
        
        if(isAnimTrigger)
            bossAnim.SetTrigger("appearOn"); // 애니메이션
        pushBody.enabled = false;                 // 바디푸쉬 끄기
        
        AudioManager.instance.EnemySfxCreate(9,false,gameObject); // 사운드
        while (true)
        {
            if (bodyGlowFadeValue >= 0)
            {
                bodyGlowFadeValue          -= Time.deltaTime * warpSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                
                bossBodyMat.SetFloat(glowFadeID, bodyGlowFadeValue);
                bossBodyLightMat.SetFloat(glowFadeID, bodyGlowFadeValue);
                
                if (bodyGlowFadeValue <= 0)
                {
                    // 값 픽스
                    bodyGlowFadeValue  = 0f;
                    
                    bossBodyMat.SetFloat(glowFadeID, bodyGlowFadeValue);
                    bossBodyLightMat.SetFloat(glowFadeID, bodyGlowFadeValue);
                
                    break;
                }                       
            }
            yield return null;
        }
        
        // 순서 2 : 바디 나타나기
        // 3 or 5 왼쪽 바라보기
        if(transNum == 3 || transNum == 5 || transNum == 6)
            bodyObject.transform.localScale = new Vector3(-1f, 1f, 1f);
        // 나머지 다 오른쪽 바라보기
        else
            bodyObject.transform.localScale = new Vector3(1f, 1f, 1f);
            
        transform.position = warpTransList[transNum].position;              // 위치 이동
        
        AudioManager.instance.EnemySfxCreate(9,false,gameObject); // 사운드
        
        while (true)
        {
            // 바디 등장 
            if (bodyGlowFadeValue <= 1)
            {
                bodyGlowFadeValue          += Time.deltaTime * warpSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                
                bossBodyMat.SetFloat(glowFadeID, bodyGlowFadeValue);
                bossBodyLightMat.SetFloat(glowFadeID, bodyGlowFadeValue);
                
                if (bodyGlowFadeValue >= 1)
                {
                    // 값픽스
                    bodyGlowFadeValue  = 1f;
                    
                    bossBodyMat.SetFloat(glowFadeID, bodyGlowFadeValue);
                    bossBodyLightMat.SetFloat(glowFadeID, bodyGlowFadeValue);
                    
                    isAppear = true;                       // 나타남 상태
                    if (isAnimTrigger)
                    {
                        AudioManager.instance.WardenSfxCreate(3,true,gameObject);	// appearToIdle 사운드.
                        bossAnim.SetTrigger("appearOff"); // 애니메이션 트리거
                    }
                    pushBody.enabled = true;               // 바디푸쉬 켜기
                    break;
                }                       
            }
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    private IEnumerator DeathEvent()
    {
        yield return StartCoroutine(Warp(2,false));  // 2번 이동 및 애니메이션 재생 X
        
        yield return null;
    }
    
    private void HandleLoopSound(int soundIndex, string animStateName)
    {
        if (bossAnimStateInfo.IsName(animStateName))
        {
            // 애니메이션이 Laser or LinearGun인데, 해당 조건들을 만족하지 않으면, 조준하고 있지 않은 상태이다.
            // 그러니, 오히려 소리를 줄여야 한다.
            if ((bossAnimStateInfo.IsName("Laser") || bossAnimStateInfo.IsName("LinearGun")) && !isPlayerAiming)
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
        if (enemyLoopSoundList[0] != null)
            HandleLoopSound(0, "Laser");
        
        if (enemyLoopSoundList[1] != null)
            HandleLoopSound(1, "LinearGun");
    }
    
    private void Other()
    {
        // 애니메이션 정보 갱신
        bossAnimStateInfo = bossAnim.GetCurrentAnimatorStateInfo(0);
        
        if (BossAttack.instance.isAttackPause)
            bossAnim.speed = 0f;
        else
            bossAnim.speed = PlayerAcceleration.instance.accelerationChangedTimeValue;
    }
    
}
