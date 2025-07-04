using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestTerminal : MonoBehaviour
{
    private BoxCollider2D box2D;
    
    public Transform designatedLocation;
    
    [HideInInspector] 
    public  bool isTest;
    private bool isTouchoperationPossible;

    [HideInInspector] 
    public bool       isControlMode; // 컨트롤 모드
    
    public GameObject            controlUIGameObject;
    public List<TextMeshProUGUI> selectTextList = new List<TextMeshProUGUI>();
    public Image                 focusImage;
    
    [Header("------Highlight Text------")] 
    public TMP_FontAsset highlightFont;
    public Color         highlightTextColor;

    [Header("------Normal Text------")]
    public TMP_FontAsset normalFont;
    public Color         normalTextColor;
    
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public  int     setTurretNum;  // 테스트 통과하면, 숫자 증가

    private void Awake()
    {
        animator = GetComponent<Animator>();
        box2D    = GetComponent<BoxCollider2D>();
    }
    
    private void Start()
    {
        box2D.enabled = false;
    }

    public void Update()
    {
        // 쫒을 때, 작동불가
        if (Input.GetKeyDown(KeyCode.F) && !isTest && !isControlMode && EnemyDistanceActive.instance.enemyChaseList.Count == 0 && !PlayerHp.instance.isHit && isTouchoperationPossible && !PlayerScan.instance.isScan && !PlayerHp.instance.isRecovery && 
            PlayerFloorCollider.instance.isGrounded && !PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking && !PlayerDash.instance.isDash)
        {
            isControlMode = true;
            EventController.instance.eventState = true;
        
            UIController.instance.UISeeState(false);
            
            StartCoroutine(MoveDesignatedLocation()); // 이동
        }
    }
    
    private void FixedUpdate()
    {
        animator.speed = PlayerAcceleration.instance.accelerationChangedTimeValue;
    }
    
    private IEnumerator MoveDesignatedLocation()
    {
        while (true)
        {
            // 이동 종료
            if (Vector2.Distance(PlayerController.instance.transform.position, designatedLocation.position) < 0.25f)
            {
                // 남아있는 이동값 제거
                yield return new WaitForFixedUpdate(); // 이전 입력 겹치기 방지 대기
                PlayerController.instance.rb2D.velocity = Vector2.zero;
                PlayerController.instance.playerAnim.SetBool("run", false);
                
                // 좌우반전(터미널 바라보기)
                PlayerController.instance.bodyGameObject.transform.localScale = PlayerController.instance.transform.position.x < transform.position.x ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);
                
                // 정확한 x위치 이동
                PlayerController.instance.gameObject.transform.position = new Vector3(designatedLocation.position.x,
                    PlayerController.instance.gameObject.transform.position.y);

                StartCoroutine(ControlMode());
                break;
            }

            // 이동
            if (PlayerController.instance.transform.position.x < designatedLocation.position.x)
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

            yield return new WaitForFixedUpdate();
        }
    }

    // private IEnumerator ControlMode()
    // {
    //     // 위치도착 후, Idle자세가 되면, SetTrigger("interaction1On")
    //     while (true)
    //     {
    //         if (PlayerController.instance.playerAnimStateInfo.IsName("Idle"))
    //         {
    //             PlayerController.instance.playerAnim.SetTrigger("interaction1On");
    //             break;
    //         }
    //
    //         yield return new WaitForFixedUpdate();
    //     }
    //
    //     // 인터렉션 자세가 되는지 확인
    //     while (true)
    //     {
    //         if (PlayerController.instance.playerAnimStateInfo.IsName("Interaction1"))
    //             break;
    //
    //         yield return new WaitForFixedUpdate();
    //     }
    //     
    //     int currentSelectedNum = 1; // 취소 부터 선택
    //     focusImage.rectTransform.transform.position = selectTextList[currentSelectedNum].rectTransform.transform.position;
    //     selectTextList[currentSelectedNum].font     = highlightFont;
    //     selectTextList[currentSelectedNum].color    = highlightTextColor;
    //     
    //     selectTextList[0].font                      = normalFont;
    //     selectTextList[0].color                     = normalTextColor;
    //     
    //     controlUIGameObject.SetActive(true);                         // 컨트롤 UI 켜기
    //     MenuManager.instance.menuKeyExUI.gameObject.SetActive(true); // 키 설명
    //     
    //     while (true)
    //     {
    //         if (Input.GetKeyDown(KeyCode.Return))
    //         {
    //             if (currentSelectedNum == 0)
    //             {
    //                 TestStart();
    //                 
    //                 PlayerController.instance.playerAnim.SetTrigger("interaction1Off");
    //                 AudioManager.instance.ObjectSfxCreate(3, true, gameObject); // 인터렉션
    //                 
    //                 controlUIGameObject.SetActive(false);
    //                 MenuManager.instance.menuKeyExUI.gameObject.SetActive(false);
    //                 
    //                 // 대화창 보이기
    //                 yield return StartCoroutine(UIController.instance.DialogUIScale(0,20f,1f,1f,1f,1f,1f)); // R 대화창 보이기
		  //       
    //                 yield return new WaitForSeconds(1f);
    //                 
    //                 yield return StartCoroutine(UIController.instance.Dialog(0));
    //                 yield return StartCoroutine(UIController.instance.Dialog(0));
		  //           
    //                 StartCoroutine(UIController.instance.DialogUIScale(0,20f,0f,0f,0f,0f,1f));              // R 대화창 숨기기
    //                 
    //                 yield return new WaitForSeconds(2f);
    //                 
    //                 UIController.instance.UISeeState(true);
    //                 
    //                 isControlMode = false;
    //                 EventController.instance.eventState = false;
    //
    //                 TurretActive();
    //                 
    //                 break;
    //             }
    //             
    //             if (currentSelectedNum == 1)
    //             {
    //                 StartCoroutine(EscapeControlMode());
    //                 break;
    //             }
    //         }
    //         else if (Input.GetKeyDown(KeyCode.DownArrow) && currentSelectedNum == 0)
    //         {
    //             AudioManager.instance.UISoundPlay(0);   // UI 이동 사운드
    //             
    //             selectTextList[currentSelectedNum].font  = normalFont;
    //             selectTextList[currentSelectedNum].color = normalTextColor;
    //             
    //             currentSelectedNum++;
    //             
    //             focusImage.rectTransform.transform.position = selectTextList[currentSelectedNum].rectTransform.transform.position;
    //             
    //             selectTextList[currentSelectedNum].font  = highlightFont;
    //             selectTextList[currentSelectedNum].color = highlightTextColor;
    //         }
    //         else if (Input.GetKeyDown(KeyCode.UpArrow) && currentSelectedNum == 1)
    //         {
    //             AudioManager.instance.UISoundPlay(0);   // UI 이동 사운드
    //             
    //             selectTextList[currentSelectedNum].font  = normalFont;
    //             selectTextList[currentSelectedNum].color = normalTextColor;
    //             
    //             currentSelectedNum--;
    //             
    //             focusImage.rectTransform.transform.position = selectTextList[currentSelectedNum].rectTransform.transform.position;
    //             
    //             selectTextList[currentSelectedNum].font  = highlightFont;
    //             selectTextList[currentSelectedNum].color = highlightTextColor;
    //         }
    //         else if (Input.GetKeyDown(KeyCode.Escape))
    //         {
    //             StartCoroutine(EscapeControlMode());
    //             break;
    //         }
    //
    //         yield return null;
    //     }
    // }
    
    private IEnumerator ControlMode()
    {
        // Idle 상태를 기다린 후, "interaction1On" 트리거 설정
        yield return new WaitUntil(() => PlayerController.instance.playerAnimStateInfo.IsName("Idle"));
        PlayerController.instance.playerAnim.SetTrigger("interaction1On");
    
        // Interaction1 상태를 기다림
        yield return new WaitUntil(() => PlayerController.instance.playerAnimStateInfo.IsName("Interaction1"));
    
        int currentSelectedNum = 1; // 취소 부터 선택
        UpdateSelectionUI(currentSelectedNum);
    
        controlUIGameObject.SetActive(true);                         // 컨트롤 UI 켜기
        MenuManager.instance.menuKeyExUI.gameObject.SetActive(true); // 키 설명
        
        bool isAwaitingSelection = true;
        while (isAwaitingSelection)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (currentSelectedNum == 0)
                {
                    yield return StartCoroutine(HandleTestStart());
                    isAwaitingSelection = false;
                }
                else if (currentSelectedNum == 1)
                {
                    yield return StartCoroutine(EscapeControlMode());
                    isAwaitingSelection = false;
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && currentSelectedNum == 0)
            {
                AudioManager.instance.UISoundPlay(0);   // UI 이동 사운드
                currentSelectedNum++;
                UpdateSelectionUI(currentSelectedNum);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) && currentSelectedNum == 1)
            {
                AudioManager.instance.UISoundPlay(0);   // UI 이동 사운드
                currentSelectedNum--;
                UpdateSelectionUI(currentSelectedNum);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                yield return StartCoroutine(EscapeControlMode());
                isAwaitingSelection = false;
            }
    
            yield return null;
        }
    }

    private void UpdateSelectionUI(int selectedNum)
    {
        for (int i = 0; i < selectTextList.Count; i++)
        {
            if (i == selectedNum)
            {
                selectTextList[i].font = highlightFont;
                selectTextList[i].color = highlightTextColor;
                focusImage.rectTransform.transform.position = selectTextList[i].rectTransform.transform.position;
            }
            else
            {
                selectTextList[i].font = normalFont;
                selectTextList[i].color = normalTextColor;
            }
        }
    }

    private IEnumerator HandleTestStart()
    {
        TestStart();
    
        PlayerController.instance.playerAnim.SetTrigger("interaction1Off");
        AudioManager.instance.ObjectSfxCreate(3, true, gameObject); // 인터렉션
    
        controlUIGameObject.SetActive(false);
        MenuManager.instance.menuKeyExUI.gameObject.SetActive(false);
    
        // 대화창 보이기
        yield return StartCoroutine(UIController.instance.DialogUIScale(0, 20f, 1f, 1f, 1f, 1f, 1f)); // R 대화창 보이기
    
        yield return new WaitForSeconds(1f);
    
        yield return StartCoroutine(UIController.instance.Dialog(0));
        yield return StartCoroutine(UIController.instance.Dialog(0));
    
        StartCoroutine(UIController.instance.DialogUIScale(0, 20f, 0f, 0f, 0f, 0f, 1f));              // R 대화창 숨기기
    
        yield return new WaitForSeconds(2f);
    
        UIController.instance.UISeeState(true);
    
        isControlMode = false;
        EventController.instance.eventState = false;
    
        TurretActive();
    }
    
    private IEnumerator EscapeControlMode()
    {
        PlayerController.instance.playerAnim.SetTrigger("interaction1Off");
        AudioManager.instance.ObjectSfxCreate(3, true, gameObject); // 인터렉션     
        while (true)
        {
            if (PlayerController.instance.playerAnimStateInfo.IsName("Idle"))
                break;
            yield return new WaitForFixedUpdate();
        }

        UIController.instance.UISeeState(true);

        controlUIGameObject.SetActive(false);
        MenuManager.instance.menuKeyExUI.gameObject.SetActive(false);

        isControlMode = false;
        EventController.instance.eventState = false;
    }

    public void TerminalSetting()
    {
       animator.SetTrigger("upOn"); // 터미널 올라오기
       
       AudioManager.instance.ObjectSfxCreate(10,true,gameObject);   // up 사운드
       
       //box2D.enabled = true;
    }

    private void TestStart()
    {
        isTest = true;
        
        animator.SetTrigger("downOn");  // 터미널 내려가기
        
        AudioManager.instance.ObjectSfxCreate(9,true,gameObject);   // Down 사운드
        
        //box2D.enabled = false;
    }

    private void TurretActive()
    {
        if (setTurretNum == 0)
        {
            // 일잘형 터렛만 작동
            for (int i = 0; i < EventController.instance.turretList.Count - 2; i++)
                EventController.instance.turretList[i].isDisabled = false;
            
            Instantiate(EventController.instance.evasionPickFocusPrefabs, EventController.instance.movePickFocusMakeTransList[1].transform.position, Quaternion.identity); // 무브 픽 생성
        }
        else if (setTurretNum == 1)
        {
            // 일반형 + 회전형 터렛만 작동
            for (int i = 0; i < EventController.instance.turretList.Count; i++)
                EventController.instance.turretList[i].isDisabled = false;
                
            Instantiate(EventController.instance.evasionPickFocusPrefabs, EventController.instance.movePickFocusMakeTransList[2].transform.position, Quaternion.identity); // 무브 픽 생성
        }
    }
    
    public void TestEnd()
    {
        isTest = false;
        
        animator.SetTrigger("upOn");		// 터미널 올라오기
        
        AudioManager.instance.ObjectSfxCreate(10,true,gameObject);   // up 사운드
        
       // box2D.enabled = true;
    }

    public void TurretDeActivate()
    {
        if (setTurretNum == 0)
        {
            // 일자형 터렛 멈추기
            for (int i = 0; i < EventController.instance.turretList.Count - 2; i++)
                EventController.instance.turretList[i].isDisabled = true;
        }
        else if (setTurretNum == 1)
        {
            // 회전형 터렛 멈추기
            for (int i = 0; i < EventController.instance.turretList.Count; i++)
                EventController.instance.turretList[i].isDisabled = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isTouchoperationPossible = true;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isTouchoperationPossible = false;
    }

    public void Box2DTrue()
    {
        box2D.enabled = true;
    }
    
    public void Box2DFalse()
    {
        box2D.enabled = false;
    }
}
