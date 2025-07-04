using Spine.Unity;
using UnityEngine;

public class EnemyLightController : MonoBehaviour
{
    [Header("------Common------")]
    public EnemyController                 enemyCon;
    public SkeletonRendererCustomMaterials skeletonCustom; // 메터리얼 독립 할당

    [Header("------LightSaber------")]
    public bool  isLightSaber;            // 광선검 여부
    public float lightSaberOnSpeed;       // 광선검 켜지는 속도
    public float lightSaberOffSpeed;      // 광선검 켜지는 속도

    [Header("------Appear------")]
    public  float appearSpeed;
    [HideInInspector]
    public  bool  isAppear = true;       // 등장 페이드(기본 true / 보스전 생성은 만들자마자 false로 변경)
    private int   glowFadeID;
    private float currentFadeValue;
    
    private void Start()
    {
        glowFadeID = Shader.PropertyToID("_FullGlowDissolveFade");
        // 나타나지 않은 상태이면, 모습 안 보이게
        if (!isAppear && skeletonCustom.changeIndependentMat2 && skeletonCustom.changeIndependentMat3)
        {                                                                                   
            skeletonCustom.changeIndependentMat2.SetFloat(glowFadeID, 0f);      // 라이트  
            skeletonCustom.changeIndependentMat3.SetFloat(glowFadeID, 0f);      // 바디
        }
    }

    void Update()
    {
        LightSaberAndBodyLight();
        
        Appear();
    }

    private void Appear()
    {
        // 나타나지 않은 상태라면
        if (!isAppear && skeletonCustom.changeIndependentMat2 && skeletonCustom.changeIndependentMat3)
        {
            currentFadeValue += Time.deltaTime * appearSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (currentFadeValue < 1)
            {
                skeletonCustom.changeIndependentMat2.SetFloat(glowFadeID, currentFadeValue);    // 라이트
                skeletonCustom.changeIndependentMat3.SetFloat(glowFadeID, currentFadeValue);    // 바디
            }
            else if(currentFadeValue >= 1)
            {
                isAppear                = true;                                       // 나타남
                enemyCon.chaseTimeCount = enemyCon.chaseTime;                         // 바로 추격   
                skeletonCustom.changeIndependentMat2.SetFloat(glowFadeID, 1);    // 라이트
                skeletonCustom.changeIndependentMat3.SetFloat(glowFadeID, 1);    // 바디
            }
        }
    }

    private void LightSaberAndBodyLight()
    {
        //바디 라이트 밝기 빠르게 끄기 + 광선검 끄기.(플레이어와 같음.)
        if (enemyCon.enemyAnimStateInfo.IsName("Death")) 
        {
            // 바디 라이트
            skeletonCustom.bightFadeValue -= Time.deltaTime * 10f * PlayerAcceleration.instance.accelerationChangedTimeValue;
            skeletonCustom.changeIndependentMat2.SetFloat(skeletonCustom.fadePropertyID2_Bright, skeletonCustom.bightFadeValue);
            
            // 광선검 off
            skeletonCustom.lightSaberFadeValue -= Time.deltaTime * lightSaberOffSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
            skeletonCustom.changeIndependentMat1.SetFloat(skeletonCustom.fadePropertyID1, skeletonCustom.lightSaberFadeValue);                                                    
        }
        // 광선검 on
        else if (!enemyCon.enemyHp.isStun && enemyCon.isChasePlayer)
        {
            // 광선검 
            if (isLightSaber)
            {
                if (skeletonCustom.lightSaberFadeValue < skeletonCustom.lightSaberMaxFadeValue)
                {
                    skeletonCustom.lightSaberFadeValue += Time.deltaTime * lightSaberOnSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                    skeletonCustom.changeIndependentMat1.SetFloat(skeletonCustom.fadePropertyID1, skeletonCustom.lightSaberFadeValue);                                                 
                }
            }
            
            // 바디 라이트 리플레이스
            if (skeletonCustom.replaceFadeValue < 1f)
            {
                skeletonCustom.replaceFadeValue += Time.deltaTime * 2f * PlayerAcceleration.instance.accelerationChangedTimeValue;
                skeletonCustom.changeIndependentMat2.SetFloat(skeletonCustom.fadePropertyID2_Replace, skeletonCustom.replaceFadeValue);                                                     
            }
            
        }
        // 라이트 off
        else if (enemyCon.enemyHp.isStun || !enemyCon.isChasePlayer)
        {
            // 광선검
            if (isLightSaber)
            {
                if (skeletonCustom.lightSaberFadeValue > 0)
                {
                    skeletonCustom.lightSaberFadeValue -= Time.deltaTime * lightSaberOffSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
                    skeletonCustom.changeIndependentMat1.SetFloat(skeletonCustom.fadePropertyID1, skeletonCustom.lightSaberFadeValue);                                                    
                }
            }
            
            // 바디 라이트 리플레이스
            if (skeletonCustom.replaceFadeValue > 0)
            {
                skeletonCustom.replaceFadeValue -= Time.deltaTime * 2f * PlayerAcceleration.instance.accelerationChangedTimeValue;
                skeletonCustom.changeIndependentMat2.SetFloat(skeletonCustom.fadePropertyID2_Replace, skeletonCustom.replaceFadeValue);                                                     
            }
        }
    }
    
}
