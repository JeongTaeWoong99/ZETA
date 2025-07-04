using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Narration : MonoBehaviour
{
    public static Narration instance;
    
    [Header("------Tree Laws------")] 
    public  List<TextMeshProUGUI> treeLawsText   = new List<TextMeshProUGUI>();
    public  List<string>          treeLawsString = new List<string>();
    public  TMP_FontAsset         changeRedFont;                                // 0번 원칙 교체 폰트 에셋

    public List<GameObject>       zetaFaceSpinePartList = new List<GameObject>();
    public Animator               zetaFaceSpineAnimator;

    [Header("------Information------")] 
    public GameObject             informationFrameImage;
    public GameObject             keyFrameImage;
    
    public  List<TextMeshProUGUI> informationText   = new List<TextMeshProUGUI>();
    public  List<string>          informationString = new List<string>();
    private Coroutine             cursorCoroutine;

    [Header("------Demo End------")] 
    public TextMeshProUGUI demoEndText;
    public string          demoString;

    private void Awake()
    {
        instance = this;
    }
    
    public IEnumerator ThreeLaws()  // 모음 자음 1개씩 설명
    {
        // 0번 원칙
        EventController.instance.dosRainList.SetActive(true);   // 도스레인 true

        AudioManager.instance.DirectingPlay(0); // 도스레인 사운드 재생
        
        yield return new WaitForSeconds(5f);
        
        AudioManager.instance.DirectingPlay(7); // 타이핑 사운드 재생 on
        yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(treeLawsString[0],treeLawsText[0],false,false,false,0.025f));
        AudioManager.instance.DirectingStop(7); // 타이핑 사운드 재생 off
        
        yield return new WaitForSeconds(5f);
        
        // 0번 원칙 색 변화 + 글리치8 on + 원래 3개의 원칙 보이기 + 페이스 기계+파이트 및 케이블 사라지기
        for (int i = 1; i < 4; i++)  // 나레이션 스트링 갯수 만큼
        {
            treeLawsText[i].text = treeLawsString[i];
        }
        treeLawsText[0].fontMaterial = changeRedFont.material;
        SettingManager.instance.glitch8.enable.value  = true;
        
        AudioManager.instance.DirectingStop(0); // 도스레인 사운드 멈추기
        AudioManager.instance.DirectingPlay(1);        // 글리치 사운드 재생

        foreach (var zetaFaceSpinePartLists in zetaFaceSpinePartList)   // 파츠 없애기
            zetaFaceSpinePartLists.gameObject.SetActive(false);
        zetaFaceSpineAnimator.speed = 0;                                         // 애니메이션 멈추기

        yield return new WaitForSeconds(2f);
        
        // 모든 원칙 텍스트 없애기 + 글리치8 off + 도스레인 끄기
        AudioManager.instance.DirectingStop(1);           // 글리치 사운드 멈추기
        EventController.instance.dosRainList.SetActive(false);   // 도스레인 false
        SettingManager.instance.glitch8.enable.value = false;
        foreach (var narrationTexts in treeLawsText)
            narrationTexts.text = "";
            
        // 페이스 라이트 끄기 + 바디 라이트 켜기
        PlayerController.instance.bodyHighlightLight.gameObject.SetActive(true);		 // 바디 강조 라이트 끄기
        EventController.instance.zetaFaceLight.gameObject.SetActive(false);              // 얼굴 라이트 끄기
        
        // 카메라 전환
        CameraController.instance.target                   = EventController.instance.stasisChamberCameraPos[1];
        CameraController.instance.transform.position       = new Vector3(EventController.instance.stasisChamberCameraPos[1].position.x,EventController.instance.stasisChamberCameraPos[1].position.y,CameraController.instance.transform.position.z);
        CameraController.instance.moveSpeed                = EventController.instance.stasisChamberCameraMoveSpeed;
        CameraController.instance.mainCam.orthographicSize = EventController.instance.moveStateCameraSize;
        
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator Information() // 모음 자음 1개씩 설명
    {
        // 1~4 정보
        informationFrameImage.SetActive(true);
        AudioManager.instance.DirectingPlay(7); // 타이핑 사운드 재생 on
        
        for (int i = 0; i <= 7; i++) // 나레이션 스트링 갯수 만큼
            yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(informationString[i],informationText[i],false,true,false,0.025f));
        
        AudioManager.instance.DirectingStop(7); // 타이핑 사운드 재생 off
        
        // 성능 실험을 위해, 정체 상태를 해제합니다.
        yield return new WaitForSeconds(4f);
        
        AudioManager.instance.DirectingPlay(7); // 타이핑 사운드 재생 on
        
        keyFrameImage.SetActive(true);
        cursorCoroutine = StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(informationString[8],informationText[8],false,true,false,0.025f));
        yield return cursorCoroutine;
        
        AudioManager.instance.DirectingStop(7); // 타이핑 사운드 재생 off
        
        yield return new WaitForSeconds(4f);
    }

    public void DisableInformation() // 정보창 없애기
    {
        foreach (var informationTexts in informationText)
            informationTexts.text = "";
        
        informationFrameImage.SetActive(false);
        keyFrameImage.GetComponent<Animator>().SetTrigger("end");
        
        StopCoroutine(cursorCoroutine); // 커서 코루틴 종료 ★
    }

    public IEnumerator DemoEndNarration() // 모음 자음 1개씩 설명
    {
        AudioManager.instance.DirectingPlay(7);        // 타이핑 사운드 재생 on
        yield return StartCoroutine(KoreanTyperDemo_Cursor.instance.TypingCoroutine(demoString,demoEndText,false,true,false,0.025f));
        AudioManager.instance.DirectingStop(7); // 타이핑 사운드 재생 off

        yield return new WaitForSeconds(5);
    }
}
