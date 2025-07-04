using System.Collections;
using System.Collections.Generic;
using Controller;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class EventController : MonoBehaviour
{
    public static EventController instance;
	
    [HideInInspector] 
    public bool eventState; // 게임시작 정지장 상태 및 씬 시작 시 이벤트 모드(이동 등)
    
    [Header("------StasisChamber------")] 
    public bool             isStasisChamber;			                // 정체실인지
    
    public  Light2D       		zetaFaceLight;						 	// 스파인 제타 얼굴 강조 라이트
    private float               originZetaFaceLightIntensity;		    // 설정된 오리진 값
    
    public List<GameObject> spineZetaPartList = new List<GameObject>(); // 
    	
    public GameObject       dosRainList; // 도스 레인
    
    public List<Transform>  stasisChamberCameraPos = new List<Transform>(); // 0번 이벤트 전 위치 // 1번 새 게임 시작 후, 위치
    public float            stasisChamberCameraMoveSpeed;
    public float            moveStateCameraSize;							// 게임 시작 후, 카메라 사이즈
    
    public Light2D          customLight;						  		    // 커스텀 복도 라이트
    private float           customLightIntensity;							// 설정된 오리진 값
     
    public Animator         mainCapsuleAnim;
    
    public  List<float>  changeSpeed =new List<float>();				                 // 변화 스피드(0 = 메인화면 라이트 속도, 1 = 바디 내려오는 속도)
    
    [Header("------PerformanceLab------")] 
    public bool isPerformanceLab;             // 시작 이동 후 보스룸 인지

    public List<ElevatorTerminal> elevatorTerminalList = new List<ElevatorTerminal>();
    
    public Transform performanceLabCameraPos;
    public float     performanceLabCameraMoveSpeed;
    
    public List<Transform> labEventTrans = new List<Transform>();	// 이동 위치
    
    [HideInInspector] 
    public int tutorialDashCheckCount;      // 성능실험실 대쉬 체크
    [HideInInspector] 
    public int tutorialDashJumpCheckCount;  // 성능실험실 대쉬 점프 체크
    [HideInInspector] 
    public int tutorialAttack3CheckCount;   // 성능실험실 인풋 체크
    [HideInInspector] 
    public int tutorialDestroyBotCount;     // 파괴된 봇 수 체크
    [HideInInspector] 
    public bool isTutorialEvasion;			// 회피 닿았는지

	//------movePick + Jump Platform------
    public GameObject       movePickFocusPrefabs;
    [HideInInspector] 
    public int              movePickFocusTouchCount;					           // 이동 이벤트 체크
    public List<Transform>  movePickFocusMakeTransList = new List<Transform>();
    
    public List<GameObject> jumpPlatList;
    
    //------TrainingBot------
    public List<GameObject> trainingBotList = new List<GameObject>(); // 봇 리스트
    [HideInInspector] 
    public int              currentAppealBotNum;					  // 활성화 할 봇 넘버
    
    //------PowerUp------
    public  List<GameObject>     powerUpEffectList  = new List<GameObject>();
    private List<ParticleSystem> particleSystemList = new List<ParticleSystem>();
    
    //------Evasion + Side Wall------
    public GameObject     evasionPickFocusPrefabs;
    public List<Animator> wallAnimatorList = new List<Animator>();	// 벽 이동 애니메이션
    public List<Turret>   turretList       = new List<Turret>();	// 사용 터렛
    public TestTerminal   testTerminal;							    // 테스트 터미널
    public PattenTurret   pattenTurret;								// 패턴 터렛 스크립트
    
    //------Siren + Last Event------
    public  Light2D topDownLight2D;
    public  float   changeLightSpeed = 1f;
    public  int     switchNum        = 3;
    private Color   originalColor;

    public EnemyGenerator enemyGenerator;

    public  List<GameObject> tutorialExplanationList = new List<GameObject>();  // 설명 창 리스트
    private int              tutorialExplanationListNum;
    
    [Header("------BossRoom_1F------")] 
    public bool             isBossRoom;               // 시작 이동 후 보스룸 인지
    public Transform        bossRoomCameraPos;
    public float            bossRoomCameraMoveSpeed;

    public List<Animator>   bigGateList = new List<Animator>();

    public List<LaserBeamController> laserBeamControllerList  = new List<LaserBeamController>();

    [Header("------Touch Explanation------")] 
    public List<GameObject> touchExplanation = new List<GameObject>();                     // 무브 터치 이벤트 활성화(정체실 : ← → 방향키 설명 이벤트 // 성능실험실 : 점프, 오르기 등등)
    
    [Header("------SeenStart AutoSave------")]
    public bool isAutoSaveSeen;

    [Header("------SeenStart Dialog------")]
    public bool isSeenStartDialog;
    public int  seenStartDialogLoop;	// 시작 대사 몇번 반복

    [Header("------SeenStart BGM------")] 
    public int BGMnum;

    [Header("------GateControl------")] 
    public Transform  startMovePos;
    public Gate       startMoveControlGate;
    public List<Gate> eventControlGate = new List<Gate>();

    [Header("------KeyLock------")]
    [HideInInspector]
    public bool moveLock;
    [HideInInspector]
    public bool jumpLock;
    [HideInInspector]
    public bool dashLock;
    [HideInInspector]
    public bool attackLock;
    [HideInInspector]
    public bool recoveryLock;
    [HideInInspector]
    public bool scanLock;
    [HideInInspector]
    public bool accelerationLock;
    [HideInInspector]
    public bool hackingLock;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
	    // StasisChamber
        if (isStasisChamber)
        {
	        eventState                   = true;                                             // 이벤트 상태
            AllKeyLockTrue();                                                                // 모든 키 락
            
            PlayerController.instance.playerAnim.SetTrigger("stasisOn");                // 상태 애니메이션 변경
            PlayerController.instance.activeMoveSpeed = PlayerController.instance.walkSpeed; // 속도값 변경
            PlayerController.instance.bodyHighlightLight.gameObject.SetActive(false);		 // 바디 강조 라이트 끄기
            
            // 바디 강조 라이트 초기화
            customLightIntensity  = customLight.intensity;			// 설정값 저장
            customLight.intensity = 0f;

			// 얼굴 스파인 라이트 초기화
            originZetaFaceLightIntensity = zetaFaceLight.intensity; // 설정값 저장
            zetaFaceLight.intensity      = 0f;
            
            // 메인화면 정체실 몸 띄위기
            PlayerController.instance.bodyGameObject.transform.localPosition += new Vector3(0f, 0.3f, 0f);
            
            // 레이어 전환
            PlayerController.instance.sortingGroup.sortingLayerID = 0;	// 디폴트
            PlayerController.instance.sortingGroup.sortingOrder   = 14; // 14번

            // 카메라 이동
            CameraController.instance.target             = stasisChamberCameraPos[0];
            CameraController.instance.transform.position = new Vector3(stasisChamberCameraPos[0].position.x,stasisChamberCameraPos[0].position.y,CameraController.instance.transform.position.z);
            CameraController.instance.moveSpeed          = stasisChamberCameraMoveSpeed;
            
            // 스파인 제타 보이는지 여부(노멀 0 -> 보임 // 데모 클리어 1 > 보이지 않음)
            if (PlayerPrefs.HasKey("DemoClear") && PlayerPrefs.GetInt("DemoClear") == 1)
            {
	            foreach (var spineZetaPartLists in spineZetaPartList)
		            spineZetaPartLists.gameObject.SetActive(false);
            }
            
            // 시작 화면 코루틴
            StartCoroutine(StasisRoomUIProduction());
        }
        // PerformanceLab
        else if (isPerformanceLab)
        {
            originalColor = topDownLight2D.color;											 // 경고라이트 원래 색 저장
			
            PlayerController.instance.activeMoveSpeed = PlayerController.instance.walkSpeed; // 속도값 변경
            PlayerController.instance.playerAnim.SetTrigger("idleSideOn");              // 상태 애니메이션 변경
        }
        // BossRoom
        else if (isBossRoom)
        {
	        PlayerController.instance.activeMoveSpeed = PlayerController.instance.runSpeed;
        }
        // Normal Map
        else
        {
	        PlayerController.instance.activeMoveSpeed = PlayerController.instance.runSpeed;
        }
        
        // 저장된 데이터씬 이름과 현재씬 이름 비교 
        bool isSavedSeen = false;
        string currentSceneName = SceneManager.GetActiveScene().name;
        string savedSeedName    = PlayerPrefs.GetString("SaveSeenName");
        if (currentSceneName == savedSeedName)
	        isSavedSeen = true;
	    else
	        isSavedSeen = false;

		// 저장된 터미널 찾기
		// 저장된 데이터씬 이름과 현재씬의 이름이 같다면,
        GameObject saveTerminal = null;
        if (isSavedSeen)
        {
	        foreach (var saveTerminalLists in SaveManager.instance.saveTerminalList)
	        {
		        if (saveTerminalLists.name == PlayerPrefs.GetString("SaveTerminalName"))
			        saveTerminal = saveTerminalLists;
	        }
        }
		
		// 세이브된 터미널이 없으면, 지정 위치 뛰어가기 이동
		// (노멀맵에서 세이브를 하지 않은 경우 + 성능실험실 + 보스룸)
        if (startMovePos && saveTerminal == null)
        {
            eventState     = true;
            StartCoroutine(StartMove());
        }
        // 세이브된 터미널이 있으면, 세이브된 터미널로 이동
        // (노멀맵에서 세이브를 한 경우)
        else if(startMovePos && saveTerminal != null)
        {
	        eventState     = true;
	        StartCoroutine(SavedTransMove(saveTerminal));
        }
        // 세이브된 터미널이 없고, 지정된 위치가 없는 경우
        // (메인메뉴 시작의 경우)
        // 아무것도 하지 않음.
        
        // 파워업
        for (int i = 0; i < powerUpEffectList.Count; i++)
        {
	        particleSystemList.Add(powerUpEffectList[i].GetComponent<ParticleSystem>());    // 넣기
	        particleSystemList[i].Stop();                                                       // 바로 멈추기
        }
    }
	
    // ----------------------------Common----------------------------
    private IEnumerator SavedTransMove(GameObject saveTerminal)
    {
		// 카메라 및 플레이어 위치 이동(designatedLocation위치로 이동 // 타겟변경)
		PlayerController.instance.gameObject.transform.position       = saveTerminal.GetComponent<SaveTerminal>().designatedLocation.transform.position;	// 위치
		PlayerController.instance.bodyGameObject.transform.localScale = saveTerminal.GetComponent<SaveTerminal>().designatedLocation.transform.localScale;  // 보는 방향
		CameraController.instance.gameObject.transform.position = new Vector3(PlayerController.instance.gameObject.transform.position.x,PlayerController.instance.gameObject.transform.position.y + 2f,-10f);
		CameraController.instance.target = PlayerController.instance.transform;
		
		// 제어 게이트가 있는 경우(닫아버리기)
		if(startMoveControlGate)
			startMoveControlGate.isEventControl = true;                    // startMoveEndCloseGate문이 열리지 않도록 isEventControl로 제어하기     
    
	    // 페이드 대기
	    yield return new WaitForFixedUpdate();
	    while (true)
	    {
		    if(!FadeManager.instance.isFadeActiveState)
			    break;
            
		    yield return new WaitForFixedUpdate();
	    }
	    
	    // 상태 변경
	    eventState = false;     
	    AllKeyLockFalse();                                                      
	    
	    // 플레이어 UI 켜기
	    UIController.instance.UISeeState(true);
	    
		AudioManager.instance.currentAmbientSoundNum = BGMnum;	  // BGM재생
	    
	    startMoveControlGate.anim.SetTrigger("baseCloseOn"); // 닫기 
    }
    
    private IEnumerator StartMove()
    {
        // 페이드 대기
        yield return new WaitForFixedUpdate();
        while (true)
        {
            if(!FadeManager.instance.isFadeActiveState)
                break;
            
            yield return new WaitForFixedUpdate();
        }

		// 보스룸의 경우, 이동 전 이벤트
		// 문열기 및 레이어 변경
        if (isBossRoom)
        {
			// L 빅 게이트 열기
	        bigGateList[0].SetTrigger("openLgateOn");
	        
	        yield return new WaitForSeconds(1.5f);
        }

        bool isStartMoveState = true; // 이동이 끝나면 false
        while (isStartMoveState)
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
                isStartMoveState = false; // 시작 이동 종료       
                // 남아있는 이동값 제거
                yield return new WaitForFixedUpdate();
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
                
                // 시작 이동을 완료하고, 재생될 BGM
                // 보스룸은 등장을 다 하고, 패턴이 시작될 때, BGM이 재생되도록 함.
                if(!isBossRoom)
	                AudioManager.instance.currentAmbientSoundNum = BGMnum;	// BGM재생
                
                // 성능실험실 : 시작이동 종료 후, 이벤트 순서에 맞추서, 제어권 각각 부여
                if (isPerformanceLab)
                {
                    StartCoroutine(PerformanceLabEvent());
                }
                // 보스룸 : 시작이동 종료 후 보스룸이면, 보스 등장 이벤트 발생
                else if (isBossRoom)
                {
                    StartCoroutine(BossRoomStartEvent());
                }
                // 일반룸 : 시작이동 종료 후 자유이동
                else
                {
	                // 이동 후, 대사가 있는 경우
                    if (isSeenStartDialog)
                    {
	                    yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
        
	                    for (int i = 0; i < seenStartDialogLoop; i++)																															 // seenStartDialogLoop만큼 반복
							yield return StartCoroutine(UIController.instance.Dialog(0));                                                                                                    
		                    
	                    yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f)); // R 대화창 숨기기
                    }
                    
					// 이동 종료 후, 오토 세이브 하는 씬의 경우, 씬 이름 저장.
					// 성능실험실(튜토리얼)이 끝나고, 2B에 처음 들어왔을 때
                    if (isAutoSaveSeen)
                    {
	                    // 현재씬 이름 플레이어프리펩 저장
	                    string currentSceneName = SceneManager.GetActiveScene().name;
	                    PlayerPrefs.SetString("SaveSeenName",currentSceneName);
                    }
                    
                    // 카메라 타겟 변경(시작위치 -> 플레이어)
                    CameraController.instance.target = PlayerController.instance.transform;     
                    
                    // 타이틀 보이기 및 사라지기(코루틴 기다리기)
                    StartCoroutine(UIController.instance.TitleActiveCoroutine());
                    
                    // 플레이어 UI 켜기
                    UIController.instance.UISeeState(true);
                    
                    // 상태 변경
                    eventState = false;
                }
            }       
            yield return new WaitForFixedUpdate();
        }
    }

    // ----------------------------StasisChamber----------------------------
    private IEnumerator StasisRoomUIProduction()
    {
	    // 1. 스파인 러프 페이스 라이트 밝히기 + BGM 재생
		while (true)
		{
			zetaFaceLight.intensity = Mathf.MoveTowards(zetaFaceLight.intensity , originZetaFaceLightIntensity, (0.6f * Time.deltaTime));
			
			if (zetaFaceLight.intensity >= originZetaFaceLightIntensity)
		    {
		        zetaFaceLight.intensity = originZetaFaceLightIntensity;
		        AudioManager.instance.currentAmbientSoundNum = 1;						// BGM재생
				break;
			}
			yield return null;
		}
		// 2. 파티클 로고 켜기
		MenuManager.instance.titleParticleFXObject.gameObject.SetActive(true);  // 파티클 로고

		yield return new WaitForSeconds(4f);
		
		// 3. OFF 이미지 서서히 보이기(+타이틀 라인)
		foreach (var stasisRoomMenuOffImageLists in MenuManager.instance.stasisRoomMenuOffImageList)
			stasisRoomMenuOffImageLists.gameObject.SetActive(true);
		yield return StartCoroutine(MenuManager.instance.OffImageSlowSee(true,2f));

		// 4. 포커스 UI 열리기 
		MenuManager.instance.focusGameObject.gameObject.SetActive(true);      // 포커스 켜기
		
		StartCoroutine(MenuManager.instance.FocusUIScale(MenuManager.instance.focusRectTransformList[0],20f,1f,1f)); // 포커스 메인 키우기(+ 기다리기)
		StartCoroutine(MenuManager.instance.FocusUIScale(MenuManager.instance.focusRectTransformList[1],20f,1f,1f)); // 포커스 서브 키우기
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(MenuManager.instance.FocusUIScale(MenuManager.instance.focusRectTransformList[2],20f,1f,1f)); // 포커스 서브 키우기
		yield return new WaitForSeconds(0.1f);
		AudioManager.instance.UISoundPlay(2); // 1회만 재생되도록 함.(어짜피 재생 중, 실행되면 자동으로 종료되고, 다시 재생함.)
		yield return StartCoroutine(MenuManager.instance.FocusUIScale(MenuManager.instance.focusRectTransformList[3],20f,1f,1f)); // 포커스 서브 키우기(+ 기다리기)
		
		// 5. 나머지 UI 켜기 및 제어권 부여
		MenuManager.instance.stasisRoomMenuOnImageList[0].gameObject.SetActive(true);				   // 0번 강조 켜기
		MenuManager.instance.stasisRoomMenuOffImageList[0].gameObject.SetActive(false);				   // 0번 비강조 끄기
		
		MenuManager.instance.versionText.gameObject.SetActive(true);						  		   // Version 텍스트 켜기
		
		MenuManager.instance.mainFrame.gameObject.SetActive(true);									   // 메인 프레임 보이기
		MenuManager.instance.menuKeyExUI.SetActive(true);		                                       // 메인창 키 설명 UI 보이기
		
		MenuManager.instance.isStasisRoomMenu = true;						                           // 메인 메뉴 조작권 부여				                   
	}
	
    public IEnumerator NewStory()
	{
		// 순서 0 : 저장된 정보 초기화
		MenuManager.instance.ResetSaveData();
		
		// 순서 1 : 나머지 UI 끄기 및 제어권 뺏기
		MenuManager.instance.isStasisRoomMenu = false;
		
		MenuManager.instance.versionText.gameObject.SetActive(false);     // Version 텍스트 끄기
		MenuManager.instance.mainFrame.gameObject.SetActive(false);		  // 메인 프레임 끄기
		MenuManager.instance.menuKeyExUI.SetActive(false);		          // 메인창 키 설명 UI 끄기
		
		// 순서 2 : 포커스 UI 닫히기
		StartCoroutine(MenuManager.instance.FocusUIScale(MenuManager.instance.focusRectTransformList[1],20f,0f,1f)); // 포커스 서브 작아지기
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(MenuManager.instance.FocusUIScale(MenuManager.instance.focusRectTransformList[2],20f,0f,1f)); // 포커스 서브 작아지기
		yield return new WaitForSeconds(0.1f);
		StartCoroutine(MenuManager.instance.FocusUIScale(MenuManager.instance.focusRectTransformList[3],20f,0f,1f)); // 포커스 서브 작아지기(+ 기다리기)
		AudioManager.instance.UISoundPlay(3); // 1회만 재생되도록 함.(어짜피 재생 중, 실행되면 자동으로 종료되고, 다시 재생함.)
		yield return StartCoroutine(MenuManager.instance.FocusUIScale(MenuManager.instance.focusRectTransformList[0],20f,0f,1f)); // 포커스 메인 작아지기(+ 기다리기)

		// 순서 3 : 모든 텍스트 이미지 사라지기(+새 게임 강조 이미지 포함)
		MenuManager.instance.titleQuadLogoAnim.SetTrigger("FadeOut");
		yield return StartCoroutine(MenuManager.instance.OffImageSlowSee(false,2f));
		
		yield return new WaitForSeconds(2f);
		
		// 순서 4 : 로봇의 3원칙(종료 후 다음으로 넘어감) + 카메라 전환 + + BGM 종료
		AudioManager.instance.currentAmbientSoundNum = 999;			      // BGM 종료
		yield return StartCoroutine(Narration.instance.ThreeLaws());
		
		yield return new WaitForSeconds(0.5f);

		// 순서 5 : 나레이션 켄버스 크기 변경 + 카메라 전환 및 제타 정보창(종료 후 다음으로 넘어감)
		Narration.instance.GetComponent<RectTransform>().position   = new Vector3(-1.5f, -1f, 0f);
		Narration.instance.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 0.01f, 0.01f);
		
		yield return StartCoroutine(Narration.instance.Information());
		
		// 순서 6 : 정지장 이벤트
		while (true)																	
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				// 제타 정보창 없애기 
				Narration.instance.DisableInformation();
				
				// 설명창 off 및 캡슐 문 열기
				mainCapsuleAnim.SetTrigger("openOn");												 // 캡슐 on 애니 트리거
				AudioManager.instance.ObjectSfxCreate(4,false,mainCapsuleAnim.gameObject); // open 사운드 생성

				yield return new WaitForSeconds(3f);									 // 캡슐 문 다 내려오기 대기시간
				
				// 레이어 전환
				PlayerController.instance.sortingGroup.sortingLayerID = -1181832937;	 // 플레이어 레이어 전환
				PlayerController.instance.sortingGroup.sortingOrder   = 0;               // 0번

				// 애니메이션 타이밍에 맞춰서, 같이 넘어가면서 실행되도록.
				while (true)
				{
					float normalizedTime = PlayerController.instance.playerAnimStateInfo.normalizedTime % 1;
					if (PlayerController.instance.playerAnimStateInfo.IsName("Stasis") && normalizedTime < 0.05f || normalizedTime > 0.95f)
					{
						// 몸 내려오기 및 애니메이션 전환
						StartCoroutine(BodyDown());										 // 몸 내려오기
						PlayerController.instance.playerAnim.SetTrigger("stasisOff");       // 정체모드 모드 off

						break;
					}
					
					yield return new WaitForFixedUpdate();
				}
				
				// BodyDown이 끝 체크(Input 체크 while문 벗어나기)
				while (true)
				{
					if (PlayerController.instance.playerAnimStateInfo.IsName("IdleSide"))
						break;

					yield return new WaitForFixedUpdate();
				}

				break;
			}
			
			yield return null;
		}
		
		// 7. 주변 밝히기
		yield return StartCoroutine(LightsReachedTarget()); // 빛 밝게
		
		// 8. 카메라 포커스 변경
		CameraController.instance.target    = PlayerController.instance.transform;      // 카메라 플레이어 따라가기
		yield return new WaitForSeconds(2f);
		
		// 9. 이동 대화창
		yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
		yield return new WaitForSeconds(1f);
		
		yield return StartCoroutine(UIController.instance.Dialog(0));	// 대화 0번 
		yield return StartCoroutine(UIController.instance.Dialog(0));	// 대화 1번 
		
		yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f)); // R 대화창 숨기기
		
		// 10 . 타이틀 보이기 및 사라지기(코루틴 기다리기) + 좌우 이동 안내 이벤트 활성화
		yield return StartCoroutine(UIController.instance.TitleActiveCoroutine()); // 스테이지 코루틴
		
		touchExplanation[0].SetActive(true);		
		
		// 11. 제어권(이동만 가능) + 터치 이벤트 활성화
		moveLock                     = false;			           // 이동 락 풀기 -> 이동 가능
		eventState                   = false;	                   // 이벤트 상태 풀기
	}

    private IEnumerator LightsReachedTarget()
	{
		while (true)
		{
			customLight.intensity = Mathf.MoveTowards(customLight.intensity, customLightIntensity, changeSpeed[0] * Time.deltaTime);
	
			if (Mathf.Abs(customLight.intensity - customLightIntensity) < 0.1f)
			{
				AudioManager.instance.currentAmbientSoundNum = 0;	// 정체실 엔비언트 사운드 재생
				break;
			}

			yield return null;
		}
	}
	
	private IEnumerator BodyDown()
	{
		while (true)
		{
			// Y위치 이동
			PlayerController.instance.bodyGameObject.transform.localPosition = Vector3.MoveTowards(PlayerController.instance.bodyGameObject.transform.localPosition, 
																									new Vector3(0f,0f,0f), (changeSpeed[1]) * Time.deltaTime);
			
			// 내려오기 끝 체크
			if (Mathf.Abs(PlayerController.instance.bodyGameObject.transform.localPosition.y - 0f) < 0.01f &&
			    PlayerController.instance.playerAnimStateInfo.IsName("IdleSide"))
			{
				break;
			}
			
			yield return null;
		}
	}
	
	// ----------------------------PerformanceLab----------------------------
    private IEnumerator PerformanceLabEvent()
    {
		// 입장 ~ 기초 이동능력 시험(점프 / 코너 오르기 / 롱점프)
	    yield return StartCoroutine(PerformanceEvent1());
	    
	    // 심화 이동능력 시험(대쉬 / 대쉬 점프)
	    yield return StartCoroutine(PerformanceEvent2());
	    
	    // 기초 전투능력 시험(공격 / 대쉬 공격)
	    yield return StartCoroutine(PerformanceEvent3());
	    
	    // [특별한 힘]봉인 해체 + 게이지 채우기 + 1차 회피 이벤트 체크 + 2차 회피 이벤트 체크 
	    yield return StartCoroutine(PerformanceEvent4());
	    
	    // 지정 위치 이동 + 침입 이벤트 발생 + 드론 생성 + 드론 파괴
	    yield return StartCoroutine(PerformanceEvent5());
    }

    private IEnumerator PerformanceEvent1()
    {
	    // 순서 1 : 엘리베이터 타고 내려가기.
		AudioManager.instance.ObjectSfxCreate(1,true,gameObject);       // 승상기 On사운드 재생(1회 재생)
		yield return StartCoroutine(elevatorTerminalList[0].Move(-1f)); // 승강기 이동
		
		// 순서 2 : 카메라 이동 + 지정된 위치로 이동
        CameraController.instance.target    = performanceLabCameraPos;
        CameraController.instance.moveSpeed = performanceLabCameraMoveSpeed;

        yield return StartCoroutine(EventMove(0));	// 입장 위치 0번
        
        // 순서 3 : 타이틀 보이기 및 사라지기(코루틴 기다리기)
        yield return StartCoroutine(UIController.instance.TitleActiveCoroutine());
        
        // 순서 4 : 대화 이벤트(입장 ~ 기초 이동능력 실험)
        yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
        yield return StartCoroutine(UIController.instance.Dialog(0));                                                                                            // 대화 R (0번)
        
        yield return StartCoroutine(UIController.instance.DialogUIScale(1,20f,1f,1f,1f,1f,1f)); // L 대화창 보이기
        yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                            // 대화 L (1번)
        
        for (int i = 0; i < 4; i++)																																				  // 대화 R 4번 반복(2~5번까지)
	        yield return StartCoroutine(UIController.instance.Dialog(0));                                                                                                    
		
        PlayerController.instance.playerAnim.SetTrigger("idleOn");				   // 구속 해제.
        AudioManager.instance.PlayerSfxCreate(12,false);
        
        while (true)
        {
	        if (PlayerController.instance.playerAnimStateInfo.IsName("Idle"))
	        {
		        break;
	        }
	        yield return new WaitForFixedUpdate();
        }
        
        for (int i = 0; i < 4; i++)																																				  // 대화 R 3번 반복(6~9번까지)
	        yield return StartCoroutine(UIController.instance.Dialog(0));                                                                                                 
        
        StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
        StartCoroutine(UIController.instance.DialogUIScale(1,20f,0f,0f,0f,0f,1f));              // L 대화창 숨기기
        yield return new WaitForSeconds(2f);
        
        // 순서 5 : 임무창 보이기 + 임무 수행
        yield return StartCoroutine(UIController.instance.MissionUIScale(UIController.instance.missionHeadFrameRect,10f,1f,1f));   // 해드 보이기 미션창 보이기
        yield return StartCoroutine(UIController.instance.MissionUIScale(UIController.instance.missionBodyFrameRect,10f,1f,0.5f)); // 바디 보이기 미션창 보이기(미션이 1개이니 0.5만 키우기)
		
		UIController.instance.missionCheckBoxList[0].gameObject.SetActive(true);
		yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(UIController.instance.missionString[0],UIController.instance.missionTextList[0],false,false,false,0.025f)); // 0번 텍스트에 미션 0번

        UIController.instance.UISeeState(true); // UI보이기
        
        touchExplanation[0].SetActive(true);	// 터치 설명
        touchExplanation[1].SetActive(true);    // 터치 설명

        Instantiate(movePickFocusPrefabs, movePickFocusMakeTransList[0].transform.position, Quaternion.identity); // 무브 픽 생성
        jumpPlatList[0].GetComponent<JumpPlatform>().ActiveJumpPlat();											  // 발판 생성
                
        eventState = false;	// 상태 전환
                
        moveLock   = false; // 이동			
        jumpLock   = false; // 점프
                
        PlayerController.instance.activeMoveSpeed = PlayerController.instance.runSpeed; // 속도 변경
        
        while (true)
        {
	        // 무스 픽 터치 체크
	        if(movePickFocusTouchCount < 3)															   // 3번 달성 전 지속적으롤 갱신
	            UIController.instance.missionTextList[0].text = UIController.instance.missionString[0] + " " + movePickFocusTouchCount + " / 3"; 
	        else if (movePickFocusTouchCount == 3 && !UIController.instance.missionCheckBoxList[0].isOn) // 3번 달성(1회 실행)
	        {
	            UIController.instance.missionTextList[0].text     = UIController.instance.missionString[0] + " " + movePickFocusTouchCount + " / 3";
	            UIController.instance.MissionCompleteSettings(0);
	        }
	        
	        if (movePickFocusTouchCount >= 3)
            {
	            // 상태 변경
                UIController.instance.UISeeState(false);
                
                eventState = true;
                AllKeyLockTrue();

                touchExplanation[0].SetActive(false);	// 터치 설명
                touchExplanation[1].SetActive(false);   // 터치 설명
                touchExplanation[2].SetActive(false);   // 터치 설명

                // 남아있는 이동값 제거 및 상태 변경
                yield return new WaitForFixedUpdate();
                PlayerController.instance.rb2D.velocity = new Vector2(0f,PlayerController.instance.rb2D.velocity.y);  // 남은 이동값 제거
                PlayerController.instance.playerAnim.SetBool("run",false);                                    // 달리기 중이면 잠금
                
                yield return new WaitForSeconds(1f);
	            
                // 미션 내용 초기화 + 텍스트 토글 사라지기 + 미션창 바디 접기
                yield return StartCoroutine(UIController.instance.MissionResetSettings());
                
                break;
            }
	        yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator PerformanceEvent2()
    {
	    // 순서 1 : 대화 이벤트(심화 이동능력 시험)
	    yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
        
	    for (int i = 0; i < 3; i++)																																				  // 대화 R 3번 반복(9~11번까지)
		    yield return StartCoroutine(UIController.instance.Dialog(0));                                                                                                    
	    
	    StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
	    yield return new WaitForSeconds(2f);
	    
	    // 순서 2 : 임무창 보이기 + 임무 수행
	    yield return StartCoroutine(UIController.instance.MissionUIScale(UIController.instance.missionBodyFrameRect,10f,1f,1f)); // 바디 보이기 미션창 보이기(미션이 2개이니 1만큼 키우기)
	    UIController.instance.missionCheckBoxList[0].gameObject.SetActive(true);
	    UIController.instance.missionCheckBoxList[1].gameObject.SetActive(true);
	    StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(UIController.instance.missionString[1],UIController.instance.missionTextList[0],false,false,false,0.025f)); // 0번 텍스트에 미션 0번
	    yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(UIController.instance.missionString[2],UIController.instance.missionTextList[1],false,false,false,0.025f)); // 0번 텍스트에 미션 0번

	    UIController.instance.UISeeState(true); // UI보이기
                
	    eventState = false;	// 상태 전환
                
	    moveLock   = false; // 이동			
	    jumpLock   = false; // 점프
	    dashLock   = false; // 대쉬
	    
	    while (true)
        {
	        // 대쉬 체크
	        if(tutorialDashCheckCount < 3)															   // 3번 달성 전 지속적으롤 갱신
	            UIController.instance.missionTextList[0].text = UIController.instance.missionString[1] + " " + tutorialDashCheckCount + " / 3"; 
	        else if (tutorialDashCheckCount == 3 && !UIController.instance.missionCheckBoxList[0].isOn) // 3번 달성(1회 실행)
	        {
	            UIController.instance.missionTextList[0].text     = UIController.instance.missionString[1] + " " + tutorialDashCheckCount + " / 3";
	            UIController.instance.MissionCompleteSettings(0);
	        }
	        
	        // 대쉬 점프 체크
	        if(tutorialDashJumpCheckCount < 3)															   // 3번 달성 전 지속적으롤 갱신
		        UIController.instance.missionTextList[1].text = UIController.instance.missionString[2] + " " + tutorialDashJumpCheckCount + " / 3"; 
	        else if (tutorialDashJumpCheckCount == 3 && !UIController.instance.missionCheckBoxList[1].isOn) // 3번 달성(1회 실행)
	        { 
		        UIController.instance.missionTextList[1].text     = UIController.instance.missionString[2] + " " + tutorialDashJumpCheckCount + " / 3";
		        UIController.instance.MissionCompleteSettings(1);
	        }
	        
	        if (tutorialDashCheckCount >= 3 && tutorialDashJumpCheckCount >= 3)
            {
	            // 상태 변경
                UIController.instance.UISeeState(false);
                
                eventState = true;
                AllKeyLockTrue();
                
                yield return new WaitForSeconds(1f);

                // 남아있는 이동값 제거 및 상태 변경
                yield return new WaitForFixedUpdate();
                PlayerController.instance.rb2D.velocity = new Vector2(0f,PlayerController.instance.rb2D.velocity.y);  // 남은 이동값 제거
                PlayerController.instance.playerAnim.SetBool("run",false);                                    // 달리기 중이면 잠금
                
                yield return new WaitForSeconds(1f);
	            
                // 미션 내용 초기화 + 텍스트 토글 사라지기 + 미션창 바디 접기
                yield return StartCoroutine(UIController.instance.MissionResetSettings());
                
                break;
            }
	        yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator PerformanceEvent3()
    {
	    // 순서 1 : 대화 이벤트(기초 전투능력 시험)
	    yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
        
	    for (int i = 0; i < 5; i++)																																				  // 대화 R 5번 반복(12~16번까지)
		    yield return StartCoroutine(UIController.instance.Dialog(0));                                                                                                    
	    
	    StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
	    yield return new WaitForSeconds(2f);
	    
	    // 순서 2 : 임무창 보이기 + 임무 수행
	    yield return StartCoroutine(UIController.instance.MissionUIScale(UIController.instance.missionBodyFrameRect,10f,1f,1f)); // 바디 보이기 미션창 보이기(미션이 2개이니 1만큼 키우기)
	    UIController.instance.missionCheckBoxList[0].gameObject.SetActive(true);
	    UIController.instance.missionCheckBoxList[1].gameObject.SetActive(true);
	    StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(UIController.instance.missionString[3],UIController.instance.missionTextList[0],false,false,false,0.025f)); // 0번 텍스트에 미션 0번
	    yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(UIController.instance.missionString[4],UIController.instance.missionTextList[1],false,false,false,0.025f)); // 0번 텍스트에 미션 0번

	    UIController.instance.UISeeState(true); // UI보이기
                
	    eventState = false;	// 상태 전환
                
	    moveLock   = false; // 이동			
	    jumpLock   = false; // 점프
	    dashLock   = false; // 대쉬
	    attackLock = false; // 공격
	    
	    trainingBotList[currentAppealBotNum].GetComponent<TrainingBot>().CreatBot();	// 봇 활성화
		
	    while (true)
        {
	        // 사망 봇 숫자 체크
	        if(tutorialDestroyBotCount < 3)															   // 3번 달성 전 지속적으롤 갱신
	            UIController.instance.missionTextList[0].text = UIController.instance.missionString[3] + " " + tutorialDestroyBotCount + " / 3"; 
	        else if (tutorialDestroyBotCount == 3 && !UIController.instance.missionCheckBoxList[0].isOn) // 3번 달성(1회 실행)
	        {
	            UIController.instance.missionTextList[0].text     = UIController.instance.missionString[3] + " " + tutorialDestroyBotCount + " / 3";
	            UIController.instance.MissionCompleteSettings(0);
	        }
	        
	        // 대쉬 공격 체크
	        if(tutorialAttack3CheckCount < 3)															   // 3번 달성 전 지속적으롤 갱신
		        UIController.instance.missionTextList[1].text = UIController.instance.missionString[4] + " " + tutorialAttack3CheckCount + " / 3"; 
	        else if (tutorialAttack3CheckCount == 3 && !UIController.instance.missionCheckBoxList[1].isOn) // 3번 달성(1회 실행)
	        { 
		        UIController.instance.missionTextList[1].text     = UIController.instance.missionString[4] + " " + tutorialAttack3CheckCount + " / 3";
		        UIController.instance.MissionCompleteSettings(1);
	        }
	        
	        if (tutorialDestroyBotCount >= 3 && tutorialAttack3CheckCount >= 3)
            {
	            // 상태 변경
                UIController.instance.UISeeState(false);
                
                eventState = true;
                AllKeyLockTrue();

                // 남아있는 이동값 제거 및 상태 변경
                yield return new WaitForFixedUpdate();
                PlayerController.instance.rb2D.velocity = new Vector2(0f,PlayerController.instance.rb2D.velocity.y);  // 남은 이동값 제거
                PlayerController.instance.playerAnim.SetBool("run",false);                                    // 달리기 중이면 잠금
                
                yield return new WaitForSeconds(1f);
	            
                // 미션 내용 초기화 + 텍스트 토글 사라지기 + 미션창 바디 접기
                yield return StartCoroutine(UIController.instance.MissionResetSettings());
                
                break;
            }
	        yield return new WaitForFixedUpdate();
        }
    }
	
    private IEnumerator PerformanceEvent4()
    {
		// 중앙 이동
	    yield return StartCoroutine(EventMove(1));
    
	    // 순서 1 : 대화 이벤트(전반부)
	    yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
	    
	    for (var i = 0; i < 4; i++)	// 대화 R 4번 반복(17~20)
			yield return StartCoroutine(UIController.instance.Dialog(0));
	    
	    yield return StartCoroutine(UIController.instance.DialogUIScale(1,20f,1f,1f,1f,1f,1f));  // L 대화창 보이기
	    
	    yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                             // 대화 L (21번)
	    
	    for (var i = 0; i < 4; i++)	// 대화 R 5번 반복(22~26)
			yield return StartCoroutine(UIController.instance.Dialog(0));
	    
	    StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
	    StartCoroutine(UIController.instance.DialogUIScale(1,20f,0f,0f,0f,0f,1f));              // L 대화창 숨기기
	    yield return new WaitForSeconds(2f);
	    
	    // 순서 2 : 파워업
	    UIController.instance.UISeeState(true); // UI보이기
	    
	    PlayerController.instance.playerAnim.SetTrigger("powerUpOn");
	    AudioManager.instance.PlayerSfxCreate(11,false);

	    UIController.instance.gageSlider.maxValue = UIController.instance.gageSliderMaxvalue; // 게이지 최대값 변경
	    float gageTimeCount                       = 0;
	    float accelerationTimePerGage             = 0.2f;	// accelerationTimePerGage시간 당, 게이지가 0.1칸씩 참.
		
	    while (true)
	    {
		    if (PlayerController.instance.playerAnimStateInfo.IsName("PowerUp"))
		    {
			    // 파워업 이펙트 재생
			    foreach (var particleSystemLists in particleSystemList)
				    particleSystemLists.Play();
			    break;
		    }
		    yield return new WaitForFixedUpdate();
	    }
	    
	    while (true)
	    {
		    // 게이지 증가
		    gageTimeCount += Time.fixedDeltaTime;
		    if (gageTimeCount > accelerationTimePerGage)
		    {
			    gageTimeCount                           = 0f;   // 초기화
			    UIController.instance.gageSlider.value += 0.1f; // 게이지 0.1칸씩 증가.
		    }

			// 게이지 모두 차면, 나가기
		    if (UIController.instance.gageSlider.value >= UIController.instance.gageSlider.maxValue)
		    {
				PlayerController.instance.playerAnim.SetTrigger("powerUpOff");
				// 파워업 이펙트 멈추기
				foreach (var particleSystemLists in particleSystemList)
					particleSystemLists.Stop();
				break;
		    }
		    yield return new WaitForFixedUpdate();
	    }
	    
	    while (true)
	    {
		    if (PlayerController.instance.playerAnimStateInfo.IsName("Idle"))
			    break;
		    yield return new WaitForFixedUpdate();
	    }
	    
	    // 순서 3 : 대화 이벤트(후반부-Q키 설명)
	    yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
	    
	    for (var i = 0; i < 3; i++)	// 대화 R 4번 반복(28~30)
		    yield return StartCoroutine(UIController.instance.Dialog(0));
	    
	    AudioManager.instance.DirectingPlay(9);				// 열리기 사운드
	    wallAnimatorList[0].SetTrigger("leftOpenOn");	// 벽 열리기
		wallAnimatorList[1].SetTrigger("rightOpenOn"); // 벽 열리기

		foreach (var trainingBotLists in trainingBotList) // 모든 봇 활성화
		{
			trainingBotLists.GetComponent<TrainingBot>().CreatBot();
			trainingBotLists.GetComponent<TrainingBot>().infiniteHPMode = true;	// 체력 무한 모드
		}
		
		yield return new WaitForSeconds(1f);
		
		testTerminal.TerminalSetting(); // 터미널 세팅
		
		yield return new WaitForSeconds(1f);

		yield return StartCoroutine(UIController.instance.Dialog(0)); // 대화 R (29)
		
	    StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
	    yield return new WaitForSeconds(2f);
	    
	    // 순서 4 : 미션
	    yield return StartCoroutine(UIController.instance.MissionUIScale(UIController.instance.missionBodyFrameRect,10f,1f,0.5f)); // 바디 보이기 미션창 보이기(미션이 1개이니 0.5만큼 키우기)
	    UIController.instance.missionCheckBoxList[0].gameObject.SetActive(true);
	    yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(UIController.instance.missionString[5],UIController.instance.missionTextList[0],false,false,false,0.025f)); // 0번 텍스트에 미션 0번
		
	    UIController.instance.UISeeState(true); // UI보이기
	    
		eventState       = false; // 상태 전환
	    
		moveLock         = false; // 이동			
		jumpLock         = false; // 점프
		dashLock         = false; // 대쉬
		attackLock       = false; // 공격
		accelerationLock = false; // 엑셀
		
		// 순서 5 : 이벤트 체크
	    while (true)
        {
	        // 피격 체크 + 돌아가는 패턴이 있으면 멈추기.
	        if (PlayerHp.instance.isHit && !UIController.instance.missionCheckBoxList[0].isOn && !eventState)
	        {
		        //tutorialShootCount = 0;
				
		        eventState = true; // 상태 전환
		        AllKeyLockTrue();
		        
		        testTerminal.TurretDeActivate();		// 터렛 종료
		        pattenTurret.StopPattenCoroutine();	    // 터렛 패턴 코루틴 제거
		        
		        // 대화창 보이기
		        yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
		        
		        yield return new WaitForSeconds(1f);

		        yield return StartCoroutine(UIController.instance.Dialog(0));
		        UIController.instance.dialogStringNum -= 3;	// 대사 반복(2개 뒤로)
		        
		        StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
		        
		        yield return new WaitForSeconds(2f);
		        
		        eventState = false; // 상태 전환
		        AllKeyLockFalse();
		        
		        testTerminal.TestEnd();
	        }
	        // 이벤트 성공
	        else if (!UIController.instance.missionCheckBoxList[0].isOn && !eventState && isTutorialEvasion)
	        {
		        isTutorialEvasion = false;

		        PlayerAcceleration.instance.isAcceleration = false;

		        UIController.instance.MissionCompleteSettings(0);
		        
		        eventState = true; // 상태 전환
		        AllKeyLockTrue();
		        
		        testTerminal.TurretDeActivate();		// 터렛 종료
		        pattenTurret.StopPattenCoroutine();	    // 터렛 패턴 코루틴 제거
		        
		        // 남아있는 이동값 제거 및 상태 변경
		        yield return new WaitForFixedUpdate();
		        PlayerController.instance.rb2D.velocity = new Vector2(0f,PlayerController.instance.rb2D.velocity.y);  // 남은 이동값 제거
		        PlayerController.instance.playerAnim.SetBool("run",false);                                    // 달리기 중이면 잠금
		        
		        yield return new WaitForSeconds(1f);
		        
		        // 미션 내용 초기화 + 텍스트 토글 사라지기 + 미션창 바디 접기
		        yield return StartCoroutine(UIController.instance.MissionResetSettings());
		        
		        //------------------------------------------------------
		        // 1번째 일반형 포탑 이벤트를 성공하면 재생
		        // 2번째 일반형 + 회전형 이벤트 성공하면, 다음 이벤트로 넘어가기.
		        if (testTerminal.setTurretNum == 0)
		        {
			        // 대화창 보이기
			        yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
			        
			        yield return new WaitForSeconds(1f);
			        
			        UIController.instance.dialogStringNum++;	// 대사 1개 넘어가기
			        yield return StartCoroutine(UIController.instance.Dialog(0));	
			        
			        StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
			        
			        yield return new WaitForSeconds(2f);
			        
			        eventState       = false; // 상태 전환
	    
			        moveLock         = false; // 이동			
			        jumpLock         = false; // 점프
			        dashLock         = false; // 대쉬
			        attackLock       = false; // 공격
			        accelerationLock = false; // 엑셀
			        
			        testTerminal.setTurretNum++;	// 세팅넘버 올리기
			        
			        // 미션
			        yield return StartCoroutine(UIController.instance.MissionUIScale(UIController.instance.missionBodyFrameRect,10f,1f,0.5f)); // 바디 보이기 미션창 보이기(미션이 1개이니 0.5만큼 키우기)
			        UIController.instance.missionCheckBoxList[0].gameObject.SetActive(true);
			        yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(UIController.instance.missionString[6],UIController.instance.missionTextList[0],false,false,false,0.025f)); // 0번 텍스트에 미션 0번
			        
			        testTerminal.TestEnd();
		        }
		        // 다음 이벤트로 넘어가기
		        else if (testTerminal.setTurretNum == 1)
		        {
			        UIController.instance.dialogStringNum++; // 대사 1개 넘어가기
			        UIController.instance.UISeeState(false); // UI 숨기기(게이지 회복 숨기기 위함도 있음.)
			        
			        StartCoroutine(UIController.instance.MissionUIScale(UIController.instance.missionHeadFrameRect,10f,0f,1f));      // 미션창 숨기기
					
			        foreach (var trainingBotLists in trainingBotList)			// 봇 전체 비활성화
				        trainingBotLists.GetComponent<TrainingBot>().DeactivateBot();
			        
			        AudioManager.instance.DirectingPlay(8);				 // 열리기 사운드
			        wallAnimatorList[0].SetTrigger("leftCloseOn");  // 벽 닫히기
			        wallAnimatorList[1].SetTrigger("rightCloseOn"); // 벽 닫히기
			        
			        break;
		        }
	        }
	        yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator PerformanceEvent5()
    {
	    // 순서 1 : 지정된 위치로 이동
	    yield return StartCoroutine(EventMove(1));
	    
	    // 순서 2 : 대화(전반부) + 사이렌
	    yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
	    
	    for (var i = 0; i < 2; i++)	// 대화 R 4번 반복(34~35)
		    yield return StartCoroutine(UIController.instance.Dialog(0));
	    
	    // 싸이렌 이벤트
	    AudioManager.instance.currentAmbientSoundNum = 5;			// 성능실험실 앰비언트 -> 사이렌 앰비언트
	    yield return StartCoroutine(WarningLightColorChange());
	    AudioManager.instance.currentAmbientSoundNum = 2;	// 사이렌 앰비언트 -> 성능실험실 앰비언트
	    
	    yield return StartCoroutine(UIController.instance.DialogUIScale(1,20f,1f,1f,1f,1f,1f));  // L 대화창 보이기
	    
	    yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                           // 대화 L (36번)
	    yield return StartCoroutine(UIController.instance.Dialog(0));																							 // 대화 R (37번)
	    yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                           // 대화 L (38번)
	    yield return StartCoroutine(UIController.instance.Dialog(0));																							 // 대화 R (39번)
	    yield return StartCoroutine(UIController.instance.Dialog(0));																							 // 대화 R (39번)
	    
	    // 드론 생성
	    StartCoroutine(enemyGenerator.CreateDrone(0));
	    yield return new WaitForFixedUpdate();
	    StartCoroutine(enemyGenerator.CreateDrone(1));
	    yield return new WaitForFixedUpdate();
	    StartCoroutine(enemyGenerator.CreateDrone(2));
	    yield return new WaitForFixedUpdate();
	    yield return StartCoroutine(enemyGenerator.CreateDrone(3));
		
	    yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                            // 대화 L (40번)
	    yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                            // 대화 L (40번)
	    
	    StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
	    StartCoroutine(UIController.instance.DialogUIScale(1,20f,0f,0f,0f,0f,1f));              // L 대화창 숨기기
	    yield return new WaitForSeconds(2f);
		
	    // 임의 회복하기.
		UIController.instance.UISeeState(true);
		UIController.instance.gageSlider.value    = UIController.instance.gageSlider.maxValue;
		UIController.instance.gageSlider.maxValue = UIController.instance.gageSliderMaxvalue; // 게이지 최대값 변경
		
		eventState          = false;
		hackingLock         = false;

		tutorialExplanationList[tutorialExplanationListNum].SetActive(true);
		
		// 전투 해킹(W) 키입력
		while (true) 
		{
			if (Input.GetKeyDown(KeyCode.W))
		    {
			    PlayerHacking.instance.isHacking = true;	// 미리 해킹 상태로 변경.
			    eventState  = true;
		        hackingLock = true;
		        
		        tutorialExplanationList[tutorialExplanationListNum].SetActive(false);
		        tutorialExplanationListNum++;
		        
		        while (true)
		        {
		            // 포커스 서클이 생성되면
		            if (PlayerHacking.instance.focusCircleGameObject != null)
		            {
		                // 설명창이 꺼져있으면, 설명창 켜기.
		                if (!tutorialExplanationList[tutorialExplanationListNum].activeInHierarchy)
		                    tutorialExplanationList[tutorialExplanationListNum].SetActive(true);

		                // 현재 서클의 오른쪽에 설명창 이동.
		                tutorialExplanationList[tutorialExplanationListNum].transform.position = PlayerHacking.instance.focusCircleGameObject.transform.position + new Vector3(3f,0.25f,0f);
		            }
		            // 포커스 서클이 생성되고, 마지막 키입력 후 삭제 되자마자 -> 함께, 설명창도 꺼지기
		            else if (PlayerHacking.instance.focusCircleGameObject == null)
		            {
			            if (tutorialExplanationList[tutorialExplanationListNum].activeInHierarchy) 
		                    tutorialExplanationList[tutorialExplanationListNum].SetActive(false);
		            }
		
		            // 해킹 상태에서 벗어나면, break;
		            if (!PlayerHacking.instance.isHacking)
		            {
		                tutorialExplanationList[tutorialExplanationListNum].SetActive(false); // 설명 창 끄기
		                tutorialExplanationListNum++;
		                break;
		            }
		            yield return null;  // 이동 및 SetActive는 null에서만 작동(FixedUpdate에서 작동 x)
		        }
		        
		        break;
		    }
		    yield return null;  // 키입력 update
		}
		
		yield return new WaitForSeconds(2f);
		
		// 순서 3 : 대화(후반부)
		yield return StartCoroutine(UIController.instance.DialogUIScale(1,20f,1f,1f,1f,1f,1f)); // L 대화창 보이기
		
		yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                           // 대화 L (41번)
		
		yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
	    
		yield return StartCoroutine(UIController.instance.Dialog(0));																							 // 대화 R (42번)
		yield return StartCoroutine(UIController.instance.Dialog(0));																							 // 대화 R (43번)
		yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                           // 대화 L (44번)
		yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                           // 대화 L (45번)
		yield return StartCoroutine(UIController.instance.Dialog(0));																							 // 대화 R (46번)
		yield return StartCoroutine(UIController.instance.Dialog(1));                                                                                           // 대화 L (47번)
		
		StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
		StartCoroutine(UIController.instance.DialogUIScale(1,20f,0f,0f,0f,0f,1f));              // L 대화창 숨기기
		
		yield return new WaitForSeconds(2f);
		
		// 순서 4 : 이벤트 상태 전환 + 오른쪽 콘솔 올라오기 + 
		eventState = false;
		AllKeyLockFalse();
		
		// 플레이어 카메라 따라가기
		CameraController.instance.target             = PlayerController.instance.gameObject.transform;

		// 게이트 문 열기
		eventControlGate[0].anim.SetTrigger("eventOpenOn");
		
		// 오른쪽 터미널 활성화
		//elevatorTerminalList[1].PerformanceLabElevatorTerminalActive();
    }

    private IEnumerator EventMove(int num)
    {
	    while (true)
	    {
		    // 이동
		    if (PlayerController.instance.transform.position.x < labEventTrans[num].position.x)
		    {
			    PlayerController.instance.rb2D.velocity = new Vector2(1 * PlayerController.instance.activeMoveSpeed,
				    PlayerController.instance.rb2D.velocity.y);
			    PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		    }
		    else
		    {
			    PlayerController.instance.rb2D.velocity = new Vector2(-1 * PlayerController.instance.activeMoveSpeed,
				    PlayerController.instance.rb2D.velocity.y);
			    PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
		    }

		    PlayerController.instance.playerAnim.SetBool("run", true);
		    // 이동 종료
		    if (Vector2.Distance(PlayerController.instance.transform.position, labEventTrans[num].position) < 0.25f)
		    {
			    // 남아있는 이동값 제거
			    yield return new WaitForFixedUpdate();
			    PlayerController.instance.rb2D.velocity = Vector2.zero;
			    PlayerController.instance.playerAnim.SetBool("run", false);

			    break;
		    }
		    yield return new WaitForFixedUpdate();
	    }
    }

    private IEnumerator WarningLightColorChange()
    {
	    int numBlinks = 0;
        
	    while (true)
	    {
		    if (numBlinks >= switchNum)
		    {
			    yield break;
		    }
            
		    for (float t = 0; t < 1f; t += Time.deltaTime / changeLightSpeed)
		    {
			    topDownLight2D.color = Color.Lerp(originalColor, new Color(1f, 0f, 0f, 1f), t);
			    yield return null;
		    }
            
		    yield return new WaitForSeconds(changeLightSpeed);
            
		    for (float t = 0; t < 1f; t += Time.deltaTime / changeLightSpeed)
		    {
			    topDownLight2D.color = Color.Lerp(new Color(1f, 0f, 0f, 1f), originalColor, t);
			    yield return null;
		    }
            
		    yield return new WaitForSeconds(changeLightSpeed);

		    numBlinks++;
	    }
    }
    
    // ----------------------------BossRoom----------------------------
    private IEnumerator BossRoomStartEvent()
    {
        // 순서 1 : 카메라 이동
        CameraController.instance.target    = bossRoomCameraPos;
        CameraController.instance.moveSpeed = bossRoomCameraMoveSpeed;
        
        // 순서 2 : 타이틀 보이기 및 사라지기(코루틴 기다리기)
        yield return StartCoroutine(UIController.instance.TitleActiveCoroutine());

        // 순서 2 : 보스 페이드 등장
        yield return new WaitForSeconds(2f);

        AudioManager.instance.WardenSfxCreate(1,true,BossController.instance.gameObject);	// 등장 글로우 페이드 사운드.
        while (true)
        {
            // 바디 등장 
            if (BossController.instance.bodyGlowFadeValue <= 1)
            {
	            BossController.instance.bodyGlowFadeValue += Time.deltaTime * 0.5f;
	            BossController.instance.bossBodyMat.SetFloat(BossController.instance.glowFadeID, BossController.instance.bodyGlowFadeValue);
	            BossController.instance.bossBodyLightMat.SetFloat(BossController.instance.glowFadeID, BossController.instance.bodyGlowFadeValue);

                if (BossController.instance.bodyGlowFadeValue > 1)
                {
                    // 값 픽스
                    BossController.instance.bodyGlowFadeValue = 1f;
                    BossController.instance.bossBodyMat.SetFloat(BossController.instance.glowFadeID, BossController.instance.bodyGlowFadeValue);
                    BossController.instance.bossBodyLightMat.SetFloat(BossController.instance.glowFadeID, BossController.instance.bodyGlowFadeValue);

                    break;
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        // 순서 3 : 바디 라이트 밝히기
        AudioManager.instance.WardenSfxCreate(2,true,BossController.instance.gameObject);	// 파워온 사운드.
        while (true)
        {
            // 바디 등장 
            if (BossController.instance.bodyBrightFadeValue <= 4)
            {
	            BossController.instance.bodyBrightFadeValue += Time.deltaTime * 2f;
	            BossController.instance.bossBodyLightMat.SetFloat(BossController.instance.brightFadeID, BossController.instance.bodyBrightFadeValue);
			
                if (BossController.instance.bodyBrightFadeValue > 4)
                {
                    // 값 픽스
                    BossController.instance.bodyBrightFadeValue = 4f;
                    BossController.instance.bossBodyLightMat.SetFloat(BossController.instance.brightFadeID, BossController.instance.bodyBrightFadeValue);
                    break;
                }
            }

            yield return null;
        }
		
        // 순서 4 : 상태전환 및 제어권 부여
        BossController.instance.bossAnim.SetTrigger("appearOff"); // 나타나기
        AudioManager.instance.WardenSfxCreate(3,true,BossController.instance.gameObject);	// 파워온 사운드.
        while (true)
        {
            if (BossController.instance.bossAnimStateInfo.IsName("Idle")) // 보스 애니메이션이 Idle이 되면, 전투 시작 및 키 제어권 부여
            {
                // 보스 등장 상태
                BossController.instance.isAppear = true;
                
                // 홀로그램 벽 활성화
                foreach (var laserBeamControllerList in laserBeamControllerList)
                {
	                laserBeamControllerList.isLaserActivated = true;
	                laserBeamControllerList.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                }

                // UI 보이기
	            UIController.instance.UISeeState(true);

                // 이벤트 상태 전환
                eventState = false;
                AllKeyLockFalse();
                
                AudioManager.instance.currentAmbientSoundNum = BGMnum;	// BGM재생

                break;
            }

            yield return null;
        }
    }

    public IEnumerator BossRoomEndEvent()
    {
	    StartCoroutine(BossController.instance.LanceAppear(false));			 // 렌스가 있는 경우, 사라지기
	    yield return StartCoroutine(BossAnimatorFunction.instance.BossLightOff()); // 보스 라이트 끄기.(->기다리기)
	    
	    // 카메라 변경 + 움직이기만 가능
	    CameraController.instance.target = PlayerController.instance.gameObject.transform;
	    
	    eventState = false;
	    moveLock   = false;
	        
	    // 오른쪽 홀로그램 벽 비활성화
	    laserBeamControllerList[1].isLaserActivated = false;
	    laserBeamControllerList[1].gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }
    
    public void AllKeyLockTrue()
    {
	    moveLock         = true;
	    attackLock       = true;
	    jumpLock         = true;
	    recoveryLock     = true;
	    scanLock         = true;
	    hackingLock      = true;
	    accelerationLock = true;
	    dashLock         = true;
    }

    public void AllKeyLockFalse()
    {
	    moveLock         = false;
	    attackLock       = false;
	    jumpLock         = false;
	    recoveryLock     = false;
	    scanLock         = false;
	    hackingLock      = false;
	    accelerationLock = false;
	    dashLock         = false;
    }

}
