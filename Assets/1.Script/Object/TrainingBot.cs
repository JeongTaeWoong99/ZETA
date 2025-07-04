using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingBot : MonoBehaviour
{
    public  BoxCollider2D     bodyPush;                 // 바디푸쉬 콜라이더  
    public  Animator          animator;
    private CapsuleCollider2D cap2D;
    
    [Header("------HP------")]
    public  int	   currentHealth;           
    public  int	   maxHealth;
    [HideInInspector]
    public  bool   liveState = true;
    public  Slider hpSlider;
    public  float  hpSliderSeeTime;
    private float  hpSliderSeeTimeCount;

    [HideInInspector] 
    public bool infiniteHPMode; // 체력 무한 모드

    [Header("------Appear------")]
    public  List<SpriteRenderer> customMaterialRendererList;
    [HideInInspector]
    public List<Material>        customMaterialList;
    public GameObject            jetGameObject;

    private bool     isAppear;
    [HideInInspector]
    public  int      fadePropertyID;         // 페이드 이름
    [HideInInspector]
    public  float    fadeValue;              // 페이드값
    public  float    maxFadeValue;           // max페이드값(400% -> 4)
    public  float    fadeSpeed;

    private void Start()
    {
        cap2D = GetComponent<CapsuleCollider2D>();
        bodyPush.enabled = false;
        
        // HP
        currentHealth	   = maxHealth;
        hpSlider.maxValue  = maxHealth;							   
        hpSlider.value	   = currentHealth;
        
        // Appear
        fadePropertyID = Shader.PropertyToID("_DirectionalGlowFadeFade");
        fadeValue      = 0;
        
        foreach (var customMaterialRendererLists in customMaterialRendererList) // 받아오기
        {
            customMaterialList.Add(customMaterialRendererLists.material);
        }
        
        foreach (var customMaterialLists in customMaterialList)     // 초기화
        {
            customMaterialLists.SetFloat(fadePropertyID, 0f);
        }
        
        // 젯 끄기
        jetGameObject.SetActive(false);
    }

    private void Update()
    {
        // UI 슬라이더 관리
        UISlider();

        // Appear
        Appear();
        
        // HP
        HP();
    }

    private void FixedUpdate()
    {
        // 둥둥 떠다니는 것 영향 받도록 함.
        animator.speed = PlayerAcceleration.instance.accelerationChangedTimeValue;
    }

    private void HP()
    {
        // 1회 실행
        if (liveState && currentHealth <= 0f)
        {
            ChangeBot();
        }
    }

    public void CreatBot()
    {
        AudioManager.instance.EnemySfxCreate(9,true,gameObject);    // 활성화 글로우 페이드 사운드
        
        // HP 재설정
        currentHealth	   = maxHealth;
        hpSlider.maxValue  = maxHealth;							   
        hpSlider.value	   = currentHealth;
        
        // 상태 재설정
        liveState         = true;
        
        cap2D.enabled    = true;
        isAppear         = true;
        bodyPush.enabled = true;
    }

    // 사망시 바뀌는데 사용
    private void ChangeBot()
    {
        AudioManager.instance.EnemySfxCreate(9,true,gameObject);    // 비활성화 글로우 페이드 사운드
        
        liveState             = false;
        cap2D.enabled         = false;
        isAppear              = false;
        bodyPush.enabled      = false;
        hpSliderSeeTimeCount -= hpSliderSeeTime;
        jetGameObject.SetActive(false);

        EventController.instance.tutorialDestroyBotCount++; // 파괴 숫자 체크
        
        EventController.instance.currentAppealBotNum++;     // 활성화 번호 변경
        if (EventController.instance.currentAppealBotNum >= EventController.instance.trainingBotList.Count)  // 카운트를 넘어가면, 0으로 초기화
            EventController.instance.currentAppealBotNum = 0;
        
        EventController.instance.trainingBotList[EventController.instance.currentAppealBotNum].GetComponent<TrainingBot>().CreatBot();	// 다음 봇 활성화
    }

    // 전체 봇 비활성화에서 사용.
    public void DeactivateBot()
    {
        AudioManager.instance.EnemySfxCreate(9,true,gameObject);    // 비활성화 글로우 페이드 사운드
        
        liveState             = false;
        cap2D.enabled         = false;
        isAppear              = false;
        bodyPush.enabled      = false;
        hpSliderSeeTimeCount -= hpSliderSeeTime;
        jetGameObject.SetActive(false);
    }

    public void DamageBot(int damage)
    {
        if (!infiniteHPMode)    // 무적모드가 아닐 때, 데미지를 입도록 함.
        {
            // 체력
            currentHealth       -= damage;          // 체력감소
            hpSlider.value       = currentHealth;   // ui 갱신
            hpSliderSeeTimeCount = hpSliderSeeTime; // UI 보이기 시간 증가
        }
    }

    private void Appear()
    {
        foreach (var customMaterialLists in customMaterialList)
        {
            // 보이기
            if (isAppear && customMaterialLists.GetFloat(fadePropertyID) < maxFadeValue)
            {
                fadeValue += Time.deltaTime * fadeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                customMaterialLists.SetFloat(fadePropertyID, fadeValue);
                
                // 1회 실행
                if(customMaterialLists.GetFloat(fadePropertyID) > maxFadeValue)
                    jetGameObject.SetActive(true);
            }
            // 숨기기
            else if (!isAppear && customMaterialLists.GetFloat(fadePropertyID) > 0)
            {
                fadeValue -= Time.deltaTime * fadeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                customMaterialLists.SetFloat(fadePropertyID, fadeValue);
            }
        }
    }

    private void UISlider()
    {
        hpSliderSeeTimeCount -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
        if (hpSliderSeeTimeCount > 0f && hpSlider.gameObject.activeInHierarchy == false)
        {
            hpSlider.gameObject.SetActive(true);
        }
        else if (hpSliderSeeTimeCount < 0f && hpSlider.gameObject.activeInHierarchy)
        {
            hpSlider.gameObject.SetActive(false);
        }
    }
}
