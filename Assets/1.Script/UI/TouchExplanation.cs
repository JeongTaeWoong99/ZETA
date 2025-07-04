using System.Collections;
using TMPro;
using UnityEngine;

public class TouchExplanation : MonoBehaviour
{
    [Header("------Terminal------")]
    public ElevatorTerminal        elevatorTerminal;
    public SaveTerminal            saveTerminal;
    public HackingPossibleTerminal hackingPossibleTerminal;
    public TestTerminal            testTerminal;
    
    [Header("------Explanation------")]
    public  GameObject           explanationCanvas;   // 켄버스 게임 오브젝트
    public  TextMeshProUGUI      explanationText;     // 텍스트
    public  UnityEngine.UI.Image boardImage;          // 변경할 보드 이미지 컴포넌트
    
    private Coroutine            touchEnterCoroutine; // 실행중인 코루틴 정보
    
    private bool                 isTouch;             // 닿고 있는지
    public  bool                 noDistinguish;       // 노멀과 전투 구분이 필요하지 않은 설명창

    [Header("------Normal------")]
    public string        normalString;
    public Sprite        normalFrameSprite;
    public TMP_FontAsset normalFont;
    public Color         normalTextColor;
    
    [Header("------InBattle------")]
    public string        inBattleString;
    public Sprite        inBattleFrameSprite;
    public TMP_FontAsset inBattleFont;
    public Color         inBattleTextColor;
    
    private void FixedUpdate()
    {
        // 노멀과 전투를 따로 설명창을 띄워줘야 하는 겅우
        if (!noDistinguish)
        {
            // 엘리베이터 터미널 + 장애물 터미널 작동 중
            // -> 켄버스 끄기
            if ((elevatorTerminal        != null && (elevatorTerminal.isMoving               || elevatorTerminal.isControlMode))        ||
                (saveTerminal            != null && saveTerminal.isSaving)                                                              ||
                (hackingPossibleTerminal != null && (!hackingPossibleTerminal.isScanPossible || hackingPossibleTerminal.isControlMode)) ||
                (testTerminal            != null && (testTerminal.isTest                     || testTerminal.isControlMode)))
            {
                TouchExit();
            }
            // 엘리베이터 터미널 + 장애물 터미널 작동 하지 않고, 터치 상태이고,켄버스가 꺼져 있다면
            // -> 켄버스 켜기
            else if (isTouch && !explanationCanvas.gameObject.activeInHierarchy && ((elevatorTerminal        != null && !elevatorTerminal.isMoving             && !elevatorTerminal.isControlMode)        ||
                                                                                    (saveTerminal            != null && !saveTerminal.isSaving)                                                           ||
                                                                                    (hackingPossibleTerminal != null && hackingPossibleTerminal.isScanPossible && !hackingPossibleTerminal.isControlMode) ||
                                                                                    (testTerminal            != null && !testTerminal.isTest                   && !testTerminal.isControlMode) ))
            {
                TouchEnter();
            }
            // 닿아 있는 상태이고, 쫒는 적이 없는데, 쫒기 설명창이 켜져 있으면
            // 설명창 변경
            else if (isTouch && EnemyDistanceActive.instance.enemyChaseList.Count == 0 && boardImage.sprite == inBattleFrameSprite)
            {
                TouchExit();
                TouchEnter();
            }
            // 닿아 있는 상태이고, 쫒는 적이 있는데, 노멀 설명창이 켜져 
            // 설명창 변경
            else if (isTouch && EnemyDistanceActive.instance.enemyChaseList.Count != 0 && boardImage.sprite == normalFrameSprite)
            {
                TouchExit();
                TouchEnter();
            }
        }
    }
    
    private IEnumerator TouchExplanationString()
    {
        // 노멀(쫒는 적 없음)
        if (EnemyDistanceActive.instance.enemyChaseList.Count == 0)
        {
            boardImage.sprite     = normalFrameSprite; // 보드 변경
            explanationText.font  = normalFont;        // 폰트 변경 
            explanationText.color = normalTextColor;   // 컬러 교체
            
            for (int j = 0; j < normalString.Length + 1; j++)
            {
                explanationText.text = normalString.Substring(0, j);
                yield return new  WaitForSecondsRealtime(0.05f);
            }
        }
        // 적투 중(쫒는 적 있음)
        else
        {
            boardImage.sprite     = inBattleFrameSprite; // 보드 변경
            explanationText.font  = inBattleFont;        // 폰트 변경 
            explanationText.color = inBattleTextColor;   // 컬러 교체
            
            for (int j = 0; j < inBattleString.Length + 1; j++)
            {
                explanationText.text = inBattleString.Substring(0, j);
                yield return new  WaitForSecondsRealtime(0.05f);
            }
        }
    }

    private IEnumerator TouchExplanationStringNoDistinguish()
    {
        boardImage.sprite     = normalFrameSprite; // 보드 변경
        explanationText.font  = normalFont;        // 폰트 변경
        explanationText.color = normalTextColor;   // 컬러 교체
        
        for (int j = 0; j < normalString.Length + 1; j++)
        {
            explanationText.text = normalString.Substring(0, j);
            yield return new  WaitForSecondsRealtime(0.05f);
        }
    }

    private void TouchEnter()
    {
        explanationCanvas.SetActive(true);  // 캔버스 켜기
            
        if (touchEnterCoroutine != null)    // 캔버스 초기화
        {
            explanationText.text = "";
            StopCoroutine(touchEnterCoroutine);
        }
        
        // 구분 실행 필요
        if(!noDistinguish)
            touchEnterCoroutine = StartCoroutine(TouchExplanationString());             
        // 구분 실행 불필요
        else if (noDistinguish)
            touchEnterCoroutine = StartCoroutine(TouchExplanationStringNoDistinguish());
    }

    private void TouchExit()
    {
        explanationCanvas.SetActive(false);
        explanationText.text = "";
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isTouch = true; // 터치상태
            
            // 구분 필요 및 상태를 보고 실행
            if (!noDistinguish && ((elevatorTerminal        != null && !elevatorTerminal.isMoving             && !elevatorTerminal.isControlMode)        ||
                                   (saveTerminal            != null && !saveTerminal.isSaving)                                                           ||
                                   (hackingPossibleTerminal != null && hackingPossibleTerminal.isScanPossible && !hackingPossibleTerminal.isControlMode) || 
                                   (testTerminal            != null && !testTerminal.isTest                   && !testTerminal.isControlMode)))
            TouchEnter();
            // 구분 불필요
            else if(noDistinguish)
                TouchEnter();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isTouch = false; // 터치상태
            TouchExit();
        }
    }
}

