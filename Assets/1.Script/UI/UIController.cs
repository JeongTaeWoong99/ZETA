using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("------State------")] 
    public GameObject      HPMPFrame;
    
    public Slider          healthSlider;               // UI의 Slider    참조
    public TextMeshProUGUI healthText;
    
    public Slider           gageSlider;
    public float            gageSliderMaxvalue;
    public TextMeshProUGUI  gageText;
    public List<GameObject> gageBarList = new List<GameObject>();

    [Header("------Acceleration------")]
    public Slider accelerationTimeRemainingSlider;
    
    [Header("------Hacking------")]
    public Slider hackingTimeRemainingSlider;

    [Header("------Scan------")] 
    public GameObject      scanKeyExUI;                     // 스캔 키설명 UI
    public TextMeshProUGUI scanStateText;                   // 스캔 상태 UI

    [Header("------FPS------")]
    public  TMP_Text FPSText;
    private bool     isShow;
    private float    deltaTime = 0f;
    
    [Header("------SeenSkip------")] 
    public string previousSceneName;
    public string nextSceneName;
    
    [Header("------Title------")] 
    public  RectTransform   titleImage;      // 스테이지 프레임 이미지

    public  TextMeshProUGUI titleText;
    public  string          titleString;

    [Header("------Dialog------")]
    public  List<Image>           dialogFaceFrameList      = new List<Image>();           // 페이스 프레임
    public  List<Image>           dialogFaceList           = new List<Image>();           // 페이스
    public  List<Image>           dialogTextFrameList      = new List<Image>();           // 텍스트 프레임
    
    public  List<TextMeshProUGUI> dialogNpcTextList        = new List<TextMeshProUGUI>(); // Npc텍스트
    public  List<string>          dialogNpcNameString      = new List<string>();
    
    public  List<TextMeshProUGUI> dialogTextList           = new List<TextMeshProUGUI>(); // 텍스트
    public  List<TextMeshProUGUI> dialogEnterList          = new List<TextMeshProUGUI>(); // 엔터 이미지
    
    private List<bool>            isDialogUIOn             = new List<bool>();            // UI 켜져있는지 체크

    public  List<string>          dialogString             = new List<string>();
    [HideInInspector]
    public  int                   dialogStringNum;

    [Header("------Mission------")] 
    public RectTransform missionHeadFrameRect;
    public RectTransform missionBodyFrameRect;
    
    public TMP_FontAsset completeFont;
    public TMP_FontAsset incompleteFont;
    
    public int completePontSize;
    public int incompletePontSize;

    public  List<TextMeshProUGUI> missionTextList = new List<TextMeshProUGUI>(); // 텍스트
    public  List<string>          missionString   = new List<string>();          // 미션 대사
    private int                   missionStringNum;
    
    public  List<Toggle> missionCheckBoxList = new List<Toggle>();
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        // 게이지 값 설정
        if (EventController.instance.isStasisChamber || EventController.instance.isPerformanceLab) // 정체실 OR 성능실험실
        {
            gageSlider.maxValue = 0f;   // 성능실험실 봉인 해제되고 변경.
            gageSlider.value    = 0f;
        }
        else // 나머지(저장된 플레이어프리팹 정보로 불러오기)
        {
            gageSlider.maxValue = gageSliderMaxvalue;                  // 최대값 설정
            gageSlider.value = PlayerPrefs.GetFloat("currentGage"); // 저장된 게이지 
        }

        // 타이틀
        titleImage.localScale = new Vector3(0f,1f,0f); // 스케일 초기화
        titleText.text = "";
        
        // 대화 초기화
        foreach (var dialogFaceImageLists in dialogFaceFrameList)            // 페이스 프레임 초기화
            dialogFaceImageLists.rectTransform.localScale = new Vector2(0f, 0f);
        foreach (var dialogFaceLists in dialogFaceList)                // 페이스 초기화
            dialogFaceLists.color = new Color(1f, 1f, 1f, 0f);
        foreach (var dialogTextFrameLists in dialogTextFrameList)       // 텍스트 프레임 초기화
            dialogTextFrameLists.rectTransform.localScale = new Vector2(0f,1f);
        foreach (var dialogFaceFrameTextLists in dialogTextList)             // 텍스트 초기화
            dialogFaceFrameTextLists.text = "";

        foreach (var t in dialogFaceFrameList)                 // false 리스트 초기화
            isDialogUIOn.Add(false);
        
        // 미션창 초기화
        missionHeadFrameRect.localScale = new Vector3(0f,1f,0f); // 스케일 초기화
        missionBodyFrameRect.localScale = new Vector3(1f,0f,0f); // 스케일 초기화
        foreach (var missionTextLists in missionTextList)
        {
            missionTextLists.font     = incompleteFont;
            missionTextLists.fontSize = incompletePontSize;
            missionTextLists.text     = "";
        }
        foreach (var missionStateToggleLists in missionCheckBoxList)
        {
            missionStateToggleLists.gameObject.SetActive(false);
            missionStateToggleLists.isOn = false;
        }
    }

    private void Update()
    {
        GageState();    // 게이지 이미지 관리
        
        // 윤니티 빌드 버전에서는 돌아가지 않도록 함.
#if UNITY_EDITOR
        FPS();
        
        SeenSkip();
        
        if (Input.GetKeyDown(KeyCode.F4))
            MenuManager.instance.ResetSaveData();
    
        if (Input.GetKeyDown(KeyCode.F5))
            PlayerHp.instance.invincibleMode = true;
#endif
    }
    
    private void GageState()
    {
        int gageValue = (int)(gageSlider.value * 10f) ;
        gageText.text = gageValue.ToString();
        for (int i = 0; i < gageBarList.Count; i++)
            gageBarList[i].SetActive(i < gageValue);
    }

    private void FPS()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (isShow)
            {
                isShow = false;
                FPSText.gameObject.SetActive(false);
            }
            else
            {
                isShow = true;
                FPSText.gameObject.SetActive(true);
            }
        }
    
        if (isShow)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float ms = deltaTime * 1000f;
            float fps = 1.0f / deltaTime;
            FPSText.text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
        }
    }

    private void SeenSkip()
    {
        if (Input.GetKeyDown(KeyCode.F3) && nextSceneName.Length != 0)
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if (Input.GetKeyDown(KeyCode.F2) && previousSceneName.Length != 0)
        {
            SceneManager.LoadScene(previousSceneName);
        }
    }
    
    // UI 비강조
    private void HighlightDialogUI(int controllerNum, bool isHighLight)
    {
        float highLightAlpha = 1f;
        float ignoreAlpha    = 0.5f;
        float changeAlpha    = isHighLight ? highLightAlpha : ignoreAlpha;

        // 기존 색상 가져오기
        Color faceFrameColor = dialogFaceFrameList[controllerNum].color;
        Color faceListColor  = dialogFaceList[controllerNum].color;
        Color textFrameColor = dialogTextFrameList[controllerNum].color;
        Color textColor      = dialogTextList[controllerNum].color;

        // 알파 값만 변경
        faceFrameColor.a = changeAlpha;
        faceListColor.a  = changeAlpha;
        textFrameColor.a = changeAlpha;
        textColor.a      = changeAlpha;

        // 변경된 색상 적용
        dialogFaceFrameList[controllerNum].color = faceFrameColor;
        if (isDialogUIOn[controllerNum])
            dialogFaceList[controllerNum].color  = faceListColor;
        dialogTextFrameList[controllerNum].color = textFrameColor;
        dialogTextList[controllerNum].color      = textColor;
    }

    public IEnumerator MissionUIScale(RectTransform missionFrame,float speed, float frameTargetX, float frameTargetY)
    {
        // FaceFrame 및 TextFrame을 조정합니다.
        float   elapsedTime = 0f;
        Vector3 missionFrameScale = missionFrame.localScale;

        while (elapsedTime < 1f) // We're now interpolating between 0 and 1, rather than time.
        {
            float interpolationFactor = Mathf.Pow(elapsedTime, speed); // Adjust speed dynamically

            // FaceFrame 조정
            missionFrame.localScale = Vector3.Lerp(missionFrameScale, new Vector3(frameTargetX, frameTargetY, 1f), interpolationFactor);

            // 시간 업데이트
            elapsedTime += Time.deltaTime;
            
            yield return null;
        }
        
        // 마지막으로 값을 정확하게 맞춰줍니다.
        missionFrame.localScale = new Vector2(frameTargetX, frameTargetY);
    }
    
    private IEnumerator TitleUIScale(float speed, float frameTargetX, float frameTargetY)
    {
        // FaceFrame 및 TextFrame을 조정합니다.
        float elapsedTime = 0f;
        Vector3 missionFrameScale = titleImage.localScale;

        while (elapsedTime < 1f) // We're now interpolating between 0 and 1, rather than time.
        {
            float interpolationFactor = Mathf.Pow(elapsedTime, speed); // Adjust speed dynamically

            // FaceFrame 조정
            titleImage.localScale = Vector3.Lerp(missionFrameScale, new Vector3(frameTargetX, frameTargetY, 1f), interpolationFactor);

            // 시간 업데이트
            elapsedTime += Time.unscaledDeltaTime;
            
            yield return null;
        }

        // 마지막으로 값을 정확하게 맞춰줍니다.
        titleImage.localScale = new Vector2(frameTargetX, frameTargetY);
    }
                    
    public IEnumerator DialogUIScale(int controllerNum, float speed, float faceFrameTargetX, float faceFrameTargetY, float faceColorA, float textFrameTargetX, float textFrameTargetY)
    {
        // faceColorA가 0이면 먼저 Face 나타나기(+ 텍스트 비우기)
        if (faceColorA == 0f)
        {
            AudioManager.instance.UISoundPlay(3);                                            // 닫히는 사운드
            
            isDialogUIOn[controllerNum] = false;                                             // UI 상태 true

            dialogTextList[controllerNum].text = "";                                         // 텍스트 비우기
            dialogNpcTextList[controllerNum].text  = "";
            
            dialogFaceList[controllerNum].gameObject.SetActive(false);
            dialogFaceList[controllerNum].color = new Color(1f, 1f, 1f, faceColorA); // 값 맞추기
        }
        else if (faceColorA == 1f)
        { 
            AudioManager.instance.UISoundPlay(2);                                            // 열리는 사운드
        }
        HighlightDialogUI(controllerNum,false);                                     // 커지든 작아지든, 무조건 일단 비포커스 함.
        
        // FaceFrame 및 TextFrame을 조정합니다.
        dialogFaceFrameList[controllerNum].gameObject.SetActive(true);
        dialogTextFrameList[controllerNum].gameObject.SetActive(true);
        
        float elapsedTime = 0f;
        Vector3 faceFrameScale = dialogFaceFrameList[controllerNum].rectTransform.localScale;
        Vector3 textFrameScale = dialogTextFrameList[controllerNum].rectTransform.localScale;
        
        while (elapsedTime < 1f) // We're now interpolating between 0 and 1, rather than time.
        {
            float interpolationFactor = Mathf.Pow(elapsedTime, speed); // Adjust speed dynamically

            // FaceFrame 조정
            dialogFaceFrameList[controllerNum].rectTransform.localScale = Vector3.Lerp(faceFrameScale, new Vector3(faceFrameTargetX, faceFrameTargetY, 1f), interpolationFactor);
        
            // TextFrame 조정
            dialogTextFrameList[controllerNum].rectTransform.localScale = Vector3.Lerp(textFrameScale, new Vector3(textFrameTargetX, textFrameTargetY, 1f), interpolationFactor);
            
            // 시간 업데이트
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // 마지막으로 값을 정확하게 맞춰줍니다.
        dialogFaceFrameList[controllerNum].rectTransform.localScale = new Vector2(faceFrameTargetX, faceFrameTargetY);
        dialogTextFrameList[controllerNum].rectTransform.localScale = new Vector2(textFrameTargetX, textFrameTargetY);
        
        // faceColorA가 1이면 Face 나중에 나타나기
        if (faceColorA == 1f)
        {
            isDialogUIOn[controllerNum] = true;                                              // UI 상태 true
            
            dialogFaceList[controllerNum].gameObject.SetActive(true);
            dialogFaceList[controllerNum].color = new Color(1f, 1f, 1f, faceColorA); // 값 맞추기
            
            dialogNpcTextList[controllerNum].text  = dialogNpcNameString[controllerNum];
        }
        yield return new WaitForSeconds(1f);
    }
    
    public IEnumerator Dialog(int controllerNum) // 1자씩
    {
        // 텍스트 비우기
        dialogTextList[controllerNum].text = "";
    
        // 바로 입력 방지(스페이스)
        yield return new WaitForSeconds(0.1f);
        
        if (controllerNum == 0)
        {
            HighlightDialogUI(0,true);
            HighlightDialogUI(1,false);
        }
        else
        {
            HighlightDialogUI(1,true);
            HighlightDialogUI(0,false);
        }
        
        // 대사 재생
        dialogTextList[controllerNum].gameObject.SetActive(true);
        AudioManager.instance.DirectingPlay(7); // 타이핑 사운드 재생 on
        
        for (int j = 0; j < dialogString[dialogStringNum].Length + 1; j++)
        {
            float stringTimeCount = 0f;             // 시간 체크 초기화
            
            while (true)
            {
                stringTimeCount += Time.deltaTime; // 시간 체크
                
                // 대사 나오는 중 스킵
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    dialogTextList[controllerNum].text = dialogString[dialogStringNum]; // 대사 모두 보이기
                    j = dialogString[dialogStringNum].Length + 1;                       // break 후 for문 돌아가지 않도록
                    AudioManager.instance.UISoundPlay(1);
                    break;
                }
                
                // 다음 대사 재생
                if (stringTimeCount > 0.05)
                {
                    dialogTextList[controllerNum].text = dialogString[dialogStringNum].Substring(0, j);
                    break;
                }
                yield return null;
            }
        }
        
        AudioManager.instance.DirectingStop(7); // 타이핑 사운드 재생 off
        
        // 바로 입력 방지(스페이스)
        yield return new WaitForSeconds(0.1f);
        
        float enterTimeCount = 0f; // 시간 체크 초기화
        // 대사 코루틴 나가기
        while (true)
        {
            enterTimeCount += Time.deltaTime;    // 시간 체크
            
            // 나가기
            if (Input.GetKeyDown(KeyCode.Return))
            {
                dialogEnterList[controllerNum].gameObject.SetActive(false); // 끄기
                dialogStringNum++;                                          // 번호 증가
                AudioManager.instance.UISoundPlay(1);
                break;
            }
            
            // 엔터 블링크
            if (enterTimeCount > 0.25)
            {
                // 엔터가 켜져 있으면
                if (dialogEnterList[controllerNum].gameObject.activeInHierarchy)
                {
                    enterTimeCount = 0f;                                        // 시간 체크 초기화
                    dialogEnterList[controllerNum].gameObject.SetActive(false); // 끄기
                }
                // 엔터가 꺼져있으면
                else
                {
                    enterTimeCount = 0f;                                       // 시간 체크 초기화
                    dialogEnterList[controllerNum].gameObject.SetActive(true); // 켜기
                }
            }

            yield return null;
        }
    }
    
    public IEnumerator TitleActiveCoroutine() // 1자씩
    {
        yield return StartCoroutine(TitleUIScale(20f,1f,1f)); // 타이틀 크기 크게
        yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(titleString,titleText,false,false,false,0.05f));
        
        yield return new WaitForSecondsRealtime(2f);
        
        titleText.text = ""; // 텍스트 비우기
        StartCoroutine(TitleUIScale(20f,0f,1f)); // 타이틀 크기 작게
    }

    public void UISeeState(bool isSee)
    {
        HPMPFrame.gameObject.SetActive(isSee);
        healthSlider.gameObject.SetActive(isSee);
        healthText.gameObject.SetActive(isSee);
        gageText.gameObject.SetActive(isSee);
        //gageSlider.gameObject.SetActive(isSee);
    }

    public void MissionCompleteSettings(int missionNum)
    {
        missionCheckBoxList[missionNum].isOn = true;
        missionTextList[missionNum].font     = completeFont;
        missionTextList[missionNum].fontSize = completePontSize;
    }
    
    public IEnumerator MissionResetSettings()
    {
        foreach (var missionTextLists in missionTextList)
        {
            missionTextLists.font     = incompleteFont;
            missionTextLists.fontSize = incompletePontSize;
            missionTextLists.text     = "";
        }
        foreach (var missionStateToggleList in missionCheckBoxList)
        {
            missionStateToggleList.gameObject.SetActive(false);
            missionStateToggleList.isOn = false;
        }
        yield return StartCoroutine(MissionUIScale(missionBodyFrameRect,10f,1f,0f)); // 미션창 바디 닫기
    }
}