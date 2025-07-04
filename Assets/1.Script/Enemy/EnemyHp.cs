using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnemyHp : MonoBehaviour
{
    [Header("------Common------")]
    public EnemyController                 enemyCon;     
    public EnemyAttack                     enemyAttack;  
    public Canvas                          enemyCanvas;  
    public EnemyAnimatorFunction           enemyFunction;
    public SkeletonRendererCustomMaterials skeletonCustom;           // 라이트세이버 독립 할당
    public InAccelerationOrderLayer        inAccelerationOrderLayer; // 레이어 변경
    
    public BoxCollider2D                   bodyPush;                 // 바디푸쉬 콜라이더  
    
    public Transform                       bodyHackingTransform;     // 해킹모드에서 위치를 참조하여, 키 프리팹을 생성. 
    public Transform                       bodyBoomTrans;            // 폭파이펙트 위치

    [HideInInspector] 
    public int hitAnimNum = 1;

    [Header("------HP------")]
    public  int	   currentHealth;           
    public  int	   maxHealth;               
    [HideInInspector]
    public  bool   liveState       = true;   
    public  Slider hpSlider;
    public  float  hpSliderSeeTime;
    private float  hpSliderSeeTimeCount;

    [Header("------knockBack------")]
    public  int knockBackSpeed;
    
    [HideInInspector]                // HP의 메터리얼 색 변화에서 사용
    public float hitAnimLength;      // 히트 애니 총 길이값
    public float hitAimSpeed;        // 1기본(배속값)
    [HideInInspector]
    public float hitTimeCount;      // 타임 카운트

    [HideInInspector] 
    public int knockBackDirection;

    [Header("------Stun------")]
    public  GameObject stunElectronic;
    private float      stunTimeCount;
    [HideInInspector]
    public  bool       isStun;
    
    
    private void Start()
    {
        currentHealth	   = maxHealth;
        hpSlider.maxValue  = maxHealth;							   
        hpSlider.value	   = currentHealth;
        
        // 애니메이션재생길이정보(실제파일이름)
        AnimationClip[] clips = enemyCon.bodyAnim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            // 공통
            if (clip.name.Equals("Hit_1") || clip.name.Equals("Hit"))
                hitAnimLength = clip.length * (1/hitAimSpeed);       // 원래길이  * 가속길이(5)
        }
    }

    private void Update()
    {
        // UI 슬라이더 관리
        hpSliderSeeTimeCount -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
        if (hpSliderSeeTimeCount > 0f && hpSlider.gameObject.activeInHierarchy ==false)
        {
            hpSlider.gameObject.SetActive(true);    
        }
        else if(hpSliderSeeTimeCount < 0f && hpSlider.gameObject.activeInHierarchy)
        {
            hpSlider.gameObject.SetActive(false);    
        }
        
        // 스턴관리
        stunTimeCount -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
        if (liveState)
        {
            if (stunTimeCount > 0f)
            {
                isStun = true;
                enemyCon.bodyAnim.SetBool("isStun", isStun);
                
                // 스턴 상태가 되면, 1회 실행
                if (isStun && !stunElectronic.activeInHierarchy)
                    stunElectronic.SetActive(true);           // 스턴 이펙트 켜기
            }
            else
            {
                isStun = false;
                enemyCon.bodyAnim.SetBool("isStun", isStun);
                
                // 스턴 상태가 끝나면, 1회 실행
                if (!isStun && stunElectronic.activeInHierarchy)
                {
                    stunElectronic.SetActive(false);                       // 스턴 이펙트 끄기                 
                    enemyCon.bodyAnim.SetTrigger("hit" + hitAnimNum); // 히트 애니메이션 재생
                    
                    // 팔 회전 있는 경우(회전 복구)
                    enemyFunction.DisableIsRotation();
                    enemyFunction.EnableRotationRecovery();
                    
                    // 쉐이더 컬러 복귀    
                    skeletonCustom.changeIndependentMat2.SetFloat(PlayerHacking.instance.strongTintFadeID, 0); // 라이트 스트롱 틴트 복귀
                    skeletonCustom.changeIndependentMat3.SetFloat(PlayerHacking.instance.strongTintFadeID, 0); // 바디   스트롱 틴트 복귀
                }
            }
        }
    }

    private void FixedUpdate()
    {
        HitNockBack();
    }

    public void DamageEnemy(int damage, Transform hitPosition,float stunTime)
    {
        if (liveState)
        {
            // 체력
            currentHealth       -= damage;						       // 체력감소
            hpSlider.value       = currentHealth;                      // ui 갱신
            hpSliderSeeTimeCount = hpSliderSeeTime;                    // UI 보이기 시간 증가

            // 추적모드 활성화
            enemyCon.chaseTimeCount = enemyCon.chaseTime;                              // 쫒기시간 초기화
            enemyCon.isChasePlayer  = true;                                            // 쫒기
            enemyCon.bodyAnim.SetBool("isChasePlayer",enemyCon.isChasePlayer);    // 상태변경
            
            // 주변 적에게 알림
            enemyCon.Alter();
            
            // 히트넉백 시간갱신
            hitTimeCount       = 0f;                                                                             // 초기화
            knockBackDirection = PlayerController.instance.transform.position.x > transform.position.x ? -1 : 1; // 너백방향 조정
            
            // 스턴 상태 갱신
            // (스턴 타임이 0이 아닌 값이 들어오고, stunTimeCount이 0 이하)
            if (stunTime != 0f && stunTimeCount <= 0f)
            {
                isStun        = true;
                enemyCon.bodyAnim.SetBool("isStun", isStun);
                stunTimeCount = stunTime;              // 스턴 타임 증가
                
                // 스턴 상태 들어 왔는데, 피가 0이하
                if (currentHealth <= 0)
                {
                    // 지상 + 공중 모두 바로 폭파
                    enemyFunction.SetBodyBoom();
                    return;
                }
            }
            // 바로 사망
            // (이미 스턴 상태였으면)
            else if(stunTimeCount > 0f)
            {
                // 지상 + 공중 모두 바로 폭파
                enemyFunction.SetBodyBoom();
                return;
            }
            
            // 사망 O
            // (일반 상태 사망 - 체력 남음 X + 스턴 X)
            if (currentHealth <= 0 && !isStun)
            {
                // 히트방향 좌우반전 
                enemyCon.bodyObject.transform.localScale = new Vector3(hitPosition.transform.position.x > enemyCon.bodyObject.transform.position.x ? 1 : -1, 1, 1);
                
                // 지상 -> 모션 후 폭파
                if (!enemyCon.isFly)
                {
                    // 사운드 생성
                    AudioManager.instance.EnemySfxCreate(2, true, enemyCon.gameObject);
                    enemyCon.bodyAnim.SetTrigger("death");
                }
                // 공중 -> 바로폭파
                else
                    enemyFunction.SetBodyBoom();
                
                // 팔 회전 있는 경우(회전 복구)
                enemyFunction.DisableIsRotation();
                enemyFunction.EnableRotationRecovery();
                
                // 근접 무기가 있는 경우(콜리더 끄기)
                if(enemyAttack.meleeWeapon)
                    enemyFunction.DisableBlade();            // 콜라이더 끄기                
                
                // 사망시, 이펙트가 있는 경우 이펙트 없애기(있는 경우)
                if(enemyAttack.currentSlashEffect)
                    Destroy(enemyAttack.currentSlashEffect);
                
                liveState              = false;
                bodyPush.enabled       = false;
                enemyCon.rb2D.velocity = Vector2.zero;
                enemyCanvas.gameObject.SetActive(false);     // 캔버스 off
            }
            // 스턴 사망 X
            // (스턴 상태 피가 남아 있음)
            else if (currentHealth > 0 && isStun)
            {
                if(enemyAttack.currentSlashEffect)           // 슬레쉬 이펙트 끄기
                    Destroy(enemyAttack.currentSlashEffect);
                                                             
                if(enemyAttack.meleeWeapon)
                    enemyFunction.DisableBlade();            // 콜라이더 끄기
            }
            // 사망 X
            // (일반 상태 히트 - 체력 남음 O + 모션락 X + 스턴 X)
            // + (어택 쿨타임이 남았거나 or 도망 중 이거나 or Run이거나 or 등뒤에서 치거나))
            else if (currentHealth > 0 && !enemyAttack.attackMotionLock && !isStun &&
                    ((enemyAttack.attackCoolTimeCount > 0f) || enemyCon.isRunAway || enemyCon.enemyAnimStateInfo.IsName("Run") || enemyCon.enemyAnimStateInfo.IsName("RunToKeep") || enemyCon.enemyAnimStateInfo.IsName("KeepToRun") || 
                     (enemyCon.bodyObject.transform.localScale.x == -1 && PlayerController.instance.transform.position.x > enemyCon.transform.position.x)                         || 
                     (enemyCon.bodyObject.transform.localScale.x ==  1 && PlayerController.instance.transform.position.x < enemyCon.transform.position.x)))
            {
                enemyCon.bodyObject.transform.localScale = new Vector3(hitPosition.transform.position.x > enemyCon.bodyObject.transform.position.x ? 1 : -1, 1, 1);
                enemyCon.bodyAnim.SetTrigger("hit" + hitAnimNum);
                // 팔 회전 있는 경우(스나이퍼)
                enemyFunction.DisableIsRotation();
                enemyFunction.EnableRotationRecovery();
            }
        }
    }
    
    private void HitNockBack()
    {
        if (!isStun)
        {
            // 공격 중, 넉백 빠르게 종료   
            if (enemyCon.enemyAnimStateInfo.IsName("Attack1") || enemyCon.enemyAnimStateInfo.IsName("Attack2") || enemyCon.enemyAnimStateInfo.IsName("Shoot"))
                hitTimeCount += Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue * 2f;
            // 기본
            else
                hitTimeCount += Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            
            // 공격 이동 + 반동 때,
            // 실행 방지 + 잔여 시간값 초기화
            if ((!enemyCon.isFly && (enemyAttack.meleeWeapon.enemyBladeCollider.enabled || enemyAttack.isFirearmRecoil)) || 
                  (enemyCon.isFly && enemyAttack.isFirearmRecoil))
            {
                hitTimeCount = hitAnimLength;
                return;
            }
            
            // 반동 뒤로 이동, 뒤가 절벽이면 이동하지 않음.
            if (enemyCon.isBackCliff)
            {
                enemyCon.rb2D.velocity = new Vector2(0f,enemyCon.rb2D.velocity.y);
                return;
            }
            
            // 0~1 노멀라이즈 이고, 어택 무브가 아니고, 반동이 아니고, 뒷통수에 절벽이 없을 때,
            if (hitTimeCount / hitAnimLength < 1f)
            {
                float animProgress = hitTimeCount / hitAnimLength;  // 0~1로 증가
                float deceleration = 1 - animProgress;              // 감속

                // 경사히트 넉백( 0 ~ 0.7 )
                if (!enemyCon.isFly && enemyCon.isSlope && animProgress >= 0f && animProgress <= 0.7f)
                {
                    float xVelocity        = knockBackDirection * knockBackSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                    enemyCon.rb2D.velocity = enemyCon.perpendicular * xVelocity * -1 * deceleration;
                }
                // 평지히트 넉백( 0 ~ 0.7 )
                else if (!enemyCon.isFly && !enemyCon.isSlope && animProgress >= 0f && animProgress <= 0.7f)
                {
                    enemyCon.rb2D.velocity = new Vector2(knockBackDirection * knockBackSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue * deceleration, 
                        enemyCon.rb2D.velocity.y * PlayerAcceleration.instance.accelerationChangedTimeValue);
                }
                else if (enemyCon.isFly && animProgress >= 0f && animProgress <= 0.7f)
                {
                    enemyCon.rb2D.velocity = new Vector2(knockBackDirection * knockBackSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue * deceleration, 
                        enemyCon.rb2D.velocity.y * PlayerAcceleration.instance.accelerationChangedTimeValue);
                }
                // 마지막 부분 이동 X ( 0.7 ~ 1 )
                else if (animProgress > 0.7f && animProgress <= 1f)
                {
                    enemyCon.rb2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                }
            }
        }
        
    }

    private void OnDestroy()
    {
        stunElectronic.SetActive(false);
    }
}