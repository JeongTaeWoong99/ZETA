using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLightController : MonoBehaviour
{
    public static PlayerLightController instance;
    
    [Header("------LightSaber------")] 
    public  Material lightSaberMaterial; 
    [HideInInspector]
    public  int      fadePropertyID1;         // 페이드 이름
    [HideInInspector]
    public  float    fadeValue1;              // 페이드값
    public  float    maxFadeValue1;           // max페이드값(400% -> 4)
    
    public  float    lightSaberHoldTime;
    private float    lightSaberHoldTimeCount;
    
    public  float lightSaberOnSpeed;
    public  float lightSaberOffSpeed;
    
    [Header("------Clothes------")]
    public Material clothesMaterial;
    [HideInInspector]
    public  int   fadePropertyID2;         // 페이드 이름
    [HideInInspector]
    public  float fadeValue2;              // 페이드값
    public  float maxFadeValue2;           // max페이드값(400% -> 4)
    public  float minFadeValue2;           // max페이드값(400% -> 4)

    public  float clothesOnSpeed;
    [HideInInspector]
    public bool isClothesLightUp;           // 올라갈지 내려갈지

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // LightSaber
        fadePropertyID1               = Shader.PropertyToID("_SourceGlowDissolveFade");
        fadeValue1                    = 0;
        lightSaberMaterial.SetFloat(fadePropertyID1, 0f);    
        
        // Clothes
        fadePropertyID2  = Shader.PropertyToID("_Brightness");
        fadeValue2       = maxFadeValue2;         
        clothesMaterial.SetFloat(fadePropertyID2, fadeValue2);
    }

    private void Update()
    {
        // 라이트세이버
        LightSaber();
        
        // 옷 라이트
        Clothes();
    }

    private void LightSaber()
    {
        lightSaberHoldTimeCount -= Time.deltaTime;
        // 광선검 on (시간갱신)
        if (PlayerController.instance.playerAnimStateInfo.IsName("Attack1")    || PlayerController.instance.playerAnimStateInfo.IsName("Attack2")    || 
            PlayerController.instance.playerAnimStateInfo.IsName("Attack3")    || 
            PlayerController.instance.playerAnimStateInfo.IsName("AirAttack1") || PlayerController.instance.playerAnimStateInfo.IsName("AirAttack2") ||
            PlayerController.instance.playerAnimStateInfo.IsName("Hit"))
        {
            lightSaberHoldTimeCount = lightSaberHoldTime;
        }
        else if(PlayerController.instance.playerAnimStateInfo.IsName("Hang")     || PlayerController.instance.playerAnimStateInfo.IsName("Death") ||
                PlayerController.instance.playerAnimStateInfo.IsName("Recovery") || PlayerController.instance.playerAnimStateInfo.IsName("Scan1") ||
                PlayerController.instance.playerAnimStateInfo.IsName("Scan2"))
        {
            lightSaberHoldTimeCount = 0f;
        }
        
        if (lightSaberHoldTimeCount > 0f && fadeValue1 < maxFadeValue1)
        {
            fadeValue1          += Time.deltaTime * lightSaberOnSpeed;
            lightSaberMaterial.SetFloat(fadePropertyID1, fadeValue1);                                                      
        }
        else if(lightSaberHoldTimeCount <= 0f && fadeValue1 > 0)
        {
            fadeValue1          -= Time.deltaTime * lightSaberOffSpeed;
            lightSaberMaterial.SetFloat(fadePropertyID1, fadeValue1);             
        }
    }
    
    private void Clothes()
    {
        if (PlayerHp.instance && PlayerHp.instance.liveState)
        {
            // 올라가기
            if (isClothesLightUp)
            {
                if (fadeValue2 < maxFadeValue2)
                {
                    fadeValue2 += Time.deltaTime * clothesOnSpeed;
                    clothesMaterial.SetFloat(fadePropertyID2, fadeValue2);
                }
                else
                    isClothesLightUp = false;
            }
            // 내려가기
            else if (!isClothesLightUp)
            {
                if (fadeValue2 > minFadeValue2)
                {
                    fadeValue2 -= Time.deltaTime * clothesOnSpeed;
                    clothesMaterial.SetFloat(fadePropertyID2, fadeValue2);
                }
                else
                    isClothesLightUp = true;
            }
        }
        else
        {
            // 사망시 옷 라이트 빠르게 끄기.(가드/스나보다 4배 빠르게 bright가 꺼짐. -> 15와 10 차이지만, 빠르게 꺼지도록 함.)
            if(fadeValue2 > 0)
            {
                fadeValue2 -= Time.deltaTime * 15f;
                clothesMaterial.SetFloat(fadePropertyID2, fadeValue2);            
            }
        }
    }
}
