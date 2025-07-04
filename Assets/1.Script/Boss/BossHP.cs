using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHP : MonoBehaviour
{
    public static BossHP instance;

    public FlyBodyFloating flyBodyFloating;

    public  int  currentHealth;
    public  int  maxHealth;

    [HideInInspector] 
    public bool isLive = true;

    public List<Transform> endEventTrans;

    public List<GameObject> wingGameObjectList = new List<GameObject>();
    public GameObject       explosionPrefabs;
    public List<GameObject> explosionTransList = new List<GameObject>();

    public List<BoomBody> boomBodyList = new List<BoomBody>();

    public List<GameObject> wingSparkList = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        currentHealth                             = maxHealth;      // 게임 시작시 currentHealth값을 maxHealth 값으로 설정
        BossUI.instance.bossHealthSlider.maxValue = maxHealth;      // 게임 시작시 healthSlider.maxValue값을 maxHealth 값으로 설정
        BossUI.instance.bossHealthSlider.value    = currentHealth;  // 게임 시작시 healthSlider.value값을 currentHealth 값으로 설정
    }

    public void DamageBoss(int damageInt)
    {
        if (isLive)
        {
            // 체력 감소
            currentHealth    -= damageInt;                                      
            // UI값 갱신
            BossUI.instance.bossHealthSlider.value = currentHealth;

            // 사망 체크
            if (currentHealth <= 0)
            {
                StartCoroutine(BossDeath());
            }
        }
    }

    // 현재 사망은 미사일 or 렌스패싱 페턴에서 일어남.
    // 상태롤 초기화 해야함.
    private IEnumerator BossDeath()
    {
        isLive = false;  // 상태변경
        
        EventController.instance.eventState        = true;
        EventController.instance.AllKeyLockTrue();
        PlayerAcceleration.instance.isAcceleration = false;

        UIController.instance.gameObject.SetActive(false);                         // UI 자체를 꺼버리기
        PlayerHp.instance.currentHealth = maxHealth;
        
        // 렌스 패싱 멈추기
        BossAnimatorFunction.instance.DisableTrailFunction();           // 칼 트레일 + 히트박스 끄기
        if(BossAnimatorFunction.instance.lancePassingCoroutine != null) // 이동 중 이라면, 이동 멈추기
            BossAnimatorFunction.instance.StopLancePassingCo();
        
        yield return new WaitForFixedUpdate();
        PlayerController.instance.rb2D.velocity = Vector2.zero;
        
        AudioManager.instance.currentAmbientSoundNum = 3;	// BGM 변경(지하2층 앰비언트 사운드로 변경)
        
        FadeManager.instance.BossDeathFadeIn();  // 페이드 인
        yield return new WaitForSeconds(1);
        
        BossController.instance.bossAnim.SetTrigger("deathToIdleOn");
        
        while (true)
        {
            if (FadeManager.instance.fadeImage.material.GetFloat(FadeManager.instance.fullAlphaDissolveFadePropertyID) >= 1)
            { 
                // 플레이어 이동
                PlayerController.instance.gameObject.transform.position       = endEventTrans[0].position;	 // 위치
                PlayerController.instance.bodyGameObject.transform.localScale = endEventTrans[0].localScale; // 보는 방향
                PlayerController.instance.rb2D.velocity = Vector2.zero;
                
                // 워든 이동
                BossController.instance.gameObject.transform.position   = endEventTrans[1].position;   // 위치
                BossController.instance.bodyObject.transform.localScale = endEventTrans[1].localScale; // 보는 방향
                
                foreach (var wingSparkLists in wingSparkList)   // 스파크 켜기
                    wingSparkLists.gameObject.SetActive(true);
                
                FadeManager.instance.BossDeathFadeOut(); // 페이드 아웃
                yield return new WaitForSeconds(1);

                break;
            }
            yield return new WaitForFixedUpdate();
        }

        while (true)
        {
            if (FadeManager.instance.fadeImage.material.GetFloat(FadeManager.instance.fullAlphaDissolveFadePropertyID) <= 0)
            {
                FadeManager.instance.isFadeActiveState = false;                // 이벤트 상태 체크
            
                BossController.instance.bossAnim.SetTrigger("deathOn");

                AudioManager.instance.WardenSfxCreate(16,true,wingSparkList[0]); // 스파크 사운드 생성(1회 재생)
                AudioManager.instance.WardenSfxCreate(16,true,wingSparkList[1]); // 스파크 사운드 생성(1회 재생)

                break;
            }
            yield return new WaitForFixedUpdate();
        }

    }
}
