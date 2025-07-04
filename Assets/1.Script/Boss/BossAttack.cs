using System;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    public static BossAttack instance;

    [Header("------Common------")] 
    public MeleeWeapon    bossMeleeWeapon;
    
    public List<GameObject>     lanceLighteningEffectList         = new List<GameObject>();
    [HideInInspector]
    public List<ParticleSystem> lanceLighteningParticleSystemList = new List<ParticleSystem>();
    
    [Header("------AttackPause------")] 
    public GameObject attackPauseEffect;          // 프리팹
    public Transform  attackPauseEffectMakeTrans; // 생성위치
    public float      attackPauseTime;            // 공격 퍼즈시간
    [HideInInspector] 
    public bool       isAttackPause;              // 어택퍼즈 상태
    [HideInInspector]
    public float      attackPauseTimeCount;       // 타임카운트
    
    [Header("------LancePassing------")]
    public float      maxMoveSpeed; 
    public int        attackDamage1;
    public GameObject attackHitEffect;                     // 각자의 히트 이펙트
    
    [Header("------SlashEffect 2------")]
    public Transform  effectFollowTrans;       // 이펙트가 따라다닐 위치(자연스러운 이동을 위한 위치)
    [HideInInspector]
    public GameObject currentSlashEffect;      // 현재 만들어진 슬레쉬 이펙트
    [HideInInspector]       
    public bool       closeRangeAttackPossible; // 근접공격 가능여부
    
    public GameObject attackSlashEffect2;      // 렌스 휘두르기(Lance2)
    public int        attackDamage2;
    public float      attackSpeed2;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 렌스 라이트닝 이펙트 넣기 및 끄기
        // 회복이펙트가 켜져있을 때 끄기(1회)
        for (int i = 0; i < lanceLighteningEffectList.Count; i++)
        {
            lanceLighteningParticleSystemList.Add(lanceLighteningEffectList[i].GetComponent<ParticleSystem>()); // 넣기
            lanceLighteningParticleSystemList[i].Stop();                                                            // 바로 멈추기
        }
        if (lanceLighteningParticleSystemList[0].isPlaying)
        {
            for (int i = 0; i < lanceLighteningParticleSystemList.Count; i++)
            {
                lanceLighteningParticleSystemList[i].Stop();
            }
        }
    }

    private void Update()
    {
        // 공격 퍼즈 시간 체크
        attackPauseTimeCount -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
        if (attackPauseTimeCount > 0f)
            isAttackPause = true;
        else
            isAttackPause = false;
    }
    
    private void FixedUpdate()
    {
        if (BossController.instance.bossAnimStateInfo.IsName("Lance2") && bossMeleeWeapon.enemyBladeCollider.enabled)
        {
            BossController.instance.rb2D.velocity = new Vector2(-BossController.instance.bodyObject.transform.localScale.x,0f) * attackSpeed2 * PlayerAcceleration.instance.accelerationChangedTimeValue;
        }
        else if (BossController.instance.bossAnimStateInfo.IsName("Lance2") && !bossMeleeWeapon.enemyBladeCollider.enabled)
        {
            BossController.instance.rb2D.velocity = Vector2.zero;
        }
        
        // 현재 만들어진 슬레쉬 이펙트가 있다면(그룹 레이어 문제로 따로 빼서 이동시킴)
        if (currentSlashEffect && BossController.instance.bossAnimStateInfo.IsName("Lance2"))
        {
            // 공격 2 (위아래로 휘두르기 따라가기 위치)
            int bossBodyX = BossController.instance.bodyObject.transform.localScale.x == 1 ? 1 : -1;
            currentSlashEffect.transform.position = effectFollowTrans.position + new Vector3(bossBodyX * 0.7f,-0.1f,0f);
        }
    }

    // 공격범위 안
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !PlayerAcceleration.instance.isAcceleration)
        {
            closeRangeAttackPossible = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !PlayerAcceleration.instance.isAcceleration)
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
