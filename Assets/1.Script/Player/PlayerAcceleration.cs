using System;
using UnityEngine;

public class PlayerAcceleration : MonoBehaviour
{
    public static PlayerAcceleration instance;
    
    public GameObject colorFilter;
    [HideInInspector]
    public Material   colorFilterMaterial;
    
    public float      filterChangeSpeed;
    
    [HideInInspector] 
    public bool       isAcceleration;
    
    public float accelerationStatScale;             // 바뀌는 시간값의 최소값 -> 0.2임.
    public float accelerationStartUseGaugeValue;    // 시작을 하면서, 사용하는 게이지값(=레이스 클로킹 시작시 드는 마나와 같은 역할)
    
    public  float accelerationTimePerGage;          // accelerationTimePerGage 시간이 지나면, 게이지가 0.1씩 감소하도록 함.
    private float accelerationTimePerGageCount;
    
    [HideInInspector]
    public float inputAccelerationXtrans;           // S 키를 눌렀을 때의 위치
    [HideInInspector] 
    public float inputAccelerationYtrans;           // S 키를 눌렀을 때의 위치
    
    public float accelerationMinGageValue;          // 사용 최소 비용
    
    [HideInInspector]
    public float accelerationChangedTimeValue;      // 가속에 따라 바뀌는 시간값 -> 실시간 바뀌는 값!!!!!!
    
        
    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        colorFilterMaterial = colorFilter.GetComponent<SpriteRenderer>().material;

        accelerationChangedTimeValue = 1f;                                         // 기본 가속 시간값은 1이다.
    }
    
    private void Update()
    {
        // 입력
        if (Input.GetKeyDown(KeyCode.Q) && PlayerHp.instance.liveState   && !PlayerHp.instance.isHit             && !PlayerHacking.instance.isHacking &&  
            !PlayerScan.instance.isScan && !PlayerHp.instance.isRecovery && !EventController.instance.eventState && !MenuManager.instance.isNormalMenu)
        {
            // 가속 켜기(필터가 완전히 꺼지고 나서, 켤 수 있도록 함.)
            if (!isAcceleration && accelerationChangedTimeValue == 1 && colorFilterMaterial.GetFloat("_Contrast_Intensity") == 2f)
            {
                if (!EventController.instance.accelerationLock && UIController.instance.gageSlider.value >= accelerationMinGageValue)
                {
                    // 상태변경
                    isAcceleration               = true;

                    // 초기화
                    accelerationTimePerGageCount = 0f;
                
                    // 엑셀상태 중 적들이 바라볼 방향 저장
                    float xValue            = PlayerController.instance.transform.position.x;             
                    float yValue            = PlayerController.instance.transform.position.y;             
                    inputAccelerationXtrans = xValue;                                                     
                    inputAccelerationYtrans = yValue;

                    // 시작 사용 게이지 빼기
                    UIController.instance.gageSlider.value -= accelerationStartUseGaugeValue; // 시동값 빼기
                    
                    AudioManager.instance.DirectingPlay(3);

                    AudioManager.instance.currentAmbientSoundNum = 999; // BGM 끄기
                }
                else
                {
                    AudioManager.instance.UISoundPlay(4);   // 스킬 실패 사운드
                }
            }
            // 가속 끄기(필터가 완전히 켜지고 나서, 끌 수 있도록 함.)
            else if (isAcceleration && colorFilterMaterial.GetFloat("_Contrast_Intensity") == 0f)
                AccelerationEnd();
        }
        
        // 필터 및 상태전환 체크
        // 가속 O
        if (isAcceleration)
        {
            // 필터 강조(색 강조)
            colorFilterMaterial.SetFloat("_Contrast_Intensity", 
                Mathf.MoveTowards(colorFilterMaterial.GetFloat("_Contrast_Intensity"), 0f, Time.deltaTime * filterChangeSpeed));
            
            // 게이지 감소
            accelerationTimePerGageCount += Time.deltaTime;
            if (accelerationTimePerGageCount > accelerationTimePerGage)
            {
                accelerationTimePerGageCount            = 0f;   // 초기화
                UIController.instance.gageSlider.value -= 0.1f; // 게이지 1칸 = 0.1감소
            }
            
            // 가속 상태변경 상태 변경
            if (UIController.instance.gageSlider.value <= 0)
                AccelerationEnd();
        }
        // 가속 X(시간종료 or 피격)
        else if(!isAcceleration)
        {
            // 필터(원상복구)
            colorFilterMaterial.SetFloat("_Contrast_Intensity", 
                Mathf.MoveTowards(colorFilterMaterial.GetFloat("_Contrast_Intensity"), 2f, Time.deltaTime * filterChangeSpeed));
        }
    }
    
    private void FixedUpdate()
    {
        // 상태에 따라, accelerationChangedTimeValue값 변경.
        if (isAcceleration && accelerationChangedTimeValue > accelerationStatScale)
        {
            accelerationChangedTimeValue -= Time.fixedDeltaTime * 2f;
            if (accelerationChangedTimeValue < accelerationStatScale)
                accelerationChangedTimeValue = 0.2f;
        }
        else if (!isAcceleration && accelerationChangedTimeValue < 1)
        {
            accelerationChangedTimeValue += Time.fixedDeltaTime * 2f;
            if (accelerationChangedTimeValue > 1)
                accelerationChangedTimeValue = 1;
        }
    }

    public void AccelerationEnd()
    {
        isAcceleration               = false;
        
        AudioManager.instance.currentAmbientSoundNum = EventController.instance.BGMnum; // BGM 복구
    }
}