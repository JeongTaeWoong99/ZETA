using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class Turret : MonoBehaviour
{
    [Header("------Common------")] 
    public GameObject laserPrefabs;
    public GameObject shootEffect;

    public Transform shootTrans;
    public Transform shootEffectTrans;

    public  float shootCoolTime;
    private float shootCoolTimeCount;
    
    public List<CustomMaterialObject> CustomMaterialObject = new List<CustomMaterialObject>();           // 바디라이트의 메터리얼
    public float                      lightChangeSpeed;

    public bool isDisabled;
    
    [HideInInspector]
    public bool isControlTerminalScanned; // 터미널에 종속되어 있는 터렛인지
    
    [Header("------Sound------")]
    public  AudioSource[] loopSoundList;
    private List<float>   originVolumeValueList = new List<float>();
    public  List<float>   volumeUpSpeed         = new List<float>();
    
    [Header("------AttackPause------")] 
    public GameObject attackPauseEffect;               // 프리팹
    public Transform  attackPauseEffectMakeTrans;      // 생성위치
    public float      attackPauseTime;                 // 공격 퍼즈시간
    [HideInInspector]
    public float      attackPauseTimeCount;            // 타임카운트
    
    [HideInInspector]
    public bool       isAttackCoroutine;               // 공격 코루틴 실행 중
    
    [Header("------Straight Turret------")]
    public bool isStraightTurret;
    public bool isPattenTurret;

    [Header("------Rotating Turret------")]
    public  bool             isRotatingTurret;
    public  bool             isConvert;         // 조준 좌우반전
    public  GameObject       rotationGun;
    public  AlterLine        alterLine;
    public  LayerMask        wallLayer;
    
    private bool isAttackRange;    // 범위 체크
    private bool isAttackPossible; // 가로막는 벽이 없음.

    public float rotationSpeed;

    public  float laserMakeNum;          // 슛 횟수
    public  float attackInterval;
    private float attackIntervalCount;
    
    [HideInInspector]
    public Vector2     commonDirection;             // 회전 벡터(공중 공격 반동 방향에 사용)
    private float      originMainRotationZ;         // 앵글 계산에 사용
    
    private int       laserRotationChangeSymbol;   // 불릿 증감값 -10 0 10 순으로 들어갈 수 있도록, -1 0 1 순으로 곱해주도록 함.

    private void Start()
    {
        // 라이트 밝기 초기화
        foreach (var CustomMaterialObjects in CustomMaterialObject)
            CustomMaterialObjects.spriteRenderer.material.SetFloat("_StrongTintFade", 0f);
        
        // 회전형 포탑
        if (isRotatingTurret)
        {
            // 회전 초기화
            originMainRotationZ  = rotationGun.transform.localRotation.eulerAngles.z;  
            
            // 얼터라인 상태 초기화
            alterLine.isAlterLineOn = false;
        
            // Sound
            // 오리지널 볼륨 길이 저장 및 볼륨 초기화
            foreach (var turretLoopSoundLists in loopSoundList)
            {
                if (turretLoopSoundLists != null)
                {
                    originVolumeValueList.Add(turretLoopSoundLists.volume); // 오리지널값 넣기
                    turretLoopSoundLists.Stop();                            // 멈추기
                    turretLoopSoundLists.volume = 0f;                       // 볼륨값 없애기
                }
                else
                {
                    originVolumeValueList.Add(0f);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // 라이트 관리
        // 회전형 터렛
        if (isRotatingTurret)
        {
            // 라이트 관리
            // 라이트 UP
            if (!isDisabled && !isControlTerminalScanned && isAttackRange && isAttackPossible && CustomMaterialObject[0].spriteRenderer.material.GetFloat("_StrongTintFade") < 1f)
            {   
                foreach (var CustomMaterialObjects in CustomMaterialObject)
                {
                    CustomMaterialObjects.spriteRenderer.material.SetFloat("_StrongTintFade", Mathf.MoveTowards(CustomMaterialObjects.spriteRenderer.material.GetFloat("_StrongTintFade"),1f,
                    Time.fixedDeltaTime * lightChangeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue));
                }
            }
            // 라이트 Down
            else if (isDisabled || isControlTerminalScanned || (!isDisabled && !alterLine.isAlterLineOn && CustomMaterialObject[0].spriteRenderer.material.GetFloat("_StrongTintFade") > 0f))
            {
                foreach (var CustomMaterialObjects in CustomMaterialObject)
                {
                    CustomMaterialObjects.spriteRenderer.material.SetFloat("_StrongTintFade", Mathf.MoveTowards(CustomMaterialObjects.spriteRenderer.material.GetFloat("_StrongTintFade"),0f, 
                    Time.fixedDeltaTime * lightChangeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue));
                }
            }
            
            // 공격 가능 관리
            // 공격 범위 안이면서, 공격이 가능한지 체크
            if (isAttackRange && !isDisabled)
            {
                // 중간에 벽이 있는지 여부
                RaycastHit2D obstaclePlatform = Physics2D.Raycast(transform.position, PlayerController.instance.transform.position - transform.position, 
                    Vector2.Distance(transform.position,PlayerController.instance.transform.position), wallLayer);
                // false면 벽이 없음 -> 추격실행
                if (!obstaclePlatform)
                    isAttackPossible = true;
                else
                    isAttackPossible = false;
            }
            else
            {
                isAttackPossible = false;
            }
        }
        else if (isStraightTurret)
        {
            // 라이트 관리
            // 라이트 UP
            if (!isDisabled && !isControlTerminalScanned && CustomMaterialObject[0].spriteRenderer.material.GetFloat("_StrongTintFade") < 1f)
            {
                foreach (var CustomMaterialObjects in CustomMaterialObject)
                {
                    CustomMaterialObjects.spriteRenderer.material.SetFloat("_StrongTintFade", Mathf.MoveTowards(CustomMaterialObjects.spriteRenderer.material.GetFloat("_StrongTintFade"),1f,
                    Time.fixedDeltaTime * lightChangeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue));
                }
            }
            // 라이트 Down
            else if ((isDisabled || isControlTerminalScanned) && CustomMaterialObject[0].spriteRenderer.material.GetFloat("_StrongTintFade") > 0f)
            {
                foreach (var CustomMaterialObjects in CustomMaterialObject)
                {
                    CustomMaterialObjects.spriteRenderer.material.SetFloat("_StrongTintFade", Mathf.MoveTowards(CustomMaterialObjects.spriteRenderer.material.GetFloat("_StrongTintFade"),0f,
                    Time.fixedDeltaTime * lightChangeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue));
                }
            }
        }
        
        // 사운드 재생(+ 조준 사운드)
        // 회전형 포탑
        if(isRotatingTurret)
            LoopSound();
    }

    private void Update()
    {
        // 일반형 포탑
        if (isStraightTurret && !isDisabled && !isPattenTurret && !isControlTerminalScanned && !isAttackCoroutine)
        {
            shootCoolTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            
            // 공격 코루틴 실행
            if (shootCoolTimeCount > shootCoolTime)
            {
                isAttackCoroutine = true;
                StartCoroutine(StraightTurretAttack());
            }
        }
        // 회전형 포탑
        else if (isRotatingTurret && !isAttackCoroutine && !isControlTerminalScanned)
        {
            // 공격 범위 + 공격 가능
            if (!isDisabled && isAttackRange && isAttackPossible)
            {
                alterLine.isAlterLineOn = true;
                
                shootCoolTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;    // 시간 증가
            
                if (PlayerAcceleration.instance.isAcceleration)
                    commonDirection = new Vector2(rotationGun.transform.position.x - PlayerAcceleration.instance.inputAccelerationXtrans, rotationGun.transform.position.y - PlayerAcceleration.instance.inputAccelerationYtrans);
                else
                    commonDirection = new Vector2(rotationGun.transform.position.x - PlayerController.instance.transform.position.x, rotationGun.transform.position.y - PlayerController.instance.transform.position.y);
                
                // 조준 좌우반전
                if (isConvert)
                    commonDirection *= -1;
                float angle = Mathf.Atan2(commonDirection.normalized.y, commonDirection.normalized.x) * Mathf.Rad2Deg; // 회전하는 앵글값
                    
                
                Quaternion angleAxis            = Quaternion.AngleAxis(angle + (originMainRotationZ * transform.localScale.x), Vector3.forward);
                Quaternion rotation             = Quaternion.Slerp(rotationGun.transform.rotation, angleAxis, rotationSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
                rotationGun.transform.rotation = rotation;
                
                // 공격 코루틴 실행
                if (shootCoolTimeCount > shootCoolTime)
                {
                    isAttackCoroutine = true;
                    StartCoroutine(RotationTurretAttack());
                }
            }
            // 공격 범위 밖
            else
            {
                shootCoolTimeCount -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;    // 시간 감소
                if (shootCoolTimeCount < 0)
                {
                    alterLine.isAlterLineOn = false;
                    shootCoolTimeCount = 0;
                }
            }
        }
    }
    
    private void HandleLoopSound(int soundIndex,bool isAiming, bool isAttackCo)
    {
        if (isAiming && !isAttackCo && !isDisabled)
        {
            // 소리 높히기
            if (!loopSoundList[soundIndex].isPlaying)
                loopSoundList[soundIndex].Play();

            if (originVolumeValueList[soundIndex] > loopSoundList[soundIndex].volume)
                loopSoundList[soundIndex].volume += volumeUpSpeed[soundIndex] * Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
        }
        else
        {
            // 소리 줄이기
            loopSoundList[soundIndex].volume -= volumeUpSpeed[soundIndex] * Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (loopSoundList[soundIndex].volume == 0f && loopSoundList[soundIndex].isPlaying)
            {
                loopSoundList[soundIndex].time = 0f; // 초기화
                loopSoundList[soundIndex].Stop();
            }
        }
    }

    private void LoopSound()
    {
        HandleLoopSound(0,alterLine.isAlterLineOn,isAttackCoroutine);    // Aiming 사운드(조준 중 + 공격 코루틴 아니면, 사운드 재생)
    }
    
    // 일자형 터렛 공격 코루틴
    public IEnumerator StraightTurretAttack()
    {
        // 어택 퍼즈 이펙트 및 퍼즈 대기
        Instantiate(attackPauseEffect, attackPauseEffectMakeTrans.position, Quaternion.identity);
        attackPauseTimeCount    = attackPauseTime;
        
        while (true)
        {
            attackPauseTimeCount -= Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;

            if (attackPauseTimeCount <= 0)
            {
                Instantiate(laserPrefabs, shootTrans.transform.position,       shootTrans.rotation);       // 레이저
                Instantiate(shootEffect,  shootEffectTrans.transform.position, shootEffectTrans.rotation); // 슛 이펙트
                AudioManager.instance.ObjectSfxCreate(0, true,gameObject);                   // 슛 사운드
                
                shootCoolTimeCount = 0f; // 쿨타임 초기화
                
                isAttackCoroutine = false;
                
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    // 회전형 터렛 공격 코루틴
    private IEnumerator RotationTurretAttack()
    {
        // 어택 퍼즈 이펙트 및 퍼즈 대기
        Instantiate(attackPauseEffect, attackPauseEffectMakeTrans.position, Quaternion.identity);
        
        attackPauseTimeCount    = attackPauseTime;
        while (true)    // 퍼즈 대기
        {
            attackPauseTimeCount -= Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;

            if (attackPauseTimeCount <= 0)
                break;
            yield return new WaitForFixedUpdate();
        }
        
        // 레이저 발사
        int laserMakeCount  = 0;
        attackIntervalCount = 0;
        alterLine.isAlterLineOn = false;
        while (true)
        {   
            attackIntervalCount += Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (attackIntervalCount > attackInterval)
            {
                // 조준 좌우반전
                Quaternion rotation = shootTrans.rotation;
                if (isConvert)
                    rotation = Quaternion.Euler(rotation.eulerAngles.x, rotation.eulerAngles.y, rotation.eulerAngles.z + 180f);
                
                GameObject clone = Instantiate(laserPrefabs, shootTrans.transform.position, rotation); 
                clone.GetComponent<Projectle>().laserRotationChangeSymbol = laserRotationChangeSymbol;
                laserRotationChangeSymbol++;
                if (laserRotationChangeSymbol >= 1)
                    laserRotationChangeSymbol = -1;

                Instantiate(shootEffect,  shootEffectTrans.transform.position, shootEffectTrans.rotation); // 슛 이펙트
                AudioManager.instance.ObjectSfxCreate(0, true,gameObject);                   // 슛 사운드

                laserMakeCount     += 1;
                attackIntervalCount = 0;
            }
            
            if (laserMakeNum == laserMakeCount)
            {
                // 발사 후 대기
                attackPauseTimeCount    = attackPauseTime;
                while (true)    // 퍼즈 대기
                {
                    attackPauseTimeCount -= Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;

                    if (attackPauseTimeCount <= 0)
                        break;
                    yield return new WaitForFixedUpdate();
                }
                
                //EventController.instance.tutorialShootCount += 2;   // 튜토리얼 체크
                
                shootCoolTimeCount = 0f; // 쿨타임 초기화
                
                isAttackCoroutine = false;

                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnEnable()
    {
        shootCoolTimeCount = 0f;    // 쿨타임 초기화
        isAttackCoroutine  = false; // 코루린 실행 중, 비활성화 되면, 코루틴도 자동으로 종료됨.
    }
    
    // 공격범위 안
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !PlayerAcceleration.instance.isAcceleration)
        {
            isAttackRange = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !PlayerAcceleration.instance.isAcceleration)
        {
            isAttackRange = true;
        }
    }

    // 공격범위 밖
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isAttackRange = false;
        }
    }
}
