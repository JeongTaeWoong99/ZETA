using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BossAnimatorFunction : MonoBehaviour
{
    public static BossAnimatorFunction instance;
    
    [Header("------Lance------")] 
    public MeleeWeapon    bossMeleeWeapon;
    [HideInInspector] 
    public Coroutine lancePassingCoroutine; // 현재 실행 중인 렌스페싱 코루틴
    
    [Header("------Missile------")] 
    private int missilePreviousNumR = 2;    // 이전 오른쪽 해치 생선 값
    private int missilePreviousNumL = 2;    // 이전 왼쪽  해치 생성 값
    
    [Header("------Waves------")] 
    public GameObject wavesEffect;
    public LayerMask  wavesHitLayer;
    public int        wavesDamage;
    public float      wavesCircleRadius;

    [Header("------LinearGun------")]
    public  GameObject       linearGunFocusPrefabs;
    public  GameObject       linearGunFocusExplosionPrefabs;
    private GameObject       currentLinearGunFocusGameObject;
    
    public  List<GameObject> linearGunEnergyList = new List<GameObject>();
    public  float            linearGunEnergyScaleSpeed;
    private bool             islinearGunEnergyScaleUp;
    
    public  LayerMask  linearGunHitLayer;
    public  int        linearGunDamage;

    [Header("------Mine------")]
    public GameObject      minePrefabs;
    public List<Transform> mineCreateTransList = new List<Transform>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 에너지 구체 크기 초기화
        islinearGunEnergyScaleUp = false;
        foreach (var linearGunEnergyLists in linearGunEnergyList)
            linearGunEnergyLists.transform.localScale = new UnityEngine.Vector3(0f,0f,0f);
    }

    private void FixedUpdate()
    {
        // 레일건 에너지 스케일
        // 키우기(정배속으로 커짐)
        if (islinearGunEnergyScaleUp && BossController.instance.bossAnimStateInfo.IsName("LinearGun"))
        {
            foreach (var linearGunEnergyLists in linearGunEnergyList)
                linearGunEnergyLists.transform.localScale += new UnityEngine.Vector3(1f, 1f, 1f) * (linearGunEnergyScaleSpeed * Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
        }
        // 줄이기(5배속으로 작아짐)
        else
        {
            foreach (var linearGunEnergyLists in linearGunEnergyList)
            {
                // 0보다 작아지면, new UnityEngine.Vector3(0f,0f,0f);로 고정.
                if (linearGunEnergyLists.transform.localScale.x <= 0f)
                    linearGunEnergyLists.transform.localScale = new UnityEngine.Vector3(0f,0f,0f);
                else
                    linearGunEnergyLists.transform.localScale -= new UnityEngine.Vector3(1f, 1f, 1f) * (5f * linearGunEnergyScaleSpeed * Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
            }
        }
        
        // 렌스 트레일
        //  normal      -> acceleration
        if (PlayerAcceleration.instance.isAcceleration && bossMeleeWeapon.normalTrail.IsEffectEnabled() && !bossMeleeWeapon.accelerationTrail.IsEffectEnabled())
        {
            DisableTrailFunction();
            EnableTrailFunction();
        }
        // acceleration -> normal
        else if (!PlayerAcceleration.instance.isAcceleration && bossMeleeWeapon.accelerationTrail.IsEffectEnabled() && !bossMeleeWeapon.normalTrail.IsEffectEnabled())
        {
            DisableTrailFunction();
            EnableTrailFunction();
        }
    }

    // 랜스 1 전용(트레일 이펙트)
    private void EnableTrailFunction()
    {
        bossMeleeWeapon.enemyBladeCollider.enabled = true;                                                                                                   // 칼 히트 on
        bossMeleeWeapon.isAttackPlayerThisTime     = false;                                                                                                  // 켜진 직후 히트한적 없음
        
        if(PlayerAcceleration.instance.isAcceleration)
            bossMeleeWeapon.accelerationTrail.EnableTrail();      
        else
            bossMeleeWeapon.normalTrail.EnableTrail();             
    }
    
    // 랜스 1 전용(트레일 이펙트)
    public void DisableTrailFunction()
    {
        bossMeleeWeapon.enemyBladeCollider.enabled  = false; // 칼 히트 off
    
        bossMeleeWeapon.accelerationTrail.DisableTrail();    // 트레일 off
        bossMeleeWeapon.normalTrail      .DisableTrail();    // 트레일 off
    }

    public void Lance1Passing()
    {
        EnableTrailFunction();
        lancePassingCoroutine = StartCoroutine(Lance1PassingCoroutine());
    }

    private IEnumerator Lance1PassingCoroutine()
    {
        int   warpEndNum      = 0;                                                                                                                            // 끝나는 지점
        float totalLength     = Vector2.Distance(BossController.instance.warpTransList[2].transform.position, BossController.instance.warpTransList[3].transform.position); // 총 거리
        if (BossController.instance.currentWarpNum == 2)                            
            warpEndNum  = 3;
        else if (BossController.instance.currentWarpNum == 3)
            warpEndNum = 2;
        
        // 속도는 dash와 같은 메커니즘으로 작아짐.
        while (true)
        {
            // 대쉬와 같은 원리인데, 여기서는 애니메이션의 상태가 아니라, 토탈 거리에 따른 자신의 위치를 progress에서 사용함.
            // 현재 위치(0 --> 1)
            float progress = (1f - Vector2.Distance(BossController.instance.transform.position, BossController.instance.warpTransList[warpEndNum].transform.position) / totalLength);
            // 감속값 (1 --> 0)
            // 0에 가까워 질 수록, maxMoveSpeed * productValue으로 이동 속도가 작아짐.
            float productValue = 1f - progress;
            
            BossController.instance.rb2D.MovePosition(BossController.instance.rb2D.position + 
                                                      (new Vector2(-BossController.instance.bodyObject.transform.localScale.x,0f) * (Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue * 
                                                          BossAttack.instance.maxMoveSpeed * productValue)));
            
            // 감속이 0.01이하이면, 도착 break;
            if (productValue <= 0.01f)
            {
                break;
            }
            
            //Debug.Log(progress + " | " + productValue + " | " + bossAttack.maxMoveSpeed * productValue);   
            yield return new WaitForFixedUpdate();
        }
        lancePassingCoroutine = null;
    }

    // 랜스 2 전용(파티클 이펙트)
    public void EnableBlade()
    {
        bossMeleeWeapon.enemyBladeCollider.enabled = true;                                                                                                   // 칼 히트 on
        bossMeleeWeapon.isAttackPlayerThisTime     = false;                                                                                                  // 켜진 직후 히트한적 없음
    }
    
    // 랜스 2 전용(파티클 이펙트)
    public void DisableBlade()
    {
        bossMeleeWeapon.enemyBladeCollider.enabled  = false; // 칼 히트 off
    }
    
    // 루프 카운트 체크
    public void LoopCount()
    {
        BossController.instance.loopCount += 1;
    }

    public void MakeMissileR()
    {
        // 해치 R 0~2번 선택
        int creatNumR;
        missilePreviousNumR++;
        // 오른쪽 최대넘 보다 크면 초기화 후 저장
        if (missilePreviousNumR > BossController.instance.hatchTransListR.Count - 1)
        {
            missilePreviousNumR = 0;
            creatNumR = missilePreviousNumR;    // 초기화 R
        }
        // 최대값을 넘지 않으면, 바로 저장
        else
            creatNumR = missilePreviousNumR;
            
        // 미사일
        // 바디 오른쪽
        if (BossController.instance.bodyObject.transform.localScale.x == 1)
        {
            BossController.instance.missileList.Add(Instantiate(BossController.instance.missilePrefabs, BossController.instance.hatchTransListR[creatNumR].position, BossController.instance.hatchTransListR[creatNumR].rotation));     // 미사일(+ADD 리스트)
            Instantiate(BossController.instance.missileShootEffect, BossController.instance.hatchTransListR[creatNumR].position, BossController.instance.hatchTransListR[creatNumR].rotation);                              // 미사일 생성 이펙트
        }
        // 바디 왼쪽
        else
        {
            BossController.instance.missileList.Add(Instantiate(BossController.instance.missilePrefabs, BossController.instance.hatchTransListR[creatNumR].position, 
                BossController.instance.hatchTransListR[creatNumR].rotation * Quaternion.Euler(0f, 0f, 180f)));                                               // 미사일(+ADD 리스트)
            Instantiate(BossController.instance.missileShootEffect, BossController.instance.hatchTransListR[creatNumR].position, BossController.instance.hatchTransListR[creatNumR].rotation * Quaternion.Euler(0f, 0f, 180f)); // 미사일 생성 이펙트
        }
    }
    
    public void MakeMissileL()
    {
        // 해치 R 3~5번 선택
        int creatNumL;
        missilePreviousNumL++;
        // 오른쪽 최대넘 보다 크면 초기화 후 저장
        if (missilePreviousNumL > BossController.instance.hatchTransListL.Count - 1)
        {
            missilePreviousNumL = 0; // 초기화 L
            creatNumL = missilePreviousNumL;
        }
        // 최대값을 넘지 않으면, 바로 저장
        else
            creatNumL = missilePreviousNumL;

        // 미사일
        // 바디 오른쪽
        if (BossController.instance.bodyObject.transform.localScale.x == 1)
        {
            BossController.instance.missileList.Add(Instantiate(BossController.instance.missilePrefabs, BossController.instance.hatchTransListL[creatNumL].position, BossController.instance.hatchTransListL[creatNumL].rotation));     // 미사일(+ADD 리스트)
            Instantiate(BossController.instance.missileShootEffect, BossController.instance.hatchTransListL[creatNumL].position, BossController.instance.hatchTransListL[creatNumL].rotation);                              // 미사일 생성 이펙트
        }
        // 바디 왼쪽
        else
        {
            BossController.instance.missileList.Add(Instantiate(BossController.instance.missilePrefabs, BossController.instance.hatchTransListL[creatNumL].position, 
                BossController.instance.hatchTransListL[creatNumL].rotation * Quaternion.Euler(0f, 0f, 180f)));                                              // 미사일(+ADD 리스트)
            Instantiate(BossController.instance.missileShootEffect, BossController.instance.hatchTransListL[creatNumL].position, BossController.instance.hatchTransListL[creatNumL].rotation * Quaternion.Euler(0f, 0f, 180f)); // 미사일 생성 이펙트                              // 미사일 생성 이펙트
        }
        
    }

    // 아래서 아래서 위로 휘두르기
    public void MakeAttackEffectLance2_1()
    {
        // 공격 이펙트
        if (BossController.instance.bodyObject.transform.localScale.x == -1)
        {
            BossAttack.instance.currentSlashEffect = Instantiate(BossAttack.instance.attackSlashEffect2, BossAttack.instance.effectFollowTrans.position, Quaternion.Euler(180f, 0f, 150));   // 살짝 기울기 수정
        }
        else
        {
            BossAttack.instance.currentSlashEffect = Instantiate(BossAttack.instance.attackSlashEffect2, BossAttack.instance.effectFollowTrans.position, Quaternion.Euler(0f, 0f, -30f));      // 살짝 기울기 수정
        }
    }
    
    // 아래서 위에서 아래로 휘두르기
    public void MakeAttackEffectLance2_2()
    {
        // 공격 이펙트
        if (BossController.instance.bodyObject.transform.localScale.x == -1)
        {
            BossAttack.instance.currentSlashEffect = Instantiate(BossAttack.instance.attackSlashEffect2, BossAttack.instance.effectFollowTrans.position, Quaternion.Euler(0f, 0f, 210f));
        }
        else
        {
            BossAttack.instance.currentSlashEffect = Instantiate(BossAttack.instance.attackSlashEffect2, BossAttack.instance.effectFollowTrans.position, Quaternion.Euler(180f, 0f, 30f));
        }
    }
    
    // 랜스 어택 퍼즈
    public void AttackPause()
    {
        // 퍼즈 깜빡임 랜스에 넣기
        GameObject newObj = Instantiate(BossAttack.instance.attackPauseEffect, BossAttack.instance.attackPauseEffectMakeTrans.position, Quaternion.identity);
        newObj.transform.parent = BossAttack.instance.attackPauseEffectMakeTrans; 
        BossAttack.instance.isAttackPause        = true;
        BossAttack.instance.attackPauseTimeCount = BossAttack.instance.attackPauseTime;
    }

    // 공기팡 이펙트
    public void AirPanEffect()
    {
        Instantiate(wavesEffect, BossController.instance.bodyObject.transform.position, Quaternion.identity);
        Collider2D[] hit = Physics2D.OverlapCircleAll(BossController.instance.bodyObject.transform.position, wavesCircleRadius, wavesHitLayer);

        foreach (var hits in hit)
        {
            if(hits.CompareTag("Player"))
                hits.GetComponent<PlayerHp>().DamagePlayer(transform,wavesDamage);
        }
    }

    public void LinearGunExplosion()
    {
        if (PlayerHp.instance)
        {
            AudioManager.instance.WardenSfxCreate(8,false,currentLinearGunFocusGameObject); // LinearGun Explosion 사운드. (폭파 위치에서 생성.)
            GameObject exTrans = Instantiate(linearGunFocusExplosionPrefabs,currentLinearGunFocusGameObject.transform.position, Quaternion.identity);
            Collider2D[] hit   = Physics2D.OverlapCircleAll(currentLinearGunFocusGameObject.transform.position, linearGunFocusPrefabs.transform.localScale.x, linearGunHitLayer);
            
            foreach (var hits in hit)
            {
                if(hits.CompareTag("Player"))
                    hits.GetComponent<PlayerHp>().DamagePlayer(exTrans.transform,linearGunDamage);
            }
        }
        
        // LinearGunFocusDestroy
        Destroy(currentLinearGunFocusGameObject);
    }
    
    
    public void LinearGunFocusCreate()
    {
        if(PlayerHp.instance)
            currentLinearGunFocusGameObject = Instantiate(linearGunFocusPrefabs, PlayerController.instance.transform.position, Quaternion.identity);
    }
    
    public void MineCreate()
    {
        switch (BossController.instance.loopCount)
        {
            case 1:
                Instantiate(minePrefabs,mineCreateTransList[0].position , Quaternion.identity);
                Instantiate(minePrefabs,mineCreateTransList[6].position , Quaternion.identity);
                break;
            case 2:
                Instantiate(minePrefabs,mineCreateTransList[1].position , Quaternion.identity);
                Instantiate(minePrefabs,mineCreateTransList[5].position , Quaternion.identity);
                break;
            case 3:
                Instantiate(minePrefabs,mineCreateTransList[2].position , Quaternion.identity);
                Instantiate(minePrefabs,mineCreateTransList[3].position , Quaternion.identity);
                Instantiate(minePrefabs,mineCreateTransList[4].position , Quaternion.identity);
                break;
        }
    }

    // 레일건 에너지 구체 커지기
    public void LinearGunEnergyEnable()
    {
        islinearGunEnergyScaleUp = true;
        BossController.instance.isPlayerAiming = true;
    }
    
    // 레일건 에너지 구체 작아지기
    public void LinearGunEnergyDisable()
    {
        islinearGunEnergyScaleUp = false;
        BossController.instance.isPlayerAiming = false;
    }
    
    // 워든 사망 애니메이션이 끝나고
    private void LanceDeathAppearFalse()
    {
        StartCoroutine(EventController.instance.BossRoomEndEvent());    // 보스 사망 이벤트 활성화
    }

    public IEnumerator BossLightOff()
    {
        AudioManager.instance.WardenSfxCreate(17,true,gameObject); // 사망 전원 Turn OFF 사운드(1회 재생)
        
        while (true)
        {
            // 바디 라이트 밝기
            if (BossController.instance.bodyBrightFadeValue > 0)
            {
                BossController.instance.bodyBrightFadeValue -= Time.deltaTime * 2f;
                BossController.instance.bossBodyLightMat.SetFloat(BossController.instance.brightFadeID, BossController.instance.bodyBrightFadeValue);
			
                if (BossController.instance.bodyBrightFadeValue <= 0)
                {
                    // 값 픽스
                    BossController.instance.bodyBrightFadeValue = 0f;
                    BossController.instance.bossBodyLightMat.SetFloat(BossController.instance.brightFadeID, BossController.instance.bodyBrightFadeValue);
                    break;
                }
            }

            yield return null;
        }
    }
    
    public void StopLancePassingCo()
    {
        StopCoroutine(lancePassingCoroutine); // 이동 코루틴 멈추기
    }
    
    public void WingExplosion()
    {
        // 스파크 끄기
        foreach (var wingSparkLists in BossHP.instance.wingSparkList)
            wingSparkLists.gameObject.SetActive(false);

        // 날개 끄기
        foreach (var wingGameObjectLists in BossHP.instance.wingGameObjectList)
            wingGameObjectLists.SetActive(false);
        
        // 폭파 이펙트
        AudioManager.instance.WardenSfxCreate(7,true,BossHP.instance.explosionTransList[0]);
        Instantiate(BossHP.instance.explosionPrefabs, BossHP.instance.explosionTransList[0].transform.position, quaternion.identity);
        AudioManager.instance.WardenSfxCreate(7,true,BossHP.instance.explosionTransList[1]);
        Instantiate(BossHP.instance.explosionPrefabs, BossHP.instance.explosionTransList[1].transform.position, quaternion.identity);
        
        // 트리거 작동 및 종속관계 제거
        foreach (var boom in BossHP.instance.boomBodyList)
        {
            boom.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);    // 투명도 보이기
            Transform objectTransform = boom.gameObject.transform;                                // 부모에서 벗어나기
            objectTransform.SetParent(null);                                                    // 부모에서 벗어나기
            boom.BodyBoom();                                                                      // 날라가기
        }
        
        BossController.instance.rb2D.gravityScale = 5; // 떨어지기
        
        BossHP.instance.flyBodyFloating.enabled                    = false;        // 공중부유 끄기
        BossController.instance.bodyObject.transform.localPosition = Vector3.zero; // 원래 위치로 돌아가기
        
        BossController.instance.pushBody.enabled = false;                          // 바디 밀치기 끄기
        
        AudioManager.instance.EnemySfxCreate(2, true, gameObject);  // 무릎꾾는 사운드 (1회 재생)

        StartCoroutine(EventController.instance.BossRoomEndEvent());
    }
}
