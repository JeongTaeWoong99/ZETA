using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class VentLight : MonoBehaviour
{
    private Light2D light2D;
    private float   lgihtIntensity;
    
    [HideInInspector]
    public  bool    isPlayerTouch;     // 플레이어 터치
    [HideInInspector]
    public  bool    isScanCameraTouch; // 플레이어 터치
    [HideInInspector]
    public  bool    isScan;            // 해킹 게이트가 관리하는 라이트(=스캔에 성공하면, 라이트가 완전히 밝아짐.)
    
    public  float   lightSpeed;

    private void Start()
    {
        light2D = GetComponent<Light2D>();
        lgihtIntensity = light2D.intensity; // 저장
        light2D.intensity = 0f;             // 초기화
    }

    private void Update()
    {
        if (isPlayerTouch || isScanCameraTouch || isScan)
        {
            if (light2D.intensity < lgihtIntensity)
            {
                light2D.intensity += lightSpeed * Time.unscaledDeltaTime;
            }
        }
        else
        {
            if (light2D.intensity > 0)
            {
                light2D.intensity -= lightSpeed * Time.unscaledDeltaTime;
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerTouch = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerTouch = false;
        }
    }
}
