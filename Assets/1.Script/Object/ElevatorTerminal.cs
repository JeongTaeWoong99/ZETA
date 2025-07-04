using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElevatorTerminal : MonoBehaviour
{
    public Rigidbody2D elevatorRB2D;
    public Transform elevatorTrans;
    public Transform posA;
    public Transform posB;
    public Transform designatedLocation;
    
    public float maxMoveSpeed;
    public float startMoveSpeedPlus; // 초반 가속이 붙는 추가

    public bool isStartDestinationA; // up에서 시작 -> true // down에서 시작 -> false
    public List<Gate> elevatorGatesListA = new List<Gate>(); // 목적지 A옆에 있는 게이트 리스트
    public List<Gate> elevatorGatesListB = new List<Gate>(); // 목적지 B옆에 있는 게이트 리스트

    private bool isTouchoperationPossible;

    [HideInInspector] 
    public bool isMoving; // 작동상황

    public float accelerationEnd = 0.8f; // 끝나는 지점 가속 속도(0.8~1 구간 감속)

    public CustomMaterialObject CustomMaterialObject; // 바디라이트의 메터리얼
    public float                lightChangeSpeed;
    
    [HideInInspector] 
    public bool                  isControlMode; // 컨트롤 모드
    public GameObject            controlUI;
    public List<TextMeshProUGUI> selectTextList = new List<TextMeshProUGUI>();
    public Image                 focusImage;
    
    [Header("------Highlight Text------")] 
    public TMP_FontAsset highlightFont;
    public Color         highlightTextColor;

    [Header("------Normal Text------")]
    public TMP_FontAsset normalFont;
    public Color         normalTextColor;

    [Header("------Sound------")] 
    public AudioSource[] loopSoundList;
    private List<float> originVolumeValueList = new List<float>();
    public List<float> volumeUpSpeed = new List<float>();

    [Header("------Lab Elevator------")] 
    private Animator      animator; 
    private BoxCollider2D box2D;

    [HideInInspector] 
    public bool isEscapeElevatorActive;  // 탈출 렙 엘레베이터 터치가 활성화 되었는지
    [HideInInspector] 
    public bool isEscapeElevatorArrive; // 탈출 렙 엘레베이터가 도착했는지

    private void Awake()
    {
        animator = GetComponent<Animator>();
        box2D    = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // Sound
        // 오리지널 볼륨 길이 저장 및 볼륨 초기화
        foreach (var turretLoopSoundLists in loopSoundList)
        {
            if (turretLoopSoundLists != null)
            {
                originVolumeValueList.Add(turretLoopSoundLists.volume); // 오리지널값 넣기
                turretLoopSoundLists.Stop(); // 멈추기
                turretLoopSoundLists.volume = 0f; // 볼륨값 없애기
            }
            else
            {
                originVolumeValueList.Add(0f);
            }
        }
    }

    public void Update()
    {
        // 쫒을 때, 작동불가
        if (Input.GetKeyDown(KeyCode.F) && !isMoving && !isControlMode && EnemyDistanceActive.instance.enemyChaseList.Count == 0 && !PlayerHp.instance.isHit &&
            isTouchoperationPossible && !PlayerScan.instance.isScan && !PlayerHp.instance.isRecovery && PlayerFloorCollider.instance.isGrounded &&
            !PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking && !PlayerDash.instance.isDash)
        {
            isControlMode = true;
            EventController.instance.eventState = true;

            UIController.instance.UISeeState(false);

            StartCoroutine(MoveDesignatedLocation()); // 이동
        }
    }

    private void FixedUpdate()
    {
        if (CustomMaterialObject != null)
        {
            // 라이트 UP
            if (isMoving && CustomMaterialObject.spriteRenderer.material.GetFloat("_Brightness") < 1.5f)
                CustomMaterialObject.spriteRenderer.material.SetFloat("_Brightness", Mathf.MoveTowards(
                    CustomMaterialObject.spriteRenderer.material.GetFloat("_Brightness"), 1.5f,
                    Time.fixedDeltaTime * lightChangeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue));
            // 라이트 Down
            else if (!isMoving && CustomMaterialObject.spriteRenderer.material.GetFloat("_Brightness") > 0f)
                CustomMaterialObject.spriteRenderer.material.SetFloat("_Brightness", Mathf.MoveTowards(
                    CustomMaterialObject.spriteRenderer.material.GetFloat("_Brightness"), 0f,
                    Time.fixedDeltaTime * lightChangeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue));
        }

        //사운드(엘리베이터 무브 루프 사운드 재생)
        HandleLoopSound(0, isMoving);
    }

    private void HandleLoopSound(int soundIndex, bool moving)
    {
        if (moving)
        {
            // 소리 높히기
            if (!loopSoundList[soundIndex].isPlaying)
                loopSoundList[soundIndex].Play();

            if (originVolumeValueList[soundIndex] > loopSoundList[soundIndex].volume)
                loopSoundList[soundIndex].volume += volumeUpSpeed[soundIndex] * Time.fixedDeltaTime *
                                                    PlayerAcceleration.instance.accelerationChangedTimeValue;
        }
        else
        {
            // 소리 줄이기
            loopSoundList[soundIndex].volume -= volumeUpSpeed[soundIndex] * Time.fixedDeltaTime *
                                                PlayerAcceleration.instance.accelerationChangedTimeValue;
            if (loopSoundList[soundIndex].volume == 0f && loopSoundList[soundIndex].isPlaying)
            {
                loopSoundList[soundIndex].time = 0f; // 초기화
                loopSoundList[soundIndex].Stop();
            }
        }
    }

    private IEnumerator MoveDesignatedLocation()
    {
        while (true)
        {
            // 이동 종료
            if (Vector2.Distance(PlayerController.instance.transform.position, designatedLocation.position) < 0.25f)
            {
                // 남아있는 이동값 제거
                yield return new WaitForFixedUpdate();

                PlayerController.instance.rb2D.velocity = Vector2.zero;
                PlayerController.instance.playerAnim.SetBool("run", false);

                // 좌우반전(터미널 바라보기)
                PlayerController.instance.bodyGameObject.transform.localScale
                    = PlayerController.instance.transform.position.x < transform.position.x
                        ? new Vector3(1f, 1f, 1f)
                        : new Vector3(-1f, 1f, 1f);

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

    private IEnumerator ControlMode()
    {
        // 위치도착 후, Idle자세가 되면, SetTrigger("interaction1On")
        while (true)
        {
            if (PlayerController.instance.playerAnimStateInfo.IsName("Idle"))
            {
                PlayerController.instance.playerAnim.SetTrigger("interaction1On");
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        // 인터렉션 자세가 되는지 확인
        while (true)
        {
            if (PlayerController.instance.playerAnimStateInfo.IsName("Interaction1"))
                break;

            yield return new WaitForFixedUpdate();
        }

        int currentSelectedNum = 1; // 취소 부터 선택
        focusImage.rectTransform.transform.position = selectTextList[currentSelectedNum].rectTransform.transform.position;
        selectTextList[currentSelectedNum].font     = highlightFont;
        selectTextList[currentSelectedNum].color    = highlightTextColor;
        
        selectTextList[0].font                      = normalFont;
        selectTextList[0].color                     = normalTextColor;
        
        controlUI.SetActive(true);                                   // 컨트롤 UI 켜기
        MenuManager.instance.menuKeyExUI.gameObject.SetActive(true); // 키 설명

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (currentSelectedNum == 0)
                {
                    UIController.instance.UISeeState(true);
                    MenuManager.instance.menuKeyExUI.gameObject.SetActive(false);
                    controlUI.SetActive(false);
                    AudioManager.instance.ObjectSfxCreate(3, true, gameObject); // 인터렉션 사운드(1회 재생)
                    StartCoroutine(StateCheck());
                }
                else if (currentSelectedNum == 1)
                {
                    StartCoroutine(EscapeControlMode());
                }

                break;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) && currentSelectedNum == 1)
            {
                AudioManager.instance.UISoundPlay(0);   // UI 이동 사운드
                
                selectTextList[currentSelectedNum].font  = normalFont;
                selectTextList[currentSelectedNum].color = normalTextColor;
                
                currentSelectedNum--;
                
                focusImage.rectTransform.transform.position = selectTextList[currentSelectedNum].rectTransform.transform.position;
                
                selectTextList[currentSelectedNum].font  = highlightFont;
                selectTextList[currentSelectedNum].color = highlightTextColor;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && currentSelectedNum == 0)
            {
                AudioManager.instance.UISoundPlay(0);   // UI 이동 사운드
                
                selectTextList[currentSelectedNum].font  = normalFont;
                selectTextList[currentSelectedNum].color = normalTextColor;
                
                currentSelectedNum++;
                
                focusImage.rectTransform.transform.position = selectTextList[currentSelectedNum].rectTransform.transform.position;
                
                selectTextList[currentSelectedNum].font  = highlightFont;
                selectTextList[currentSelectedNum].color = highlightTextColor;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(EscapeControlMode());
                break;
            }

            yield return null;
        }
    }

    private IEnumerator EscapeControlMode()
    {
        PlayerController.instance.playerAnim.SetTrigger("interaction1Off");
        AudioManager.instance.ObjectSfxCreate(3, true, gameObject); // 인터렉션 사운드

        while (true)
        {
            if (PlayerController.instance.playerAnimStateInfo.IsName("Idle"))
                break;

            yield return new WaitForFixedUpdate();
        }

        UIController.instance.UISeeState(true);

        controlUI.SetActive(false);
        MenuManager.instance.menuKeyExUI.gameObject.SetActive(false);

        isControlMode = false;
        EventController.instance.eventState = false;
    }

    private IEnumerator StateCheck()
    {
        AudioManager.instance.ObjectSfxCreate(1, true, gameObject); // 승상기 On사운드 재생(1회 재생)
        PlayerController.instance.playerAnim.SetTrigger("interaction1Off");

        // 이동 가능. 초기화
        bool isMovePossible = true;

        // 게이트가 닫혀 있는지 확인.
        if (isStartDestinationA) // 게이트 A들 닫혀 있는지 확인.
        {
            foreach (var elevatorGatesListAs in elevatorGatesListA)
            {
                if (elevatorGatesListAs.gateAnimStateInfo.normalizedTime < 1f) // 계속 타임은 늘어남.(0 -> NNN)
                    isMovePossible = false; // 1개라도 종료되지 않은 문이 있으면, 이동불가
            }
        }
        else // 게이트 B들 닫혀 있는지 확인.
        {
            foreach (var elevatorGatesListBs in elevatorGatesListB) // 게이트 B들 체크
            {
                if (elevatorGatesListBs.gateAnimStateInfo.normalizedTime < 1f) // 계속 타임은 늘어남.(0 -> NNN)
                    isMovePossible = false; // 1개라도 종료되지 않은 문이 있으면, 이동불가
            }
        }

        if (isMovePossible)
        {
            // 남아있는 이동값 제거
            yield return new WaitForFixedUpdate();
            PlayerController.instance.rb2D.velocity = Vector2.zero; // 남은 이동값 제거
            PlayerController.instance.playerAnim.SetBool("run", false); // 달리기 중이면 잠금

            // 이동
            // A에서, B으로 이동(Up  에서 Down -> -1)
            // B에서, A으로 이동(Down에서 Up   -> 1)
            StartCoroutine(isStartDestinationA ? Move(-1) : Move(1));

            // 출발과 함께, 열리있는 문 닫기.(+엘리베이터 컨트롤모드 true로 닿았을 때 열리지 않기)
            if (isStartDestinationA) // 게이트 A들 닫기.
            {
                foreach (var elevatorGatesListAs in elevatorGatesListA)
                    elevatorGatesListAs.anim.SetTrigger("closeOn");
            }
            else // 게이트 B들 닫기.
            {
                foreach (var elevatorGatesListBs in elevatorGatesListB)
                    elevatorGatesListBs.anim.SetTrigger("closeOn");
            }

            // 상태값은 AB 모두 변경.
            foreach (var elevatorGatesListAs in elevatorGatesListA)
            {
                elevatorGatesListAs.openState         = false; // 스테이트 변경
                elevatorGatesListAs.isElevatorControl = true;  // 엘리베이터 컨트롤 모드
            }

            foreach (var elevatorGatesListBs in elevatorGatesListB)
            {
                elevatorGatesListBs.openState         = false; // 스테이트 변경
                elevatorGatesListBs.isElevatorControl = true;  // 엘리베이터 컨트롤 모드
            }

            yield return new WaitForSeconds(2f); // 문 지나치는 문제로 인해, 시간을 길게 기다렸다가, 제어권 부여
            
            // 일반
            // 성능실험실의 경우... 오른쪽 탈출 엘레베이터가 활성화 되면, 이벤트를 따라간다...
            if (!isEscapeElevatorActive)
            {
                isControlMode = false;
                EventController.instance.eventState = false;
            }
            else
            {
                EventController.instance.eventState = true;
                EventController.instance.AllKeyLockTrue();
            }

            // 게이트 닫혔는지 체크
            while (true)
            {
                bool isGateAllCloseCheck = true;

                // 상태값은 AB 모두 확인.
                foreach (var elevatorGatesListAs in elevatorGatesListA)
                {
                    if (elevatorGatesListAs.gateAnimStateInfo.normalizedTime <
                        1f) // 계속 타임은 늘어남.(0 -> NNN) // 한개라도 1보다 작으면, 아직 다 안닫힘.
                        isGateAllCloseCheck = false;
                }

                foreach (var elevatorGatesListBs in elevatorGatesListB)
                {
                    if (elevatorGatesListBs.gateAnimStateInfo.normalizedTime <
                        1f) // 계속 타임은 늘어남.(0 -> NNN) // 한개라도 1보다 작으면, 아직 다 안닫힘.
                        isGateAllCloseCheck = false;
                }

                if (isGateAllCloseCheck)
                {
                    EventController.instance.eventState = false; // 이벤트 잠금
                    break;
                }

                yield return null;
            }
        }

        yield return null;
    }

    public IEnumerator Move(float direction)
    {
        isMoving = true;

        Vector2 moveEndPos;
        float totalLength;
        // Down 이동
        if (direction == -1)
        {
            moveEndPos = posB.position;
            totalLength = Vector2.Distance(elevatorTrans.position, moveEndPos);
        }
        // UP 이동
        else
        {
            moveEndPos = posA.position;
            totalLength = Vector2.Distance(elevatorTrans.position, moveEndPos);
        }

        float activeMoveSpeed = 0f;
        bool isOffSoundCreate = false;

        // while (true)
        // {
        //     // 토탈 거리(accelerationEnd에서 사용)
        //     float percent = 1 - (Vector2.Distance(elevatorTrans.position, moveEndPos) / totalLength);
        //
        //     // 이속 증가(초반 이동속도 증가에 사용)
        //     if (activeMoveSpeed < maxMoveSpeed)
        //         activeMoveSpeed += Time.fixedDeltaTime * startMoveSpeedPlus * PlayerAcceleration.instance.accelerationChangedTimeValue;
        //
        //     // 초반 ~ 최고속도: 이동속도 구간
        //     if (percent < accelerationEnd)
        //         elevatorRB2D.MovePosition(elevatorRB2D.position + (new Vector2(0f, direction) * (Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue * activeMoveSpeed)));
        //     // 도착 : 가속도가 줄어드는 구간
        //     else if (percent >= accelerationEnd)
        //     {
        //         if (!isOffSoundCreate)
        //         {
        //             isOffSoundCreate = true;
        //             AudioManager.instance.ObjectSfxCreate(2, true, gameObject); // Off사운드 재생(1회 재생 생성)
        //         }
        //
        //         float velocityScale = Mathf.Lerp(1f, 0f, (percent - accelerationEnd) / (1f - accelerationEnd));
        //         elevatorRB2D.MovePosition(elevatorRB2D.position + (new Vector2(0f, direction) * (Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue * activeMoveSpeed * velocityScale)));
        //
        //         if (Vector2.Distance(elevatorTrans.position, moveEndPos) < 0.005f)
        //         {
        //             elevatorRB2D.MovePosition(moveEndPos); // 위치 정확히 이동.
        //             break;
        //         }
        //     }
        //
        //     yield return new WaitForFixedUpdate();
        // }
        while (true)
        {
            // 토탈 거리(accelerationEnd에서 사용)
            float percent = 1 - (Vector2.Distance(elevatorTrans.position, moveEndPos) / totalLength);

            // 이속 증가(초반 이동속도 증가에 사용)
            if (activeMoveSpeed < maxMoveSpeed)
                activeMoveSpeed += Time.fixedDeltaTime * startMoveSpeedPlus * PlayerAcceleration.instance.accelerationChangedTimeValue;

            Vector2 velocity = Vector2.zero;

            // 초반 ~ 최고속도: 이동속도 구간
            if (percent < accelerationEnd)
            {
                velocity = new Vector2(0f, direction) * (PlayerAcceleration.instance.accelerationChangedTimeValue * activeMoveSpeed);
            }
            // 도착 : 가속도가 줄어드는 구간
            else if (percent >= accelerationEnd)
            {
                if (!isOffSoundCreate)
                {
                    isOffSoundCreate = true;
                    AudioManager.instance.ObjectSfxCreate(2, true, gameObject); // Off사운드 재생(1회 재생 생성)
                }

                float velocityScale = Mathf.Lerp(1f, 0f, (percent - accelerationEnd) / (1f - accelerationEnd));
                velocity = new Vector2(0f, direction) * (PlayerAcceleration.instance.accelerationChangedTimeValue * activeMoveSpeed * velocityScale);

                if (Vector2.Distance(elevatorTrans.position, moveEndPos) < 0.005f)
                {
                    elevatorRB2D.velocity = Vector2.zero;  // 속도 초기화
                    elevatorRB2D.MovePosition(moveEndPos); // 위치 정확히 이동.
                    break;
                }
            }
        
            elevatorRB2D.velocity = velocity;

            yield return new WaitForFixedUpdate();
        }


        // 도착과 함께, 게이트 열기.(+엘리베이터 컨트롤모드 true로 닿았을 때 열리지 않기)
        if (isStartDestinationA) // 목적지 A에서 B로 이동했기 때문에, B를 열어준다.
        {
            foreach (var elevatorGatesListBs in elevatorGatesListB)
            {
                elevatorGatesListBs.anim.SetTrigger("baseCloseOn");
                elevatorGatesListBs.anim.SetTrigger("eventOpenOn");
                elevatorGatesListBs.openState = true;
            }
        }
        else // 목적지 B에서 A로 이동했기 때문에, A를 열어준다.
        {
            foreach (var elevatorGatesListAs in elevatorGatesListA)
            {
                elevatorGatesListAs.anim.SetTrigger("baseCloseOn");
                elevatorGatesListAs.anim.SetTrigger("eventOpenOn");
                elevatorGatesListAs.openState = true;
            }
        }

        yield return new WaitForFixedUpdate();

        while (true)
        {
            bool isGateAllOpenCheck = true; // 게이트 모두 열렸는지 체크

            if (isStartDestinationA) // 목적지 A에서 B로 이동했기 때문에, B를 체크한다.
            {
                foreach (var elevatorGatesListBs in elevatorGatesListB)
                {
                    if (elevatorGatesListBs.gateAnimStateInfo.normalizedTime <
                        1f) // 계속 타임은 늘어남.(0 -> NNN) // 한개라도 1보다 작으면, 아직 다 안닫힘.
                        isGateAllOpenCheck = false; // 모두 열림 완료.
                }
            }
            else // 목적지 B에서 A로 이동했기 때문에, A를 체크한다.
            {
                foreach (var elevatorGatesListAs in elevatorGatesListA)
                {
                    if (elevatorGatesListAs.gateAnimStateInfo.normalizedTime <
                        1f) // 계속 타임은 늘어남.(0 -> NNN) // 한개라도 1보다 작으면, 아직 다 안닫힘.
                        isGateAllOpenCheck = false; // 모두 열림 완료.
                }
            }

            // 모두 열림. 빠져나가기
            if (isGateAllOpenCheck)
            {
                if (isStartDestinationA) // 게이트 A들 확인.
                {
                    foreach (var elevatorGatesListAs in elevatorGatesListA)
                        elevatorGatesListAs.isElevatorControl = false;

                    isStartDestinationA = false; // 상태 변경
                }
                else // 게이트 B들 확인.
                {
                    foreach (var elevatorGatesListBs in elevatorGatesListB)
                        elevatorGatesListBs.isElevatorControl = false;

                    isStartDestinationA = true; // 상태 변경
                }

                isMoving = false;

                // 성능실험실 오른쪽 탈출 엘레베이터 활성화 -> 작동 -> 도착
                if (isEscapeElevatorActive)
                    isEscapeElevatorArrive = true;
                
                break;
            }

            yield return null;
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

    public void PerformanceLabElevatorTerminalActive()
    {
        isEscapeElevatorActive = true;
    
        box2D.enabled = true;
        
        animator.SetTrigger("upOn");		// 터미널 올라오기
    }

}
