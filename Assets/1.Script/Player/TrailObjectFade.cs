using System;
using Spine.Unity;
using UnityEngine;

public class TrailObjectFade : MonoBehaviour
{
    public  SkeletonRendererCustomMaterials customMaterials;
    private Animator                        animator;
    
    private AnimatorStateInfo spineTrailAnimStateInfo;   // 애니메이션 정보

    private bool  isFade;         // 페이드 작동
    private int   fadePropertyID; // 페이드 이름
    private float fadeValue;      // 페이드값
    public  float fadeSpeed;      // 사라지는 시간

    private void Start()
    {
        animator        = GetComponent<Animator>();
    
        fadePropertyID = Shader.PropertyToID("_CustomFadeAlpha");

        gameObject.transform.localScale = PlayerController.instance.bodyGameObject.transform.localScale;               // 좌우 설정
        animator.Play(PlayerController.instance.playerAnim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 
            PlayerController.instance.playerAnim.GetCurrentAnimatorStateInfo(0).normalizedTime);               // 애니메이션 설정
        animator.GetComponent<Animator>().speed = 0f;                                                                  // 애니메이션 멈추기
    }
    
    private void FixedUpdate()
    {
        spineTrailAnimStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
        // Idle이 아니면 -> 애니메이션이 Play되고 변경된 것.
        if (!isFade)
        {
            isFade    = true;
            fadeValue = 0.5f;                                                          // 시작 페이드값 0.5f
            customMaterials.changeIndependentMat3.SetFloat(fadePropertyID, fadeValue); // 독립 메터리얼 3번 바디의 개인 메터리얼 값 보이기
        }
        else if (isFade)
        {
            fadeValue -= fadeSpeed * Time.fixedDeltaTime;
            customMaterials.changeIndependentMat3.SetFloat(fadePropertyID, fadeValue); // 독립 메터리얼 3번 바디의 개인 메터리얼 값변환
            
            if (customMaterials.changeIndependentMat3.GetFloat(fadePropertyID) <= 0.01f)
            {
                Destroy(gameObject);
            }
        }

    }
}
