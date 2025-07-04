using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;

    [Header("------Fade------")]
    public  Image fadeImage;
    [HideInInspector] 
    public  bool  fadeState;
    [HideInInspector]
    public int   fullAlphaDissolveFadePropertyID;
    private float fadeValue;
    public  float fadeSpeed;

    public bool   thisSeenStartFadeState;   // 시작 페이드 상태(true 없애면서 시작 / false 없는 상태 시작)

    [HideInInspector] 
    public bool isFadeActiveState;
    
    [Header("------DeathRegression------")]
    public  float playerDeathTimeNextSeen;
    private float playerDeathTimeNextSeenCount;
    
    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        fullAlphaDissolveFadePropertyID = Shader.PropertyToID("_FullAlphaDissolveFade");
        fadeImage.material.SetFloat(fullAlphaDissolveFadePropertyID, 0f);       // 초기화
        
        // TRUE시 페이드를 없애면서 시작함.
        if (thisSeenStartFadeState)
        {
            fadeValue         = 1f;                                                  // 최고값
            fadeImage.material.SetFloat(fullAlphaDissolveFadePropertyID, fadeValue); // 세팅
            StartCoroutine(NextSeenFadeOut());
        }

    }

    private void Update()
    {
        Fade();             // 페이드
        
        DeathRegression();  // 사망회귀
    }

    private void Fade()
    {
        // 페이드 보이기
        if(fadeState)
        {
            if (fadeValue < 1)
            {
                fadeValue          += fadeSpeed * Time.unscaledDeltaTime;
                fadeImage.material.SetFloat(fullAlphaDissolveFadePropertyID, fadeValue);
            }
        }
        // 페이드 없애기
        else if(!fadeState)
        {
            if (fadeValue > 0)
            {
                fadeValue          -= fadeSpeed * Time.unscaledDeltaTime;
                fadeImage.material.SetFloat(fullAlphaDissolveFadePropertyID, fadeValue);
            }
        }
    }
    
    public IEnumerator NextSeenFadeIn()
    {
        isFadeActiveState = true;               // 이벤트 상태 체크
        yield return new WaitForSecondsRealtime(1);
        fadeState         = true;               // 작동
        yield return new WaitForSecondsRealtime(1);
        isFadeActiveState = false;              // 이벤트 상태 체크
    }
    
    public IEnumerator NextSeenFadeOut()
    {
        isFadeActiveState = true;                // 이벤트 상태 체크
        fadeState         = true;                // 작동 방지
        yield return new WaitForSecondsRealtime(3);      
        fadeState         = false;               // 작동
        yield return new WaitForSecondsRealtime(1);      
        isFadeActiveState = false;               // 이벤트 상태 체크
    }
    
    private void DeathRegression()  // 사망회귀
    {
        if (!PlayerHp.instance.liveState)
        {
            playerDeathTimeNextSeenCount += Time.deltaTime;
            if (playerDeathTimeNextSeenCount > playerDeathTimeNextSeen)
                SceneManager.LoadScene(PlayerPrefs.GetString("SaveSeenName"));
        }
    }
    
    public void BossDeathFadeIn()
    {
        isFadeActiveState = true;               // 이벤트 상태 체크
        fadeState         = true;               // 작동
    }
    
    public void BossDeathFadeOut()
    {
        fadeState         = false;               // 작동
    }
}
