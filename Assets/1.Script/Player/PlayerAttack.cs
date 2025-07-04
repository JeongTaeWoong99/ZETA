using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack instance;
    
    [Header("------Common------")]
    public GameObject hitEffect;
    public LayerMask  hitLayer;

    [HideInInspector] 
    public bool isAttackState;
    
    [HideInInspector]
    public int   attackComboNum;
    
    [HideInInspector] 
    public  bool isAttackMove;           // PlayerAnimatorFunction에서 제어
        
    public float comboHoldTime;
    [HideInInspector]
    public float comboHoldTimeCount;

    [HideInInspector] 
    public  bool isEffectHitJump;              // 관성에서 사용. 공중 유효타가 터지면, 이동을 멈춤.
    
    [HideInInspector]
    public bool thisTimeAttackMoveState;       // 현재 누른 상태에서 앞으로 밀기를 할 것인지(누를 때 마다 초기화 됨.)

    [HideInInspector] 
    public bool enterAhead;                    // 선행입력

    [Header("------SlashEffect1_2------")]
    public Transform  effectFollowTrans;       // 이펙트가 따라다닐 위치(자연스러운 이동을 위한 위치)
    [HideInInspector]
    public GameObject currentSlashEffect;      // 현재 만들어진 슬레쉬 이펙트
   
    public GameObject attackSlashEffect1_2;    // 슬레쉬 이펙트 파티클 + 슬레쉬 도트 이펙트 파티클 
    public int        attackDamage1_2;
    public float      gageUpValue1_2;
    public float      attackSpeed1_2;
    
    [Header("------SlashEffect1_3------")]
    public GameObject attackSlashEffect3;                 // 슬레쉬 이펙트 파티클
    public GameObject attackSlashEffect3_Dot;             // 슬레쉬 도트 파티클(좌우반전시 따라가지 않도록_실행 시간이 길어서, 따루 분리)
    public int        attackDamage3;
    public float      gageUpValue3;
    private float     activeAttack3MoveSpeed;
    
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        Attack();
    }
    
    private void FixedUpdate()
    {
        AttackMove();
        
        // 콤보 타임 체크
        // 공격 모션 중이 아니면, 콤보홀드타임 감소
        if (!isAttackState)
            comboHoldTimeCount -= Time.deltaTime;

        // 이펙트 이동
        // 현재 만들어진 슬레쉬 이펙트가 있다면(그룹 레이어 문제로 따로 빼서 이동시킴)
        if (currentSlashEffect)
        {
            int playerBodyX = PlayerController.instance.bodyGameObject.transform.localScale.x == 1 ? 1 : -1;
            // 공격 1~2 따라가기 위치
            if (currentSlashEffect.name == attackSlashEffect1_2.name + "(Clone)" && isAttackState && !PlayerController.instance.playerAnimStateInfo.IsName("Attack3"))
                currentSlashEffect.transform.position = effectFollowTrans.position + new Vector3(playerBodyX * 0.5f,0f,0f);
            // 공격 3 따라가기 위치
            else if (currentSlashEffect.name == attackSlashEffect3.name + "(Clone)" && isAttackState && PlayerController.instance.playerAnimStateInfo.IsName("Attack3"))
                currentSlashEffect.transform.position = effectFollowTrans.position + new Vector3(playerBodyX * 0.5f,0f,0f);
        }
    }
    
    private void Attack()
    {
        // 콤보 입력(애니메이션 구간은 같게 해도 똑같음. 배속을 다르게 하기 때문에, 전체 재생 프레임은 20으로 동일함.)
        // 10프레임부터 다음 공격 입력 가능
        var progress       = PlayerController.instance.playerAnimStateInfo.normalizedTime;
        var isAttackInProgress = (PlayerController.instance.playerAnimStateInfo.IsName("Attack1")    || PlayerController.instance.playerAnimStateInfo.IsName("Attack2") ||
                                  PlayerController.instance.playerAnimStateInfo.IsName("AirAttack1") || PlayerController.instance.playerAnimStateInfo.IsName("AirAttack2")) && progress < 0.5f;
        
        // 공중 공격 후, 애니메이션 끝나기 전에 지상 착지하면, 50% 이상이면, 넘어갈 수 있도록 함.(AirAttack1~2의 모든 펑션이 실행되는 것이 50%이상 10프레임 이후이기 때문)
        if ((PlayerController.instance.playerAnimStateInfo.IsName("AirAttack1") || PlayerController.instance.playerAnimStateInfo.IsName("AirAttack2")) && progress > 0.5)
            PlayerController.instance.playerAnim.SetBool("isAirAttackEnd", true);
        else
            PlayerController.instance.playerAnim.SetBool("isAirAttackEnd", false);
        
        // 선입력
        if (Input.GetKeyDown(KeyCode.A) && !enterAhead && isAttackState && !PlayerController.instance.playerAnimStateInfo.IsName("Attack3"))
            enterAhead = true;

        if ((Input.GetKeyDown(KeyCode.A) || enterAhead) && !Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.Space)     && 
            PlayerHp.instance.liveState && !PlayerController.instance.isHangWall        && !MenuManager.instance.isNormalMenu   &&
            !isAttackInProgress         && !PlayerController.instance.playerAnimStateInfo.IsName("Attack3") &&
            !PlayerHp.instance.isHit    && !PlayerHacking.instance.isHacking            && !PlayerScan.instance.isScan          && !PlayerHp.instance.isRecovery  &&
            !EventController.instance.eventState                                        && !EventController.instance.attackLock && !PlayerController.instance.isCornerClimb)    
        {
            // 앞으로 이동 여부 초기화
            thisTimeAttackMoveState = true;

            // 콤보체크
            if (comboHoldTimeCount > 0f)
                attackComboNum++;
            else
                attackComboNum = 1;
            
            // 콤보 타임 초기화
            comboHoldTimeCount = comboHoldTime;

            // 공중 및 지상은 1~2만 작동하도록
            if (attackComboNum >= 3)
                attackComboNum = 1;
            
            // 공중에서는 점프하고, 앞으로 밀려나가는 방향으로만 공격 하도록 + 공격3은 방향전환 안됨.
            if (PlayerFloorCollider.instance.isGrounded && !PlayerDash.instance.isDash)
            {
                // 좌우반전(누르고 있는 키에 따라서 대쉬하거나, 바라보는 방향)
                if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
                    PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(1, 1, 1); // 좌우반전
                else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
                    PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(-1, 1, 1); // 좌우반전
            }
            
            // 대쉬 연계 공격(대쉬 중 공격하면, 3이 나감)
            // 대쉬 공격, 트리거 실행(대쉬 중, 0<=0.8 구간에서 눌렀을 때)
            if (PlayerDash.instance.isDash && PlayerController.instance.playerAnimStateInfo.normalizedTime <= 0.8)
            {
                attackComboNum = 3;
                activeAttack3MoveSpeed = PlayerDash.instance.decelerationValue * PlayerDash.instance.dashSpeed;   // 시작 최대 스피드(대쉬를 눌렀을 때, 감속과 대쉬의 스피드를 이용해서 결정)
                PlayerController.instance.playerAnim.SetTrigger("attack" + attackComboNum +"On");
            }
            // 일반 공격, 트리거 실행(대쉬 중, 0>0.8 구간에서 눌렀을 때, 대쉬 중이 아닐 때)
            else if(!PlayerDash.instance.isDash || (PlayerDash.instance.isDash && PlayerController.instance.playerAnimStateInfo.normalizedTime > 0.8))
            {
                // 지상
                if (PlayerFloorCollider.instance.isGrounded)
                    PlayerController.instance.playerAnim.SetTrigger("attack" + attackComboNum + "On");
                // 공중
                else
                    PlayerController.instance.playerAnim.SetTrigger("airAttack" + attackComboNum + "On");
            }
            
            // 상태값 변경
            isAttackState              = true;
            
            PlayerDash.instance.isDash = false;
            PlayerHp.instance.isHit    = false;
            enterAhead                 = false;

            AudioManager.instance.PlayerSfxCreate(attackComboNum is 1 or 2 ? 0 : 2, true);

            // 튜토리얼 인풋 체크
            if (attackComboNum == 3)
                EventController.instance.tutorialAttack3CheckCount += 1;
        }
    }

    private void AttackMove()
    {
        // 땅 + 공격 애니메이션
        if (isAttackState)
        {
            // 이동 O
            if (isAttackMove)
            {
                // 이동제한(공격을 누른 타이밍에)
                // 오른쪽 공격 이동 + 왼쪽 밀어냄 -> 공격이동 x
                // 왼쪽공격 이동 + 오른쪽 밀어냄  -> 공격이동 x
                if ((PlayerController.instance.bodyGameObject.transform.localScale.x == 1  && PlayerBodyPush.instance.isBodyLeftPush) ||
                    (PlayerController.instance.bodyGameObject.transform.localScale.x == -1 && PlayerBodyPush.instance.isBodyRightPush))
                {
                    thisTimeAttackMoveState = false;
                    return;
                }
                
                // A 버튼 누르면 갱신
                if(!thisTimeAttackMoveState)
                    return;
                
                if(PlayerController.instance.playerAnimStateInfo.IsName("Attack1") || PlayerController.instance.playerAnimStateInfo.IsName("Attack2"))
                {
                    // 경사
                    if (PlayerController.instance.isSlope)
                        PlayerController.instance.rb2D.velocity = PlayerController.instance.perpendicular * attackSpeed1_2 * PlayerController.instance.bodyGameObject.transform.localScale.x * -1;
                    // 평지(y값은 벨로시티y적용)
                    else
                        PlayerController.instance.rb2D.velocity = new Vector2(PlayerController.instance.bodyGameObject.transform.localScale.x * attackSpeed1_2, PlayerController.instance.rb2D.velocity.y); // y값은 중력에 따라서 이동 유지
                }
                else if (PlayerController.instance.playerAnimStateInfo.IsName("Attack3"))
                {
                    float progress    = PlayerController.instance.playerAnimStateInfo.normalizedTime * 2f; // ★ 0 ~ 0.5 구간 이동하기 때문에, 이 구간을 0~1 감속 구간이라고 생각한다. 18 / 36 프레임 이동.
                    float speedChange = 0f;
                    if(progress < 1)
                        speedChange = (1 - progress); // 점점 작아짐.
                    
                    // 경사
                    if (PlayerController.instance.isSlope)
                        PlayerController.instance.rb2D.velocity = PlayerController.instance.perpendicular * speedChange * activeAttack3MoveSpeed * PlayerController.instance.bodyGameObject.transform.localScale.x * -1;
                    // 평지(+ 공격하다가 공중으로 간 경우 -> y값은 -1값 적용 = 대쉬와 동일)
                    else
                        PlayerController.instance.rb2D.velocity = new Vector2(PlayerController.instance.bodyGameObject.transform.localScale.x * speedChange * activeAttack3MoveSpeed, -1);
                }
            }
        }
    }
    
    // PlayerAnimatorFunction 발동
    public void Hit()
    {
        // 히트 체크
        Collider2D[] hit = Physics2D.OverlapBoxAll(transform.position, new Vector2(2.4f, 1.5f), 0, hitLayer);
        
        // 유효타 있는지 체크(초기화)
        bool effectiveHitJump  = false;  // 적 O // 오브젝트 X
        bool effectiveHitSound = false;  // 적 O // 오브젝트 O
        
        for (var i = 0; i < hit.Length; ++i)
        {
            // 액티브 데미지
            bool isAttack1 = PlayerController.instance.playerAnimStateInfo.IsName("Attack1") || PlayerController.instance.playerAnimStateInfo.IsName("AirAttack1");
            bool isAttack2 = PlayerController.instance.playerAnimStateInfo.IsName("Attack2") || PlayerController.instance.playerAnimStateInfo.IsName("AirAttack2");
            bool isAttack3 = PlayerController.instance.playerAnimStateInfo.IsName("Attack3");
            int  changeDamage = 0;

            // 데미지
            if (isAttack3)
                changeDamage = attackDamage3;
            else
                changeDamage = attackDamage1_2;
            
            // 적
            if (hit[i].CompareTag("Enemy") && hit[i].GetComponent<EnemyHp>().liveState && hit[i].GetComponent<EnemyLightController>().isAppear)
            {
                // 히트 애니메이션 실행 번호 설정.
                if (isAttack1)
                    hit[i].GetComponent<EnemyHp>().hitAnimNum = 1;  // 히트 애니메이션 넘버 변경
                else if (isAttack2)
                    hit[i].GetComponent<EnemyHp>().hitAnimNum = 2;  // 히트 애니메이션 넘버 변경
                else if (isAttack3)
                    hit[i].GetComponent<EnemyHp>().hitAnimNum = 3;  // 히트 애니메이션 넘버 변경

                Instantiate(hitEffect, hit[i].GetComponent<EnemyHp>().bodyBoomTrans.transform.position, Quaternion.Euler(0f, 90f, 0f));
                
                hit[i].GetComponent<EnemyHp>().DamageEnemy(changeDamage, PlayerController.instance.transform,0f);
                
                effectiveHitJump  = true;
                effectiveHitSound = true;
                
                AttackGagePlus(changeDamage);
            }
            // 오브젝트
            if (hit[i].CompareTag("Object") && hit[i].GetComponent<BrokenObject>())
            {
                // 마인
                if (hit[i].GetComponent<BrokenObject>().mine && !hit[i].GetComponent<BrokenObject>().mine.isTriggerOn && hit[i].GetComponent<BrokenObject>().mine.isAppear)
                {
                    Instantiate(hitEffect, hit[i].transform.position, Quaternion.Euler(0f, 90f, 0f));
                    hit[i].GetComponent<BrokenObject>().SetMineBoom();
                    effectiveHitSound = true;
                }
                // 노멀 박스류
                else if(!hit[i].GetComponent<BrokenObject>().mine)
                {
                    Instantiate(hitEffect, hit[i].transform.position, Quaternion.Euler(0f, 90f, 0f));
                    hit[i].GetComponent<BrokenObject>().SetNormalBoom();
                    effectiveHitSound = true;
                }
            }
            
            // 오브젝트(트레이닝봇)
            if (hit[i].CompareTag("Object") && hit[i].GetComponent<TrainingBot>())
            {
                Instantiate(hitEffect, hit[i].transform.position, Quaternion.Euler(0f, 90f, 0f));
                
                hit[i].GetComponent<TrainingBot>().DamageBot(changeDamage);
                
                effectiveHitJump  = true;
                effectiveHitSound = true;
                
                AttackGagePlus(changeDamage);
            }
            
            // 보스 
            if (hit[i].CompareTag("Boss") && hit[i].GetComponent<BossController>().isAppear && hit[i].GetComponent<BossHP>().isLive)
            {
                Instantiate(hitEffect, hit[i].GetComponent<BossController>().bodyObject.transform.position, Quaternion.Euler(0f, 90f, 0f));
                
                hit[i].GetComponent<BossHP>().DamageBoss(changeDamage);
                
                effectiveHitJump  = true;
                effectiveHitSound = true;

                AttackGagePlus(changeDamage);
            }
        }
        
        // 유효타 점프 + 공중일 경우 -> 히트점프
        if (effectiveHitJump && !PlayerFloorCollider.instance.isGrounded)
        {
            isEffectHitJump = true;                     // 관성 이동을 멈추기 위해서 사용.
            StartCoroutine(EffectiveHitJump());
        }

        // 유효타 사운드 -> 사운드 생성
        if (effectiveHitSound)
            AudioManager.instance.PlayerSfxCreate(attackComboNum is 1 or 2 ? 1 : 3, true);
        
        // 공격 3 유효타 도트 히트 이펙트 생성
        if (effectiveHitSound && attackComboNum == 3)
        {
            if (PlayerController.instance.bodyGameObject.transform.localScale.x == 1)
                Instantiate(attackSlashEffect3_Dot, effectFollowTrans.position + Vector3.right, Quaternion.Euler(0f, 180f, -25)); // Y 좌우반전 // Z 살짝 위로 생성
            else
                Instantiate(attackSlashEffect3_Dot, effectFollowTrans.position + Vector3.left, Quaternion.Euler(0f, 0f, -25));   
        }
    }
    
    private void OnDrawGizmos()
    {   
        // 공격 1~3 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector2(2.4f, 1.5f));
    }

    private void AttackGagePlus(int changeDamage)
    {
        // 게이지 회복(엑셀 x의 경우)
        if (!PlayerAcceleration.instance.isAcceleration)
        {
            if(changeDamage == attackDamage3)
                UIController.instance.gageSlider.value += gageUpValue3;
            else
                UIController.instance.gageSlider.value += gageUpValue1_2;
        }
    }

    private IEnumerator EffectiveHitJump()
    {
        PlayerController.instance.rb2D.velocity = Vector2.zero;
        
        yield return new WaitForFixedUpdate();  // 잔여값 제거
        yield return new WaitForFixedUpdate();  // 잔여값 제거
        
        PlayerController.instance.rb2D.AddForce(new Vector2(0f, 175f));

        isEffectHitJump = false;    // 복구
    }
}