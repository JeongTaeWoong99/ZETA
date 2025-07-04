using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class MeleeWeapon : MonoBehaviour
{
    [Header("------Common------")]
    public PolygonCollider2D enemyBladeCollider;        
    [HideInInspector] 
    public bool              isAttackPlayerThisTime;    // false는 enemyBladeCollider가 켜졌을 때, 히트한 적이 없음 // true는  enemyBladeCollider가 켜졌을 때, 히트한적 있음
    
    [Header("------Enemy------")]
    public EnemyController    enemyCon;
    public EnemyAttack        enemyAttack;
    public EnemyHp            enemyHp;

    [Header("------Warden------")] 
    public BossController bossCon;
    public BossAttack     bossAttack;
    public BossHP         bossHp;
    
    public SpriteTrail.SpriteTrail normalTrail;        // normal       칼 트레일
    public SpriteTrail.SpriteTrail accelerationTrail;  // acceleration 칼 트레일
    
    private void FixedUpdate()
    {
        // 사망시 끄기
        // 적
        if (enemyCon)
        {
            if (!enemyHp.liveState)
            {
                enemyBladeCollider.enabled = false;
            }
        }
        
        // 보스
        if (bossCon)
        {
            if (!bossHp.isLive)
            {
                normalTrail.DisableTrail();
                accelerationTrail.DisableTrail();
                enemyBladeCollider.enabled = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isAttackPlayerThisTime)
        {
            // 일반적
            if (enemyCon)
            {
                if(other.GetComponent<PlayerHp>().liveState)
                    other.GetComponent<PlayerHp>().DamagePlayer(enemyCon.bodyObject.transform,enemyAttack.attackDamage);
                    
                // 히트 사운드 및 이펙트
                // 가드의 경우
                if (!enemyAttack.rotationMain)
                {
                    AudioManager.instance.EnemySfxCreate(1, true, enemyCon.gameObject);                         // 사운드
                    Instantiate(enemyAttack.attackHitEffect, other.transform.position, quaternion.identity);  // 이펙트
                }
                // 스나의 경우
                else if (enemyAttack.rotationMain)
                {
                    AudioManager.instance.EnemySfxCreate(8, true, enemyCon.gameObject);
                    Instantiate(enemyAttack.attackHitEffect, other.transform.position, quaternion.identity);  // 이펙트
                }
                
                isAttackPlayerThisTime = true;      // 히트한적 있음 처리
            }
            
            // 보스
            if (bossCon)
            {
                if (other.GetComponent<PlayerHp>().liveState)
                {
                    // Lance1 데미지
                    if(bossCon.bossAnimStateInfo.IsName("Lance1"))
                        other.GetComponent<PlayerHp>().DamagePlayer(bossCon.bodyObject.transform,bossAttack.attackDamage1);
                    // Lance2 데미지
                    else if (bossCon.bossAnimStateInfo.IsName("Lance2"))
                        other.GetComponent<PlayerHp>().DamagePlayer(bossCon.bodyObject.transform,bossAttack.attackDamage2);
                        
                    //                                                                                                       // 사운드
                    Instantiate(bossAttack.attackHitEffect, other.transform.position, quaternion.identity);  // 이펙트
                }

                isAttackPlayerThisTime = true;      // 히트한적 있음 처리
            }
        }
    }
    
}
