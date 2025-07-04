using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	public static MenuManager instance;
	
	[Header("------Common------")]
	public GameObject      menuKeyExUI; // 메뉴 버튼 설명 UI (공통)
	public TextMeshProUGUI versionText; // 버전 텍스트
	
	public RectTransform          focusGameObject;						              // 포커스 전체 관리
	public List<RectTransform>    focusRectTransformList = new List<RectTransform>(); // 포커스 개인 관리

	[Header("------StasisRoom Menu------")]
	public Image          mainFrame;

	public GameObject     titleParticleFXObject;
	public Animator       titleQuadLogoAnim;
	public SpriteRenderer titleLineSpriteRenderer;

	public Image          stasisRoomBlackWall;	                // 메인메뉴 화면 비강조를 위한 검정색 이미지(뒤에 희미하게 보이는 투명도)
	
	[HideInInspector]
	public bool           isStasisRoomMenu;							        // 정체실 메뉴 조작가능 상태(시작 연출 끝나면 true)
	
	public  List<Image>   stasisRoomMenuOnImageList    = new List<Image>(); // 메뉴 ON 이미지
	public  List<Image>   stasisRoomMenuOffImageList   = new List<Image>(); // 메뉴 OFF 이미지

	private int	          stasisRoomMenuCurrentNum = 0;					    // 현재 선택된 메인 메뉴의 타이틀 넘버
	
	[Header("------Normal Menu------")]
	public string       mainMenuName;
	public GameObject   titleGameObject;
	public GameObject   titleLineGameObject;
	public GameObject   lightBackgroundGameObject;
	public Image        normalMenuBlackWall;	                               // 인게임 화면 비강조를 위한 검정색 이미지(완전 검정색 투명도)

	private bool        isNormalCorutineEnd;

	[HideInInspector]
	public bool         isNormalMenu;							               // 노멀 메뉴의 상태
	
	public  List<Image> normalMenuOnImageList  = new List<Image>(); // 메뉴 ON 이미지
	public  List<Image> normalMenuOffImageList = new List<Image>(); // 메뉴 OFF 이미지
	
	private int	                  normalMenuCurrentNum = 0;						       // 현재 선택된 넘버
	
	[Header("------Setting------")]
	public GameObject             settingFrame;	              // 옵션 프레임
	
	private bool                  isTtileChage;			      // 타이틀 선택 중 인지
	private int                   currnetSeetingTitleNum = 0; // 현재 선택된 타이틀 넘버
	private int                   currentSettingValueNum = 0; // 각 타이틀(그래픽/사운드/조작)에 들어가서 선택된 넘버

	public List<GameObject>       settingTitleNormalList    = new List<GameObject>();	// 그래픽 - 사운드 - 조작
	public List<GameObject>       settingTitleHighlightList = new List<GameObject>();   // 그래픽 - 사운드 - 조작
	
	public  Color                 settingNormalColor;
	public  Color                 settingHighlightColor;
	
	// ------그래픽------
	public  List<TextMeshProUGUI> settingNameTextList    = new List<TextMeshProUGUI>();  // 세팅 이름(해상도,수직동기화 등등 ↓ 순서로 넣음.)
	
	public List<TextMeshProUGUI>  settingDetailsTextList = new List<TextMeshProUGUI>(); // 세팅 디테일 (1920 x 1080 // 켜기 끄기)
	
	public  List<int> resolutionWidthList  = new List<int>();
	public  List<int> resolutionHeightList = new List<int>();
	
	public List<int>  FpsList              = new List<int>();
	
	// ------사운드------
	public List<TextMeshProUGUI> settingNameTextList2    = new List<TextMeshProUGUI>(); // 세팅 이름(주음량 / 배경음 / 효과음)

	public List<Slider>          settingDetailsSlidersList = new List<Slider>();		  // 슬라이더 값
	public List<TextMeshProUGUI> settingDetailsTextList2   = new List<TextMeshProUGUI>(); // 세팅 디테일 텍스트 (0 ~ 100)
	
	// ------키설명------
	public List<TextMeshProUGUI> settingNameTextList3    = new List<TextMeshProUGUI>();   // 키 이름 (방향키 / S / A)
	public List<TextMeshProUGUI> settingDetailsTextList3   = new List<TextMeshProUGUI>(); // 키 세부 설명
	
	// ------조작------
	[Header("------Interaction------")] 
	public GameObject       InteractionGameObject;
	
	public List<GameObject> interactionNormalImageList    = new List<GameObject>(); // 확인 - 취소 노멀
	public List<GameObject> interactionHighLightImageList = new List<GameObject>(); // 확인 - 취소 강조
	public List<GameObject> messageList                   = new List<GameObject>();

	private int             currentInteractionNum;
	
	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		// 빌드 모드에서 작동
	#if UNITY_STANDALONE
		Cursor.visible = false;
	#endif
	
		// 노멀오픈상태
		isNormalCorutineEnd = true;
		
		// 포커스 끄기
		foreach (var focusImageLists in focusRectTransformList)
			focusImageLists.localScale = new Vector3(0f,1f,0f); // 스케일 초기화

		// 정체실 메뉴 이미지 끄기
		foreach (var stasisRoomMenuOnImageLists in stasisRoomMenuOnImageList)   // ON 끄기
			stasisRoomMenuOnImageLists.gameObject.SetActive(false);
		foreach (var stasisRoomMenuOffImageLists in stasisRoomMenuOffImageList) // OFF 투명도 0 (-> 처음 시작에서 서서히 보이기 때문)
			stasisRoomMenuOffImageLists.color = new Color(stasisRoomMenuOffImageLists.color.r, stasisRoomMenuOffImageLists.color.g, stasisRoomMenuOffImageLists.color.b, 0f);
		
		// 타이틀 라인 스프라이트 렌더러 투명도 초기화
		if(titleLineSpriteRenderer != null)
			titleLineSpriteRenderer.color = new Color(titleLineSpriteRenderer.color.r, titleLineSpriteRenderer.color.g, titleLineSpriteRenderer.color.b, 0f);
		// 타이틀 초기화
		if(titleParticleFXObject != null)
			titleParticleFXObject.SetActive(false);
		
		// 시간 정상화
		Time.timeScale      = 1f;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;
		
		// 버전 표시
		versionText.gameObject.SetActive(false);
		versionText.text = "Ver." + Application.version + ".Demo";
		
			
		// 전체 사운드 슬라이더 최고값 초기화
		foreach (var settingDetailsSlidersLists in settingDetailsSlidersList)
		{
			settingDetailsSlidersLists.maxValue = 100;
		}
		
		// 메인 사운드
		if (!PlayerPrefs.HasKey("Master"))
			PlayerPrefs.SetInt("Master", 50);

		// BGM
		if (!PlayerPrefs.HasKey("BGM")) 
			PlayerPrefs.SetInt("BGM", 50);

		// SFX
		if (!PlayerPrefs.HasKey("SFX"))
			PlayerPrefs.SetInt("SFX", 50);
		
		// 표시값 초기화
		for (int i = 0; i < settingDetailsSlidersList.Count; i++)
		{
			if (i == 0)
			{
				settingDetailsSlidersList[i].value = PlayerPrefs.GetInt("Master"); 
				if ((int)settingDetailsSlidersList[i].value == 0)	// 마스터 볼륨이 0이라면
					AudioManager.instance.mixer.SetFloat("Master", -80);
				else
					AudioManager.instance.mixer.SetFloat("Master", (PlayerPrefs.GetInt("Master") - 50) / 10);// 1번째 참조 변수 이름 // 2번째는 프리팹 이름
			}																											  // 마스터는 영향이 크지 않도록 /10으로 하고, BGM과 SFX는 /5로 나누기.
			else if (i == 1)
			{
				settingDetailsSlidersList[i].value = PlayerPrefs.GetInt("BGM");
				if ((int)settingDetailsSlidersList[i].value == 0) // 마스터 볼륨이 0이라면
					AudioManager.instance.mixer.SetFloat("BGM", -80);
				else
					AudioManager.instance.mixer.SetFloat("BGM", (PlayerPrefs.GetInt("BGM") - 50) / 5);
			}
			else if (i == 2)
			{
				settingDetailsSlidersList[i].value = PlayerPrefs.GetInt("SFX");
				if ((int)settingDetailsSlidersList[i].value == 0) // 마스터 볼륨이 0이라면
					AudioManager.instance.mixer.SetFloat("SFX", -80);
				else
					AudioManager.instance.mixer.SetFloat("SFX", (PlayerPrefs.GetInt("SFX") - 50) / 5);
			}
				
			float textFloat = settingDetailsSlidersList[i].value;
			settingDetailsTextList2[i].text = textFloat.ToString();
		}

		// 설정값 불러오기
		// 해상도 및 창모드(설정이 같이 있음)
		bool isFullScreen = false;
		if (PlayerPrefs.HasKey("FullScreenNum") && PlayerPrefs.GetInt("FullScreenNum") == 1) // 풀 스크린 on
		{
			PlayerPrefs.SetInt("FullScreenNum", 1);
			isFullScreen = true;
		}
		else if (!PlayerPrefs.HasKey("FullScreenNum") || PlayerPrefs.GetInt("FullScreenNum") == 0) // 풀 스크린 off(기본값)
		{
			PlayerPrefs.SetInt("FullScreenNum", 0);
			isFullScreen = false;
		}
		
		if (PlayerPrefs.HasKey("ResolutionNum")       && PlayerPrefs.GetInt("ResolutionNum") == 0)											// 1600 x 9000
		{
			PlayerPrefs.SetInt("ResolutionNum",0);
			Screen.SetResolution(resolutionWidthList[0], resolutionHeightList[0], isFullScreen);
		}
		else if ((PlayerPrefs.HasKey("ResolutionNum") && PlayerPrefs.GetInt("ResolutionNum") == 1) || !PlayerPrefs.HasKey("ResolutionNum")) // 1920 x 1080(기본값)
		{
			PlayerPrefs.SetInt("ResolutionNum",1);
			Screen.SetResolution(resolutionWidthList[1], resolutionHeightList[1], isFullScreen);
		}
		else if (PlayerPrefs.HasKey("ResolutionNum")  && PlayerPrefs.GetInt("ResolutionNum") == 2)											// 2560 x 1440
		{
			PlayerPrefs.SetInt("ResolutionNum",2);
			Screen.SetResolution(resolutionWidthList[2], resolutionHeightList[2], isFullScreen);
		}
		
		// Vsync
		if (PlayerPrefs.HasKey("VsyncNum") && PlayerPrefs.GetInt("VsyncNum") == 1) // 수직동기화 on
		{
			PlayerPrefs.GetInt("VsyncNum", 1);
			QualitySettings.vSyncCount = 1;
		}
		else if (!PlayerPrefs.HasKey("VsyncNum") || PlayerPrefs.GetInt("VsyncNum") == 0) // 수직동기화 off(기본값)
		{
			PlayerPrefs.GetInt("VsyncNum", 0);	
			QualitySettings.vSyncCount = 0;
		}
		
		// FPS
		if ((PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 0) || !PlayerPrefs.HasKey("FPSNum")) // 60 FPS (기본값)
		{
			PlayerPrefs.GetInt("FPSNum", 0);	
			Application.targetFrameRate = FpsList[0];
		}
		else if ((PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 1))							 // 100 FPS 
		{
			PlayerPrefs.GetInt("FPSNum", 1);
			Application.targetFrameRate = FpsList[1];
		}
		else if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 2)								 // 120 FPS
		{
			PlayerPrefs.GetInt("FPSNum", 2);
			Application.targetFrameRate = FpsList[2];
		}
		else if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 3)								 // 144 FPS 
		{
			PlayerPrefs.GetInt("FPSNum", 3);
			Application.targetFrameRate = FpsList[3];
		}
	}
	
	private void Update()
	{
		FullScreenToggleAltEnter();	// ATL + ENTER 풀스크린 토글
		
		MainMenu();		// 메인메뉴(정체실 메뉴)
		
		NormalMenu();  // 노멀메뉴(인게임 메뉴)
	}

	private void MainMenu()
	{
		// 정체실 메인 메뉴 
		if (isStasisRoomMenu && EventController.instance.eventState)
		{
			// 메인 메뉴 창 조작
			if (!settingFrame.gameObject.activeInHierarchy && !InteractionGameObject.gameObject.activeInHierarchy)
			{
				if (Input.GetKeyDown(KeyCode.DownArrow))
				{
					if (stasisRoomMenuCurrentNum < stasisRoomMenuOnImageList.Count - 1)
					{
						AudioManager.instance.UISoundPlay(0);
					
						stasisRoomMenuOnImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(false);	// 강조 끄기
						stasisRoomMenuOffImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(true);	// 비강조 켜기
						
						stasisRoomMenuCurrentNum++;

						stasisRoomMenuOnImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(true);	  // 강조 켜기
						stasisRoomMenuOffImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(false); // 비강조 끄기

						focusGameObject.transform.position = stasisRoomMenuOnImageList[stasisRoomMenuCurrentNum].rectTransform.transform.position;	// 포커스 이동
					}
				}
				else if (Input.GetKeyDown(KeyCode.UpArrow))
				{
					if (stasisRoomMenuCurrentNum > 0)
					{
						AudioManager.instance.UISoundPlay(0);
					
						stasisRoomMenuOnImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(false);	// 강조 끄기
						stasisRoomMenuOffImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(true);	// 비강조 켜기
						
						stasisRoomMenuCurrentNum--;

						stasisRoomMenuOnImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(true);   // 강조 켜기
						stasisRoomMenuOffImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(false); // 비강조 끄기

						focusGameObject.transform.position = stasisRoomMenuOnImageList[stasisRoomMenuCurrentNum].rectTransform.transform.position;	// 포커스 이동
					}
				}
				// 선택
				else if (Input.GetKeyDown(KeyCode.Return) && !Input.GetKey(KeyCode.LeftAlt))
				{
					AudioManager.instance.UISoundPlay(1);
					
					switch (stasisRoomMenuCurrentNum)
					{
						case 0:
							// 저장된 정보가 있으면, 상호작용 UI
							if (PlayerPrefs.HasKey("SaveSeenName") && PlayerPrefs.HasKey("SaveTerminalName"))
								InteractionOn();
							// 저장된 정보가 없으면, 바로 새 게임 시작
							else
								StartCoroutine(EventController.instance.NewStory());
							break;
						case 1:
							// 저장된 정보가 있으면, 컨티뉴
							if((PlayerPrefs.HasKey("SaveSeenName") && PlayerPrefs.HasKey("SaveTerminalName")))
								StartCoroutine(Continue());
							// 저장된 정보가 없으면, 새 게임
							else
								StartCoroutine(EventController.instance.NewStory());
							break;
						case 2:
							SettingOn();					  // 세팅창 
							break;
						case 3:
							InteractionOn();                  // 상호작요 다음 -> 게임 종료
							break;
					}
				}
			}
			// 옵션 선택 중
			else if (settingFrame.gameObject.activeInHierarchy)
			{
				SettingMode();
			}
			// 상호작용 선택 중
			else if (InteractionGameObject.gameObject.activeInHierarchy)
			{
				InteractionMode();
			}
		}
	}
	
	private void NormalMenu()
	{
		// 노멀 메뉴 반응
		if (Input.GetKeyDown(KeyCode.Escape)     && PlayerHp.instance.liveState && isNormalCorutineEnd &&
		    !EventController.instance.eventState && !PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking && !PlayerScan.instance.isScan && !PlayerHp.instance.isHit)
		{
			// 열기 
			if (!isNormalMenu)
			{
				isNormalCorutineEnd = false;
				StartCoroutine(OpenNormalMenu());
			}
			// 닫기(->노멀 메뉴 종료 // 창들이 켜져있지 않으면)
			else if (isNormalMenu && !settingFrame.gameObject.activeInHierarchy && !InteractionGameObject.gameObject.activeInHierarchy)
			{
				isNormalCorutineEnd = false;
				StartCoroutine(CloseNormalMenu());
			}
		}
		
		// 노멀 메인 메뉴 조작
		if (isNormalMenu && isNormalCorutineEnd)
		{
			// 노멀 메뉴 창
			if (!settingFrame.gameObject.activeInHierarchy && !InteractionGameObject.gameObject.activeInHierarchy)
			{
				if (Input.GetKeyDown(KeyCode.DownArrow))
				{
					if (normalMenuCurrentNum < normalMenuOnImageList.Count - 1)
					{
						AudioManager.instance.UISoundPlay(0);
					
						normalMenuOnImageList[normalMenuCurrentNum].gameObject.SetActive(false);	// 강조 끄기
						normalMenuOffImageList[normalMenuCurrentNum].gameObject.SetActive(true);	// 비강조 켜기
						
						normalMenuCurrentNum++;

						normalMenuOnImageList[normalMenuCurrentNum].gameObject.SetActive(true);	  // 강조 켜기
						normalMenuOffImageList[normalMenuCurrentNum].gameObject.SetActive(false); // 비강조 끄기
					
						focusGameObject.transform.position = normalMenuOnImageList[normalMenuCurrentNum].rectTransform.transform.position;	// 포커스 이동
					}
				}
				else if (Input.GetKeyDown(KeyCode.UpArrow))
				{
					if (normalMenuCurrentNum > 0)
					{
						AudioManager.instance.UISoundPlay(0);
					
						normalMenuOnImageList[normalMenuCurrentNum].gameObject.SetActive(false);	// 강조 끄기
						normalMenuOffImageList[normalMenuCurrentNum].gameObject.SetActive(true);	// 비강조 켜기
						
						normalMenuCurrentNum--;

						normalMenuOnImageList[normalMenuCurrentNum].gameObject.SetActive(true);	  // 강조 켜기
						normalMenuOffImageList[normalMenuCurrentNum].gameObject.SetActive(false); // 비강조 끄기

						focusGameObject.transform.position = normalMenuOnImageList[normalMenuCurrentNum].rectTransform.transform.position;	// 포커스 이동
					}
				}
				// 선택
				else if (Input.GetKeyDown(KeyCode.Return) && !Input.GetKey(KeyCode.LeftAlt))
				{
					switch (normalMenuCurrentNum)
					{
						case 0:
							StartCoroutine(CloseNormalMenu());	// 다시 게임으로 돌아가기
							break;
						case 1:
							SettingOn();	    // 세팅 열기
							break;
						case 2:
							InteractionOn();    // 상호작요 다음 -> 메인메뉴로 돌아가기
							break;
						case 3:
							InteractionOn();   // 상호작용 다음 -> 게임 종료
							break;
					}
				}
			}
			// 세팅 창
			else if (settingFrame.gameObject.activeInHierarchy)
			{
				SettingMode();
			}
			// 상호작용 UI 선택 중
			else if (InteractionGameObject.gameObject.activeInHierarchy)
			{
				InteractionMode();
			}
		}
	}
	
	private void SettingMode()
	{
		// 타이틀 선택 중
		if (isTtileChage)
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{	
				if (currnetSeetingTitleNum < settingTitleHighlightList.Count - 1)
				{
					AudioManager.instance.UISoundPlay(0);
				
					settingTitleNormalList[currnetSeetingTitleNum].SetActive(true);
					settingTitleHighlightList[currnetSeetingTitleNum].SetActive(false);

					currnetSeetingTitleNum++;
					
					settingTitleNormalList[currnetSeetingTitleNum].SetActive(false);
					settingTitleHighlightList[currnetSeetingTitleNum].SetActive(true);
				}
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				if (currnetSeetingTitleNum > 0)						
				{
					AudioManager.instance.UISoundPlay(0);
					
					settingTitleNormalList[currnetSeetingTitleNum].SetActive(true);
					settingTitleHighlightList[currnetSeetingTitleNum].SetActive(false);

					currnetSeetingTitleNum--;
					
					settingTitleNormalList[currnetSeetingTitleNum].SetActive(false);
					settingTitleHighlightList[currnetSeetingTitleNum].SetActive(true);
				}
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				AudioManager.instance.UISoundPlay(0);
			
				isTtileChage = false;
				if (currnetSeetingTitleNum == 0)	    // 그래픽 내려가기
				{
					settingNameTextList[0].color = settingHighlightColor; 
				}
				else if (currnetSeetingTitleNum == 1)	// 상운드 선택
				{
					settingNameTextList2[0].color = settingHighlightColor; 
				}
				else if (currnetSeetingTitleNum == 2)	// 조작 선택
				{
					settingNameTextList3[0].color = settingHighlightColor; 
					settingDetailsTextList3[0].gameObject.SetActive(true);
				}
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				SettingOff();
			}
		}
		// 그래픽 설정 
		else if (!isTtileChage && currnetSeetingTitleNum == 0)
		{
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (currentSettingValueNum < settingNameTextList.Count - 1)
				{
					AudioManager.instance.UISoundPlay(0);
				
					settingNameTextList[currentSettingValueNum].color = settingNormalColor;	   // 노멀
					currentSettingValueNum++;
					settingNameTextList[currentSettingValueNum].color = settingHighlightColor; // 강조
				}
			}
			else if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (currentSettingValueNum > 0)
				{
					AudioManager.instance.UISoundPlay(0);
					
					settingNameTextList[currentSettingValueNum].color = settingNormalColor;	  // 노멀
					currentSettingValueNum--;
					settingNameTextList[currentSettingValueNum].color = settingHighlightColor; // 강조
				}
				else if (currentSettingValueNum == 0)// 그래픽 선택 나가기
				{
					isTtileChage = true;									  // 타이틀 선택으로 돌어가기
					foreach (var settingNameTextLists in settingNameTextList) // 텍스트 비강조
						settingNameTextLists.color = settingNormalColor;
				}
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{	
				if (currentSettingValueNum < settingNameTextList.Count - 2)	// 현재는 총 4개이기 때문에, 2개씩 이기 때문에 +2
				{
					AudioManager.instance.UISoundPlay(0);
				
					settingNameTextList[currentSettingValueNum].color = settingNormalColor;	  // 노멀
					currentSettingValueNum += 2;
					settingNameTextList[currentSettingValueNum].color = settingHighlightColor; // 강조
				}
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				if (currentSettingValueNum > 1)						// 현재는 총 4개이기 때문에, 2개씩 이기 때문에 -2
				{
					AudioManager.instance.UISoundPlay(0);
				
					settingNameTextList[currentSettingValueNum].color = settingNormalColor;	  // 노멀
					currentSettingValueNum -= 2;
					settingNameTextList[currentSettingValueNum].color = settingHighlightColor; // 강조
				}
			}
			else if (Input.GetKeyDown(KeyCode.Return) && !Input.GetKey(KeyCode.LeftAlt))
			{
				switch (currentSettingValueNum)
				{
					case 0:		// 해상도
						ResolutionSettingToggle();
						break;
					case 1:		// 수직동기화
						VsyncSettingToggle();
						break;
					case 2:		// 전체화면
						FullScreenSettingToggle();
						break;
					case 3:		// 프레임
						FPSSettingToggle();
						break;
				}
			}
			// 그래픽 선택 나가기
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				AudioManager.instance.UISoundPlay(1);
			
				isTtileChage           = true;							  // 타이틀 선택으로 돌어가기
				currentSettingValueNum = 0;
				foreach (var settingNameTextLists in settingNameTextList) // 텍스트 비강조
					settingNameTextLists.color = settingNormalColor;	  
			}
		}
		// 사운드
		else if (!isTtileChage && currnetSeetingTitleNum == 1)
		{
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (currentSettingValueNum < settingNameTextList2.Count - 1)
				{
					AudioManager.instance.UISoundPlay(0);
				
					settingNameTextList2[currentSettingValueNum].color = settingNormalColor;	// 노멀
					currentSettingValueNum++;
					settingNameTextList2[currentSettingValueNum].color = settingHighlightColor; // 강조
				}
			}
			else if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (currentSettingValueNum > 0)
				{
					AudioManager.instance.UISoundPlay(0);
					
					settingNameTextList2[currentSettingValueNum].color = settingNormalColor;	// 노멀
					currentSettingValueNum--;
					settingNameTextList2[currentSettingValueNum].color = settingHighlightColor; // 강조
				}
				else if (currentSettingValueNum == 0) // 그래픽 선택 나가기
				{
					isTtileChage = true;									   // 타이틀 선택으로 돌어가기
					foreach (var settingNameTextLists in settingNameTextList2) // 텍스트 비강조
						settingNameTextLists.color = settingNormalColor;
				}
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				if (settingDetailsSlidersList[currentSettingValueNum].value < 100)
				{
					AudioManager.instance.UISoundPlay(0);
					
					settingDetailsSlidersList[currentSettingValueNum].value += 10;
					
					float textFloat = settingDetailsSlidersList[currentSettingValueNum].value;
					settingDetailsTextList2[currentSettingValueNum].text = textFloat.ToString();

					if (currentSettingValueNum == 0)
					{
						PlayerPrefs.SetInt("Master",(int)settingDetailsSlidersList[currentSettingValueNum].value);
						AudioManager.instance.mixer.SetFloat("Master", (PlayerPrefs.GetInt("Master") - 50) / 10);
					}
					else if (currentSettingValueNum == 1)
					{
						PlayerPrefs.SetInt("BGM",(int)settingDetailsSlidersList[currentSettingValueNum].value);
						AudioManager.instance.mixer.SetFloat("BGM", (PlayerPrefs.GetInt("BGM") - 50) / 5);
					}
					else if (currentSettingValueNum == 2)
					{
						PlayerPrefs.SetInt("SFX",(int)settingDetailsSlidersList[currentSettingValueNum].value);
						AudioManager.instance.mixer.SetFloat("SFX", (PlayerPrefs.GetInt("SFX") - 50) / 5);
					}
				}
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				if (settingDetailsSlidersList[currentSettingValueNum].value > 0)
				{
					AudioManager.instance.UISoundPlay(0);
				
					settingDetailsSlidersList[currentSettingValueNum].value -= 10;
					
					float textFloat = settingDetailsSlidersList[currentSettingValueNum].value;
					settingDetailsTextList2[currentSettingValueNum].text = textFloat.ToString();
					
					if (currentSettingValueNum == 0)
					{
						PlayerPrefs.SetInt("Master",(int)settingDetailsSlidersList[currentSettingValueNum].value);
						if ((int)settingDetailsSlidersList[currentSettingValueNum].value == 0)	// 마스터 볼륨이 0이라면
							AudioManager.instance.mixer.SetFloat("Master", -80);
						else
							AudioManager.instance.mixer.SetFloat("Master", (PlayerPrefs.GetInt("Master") - 50) / 10);
					}
					else if (currentSettingValueNum == 1)
					{
						PlayerPrefs.SetInt("BGM",(int)settingDetailsSlidersList[currentSettingValueNum].value);
						if ((int)settingDetailsSlidersList[currentSettingValueNum].value == 0)	// 마스터 볼륨이 0이라면
							AudioManager.instance.mixer.SetFloat("BGM", -80);
						else
							AudioManager.instance.mixer.SetFloat("BGM", (PlayerPrefs.GetInt("BGM") - 50) / 5);
					}
					else if (currentSettingValueNum == 2)
					{
						PlayerPrefs.SetInt("SFX",(int)settingDetailsSlidersList[currentSettingValueNum].value);
						if ((int)settingDetailsSlidersList[currentSettingValueNum].value == 0)	// 마스터 볼륨이 0이라면
							AudioManager.instance.mixer.SetFloat("SFX", -80);
						else
							AudioManager.instance.mixer.SetFloat("SFX", (PlayerPrefs.GetInt("SFX") - 50) / 5);
					}
				}
			}
			// 사운드 선택 나가기
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				AudioManager.instance.UISoundPlay(1);
			
				isTtileChage           = true;							   // 타이틀 선택으로 돌어가기
				currentSettingValueNum = 0;
				foreach (var settingNameTextList2s in settingNameTextList2) // 텍스트 비강조
					settingNameTextList2s.color = settingNormalColor;	  
			}
		}
		// 키설명
		else if (!isTtileChage && currnetSeetingTitleNum == 2)
		{
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (currentSettingValueNum < settingNameTextList3.Count - 1)
				{
					AudioManager.instance.UISoundPlay(0);

					settingDetailsTextList3[currentSettingValueNum].gameObject.SetActive(false);  // 이전 설명 텍스트 끄기
				
					settingNameTextList3[currentSettingValueNum].color = settingNormalColor;	  // 노멀
					currentSettingValueNum++;
					settingNameTextList3[currentSettingValueNum].color = settingHighlightColor;   // 강조
					
					settingDetailsTextList3[currentSettingValueNum].gameObject.SetActive(true);	  // 설명 텍스트 켜기
				}
			}
			else if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (currentSettingValueNum > 0)
				{
					AudioManager.instance.UISoundPlay(0);
					
					settingDetailsTextList3[currentSettingValueNum].gameObject.SetActive(false);  // 이전 설명 텍스트 끄기
					
					settingNameTextList3[currentSettingValueNum].color = settingNormalColor;	 // 노멀
					currentSettingValueNum--;
					settingNameTextList3[currentSettingValueNum].color = settingHighlightColor;  // 강조
					
					settingDetailsTextList3[currentSettingValueNum].gameObject.SetActive(true);	  // 설명 텍스트 켜기
				}
				else if (currentSettingValueNum == 0) // 키설명 선택 나가기
				{
					isTtileChage = true;									   // 타이틀 선택으로 돌어가기
					foreach (var settingNameTextLists in settingNameTextList3) // 텍스트 비강조
						settingNameTextLists.color = settingNormalColor;

					foreach (var settingDetailsTextList3s in settingDetailsTextList3)	// 설명 다 끄기
						settingDetailsTextList3s.gameObject.SetActive(false);
				}
			}
			// 키설명 나가기
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				AudioManager.instance.UISoundPlay(1);
			
				isTtileChage           = true;							   // 타이틀 선택으로 돌어가기
				currentSettingValueNum = 0;
				foreach (var settingNameTextList3s in settingNameTextList3) // 텍스트 비강조
					settingNameTextList3s.color = settingNormalColor;	  
			}
		}
	}
	
	// 옵션창 켜기
	private void SettingOn()
	{
		AudioManager.instance.UISoundPlay(1);
	
		// 비강조 검정벽 켜기
		if (isStasisRoomMenu) // 메인 메뉴 조작 상태
			stasisRoomBlackWall.gameObject.SetActive(true);

		// 텍스트 모두 비강조
		foreach (var settingTextLists in settingNameTextList)
			settingTextLists.color = settingNormalColor;
		
		// 세팅 초기화
		isTtileChage           = true;
		currnetSeetingTitleNum = 0;
		currentSettingValueNum = 0;

		// 저장된 설정으로 텍스트 내용 바꾸기(해상도 / Vsync / FullScreen / FPS / 주음량 / BGM / SFX )
		if (PlayerPrefs.HasKey("ResolutionNum")       && PlayerPrefs.GetInt("ResolutionNum") == 0)										   // 1600 x 900
			settingDetailsTextList[0].text = "1600 x 900";
		else if ((PlayerPrefs.HasKey("ResolutionNum") && PlayerPrefs.GetInt("ResolutionNum") == 1) || !PlayerPrefs.HasKey("ResolutionNum")) // 1920 x 1080(기본값)
			settingDetailsTextList[0].text = "1920 x 1080";
		else if (PlayerPrefs.HasKey("ResolutionNum")  && PlayerPrefs.GetInt("ResolutionNum") == 2)									        // 2560 x 1440
			settingDetailsTextList[0].text = "2560 x 1440";
		
		if (PlayerPrefs.HasKey("VsyncNum") && PlayerPrefs.GetInt("VsyncNum") == 1)
			settingDetailsTextList[1].text = "켜기";
		else if (!PlayerPrefs.HasKey("VsyncNum") || PlayerPrefs.GetInt("VsyncNum") == 0)
			settingDetailsTextList[1].text = "끄기";
		
		if (PlayerPrefs.HasKey("FullScreenNum") && PlayerPrefs.GetInt("FullScreenNum") == 1)		   // 풀 스크린 on
			settingDetailsTextList[2].text = "켜기";
		else if (!PlayerPrefs.HasKey("FullScreenNum") || PlayerPrefs.GetInt("FullScreenNum") == 0) // 풀 스크린 off(기본값)
			settingDetailsTextList[2].text = "끄기";
		
		if ((PlayerPrefs.HasKey("FPSNum")     && PlayerPrefs.GetInt("FPSNum") == 0) || !PlayerPrefs.HasKey("FPSNum"))	  // 60 FPS (기본값)
			settingDetailsTextList[3].text = "60";
		else if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 1)										  // 100 FPS 
			settingDetailsTextList[3].text = "100";
		else if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 2)									      // 120 FPS
			settingDetailsTextList[3].text = "120";
		else if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 3)									      // 144 FPS 
			settingDetailsTextList[3].text = "144";
		
		settingDetailsTextList2[0].text = PlayerPrefs.GetInt("Master").ToString();
		
		settingDetailsTextList2[1].text = PlayerPrefs.GetInt("BGM").ToString();
			
		settingDetailsTextList2[2].text = PlayerPrefs.GetInt("SFX").ToString();
		
		// 켜기
		settingFrame.SetActive(true);
		menuKeyExUI.SetActive(false);
		
		// 뒷 UI 숨기기
		OffBackUI(false);
		
		// 타이틀 상태 설정(-> 켜면, 그래픽이 기본으로 강조되도록)
		foreach (var settingTitleNormalLists in settingTitleNormalList)
			settingTitleNormalLists.SetActive(true);
		foreach (var settingTitleHighlightLists in settingTitleHighlightList)
			settingTitleHighlightLists.SetActive(false);
		
		settingTitleNormalList[0].SetActive(false);
		settingTitleHighlightList[0].SetActive(true);
	}
	
	private void SettingOff()
	{
		AudioManager.instance.UISoundPlay(1);
	
		// 비강조 검정벽 끄기
		if (isStasisRoomMenu)
			stasisRoomBlackWall.gameObject.SetActive(false);
		
		// 뒷 UI 보이기
		OffBackUI(true);

		settingFrame.SetActive(false);
		menuKeyExUI.SetActive(true);	// 메인창 키 설명 UI 끄기
	}


	
	private IEnumerator GoToMainMenu()
	{
		AudioManager.instance.UISoundPlay(1);
	
		isNormalMenu                        = false; // 메뉴 움직이지 못하게
		EventController.instance.eventState = true;  // 몸 움직이지 못하게
		
		yield return StartCoroutine(FadeManager.instance.NextSeenFadeIn());
		SceneManager.LoadScene(mainMenuName);
	}
	
	// 저장된 게임으로 가기
	private IEnumerator Continue()			
	{
		// 제어권 뺏기
		isStasisRoomMenu = false;

		AudioManager.instance.UISoundPlay(1);
		
		// 페이드
		yield return StartCoroutine(FadeManager.instance.NextSeenFadeIn());
		
		// 저장된 씬으로 이동(저장된 씬 이름이 잇는 경우)
		if (PlayerPrefs.HasKey("SaveSeenName"))
		{
			// 저장된 씬 + 저장된 터미널이 있으면, 체력 및 게이지를 모두 회복하고, 돌아감.
			if (PlayerPrefs.HasKey("SaveTerminalName"))
			{
				// 체력 및 게이지 MAX값 저장.
				PlayerPrefs.SetInt("currentHP",PlayerHp.instance.maxHealth);                   // 체력 최대값으로 저장
				PlayerPrefs.SetFloat("currentGage",UIController.instance.gageSlider.maxValue); // 게이지 최대값으로 저장
			}

			SceneManager.LoadScene(PlayerPrefs.GetString("SaveSeenName"));
		}
	}

	private IEnumerator OpenNormalMenu()
	{
		AudioManager.instance.UISoundPlay(1);
		
		isNormalMenu = true;
		
		// 비강조 검정벽 켜기
		normalMenuBlackWall.gameObject.SetActive(true);
		
		titleGameObject.gameObject.SetActive(true);
		titleLineGameObject.gameObject.SetActive(true);
		lightBackgroundGameObject.gameObject.SetActive(true);

		// 텍스트
		foreach (var normalMenuOffImageLists in normalMenuOffImageList)
			normalMenuOffImageLists.gameObject.SetActive(true);
		
		normalMenuCurrentNum = 0;
		normalMenuOffImageList[0].gameObject.SetActive(false);
		normalMenuOnImageList[0].gameObject.SetActive(true);

		if(!EventController.instance.isStasisChamber)
			UIController.instance.UISeeState(false);
		menuKeyExUI.SetActive(true);
		
		// 포커스 UI
		focusGameObject.gameObject.SetActive(true);
		focusGameObject.transform.position = normalMenuOnImageList[normalMenuCurrentNum].rectTransform.transform.position;	// 포커스 이동
		
		// 시간만 바로 멈추기.(Time.fixedDeltaTime 변경 x)
		Time.timeScale      = 0f;
		
		isNormalCorutineEnd = true;	// 오픈 코루틴 종료
		
		yield return null;
	}
	
	private IEnumerator CloseNormalMenu()
	{
		AudioManager.instance.UISoundPlay(1);
	
		// 비강조 검정벽 켜기
		normalMenuBlackWall.gameObject.SetActive(false);
		
		titleGameObject.gameObject.SetActive(false);
		titleLineGameObject.gameObject.SetActive(false);
		lightBackgroundGameObject.gameObject.SetActive(false);
			
		isNormalMenu = false;
		
		foreach (var normalMenuOnImageLists in normalMenuOnImageList)
			normalMenuOnImageLists.gameObject.SetActive(false);
		foreach (var normalMenuOffImageLists in normalMenuOffImageList)
			normalMenuOffImageLists.gameObject.SetActive(false);

		if(!EventController.instance.isStasisChamber)
			UIController.instance.UISeeState(true);
		menuKeyExUI.SetActive(false);

		foreach (var focusRectTransformLists in focusRectTransformList)
			focusRectTransformLists.localScale = new Vector3(0f, 1f, 1f);

		// 시간만 바로 멈추기.(Time.fixedDeltaTime 변경 x)
		Time.timeScale      = 1f;
		
		isNormalCorutineEnd = true;	// 오픈 코루틴 종료
		
		yield return null;
	}
	
	private void InteractionMode()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			if (currentInteractionNum < interactionNormalImageList.Count - 1)
			{
				AudioManager.instance.UISoundPlay(0);
				
				interactionHighLightImageList[currentInteractionNum].SetActive(false);
				interactionNormalImageList[currentInteractionNum].SetActive(true);

				currentInteractionNum++;

				interactionHighLightImageList[currentInteractionNum].SetActive(true);
				interactionNormalImageList[currentInteractionNum].SetActive(false);
			}
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			if (currentInteractionNum > 0)
			{
				AudioManager.instance.UISoundPlay(0);
				
				interactionHighLightImageList[currentInteractionNum].SetActive(false);
				interactionNormalImageList[currentInteractionNum].SetActive(true);

				currentInteractionNum--;

				interactionHighLightImageList[currentInteractionNum].SetActive(true);
				interactionNormalImageList[currentInteractionNum].SetActive(false);
			}
		}
		else if (Input.GetKeyDown(KeyCode.Return) && !Input.GetKey(KeyCode.LeftAlt))
		{
			// 확인을 선택하고 있고 + 엔터
			if (currentInteractionNum == 0)
			{
				if (isNormalMenu)
				{ 
					switch (normalMenuCurrentNum)
					{
						case 2:
							StartCoroutine(GoToMainMenu()); // 메인 메뉴로 돌아가기
							break;
						case 3:
							Exit();							      // 게임 종료
							break;
					}
				}
				else if (isStasisRoomMenu)
				{
					switch (stasisRoomMenuCurrentNum)
					{
						case 0:
							InteractionOff();
							StartCoroutine(EventController.instance.NewStory());	// 새로운 스토리
							break;
						case 3:
							Exit();														// 게임 종료
							break;
					}
				}
			}
			// 취소를 선택하고 있고 + 엔터
			else if (currentInteractionNum == 1)
			{
				InteractionOff();
			}
		}
		// 상호작용 나가기
		else if (Input.GetKeyDown(KeyCode.Escape))
		{
			InteractionOff();
		}
	}
	
	private void InteractionOn()
	{
		AudioManager.instance.UISoundPlay(1);
	
		// 비강조 검정벽 켜기
		if (isStasisRoomMenu) // 메인 메뉴 조작 상태
			stasisRoomBlackWall.gameObject.SetActive(true);
		
		InteractionGameObject.SetActive(true);

		// 안내 텍스트
		foreach (var messageLists in messageList)
			messageLists.SetActive(false);
		if (isNormalMenu) // 노멀 메뉴
		{
			if (normalMenuCurrentNum == 2)	    // 메인메뉴 복귀 안내 텍스트
				messageList[0].gameObject.SetActive(true);
			else if (normalMenuCurrentNum == 3) // 게임 종료 안내 텍스트
				messageList[1].gameObject.SetActive(true);
		}
		else if (isStasisRoomMenu) // 메인 메뉴
		{
			if(stasisRoomMenuCurrentNum == 0)	// 새 게임 시작시 안내 메세지
				messageList[2].gameObject.SetActive(true); 
			if(stasisRoomMenuCurrentNum == 3)	// 게임 종료 안내 텍스트
				messageList[1].gameObject.SetActive(true); 
		}
		
		// 뒤 UI 끄기
		OffBackUI(false);
		
		// 취소 번호 초기화
		currentInteractionNum = 1;	
		
		// 취소를 강조함.
		interactionNormalImageList[0].SetActive(true);
		interactionHighLightImageList[0].SetActive(false);
		
		interactionNormalImageList[1].SetActive(false);
		interactionHighLightImageList[1].SetActive(true);
	}
	
	private void InteractionOff()
	{
		AudioManager.instance.UISoundPlay(1);
	
		// 비강조 검정벽 끄기
		if (isStasisRoomMenu)
			stasisRoomBlackWall.gameObject.SetActive(false);

		OffBackUI(true);

		InteractionGameObject.SetActive(false);
	}

	private void OffBackUI(bool isSee)
	{
		// 정체실 메뉴 뒷 UI 끄기
		if (isStasisRoomMenu)
		{
			if(isSee)
				titleQuadLogoAnim.SetTrigger("On");
			else
				titleQuadLogoAnim.SetTrigger("Off");
			titleLineSpriteRenderer.gameObject.SetActive(isSee);
			focusGameObject.gameObject.SetActive(isSee);
			
			foreach (var stasisRoomMenuOffImageLists in stasisRoomMenuOffImageList)
				stasisRoomMenuOffImageLists.gameObject.SetActive(isSee);
			
			if (isSee)
			{
				stasisRoomMenuOffImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(false);
				stasisRoomMenuOnImageList[stasisRoomMenuCurrentNum].gameObject.SetActive(true);
			}
			else
			{
				foreach (var stasisRoomMenuOnImageList in stasisRoomMenuOnImageList)
					stasisRoomMenuOnImageList.gameObject.SetActive(false);
			}
		}
		// 노멀 메뉴 뒷 UI 끄기
		else if(isNormalMenu)
		{
			if (isSee)
			{
				titleGameObject.SetActive(true);
				titleLineGameObject.SetActive(true);
			}
			else
			{
				titleGameObject.SetActive(false);
				titleLineGameObject.SetActive(false);
			}
			focusGameObject.gameObject.SetActive(isSee);
			
			foreach (var normalMenuOffImageLists in normalMenuOffImageList)
				normalMenuOffImageLists.gameObject.SetActive(isSee);
			
			if (isSee)
			{
				normalMenuOffImageList[normalMenuCurrentNum].gameObject.SetActive(false);
				normalMenuOnImageList[normalMenuCurrentNum].gameObject.SetActive(true);
			}
			else
			{
				foreach (var normalMenuOnImageLists in normalMenuOnImageList)
					normalMenuOnImageLists.gameObject.SetActive(false);
			}
		}
	}

	private void Exit()
	{
		AudioManager.instance.UISoundPlay(1);
		Application.Quit();
	}
	
	private void ResolutionSettingToggle()
	{
		AudioManager.instance.UISoundPlay(1);
		
		// 풀스크린 여부
		bool isFullScreen = false;
		if (PlayerPrefs.HasKey("FullScreenNum") && PlayerPrefs.GetInt("FullScreenNum") == 1)		   // 풀 스크린 on
			isFullScreen = true;
		else if (!PlayerPrefs.HasKey("FullScreenNum") || PlayerPrefs.GetInt("FullScreenNum") == 0) // 풀 스크린 off(기본값)
			isFullScreen = false;
	
		// 해상도 변경
		if (PlayerPrefs.HasKey("ResolutionNum") && PlayerPrefs.GetInt("ResolutionNum") == 0)													// 1600 x 900 -> 1920 x 1080 변경
		{
			PlayerPrefs.SetInt("ResolutionNum",1);
			settingDetailsTextList[0].text = "1920 x 1080";
			Screen.SetResolution(resolutionWidthList[1], resolutionHeightList[1], isFullScreen);
		}
		else if ((PlayerPrefs.HasKey("ResolutionNum") && PlayerPrefs.GetInt("ResolutionNum") == 1) || !PlayerPrefs.HasKey("ResolutionNum")) // 1920 x 1080(기본값) -> 2560 x 1440 변경
		{
			PlayerPrefs.SetInt("ResolutionNum",2);
			settingDetailsTextList[0].text = "2560 x 1440";
			Screen.SetResolution(resolutionWidthList[2], resolutionHeightList[2], isFullScreen);
		}
		else if (PlayerPrefs.HasKey("ResolutionNum") && PlayerPrefs.GetInt("ResolutionNum") == 2)											// 2560 x 1440 -> 1600 x 900 변경
		{
			PlayerPrefs.SetInt("ResolutionNum",0);
			settingDetailsTextList[0].text = "1600 x 900";
			Screen.SetResolution(resolutionWidthList[0], resolutionHeightList[0], isFullScreen);
		}
	}
	
	private void VsyncSettingToggle()
	{
		AudioManager.instance.UISoundPlay(1);
	
		if (PlayerPrefs.HasKey("VsyncNum") && PlayerPrefs.GetInt("VsyncNum") == 1)
		{
			PlayerPrefs.SetInt("VsyncNum",0);
			QualitySettings.vSyncCount = 0;
			settingDetailsTextList[currentSettingValueNum].text = "끄기";
		}
		else if(!PlayerPrefs.HasKey("VsyncNum") || PlayerPrefs.GetInt("VsyncNum") == 0)
		{
			PlayerPrefs.SetInt("VsyncNum",1);
			QualitySettings.vSyncCount = 1;
			settingDetailsTextList[currentSettingValueNum].text = "켜기";
		}
	}

	private void FullScreenSettingToggle()
	{
		AudioManager.instance.UISoundPlay(1);
	
		// 풀스크린 변경
		if (PlayerPrefs.HasKey("FullScreenNum") && PlayerPrefs.GetInt("FullScreenNum") == 1) // 풀 스크린 on -> off 변경
		{
			PlayerPrefs.SetInt("FullScreenNum", 0);
			Screen.SetResolution(resolutionWidthList[PlayerPrefs.GetInt("ResolutionNum")], resolutionHeightList[PlayerPrefs.GetInt("ResolutionNum")], false);
			settingDetailsTextList[currentSettingValueNum].text = "끄기";
		}
		else if (!PlayerPrefs.HasKey("FullScreenNum") || PlayerPrefs.GetInt("FullScreenNum") == 0) // 풀 스크린 off(기본값) -> on 변경
		{
			PlayerPrefs.SetInt("FullScreenNum", 1);
			Screen.SetResolution(resolutionWidthList[PlayerPrefs.GetInt("ResolutionNum")], resolutionHeightList[PlayerPrefs.GetInt("ResolutionNum")], true);
			settingDetailsTextList[currentSettingValueNum].text = "켜기";
		}
	}
	
	private void FullScreenToggleAltEnter()
	{
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
		{
			// 풀스크린 변경
			if (PlayerPrefs.HasKey("FullScreenNum") && PlayerPrefs.GetInt("FullScreenNum") == 1) // 풀 스크린 on -> off 변경
			{
				PlayerPrefs.SetInt("FullScreenNum", 0);
				Screen.SetResolution(resolutionWidthList[PlayerPrefs.GetInt("ResolutionNum")], resolutionHeightList[PlayerPrefs.GetInt("ResolutionNum")], false);
				settingDetailsTextList[2].text = "끄기";
			}
			else if (!PlayerPrefs.HasKey("FullScreenNum") || PlayerPrefs.GetInt("FullScreenNum") == 0) // 풀 스크린 off(기본값) -> on 변경
			{
				PlayerPrefs.SetInt("FullScreenNum", 1);
				Screen.SetResolution(resolutionWidthList[PlayerPrefs.GetInt("ResolutionNum")], resolutionHeightList[PlayerPrefs.GetInt("ResolutionNum")], true);
				settingDetailsTextList[2].text = "켜기";
			}
		}
	}
	
	private void FPSSettingToggle()
	{
		AudioManager.instance.UISoundPlay(1);
	
		if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 0 || !PlayerPrefs.HasKey("FPSNum"))		  // 60 FPS (기본값) -> 100
		{
			Application.targetFrameRate = 99;
			PlayerPrefs.SetInt("FPSNum", 1);
			settingDetailsTextList[currentSettingValueNum].text = "100";
		}
		else if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 1)									  // 100 FPS -> 120
		{
			Application.targetFrameRate = 119;
			PlayerPrefs.SetInt("FPSNum", 2);
			settingDetailsTextList[currentSettingValueNum].text = "120";
		}
		else if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 2)									  // 120 FPS -> 144
		{
			Application.targetFrameRate = 143;
			PlayerPrefs.SetInt("FPSNum", 3);
			settingDetailsTextList[currentSettingValueNum].text = "144";
		}
		else if (PlayerPrefs.HasKey("FPSNum") && PlayerPrefs.GetInt("FPSNum") == 3)									  // 144 FPS  -> 60
		{
			Application.targetFrameRate = 59;
			PlayerPrefs.SetInt("FPSNum", 0);
			settingDetailsTextList[currentSettingValueNum].text = "60";
		}
	}

	public IEnumerator OffImageSlowSee(bool isSee, float seeSpeed)
	{
		bool allOpaque = false;

		while (!allOpaque)
		{
			allOpaque = true; 

			foreach (var stasisRoomMenuOffImageLists in stasisRoomMenuOffImageList)
			{
				// 보이는지
				if (isSee)
				{
					stasisRoomMenuOffImageLists.color += new Color(0f, 0f, 0f, 1f) * Time.fixedDeltaTime * seeSpeed;
										
					if (stasisRoomMenuOffImageLists.color.a < 1f)
						allOpaque = false;
				}
				// 사라지는지
				else
				{
					stasisRoomMenuOffImageLists.color  -= new Color(0f, 0f, 0f, 1f) * Time.fixedDeltaTime * seeSpeed;

					if (stasisRoomMenuOffImageLists.color.a > 0f)
						allOpaque = false;
				}
				
			}
			
			// 타이틀 라인 스프라이트도 같이 보이도록.
			if (isSee)
			{
				titleLineSpriteRenderer.color += new Color(0f, 0f, 0f, 1f) * Time.fixedDeltaTime * seeSpeed;
				if (titleLineSpriteRenderer.color.a < 1f)
					allOpaque = false;
			}
			
			if (!isSee)
			{
				// 0번 하이라이트(게임시작은 하이라이트 상태이니, 0번 서서히 사라지기)
				foreach (var stasisRoomMenuOnImageLists in stasisRoomMenuOnImageList)
					stasisRoomMenuOnImageLists.color -= new Color(0f, 0f, 0f, 1f) * Time.fixedDeltaTime * seeSpeed;

				// 라인 스프라이트렌더러 투명도
				titleLineSpriteRenderer.color -= new Color(0f, 0f, 0f, 1f) * Time.fixedDeltaTime * seeSpeed;
				if (stasisRoomMenuOnImageList[0].color.a > 0f &&  titleLineSpriteRenderer.color.a > 0f)
					allOpaque = false;
			}
				
			yield return new WaitForFixedUpdate();
		}
	}

	public IEnumerator FocusUIScale(RectTransform focusFrame, float speed, float frameTargetX, float frameTargetY)
	{
		// FaceFrame 및 TextFrame을 조정합니다.
		float elapsedTime = 0f;
		Vector3 focusFrameScale = focusFrame.localScale;

		while (elapsedTime < 1f) // We're now interpolating between 0 and 1, rather than time.
		{
			float interpolationFactor = Mathf.Pow(elapsedTime, speed); // Adjust speed dynamically
			
			// FaceFrame 조정
			focusFrame.localScale = Vector3.Lerp(focusFrameScale, new Vector3(frameTargetX, frameTargetY, 1f), interpolationFactor);

			// 시간 업데이트
			elapsedTime += Time.unscaledDeltaTime;
            
			yield return null;
		}
		
		// 마지막으로 값을 정확하게 맞춰줍니다.
		focusFrame.localScale = new Vector2(frameTargetX, frameTargetY);
	}

	public void ResetSaveData()
	{
		PlayerPrefs.DeleteKey("SaveTerminalName");
		PlayerPrefs.DeleteKey("SaveSeenName");
		
		PlayerPrefs.DeleteKey("DemoClear");	// 다시 얼굴 보이도록 함.
		foreach (var spineZetaPartLists in EventController.instance.spineZetaPartList)
			spineZetaPartLists.gameObject.SetActive(true);
		
	}
}