using System.Collections.Generic;
using UnityEngine;

public class PlayerHp : MonoBehaviour
{
    public static PlayerHp instance;

    [HideInInspector]
    public bool liveState = true;
    [HideInInspector] 
    public bool isHit;
    
    [Header("------HP Setting------")]
    public int   currentHealth;
    public int   maxHealth;
    
    public  float afterHitInvincible;      // 피격 후 무적 시간
    [HideInInspector]
    public float afterHitInvincibleCount;
   
    [Header("------HP Recovery------")] 
    public  List<GameObject>     recoverEffectList  = new List<GameObject>();
    private List<ParticleSystem> particleSystemList = new List<ParticleSystem>();
    
    [HideInInspector]
    public bool  isRecovery;
    public float gageRecoverySpeed;   // hp와 게이지 등가교환 스피드
    
    [Header("------KnockBack------")]
    public int knockBackSpeed;

    [HideInInspector] 
    public bool invincibleMode; // 무적모드

    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        // HP Setting
        UIController.instance.healthSlider.maxValue = maxHealth;                         // 게임 시작시 healthSlider.maxValue값을 maxHealth 값으로 설정

        if (EventController.instance.isStasisChamber || EventController.instance.isPerformanceLab) // 정체실 OR 성능실험실
            currentHealth = maxHealth;
        else                                                                                       // 나머지(저장된 플레이어프리팹 정보로 불러오기)
            currentHealth = PlayerPrefs.GetInt("currentHP"); // 저장된 체력  
        UIController.instance.healthSlider.value = currentHealth;
        UIController.instance.healthText.text    = UIController.instance.healthSlider.value.ToString();

        // HP Recovery
        for (int i = 0; i < recoverEffectList.Count; i++)
        {
            particleSystemList.Add(recoverEffectList[i].GetComponent<ParticleSystem>());    // 넣기
            particleSystemList[i].Stop();                                                       // 바로 멈추기
        }
    }

    private void Update()
    {
        //Recovery(); // 회복(R)
    }

    private void FixedUpdate()
    {
        KnockBack();       // 히트 넉백
        
        afterHitInvincibleCount -= Time.fixedDeltaTime; // 히트 후 무적시간 카운드
    }
    
    public void DamagePlayer(Transform hitEnemyPosition, int damageInt)
    {
        if (!isHit && liveState && afterHitInvincibleCount <= 0f)
        {
            if (invincibleMode)
                damageInt = 0;
        
            // 상태값 변경
            // 즉시 히트 상태(해킹Z키 누를 때, 인풋키 = update / 애니메이션상태 = 60프레임 으로 인해,
            isHit                                        = true;  // 히트상태
            SettingManager.instance.glitch6.enable.value = true;  // 히트 클리치 켜기
            
            PlayerAttack.instance.isAttackState = false;
            PlayerDash.instance.isDash          = false;
            PlayerAttack.instance.enterAhead    = false;
            
            AudioManager.instance.PlayerSfxCreate(4,true);    // 사운드 생성
            
            // 체력 감소
            currentHealth -= damageInt;
            UIController.instance.healthSlider.value = currentHealth;
            UIController.instance.healthText.text    = UIController.instance.healthSlider.value.ToString();                                       

            // 잔여 값 제거
            PlayerController.instance.rb2D.velocity = Vector2.zero;                
            
            // 카메라 흔들기 코루틴 실행
            if(!CameraShaker.instance.isShack)                                             // x0y1 -> x1y0
                CameraShaker.instance.Shake(0.2f, 0.4f, AnimationCurve.Linear(0f, 1f, 1f, 0f)); 
            
            // 가속 중 이라면, 초기화 시켜버리기(풀림)
            PlayerAcceleration.instance.AccelerationEnd();
        
            // 히트방향 좌우반전 + 넉백
            if (hitEnemyPosition.transform.position.x - gameObject.transform.position.x >= 0)
                PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(1, 1, 1);      // 좌우반전
            else
                PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(-1, 1, 1);     // 좌우반전
            
            // 사망 O(현재체력 0이하)
            if (currentHealth <= 0)
            {
                SettingManager.instance.glitch8.enable.value = true;  // 히트 글리치 + 나레이션 글리치
            
                // 상태변경 및 트리거
                liveState = false;
                PlayerController.instance.playerAnim.SetTrigger("deathOn");
                
                // 바디푸쉬 off
                PlayerBodyPush.instance.box2D.enabled = false;

                // UI 관리
                UIController.instance.accelerationTimeRemainingSlider.gameObject.SetActive(false);                                                           
                UIController.instance.UISeeState(false);

                AudioManager.instance.PlayerSfxCreate(6,true);    // 사운드 생성
            }
            // 사망 X
            else
            {
                // 트리거
                PlayerController.instance.playerAnim.SetTrigger("takeHitOn");
                // UI 관리
                UIController.instance.accelerationTimeRemainingSlider.gameObject.SetActive(false);                                                           
                UIController.instance.UISeeState(true);
            }
        }
    }

    private void KnockBack()
    {
        if (PlayerController.instance.playerAnimStateInfo.IsName("Hit"))
        {
            float animProgress = PlayerController.instance.playerAnimStateInfo.normalizedTime;
            float deceleration = 1 - animProgress;
            
            // 경사히트 넉백( 0 ~ 0.7 )
            if (PlayerFloorCollider.instance.isGrounded && PlayerController.instance.isSlope && animProgress >= 0f && animProgress <= 0.7f)
            {
                float xVelocity     = -PlayerController.instance.bodyGameObject.transform.localScale.x * knockBackSpeed;
                PlayerController.instance.rb2D.velocity = PlayerController.instance.perpendicular * (xVelocity * -1 * deceleration);
            }
            // 평지히트 넉백( 0 ~ 0.7 )
            else if (PlayerFloorCollider.instance.isGrounded && !PlayerController.instance.isSlope && animProgress >= 0f && animProgress <= 0.7f)
            {
                PlayerController.instance.rb2D.velocity = new Vector2(-PlayerController.instance.bodyGameObject.transform.localScale.x * knockBackSpeed * deceleration, 
                                                                       PlayerController.instance.rb2D.velocity.y);
            }
            // 공중( 0 ~ 0.7 )
            else if (!PlayerFloorCollider.instance.isGrounded && !PlayerController.instance.isSlope && animProgress >= 0f && animProgress <= 0.7f)
            {
                PlayerController.instance.rb2D.velocity = new Vector2(-PlayerController.instance.bodyGameObject.transform.localScale.x * knockBackSpeed * deceleration, 
                                                                       PlayerController.instance.rb2D.velocity.y);
            }
            // 마지막 부분 이동 X ( 0.7 ~ 1 )
            else if (animProgress > 0.7f && animProgress <= 1f)
            {
                PlayerController.instance.rb2D.velocity = new Vector2(0f, PlayerController.instance.rb2D.velocity.y);
            }
        }
    }
    
    private void Recovery()
    {
        // 회복모드 시작(홀드)
        if (Input.GetKey(KeyCode.R)     && liveState   && !isHit       && !PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking      && PlayerFloorCollider.instance.isGrounded &&
            !PlayerDash.instance.isDash && !PlayerScan.instance.isScan && !EventController.instance.eventState        && !EventController.instance.recoveryLock && !MenuManager.instance.isNormalMenu)
        {
            isRecovery = true;                                                            // 바로 상태전환
            PlayerController.instance.playerAnim.SetBool("recoveryHold",true);  // 애니메이션 전환
            PlayerController.instance.playerAnim.SetBool("run",false);
            PlayerController.instance.rb2D.velocity = new Vector2(0f, PlayerController.instance.rb2D.velocity.y);
        }
        // 회복 종료(홀드 종료)
        else
        {
            PlayerController.instance.playerAnim.SetBool("recoveryHold",false);  
            
            if (PlayerController.instance.playerAnimStateInfo.IsName("Recovery") || PlayerController.instance.playerAnimStateInfo.IsName("RecoveryToIdle") ||
                PlayerController.instance.playerAnimStateInfo.IsName("IdleToRecovery"))
            {
                isRecovery = true;
                PlayerController.instance.playerAnim.SetBool("run",false);
                PlayerController.instance.rb2D.velocity = new Vector2(0f, PlayerController.instance.rb2D.velocity.y);
            }
            else
                isRecovery = false;
        }
        
        // 강제 종료(강제종료 -> 히트)
        if (isRecovery && (isHit || PlayerDash.instance.isDash))
        {
            isRecovery = false;                                                            // 바로 상태전환
            PlayerController.instance.playerAnim.SetBool("recoveryHold",false);  // 애니메이션 전환
        }

        // 회복모드 중 체력회복
        if (PlayerController.instance.playerAnimStateInfo.IsName("Recovery"))
        {
            // 회복 O (게이지가 있고, 회복할 체력이 있을 때만 등가교환 실시)
            if (UIController.instance.gageSlider.value != UIController.instance.gageSlider.maxValue)
            {
                // 회복이펙트가 켜져있지 않을 때 켜기(1회)
                if (!particleSystemList[0].isPlaying)
                {
                    for (int i = 0; i < particleSystemList.Count; i++)
                    {
                        particleSystemList[i].Play();
                    }
                }
                    
                UIController.instance.gageSlider.value += Time.deltaTime * gageRecoverySpeed;
            }
            // 회복 X (게이지가 없거나, 회복할 체력이 꽉차 있거나)
            else
            {
                // 회복이펙트가 켜져있을 때 끄기(1회)
                if (particleSystemList[0].isPlaying)
                {
                    for (int i = 0; i < particleSystemList.Count; i++)
                    {
                        particleSystemList[i].Stop();
                    }
                }
            }
        }
        else if (!PlayerController.instance.playerAnimStateInfo.IsName("Recovery"))
        {
            if (particleSystemList[0].isPlaying)
            {
                for (int i = 0; i < particleSystemList.Count; i++)
                {
                    particleSystemList[i].Stop();
                }
            }
        }
    }
}