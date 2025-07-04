using System;
using UnityEngine;

public class PickFocusUI : MonoBehaviour
{
    private Animator animator;
    private bool     isTouched;
    
    [Header("------Tutorial ------")] 
    public bool isMoveTutorial;
    
    public bool isEvasionTutorial;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        AudioManager.instance.UISoundPlay(6);   // 포커스 in 사운드
    }

    private void FixedUpdate()
    {
        if (isEvasionTutorial)  // 회피 이벤트 중, 플레이어가 히트를 당하면, 삭제하기.
        {
            if (PlayerHp.instance.isHit)
                DestroyPlayEnd();
        }
    }

    // focusOutOn 애니메이션 마지막에 실행.
    public void DestroyPlayEnd()
    {
        Destroy(gameObject);
    }

    public void DestroyPlaySound()
    {
        AudioManager.instance.UISoundPlay(5);   // 포커스 out 사운드
    }
    
    // 무브 픽
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 무브 픽 트리거
        if (other.CompareTag("Player") && !isTouched)
        {
            isTouched = true;
            animator.SetTrigger("focusOutOn");

            // 초반 이동 이벤트
            if (isMoveTutorial)
            {
                EventController.instance.movePickFocusTouchCount++;

                // 다음 이동 포인트 생성
                if (EventController.instance.movePickFocusTouchCount == 1)
                {
                    Instantiate(EventController.instance.movePickFocusPrefabs, EventController.instance.movePickFocusMakeTransList[1].transform.position, Quaternion.identity);	// 무브 픽 생성
                    EventController.instance.jumpPlatList[1].GetComponent<JumpPlatform>().ActiveJumpPlat();												                        // 발판 생성
                }
                else if (EventController.instance.movePickFocusTouchCount == 2)
                {
                    Instantiate(EventController.instance.movePickFocusPrefabs, EventController.instance.movePickFocusMakeTransList[2].transform.position, Quaternion.identity);	// 무브 픽 생성
                    EventController.instance.jumpPlatList[2].GetComponent<JumpPlatform>().ActiveJumpPlat();												                        // 발판 생성
                    EventController.instance.touchExplanation[2].SetActive(true);                                                                                               // 롱점프 터치 설명
                }
            }
            // 특별한 힘 회피 이벤트
            else if (isEvasionTutorial)
            {
                EventController.instance.isTutorialEvasion = true;
            }
        }
    }
}