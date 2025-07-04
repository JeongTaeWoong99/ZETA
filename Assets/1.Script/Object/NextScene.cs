using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    public static NextScene instance;
    
    [Header("------Common------")] 
    public  string nextSceneName;   // 새로운 게임시작 씬 이름
    public  bool   isMoveAndNext;   // 넘어갈 때, 움직임 유지 or 멈추기
    
    [HideInInspector]
    public bool    isNextSeenMacro; // 다음씬 이동 메크로가 작동 중 인지
    
    public AudioListener seenAudioListener;
    
    [Header("------DemoEnd Seen Transition------")] 
    public bool          isDemoEndSeenTransition;    // 데모끝 트렌시션 이벤트
    public Light2D       whiteFadeLight2D;

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        if (isNextSeenMacro)
        {
            // 걸어가기
            if (isMoveAndNext)
            {
                PlayerController.instance.playerAnim.SetBool("run",true);
                PlayerController.instance.rb2D.velocity = new Vector2(PlayerController.instance.bodyGameObject.transform.localScale.x * PlayerController.instance.activeMoveSpeed, 
                                                                        PlayerController.instance.rb2D.velocity.y);
                PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(PlayerController.instance.bodyGameObject.transform.localScale.x, 1f, 1f);
            }
            // 멈추기
            else
            {
                PlayerController.instance.playerAnim.SetBool("run",false);
                PlayerController.instance.rb2D.velocity = Vector2.zero;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 기본 문
        if (other.CompareTag("Player"))
        {
            isNextSeenMacro = true;

            PlayerAcceleration.instance.isAcceleration = false; // 만약 엑셀 상태라면, 자동으로 해제해주기.
            
            EventController.instance.eventState = true;
            EventController.instance.AllKeyLockTrue();
            
            if(!isDemoEndSeenTransition)
                StartCoroutine(SeenTransition());
            else if (isDemoEndSeenTransition)
                StartCoroutine(DemoEndSeenTransition());
        }
    }
    
    private IEnumerator SeenTransition()
    {
        yield return StartCoroutine(FadeManager.instance.NextSeenFadeIn());
        
        PlayerPrefs.SetInt("currentHP",PlayerHp.instance.currentHealth);            // 현재 체력   저장
        PlayerPrefs.SetFloat("currentGage",UIController.instance.gageSlider.value); // 현재 게이지 저장
        
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator DemoEndSeenTransition()
    {
        EventController.instance.bigGateList[1].SetTrigger("openRgateOn");  // R게이트 열기
        
        yield return new WaitForSeconds(2f);    // 열리는 시간 기다리기
        
        AudioManager.instance.currentAmbientSoundNum = 999;	// BGM 끄기

        isMoveAndNext = true; // 뛰어서 이동
        
        // 오디오 변경(점점 발소리가 멀어지도록! 연출)
        seenAudioListener.enabled                    = true;  // 전용 리스너 켜기
        AudioManager.instance.playerListener.enabled = false; // 플레이어 오디오 끄기
        AudioManager.instance.cameraListener.enabled = false; // 카메라 오디오 끄기
        
        while (true) // 화이트 페이드
        {
            whiteFadeLight2D.intensity += 10 * Time.fixedDeltaTime;
            if (whiteFadeLight2D.intensity > 100f)
            {
                MenuManager.instance.normalMenuBlackWall.gameObject.SetActive(true);    // 검은색 화면 on
                whiteFadeLight2D.intensity = 0;                                         // 라이트 끄기
                break;
            }
            
            yield return new WaitForFixedUpdate();
        }
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(Narration.instance.DemoEndNarration());

        PlayerPrefs.SetInt("DemoClear", 1);     // 데모 클리어시, 메인화면 제타가 사라지도록 하기 위함.
        
        SceneManager.LoadScene(nextSceneName);  // 타이틀 복귀
    }
}