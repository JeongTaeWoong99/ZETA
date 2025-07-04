using System.Collections.Generic;
using UnityEngine;

public class JumpPlatform : MonoBehaviour
{
    public BoxCollider2D platformBox2D;
    public BoxCollider2D hangPlatformBox2D;

    [Header("------Appear------")]
    public List<SpriteRenderer> customMaterialRendererList;
    [HideInInspector]
    public List<Material>       customMaterialList;
    public GameObject            jetGameObject;

    public bool     isAppear;
    [HideInInspector]
    public  int      fadePropertyID;         // 페이드 이름
    [HideInInspector]
    public  float    fadeValue;              // 페이드값
    public  float    maxFadeValue;           // max페이드값(400% -> 4)
    public  float    fadeSpeed;

    private void Start()
    {
        // Appear
        fadePropertyID = Shader.PropertyToID("_DirectionalGlowFadeFade");
        fadeValue      = 0;
        
        foreach (var customMaterialRendererLists in customMaterialRendererList) // 받아오기
            customMaterialList.Add(customMaterialRendererLists.material);

        foreach (var customMaterialLists in customMaterialList)     // 초기화
            customMaterialLists.SetFloat(fadePropertyID, 0f);

        // 젯 끄기
        jetGameObject.SetActive(false);
    }
    
    private void Update()
    {
        // Appear
        Appear();
    }
    
    private void Appear()
    {
        foreach (var customMaterialLists in customMaterialList)
        {
            // 보이기
            if (isAppear && customMaterialLists.GetFloat(fadePropertyID) < maxFadeValue)
            {
                fadeValue += Time.deltaTime * fadeSpeed;
                customMaterialLists.SetFloat(fadePropertyID, fadeValue);
                
                // 1회 실행
                if(customMaterialLists.GetFloat(fadePropertyID) > maxFadeValue)
                    jetGameObject.SetActive(true);
            }
            // 숨기기
            else if (!isAppear && customMaterialLists.GetFloat(fadePropertyID) > -1)
            {
                fadeValue -= Time.deltaTime * fadeSpeed;
                customMaterialLists.SetFloat(fadePropertyID, fadeValue);
            }
        }
    }
    
    public void ActiveJumpPlat()
    {
        isAppear                  = true;
        platformBox2D.enabled     = true;
        hangPlatformBox2D.enabled = true;
        
        AudioManager.instance.EnemySfxCreate(9,true,gameObject);    // 활성화 글로우 페이드 사운드
    }
    
    public void DeactivateJumpPlat()
    {
        isAppear                  = false;
        platformBox2D.enabled     = false;
        hangPlatformBox2D.enabled = false;
        jetGameObject.SetActive(false);
        
        AudioManager.instance.EnemySfxCreate(9,true,gameObject);    // 비성화 글로우 페이드 사운드
    }
    
}
