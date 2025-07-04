using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SurvivalEvent : MonoBehaviour
{
    [Header("------EventStart------")] 
    public Transform eventCameraTrans;
    public Transform startMovePos;
    public Gate      startMoveControlGate;

    [Header("------EnemyCreate------")] 
    public  TilemapRenderer survivalRoomLightRenderer;
    private Material        survivalRoomLightMat;
    public  float           appearSpeed;
    private int             bightnessFadeID;
    private float           currentFadeValue;
    
    public  List<GameObject>     warpEffectList     = new List<GameObject>();
    private List<ParticleSystem> particleSystemList = new List<ParticleSystem>();
    
    public  float enemyCreateInterval;      // 반복 간격
    private float enemyCreateIntervalCount;
    public  int   enemyCreateRepetitions;   // 반복 횟수
    
    [Header("------EventEnd------")] 
    public Gate      endControlGate;

    private void Start()
    {
        survivalRoomLightMat = survivalRoomLightRenderer.material;
        bightnessFadeID      = Shader.PropertyToID("_Brightness");
        survivalRoomLightMat.SetFloat(bightnessFadeID, 0f);
        
        for (int i = 0; i < warpEffectList.Count; i++)
        {
            particleSystemList.Add(warpEffectList[i].GetComponent<ParticleSystem>());    // 넣기
            particleSystemList[i].Stop();                                                       // 바로 멈추기
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 상태 변경
            EventController.instance.eventState = true;
            EventController.instance.AllKeyLockTrue();
            
            PlayerAcceleration.instance.isAcceleration = false; // 만약 엑셀 상태라면, 자동으로 해제해주기.

            // 이벤트 시작
            StartCoroutine(SurvivalEventStart());
        }
    }

    private IEnumerator SurvivalEventStart()
    {
        while (true)
        {
            // 이동
            if (PlayerController.instance.transform.position.x < startMovePos.position.x)
            {
                PlayerController.instance.rb2D.velocity = new Vector2(1 * PlayerController.instance.activeMoveSpeed, PlayerController.instance.rb2D.velocity.y);
                PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                PlayerController.instance.rb2D.velocity = new Vector2(-1 * PlayerController.instance.activeMoveSpeed, PlayerController.instance.rb2D.velocity.y);
                PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
            }       
            PlayerController.instance.playerAnim.SetBool("run", true);      
            
            // 이동 종료
            if (Vector2.Distance(PlayerController.instance.transform.position, startMovePos.position) < 0.25f)
            {
                // 남아있는 이동값 제거
                yield return new WaitForFixedUpdate(); // 이전 입력 겹치기 방지 대기
                PlayerController.instance.rb2D.velocity = Vector2.zero;
                PlayerController.instance.playerAnim.SetBool("run", false);
                
                // 문 닫기(이동이 끝나고, 닫히는 게이트가 있는 경우)
                if (startMoveControlGate)
                {
                    // 완전히 열렸는지 확인(-> 완전히 열린 후 닫기)
                    while (true)
                    {
                        if (startMoveControlGate.gateAnimStateInfo.IsName("Open") && startMoveControlGate.gateAnimStateInfo.normalizedTime > 1f)
                        {
                            startMoveControlGate.anim.SetTrigger("closeOn");                                        // 닫기 
                            AudioManager.instance.ObjectSfxCreate(5,true,startMoveControlGate.gameObject); // close 사운드 생성
                            startMoveControlGate.isEventControl = true;                                                  // startMoveEndCloseGate문이 열리지 않도록 isEventControl로 제어하기     
                            
                            // closeOn 트리거로, 애니메이션이 전환 되었는지 확인하고, 넘어가기
                            while (true)
                            {
                                if(startMoveControlGate.gateAnimStateInfo.IsName("Close"))
	                                break;
                                yield return new WaitForFixedUpdate();
                            }
                            break;
                        }       
                        yield return new WaitForFixedUpdate();
                    }
                    
                    // 완전히 닫혔는지 확인(-> 완전히 닫힌 후 다음으로 넘어가기)
                    while (true)
                    {
                        if (startMoveControlGate.gateAnimStateInfo.IsName("Close") && startMoveControlGate.gateAnimStateInfo.normalizedTime > 1f)
                        {
                            break;
                        }
                        
                        yield return new WaitForFixedUpdate();
                    }
                }
                
                // 카메라 타겟 변경(플레이어 -> 생존 구역 카메라 위치)
                CameraController.instance.target = eventCameraTrans;
                
                // 대사
                yield return StartCoroutine(UIController.instance.DialogUIScale(1,20f,1f,1f,1f,1f,1f)); // L 대화창 보이기
                
                yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                            // 대화 L (0번)
                
                StartCoroutine(UIController.instance.DialogUIScale(1,20f,0f,0f,0f,0f,1f));              // L 대화창 숨기기
                yield return new WaitForSeconds(2f);

                // 대사 종료 후, 상태 변경
                EventController.instance.eventState = false;
                EventController.instance.AllKeyLockFalse();
                
                // 적 생성 코루틴 시작.
                StartCoroutine(EnemyCreateCo());
                break;
            }       
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator EnemyCreateCo()
    {
        // 생존방 라이트 밝기 업
        while (true)
        {
            currentFadeValue += Time.fixedDeltaTime * appearSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (currentFadeValue < 10)
                survivalRoomLightMat.SetFloat(bightnessFadeID, currentFadeValue);    // 라이트
            else
            {
                foreach (var particleSystemLists in particleSystemList) // 워프 이펙트 작동
                    particleSystemLists.Play();

                currentFadeValue = 10f; // 라이트 맞추기
                survivalRoomLightMat.SetFloat(bightnessFadeID, currentFadeValue);    
                
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        // 생존방 적 생성 가동
        int RepetitionsCount     = 0;                                            // 초기화
        enemyCreateIntervalCount = enemyCreateInterval;                          // 초기화(바로 생성되도록)
        int enemyCountCheck      = EnemyDistanceActive.instance.enemyList.Count; // 생성 이벤트가 시작되기 전에 초기의 적의 숫자를 저장해서,
                                                                                 // 추후 생성이 끝나고 초기 적의 숫자와 같아지면, 모든 적을 죽인것임.
        while (true)
        {
            enemyCreateIntervalCount += Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            
            // enemyCreateIntervalCount이 모두 지났고, 생성된 적을 모두 죽여야지,
            // 다음 웨이브로 넘어가도록 함.
            if (enemyCreateIntervalCount > enemyCreateInterval && EnemyDistanceActive.instance.enemyList.Count == enemyCountCheck)
            {
                // 반복 짝수 = 양쪽 스나 + 가운데 드론
                if (RepetitionsCount % 2 == 0)
                {
                    for (var i = 0; i < EnemyGenerator.instance.guardMakeTransList.Count; i++)
                    {
                        StartCoroutine(i % 2 == 0 ? EnemyGenerator.instance.CreateDrone(i) : EnemyGenerator.instance.CreateGuard(i));
                        yield return new WaitForFixedUpdate();
                    }
                }
                // 반복 홀수 = 양쪽 스나 + 가운데 가드
                else
                {
                    for (var i = 0; i < EnemyGenerator.instance.guardMakeTransList.Count; i++)
                    {
                        StartCoroutine(i % 2 == 0 ? EnemyGenerator.instance.CreateSniper(i) : EnemyGenerator.instance.CreateGuard(i));
                        yield return new WaitForFixedUpdate();
                    }
                }
                
                enemyCreateIntervalCount = 0f; // 초기화
                RepetitionsCount++;            // 반복 증가
            }
            
            // 반복 종료.
            if (enemyCreateRepetitions == RepetitionsCount)
            {
                StartCoroutine(EventEndCheck(enemyCountCheck));
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        
        // 생존방 라이트 밝기 다운
        while (true)
        {
            currentFadeValue -= Time.fixedDeltaTime * appearSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (currentFadeValue > 0)
                survivalRoomLightMat.SetFloat(bightnessFadeID, currentFadeValue);    // 라이트
            else
            {
                foreach (var particleSystemLists in particleSystemList) // 워프 이펙트 멈추기
                    particleSystemLists.Stop();
                    
                currentFadeValue = 0f; // 라이트 맞추기
                survivalRoomLightMat.SetFloat(bightnessFadeID, currentFadeValue);    
                
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    
    private IEnumerator EventEndCheck(int enemyCountCheck)
    {
        yield return new WaitForFixedUpdate(); // 리스트 들어가는거 기다리기.
        yield return new WaitForFixedUpdate(); // 리스트 들어가는거 기다리기.
        yield return new WaitForFixedUpdate(); // 리스트 들어가는거 기다리기.
        
        while (true)
        {
            // 초기의 적과 숫자가 같아졌는지 체크.(= 모든 생성된 적을 죽인 것.)
            if (EnemyDistanceActive.instance.enemyList.Count == enemyCountCheck)
            {
                // end게이트 열리기.
                endControlGate.isEnemyControlGate = false;
                endControlGate.anim.SetTrigger("eventOpenOn");
                
                // 카메라 타겟 변경(생존 구역 카메라 위치 -> 플레이어)
                CameraController.instance.target = PlayerController.instance.transform;
                
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
