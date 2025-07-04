using UnityEngine;
using UnityEngine.Serialization;

public class Mine : MonoBehaviour
{
    [Header("------Common------")] 
    public GameObject lightGameObject;
    public GameObject mineExplosionPrefabs;
    public GameObject mineRangePrefabsRed;
    public GameObject mineRangePrefabsBlue;
    public GameObject alterParticle;

    [HideInInspector]
    public GameObject redRangeGameObject;
    [HideInInspector]
    public GameObject blueRangeGameObject;
    
    private Material  bodyMat;
    private Material  lightMat;
    
    private CircleCollider2D        circle2D;
    public InAccelerationOrderLayer inAccelerationOrderLayer;
    
    [HideInInspector] 
    public bool isHackingPossible;          // 해킹이 가능한 상태인지(아직 터지기 전, 해킹을 하기 전)
    
    [Header("------Appear------")] 
    public  float appearSpeed;
    [HideInInspector]
    public  bool  isAppear;                  // 등장 페이드(기본 true / 보스전 생성은 만들자마자 false로 변경)
    private int   dissolveFadeID;
    private float currentDissolveFadeValue;
    
    [Header("------Brightness------")]
    public  float lightSpeed;           // 색이 변하는 속도
    public  float maxLightValue;
    private int   brightnessFadeID;
    private float currentBrightnessFadeValue;

    [Header("------Mine_To_Player------")]
    public  LayerMask mineHitPlayerLayer;
    [HideInInspector]
    public  bool      isTimerOperation;
    public  float     operationTime;
    private float     operationTimeCount;
    public  int       mineDamage;
    public  float     selfDestructTime;      // 자폭 시간
    private float     selfDestructTimeCount; // 자폭 시간

    [Header("------Mine_To_Enemy------")] 
    public LayerMask mineHitEnemyLayer; 
    [HideInInspector]
    public bool     isTriggerOn;            // 해킹이 된 상태인지
    public Color    mineToEnemyColor;       // 해킹 상태 컬러

    private void Start()
    {
        circle2D = GetComponent<CircleCollider2D>();
    
        isAppear = false;
        
        bodyMat  = GetComponent<SpriteRenderer>().material;
        lightMat = lightGameObject.GetComponent<SpriteRenderer>().material;

        dissolveFadeID   = Shader.PropertyToID("_FullGlowDissolveFade");
        brightnessFadeID = Shader.PropertyToID("_Brightness");
        if (!isAppear)
        {                                                                                   
            bodyMat.SetFloat(dissolveFadeID, 0f);      // 바디   (보이지 않기)
            lightMat.SetFloat(dissolveFadeID, 0f);     // 라이트 (보이지 않기)
            
            lightMat.SetFloat(brightnessFadeID, 0f);   // 라이트 (끄기)
        }

        isHackingPossible = true;

        isTimerOperation  = false;
    }

    private void FixedUpdate()
    {
        // 나타나지 않은 상태
        // 디졸브 나타나기
        if (!isAppear)
        {
            currentDissolveFadeValue += Time.fixedDeltaTime * appearSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (currentDissolveFadeValue < 1)
            {
                bodyMat.SetFloat(dissolveFadeID, currentDissolveFadeValue);    // 바디
                lightMat.SetFloat(dissolveFadeID, currentDissolveFadeValue);   // 라이트
            }
            else if(currentDissolveFadeValue >= 1)
            {
                // 밝기 높히기
                currentBrightnessFadeValue += Time.fixedDeltaTime * lightSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                if (currentBrightnessFadeValue < maxLightValue)
                {
                    lightMat.SetFloat(brightnessFadeID, currentBrightnessFadeValue);   // 바디
                }
                else
                {                   
                     isAppear                = true;            // 상태전환
                     bodyMat.SetFloat(dissolveFadeID, 1);  // 바디
                     lightMat.SetFloat(dissolveFadeID, 1); // 라이트
                    
                     redRangeGameObject = Instantiate(mineRangePrefabsRed, transform.position, Quaternion.identity); // 빨간 범위 생성
                }
            }
        }
        // 나타난 상태
        // 밝기 키우기
        // 자폭 시간 체크
        else if (isAppear)
        {
            // 자폭 시간 카운트
            selfDestructTimeCount += Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            
            // 자폭(selfDestructTimeCount가 넘어가면)
            if (selfDestructTimeCount > selfDestructTime && isHackingPossible && !isTimerOperation && !isTriggerOn)
            {
                isTimerOperation  = true;
                isHackingPossible = false;
                Instantiate(alterParticle, transform.position, Quaternion.identity);
                AudioManager.instance.ObjectSfxCreate(7,true,gameObject); // 사운드
            }
        }

        TimerOperation();

        // 보스 사망 or 플레이어 사망시 모두 터지기
        if (EventController.instance.isBossRoom && (!BossHP.instance.isLive || !PlayerHp.instance.liveState))
        {
            Instantiate(mineExplosionPrefabs, transform.position, Quaternion.identity); // 폭파 이펙트
            AudioManager.instance.EnemySfxCreate(3, false, gameObject);   // 폭발사운드(부모 X)
            
            if(blueRangeGameObject)
                Destroy(blueRangeGameObject);           // 블루 범위 파괴
            if(redRangeGameObject)
                Destroy(redRangeGameObject);            // 레드 범위 파괴
            Destroy(transform.parent.gameObject);
        }
    }
    
    // enter = stay
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어(나타났고, 해킹가능 상태이면) 타이머 작동
        if (other.CompareTag("Player") && isAppear && isHackingPossible && !isTimerOperation && !isTriggerOn && !PlayerAcceleration.instance.isAcceleration)
        {
            isTimerOperation  = true;
            isHackingPossible = false;
            Instantiate(alterParticle, transform.position, Quaternion.identity);    // 반짝임 파티클 생성
            AudioManager.instance.ObjectSfxCreate(7,true,gameObject); // 사운드
        }
        
        // 일반적 + 보스
        if (((other.CompareTag("Enemy") && other.GetComponent<EnemyLightController>().isAppear) || (other.CompareTag("Boss") && other.GetComponent<BossController>().isAppear))
            && isAppear && !isHackingPossible && isTriggerOn)
        {
            Instantiate(mineExplosionPrefabs, transform.position, Quaternion.identity); // 폭파 이펙트
            AudioManager.instance.EnemySfxCreate(3, false, gameObject);   // 폭발사운드(부모 X)
            
            Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position,circle2D.radius * transform.localScale.x, mineHitEnemyLayer);
                
            foreach (var hits in hit)
            {
                if (hits.CompareTag("Enemy"))
                {
                    hits.GetComponent<EnemyHp>().hitAnimNum = 3;
                    hits.GetComponent<EnemyHp>().DamageEnemy(mineDamage,transform,0f);
                }
                else if(hits.CompareTag("Boss"))
                    hits.GetComponent<BossHP>().DamageBoss(mineDamage);
            }
            
            Destroy(blueRangeGameObject);   // 블루 범위 파괴
            Destroy(transform.parent.gameObject);
        }
    }
    
    // stay = enter
    private void OnTriggerStay2D(Collider2D other)
    {
        // 플레이어(나타났고, 해킹가능 상태이면) 타이머 작동
        if (other.CompareTag("Player") && isAppear && isHackingPossible && !isTimerOperation && !isTriggerOn && !PlayerAcceleration.instance.isAcceleration)
        {
            isTimerOperation  = true;
            isHackingPossible = false;
            Instantiate(alterParticle, transform.position, Quaternion.identity);     // 반짝임 파티클 생성
            AudioManager.instance.ObjectSfxCreate(7,true,gameObject);  // 사운드
        }
        
        // 일반적 + 보스
        if (((other.CompareTag("Enemy") && other.GetComponent<EnemyLightController>().isAppear) || (other.CompareTag("Boss") && other.GetComponent<BossController>().isAppear))
            && isAppear && !isHackingPossible && isTriggerOn)
        {
            Instantiate(mineExplosionPrefabs, transform.position, Quaternion.identity); // 폭파 이펙트
            AudioManager.instance.EnemySfxCreate(3, false, gameObject);   // 폭발사운드(부모 X)
            
            Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position,circle2D.radius * transform.localScale.x, mineHitEnemyLayer);
                
            foreach (var hits in hit)
            {
                if (hits.CompareTag("Enemy"))
                {
                    hits.GetComponent<EnemyHp>().hitAnimNum = 3;
                    hits.GetComponent<EnemyHp>().DamageEnemy(mineDamage,transform,0f);
                }
                else if(hits.CompareTag("Boss"))
                    hits.GetComponent<BossHP>().DamageBoss(mineDamage);
            }
            
            Destroy(blueRangeGameObject);   // 블루 범위 파괴
            Destroy(transform.parent.gameObject);
        }
    }

    // 플레이어에게 작동
    private void TimerOperation()
    {
        if (isTimerOperation)
        {
            operationTimeCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            
            if (operationTimeCount > operationTime)
            {
                Instantiate(mineExplosionPrefabs, transform.position, Quaternion.identity); // 폭파 이펙트
                AudioManager.instance.EnemySfxCreate(3, false, gameObject);    // 폭발사운드(부모 X)
                
                Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position,circle2D.radius * transform.localScale.x, mineHitPlayerLayer);
                
                foreach (var hits in hit)
                {
                    if(hits.CompareTag("Player"))
                        hits.GetComponent<PlayerHp>().DamagePlayer(transform,mineDamage);
                }
                
                Destroy(redRangeGameObject);            // 레드 범위 파괴
                Destroy(transform.parent.gameObject);
            }
        }
    }
    
    public void HackingTrigger()
    {
        isHackingPossible = false;
        isTriggerOn       = true;
        
        Destroy(redRangeGameObject);
        blueRangeGameObject = Instantiate(mineRangePrefabsBlue, transform.position, Quaternion.identity); // 파란 범위 생성
    }
    
    
}
