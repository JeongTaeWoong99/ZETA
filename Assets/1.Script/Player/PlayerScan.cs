using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerScan : MonoBehaviour
{
    public static PlayerScan instance;
    
    [HideInInspector] 
    public bool isScan;
    private Vector2 inputVector2;   // E키를 눌렀을 때, 카메라 위치
    
    [Header("------AreaExpansion------")]
    public GameObject hackingOverlayEffectShader;
    
    public float      hackingOverLayMaxSize;                      // 스캔 커질 크기
    public float      hackingOverLaySpeed;                        // 스캔 필터가 퍼지는 속도

    public float      hackingStateCameraSize;

    [Header("------ScanCameraMove------")]
    public  float scanCameraMoveSpeed;   // 카메라 이속
    [HideInInspector]
    public  bool  isScanCameraMoveMode;
    public  float scanRangeConstrain;    // 거리 제한 
    public  float scanRenewalTime;       // 스캔 갱신 타임
    private float scanRenewalTimeCount;
    
    [Header("------FindObject------")]
    public  LayerMask        scanTargetLayer;                           // 스캔 할 오브젝트 타겟
    
    private List<GameObject> targetObjectList = new List<GameObject>(); // 찾은 오브젝트(해킹안된)
    private List<GameObject> ventLightList    = new List<GameObject>(); // 찾은 뒷길 라이트 리스트
    
    private int              hologramFadeID;                            // 페이드 이름
    private int              innerOutlineFadeID;                        // 틴트 이름

    [Header("------ControlMode------")]
    public  GameObject focusImagePrefabs;
    private GameObject focusGameobject;
    [HideInInspector]
    public  bool       isScanCameraStop;                                    // 카메라가 멈춰있는지 여부.
    
    [Header("------ConnectionMode------")] 
    public int connectionLifeNum;        // 연결 목숨 수
    [HideInInspector] 
    public int currentConnectionLifeNum; // 현재 남은 연결 목숨 수
    
    public float myConnectionIconSpeed;  // 이동 스피드
    public float myIconConstrainX;       // 이동 제한 X
    public float myIconConstrainY;       // 이동 제한 Y

    private Transform currentScanningObejectTrans;
    public  int       scanFailDamage;
    
    [HideInInspector]
    public bool isConnection;

    private List<GameObject> itemList = new List<GameObject>();  // 만들어진 아이템 리스트
    private List<GameObject> antiList = new List<GameObject>();  // 만들어진 아이템 리스트
    
    [HideInInspector]
    public int obtainedItemNumber;      // 얻은 아이템 수.

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        hologramFadeID      = Shader.PropertyToID("_HologramFade");
        innerOutlineFadeID  = Shader.PropertyToID("_InnerOutlineFade");
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)                    && !EventController.instance.eventState && PlayerHp.instance.liveState              && !PlayerHp.instance.isHit    
            && !PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking    && !isScan && !PlayerHp.instance.isRecovery && !MenuManager.instance.isNormalMenu)
        {
            // 해당 상태이면, 실행.
            if (!EventController.instance.scanLock && !PlayerDash.instance.isDash && EnemyDistanceActive.instance.enemyChaseList.Count == 0 && !EventController.instance.isBossRoom
                && (PlayerFloorCollider.instance.isGrounded || PlayerController.instance.isHangWall) 
                && (PlayerController.instance.playerAnimStateInfo.IsName("Idle") || PlayerController.instance.playerAnimStateInfo.IsName("Hang")))
            {
                isScan               = true;
                isScanCameraMoveMode = true;            
                inputVector2 = CameraController.instance.transform.position;    // E키를 눌렀을 때, 카메라 위치 저장.
                
                // 오디오 변경
                AudioManager.instance.playerListener.enabled = false;   // 플레이어 오디오 끄기
                AudioManager.instance.cameraListener.enabled = true;    // 카메라 오디오 켜기
                
                StartCoroutine(AreaExpansion());
            }
            // 해당 상태에서만, 사운드 재생.
            else
            {
                AudioManager.instance.UISoundPlay(4);
            }
        }
    }

    private IEnumerator AreaExpansion()
    {
        UIController.instance.UISeeState(false);                // UI 숨기기
    
        CameraController.instance.hackingPanel.SetActive(true); // 패널 켜기(패널은 카메라에 맞춰서 있음.)
        AudioManager.instance.DirectingPlay(4);                 // 1회 재생
        AudioManager.instance.DirectingPlay(5);                 // 루프

        // 값 제거.
        yield return new WaitForFixedUpdate(); // 이전 입력 겹치기 방지 대기
        PlayerController.instance.rb2D.velocity = new Vector2(0f,PlayerController.instance.rb2D.velocity.y);              
        PlayerController.instance.isHangUp      = false;                        
        PlayerController.instance.isHangDown    = false;                        
        PlayerController.instance.playerAnim.SetBool("run",false);
        
        switch (PlayerFloorCollider.instance.isGrounded)
        {
            // 애니메이션 실행
            // 바닥
            case true:
                PlayerController.instance.playerAnim.SetTrigger("scan1On");
                break;
            // 계단
            case false:
                PlayerController.instance.playerAnim.SetTrigger("scan2On");
                break;
        }

        // 후처리
        float percent = 0.0f;
        hackingOverlayEffectShader.transform.localScale = Vector3.zero;
        
        while (percent < 1)
        {
            percent +=  Time.deltaTime * hackingOverLaySpeed;
            
            // 카메라 크기 키우기
            CameraController.instance.mainCam.orthographicSize = Mathf.Lerp(CameraController.instance.originOrthographicSize,hackingStateCameraSize, percent);
            // 해킹 영역 전개
            hackingOverlayEffectShader.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(hackingOverLayMaxSize,hackingOverLayMaxSize,hackingOverLayMaxSize), percent);
            
            // 강제 종료(스캔인데, 히트 or 쫓아오는 적이 있음 or 남은 목숨이 없음.)
            if (isScan && (PlayerHp.instance.isHit || EnemyDistanceActive.instance.enemyChaseList.Count != 0))
            {
                StartCoroutine(ScanEnd());
                break;
            }
            
            // 중간에 히트 X
            if (percent >= 1f) 
            {
                StartCoroutine(ControlMode());
                break;
            }
            
            yield return null;
        }
    }
    
    private IEnumerator ControlMode()
    {
        UIController.instance.scanKeyExUI.SetActive(true);              // 스캔 키 설명 ui 켜기(스캔에서만 켜기)
        UIController.instance.scanStateText.gameObject.SetActive(true); // 스캔 상태 텍스트 켜기(스캔 및 전투 해킹에서 켜기)
        
        scanRenewalTimeCount               = 0f; 
        bool  thisCameraSeenFindTarget     = false;             // 현재 보고있는 화면 스캔했는지
        int   currentNum                   = 0;
        float scanStateAndDownLineWaitTime = 0f;                // 스캔과 다운라인 활성화를 기다리는 시간. -> 해킹에서는 부팅 중...과 스캔 중... 사이의 0.5초와 같은 역활을 한다.
        isScanCameraStop                   = false;
        
        while (true)
        {
            // 방향키를 입력하지 않고 있으면, 선택할 수 있음.
            if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
                isScanCameraStop         = true;  // 멈춰있음.
            else
            {
                UIController.instance.scanStateText.text = "이동 중...";       // 텍스트
                if (CameraController.instance.moveScanLineList[0].activeSelf) // 스캔라인이 켜져 있으면 끄기.(1회 켜기 및 실행)
                {
                    foreach (var moveScanLineLists in CameraController.instance.moveScanLineList)
                        moveScanLineLists.SetActive(false);
                }
                
                isScanCameraStop         = false; // 움직이고 있음.
                scanRenewalTimeCount     = 0f;    // 리뉴월 시간 초기화
                thisCameraSeenFindTarget = false; // 움직이면, 현재 화면에서 잡은 오브젝트들 초기화.
                ColorHighlightRestoration();      // 색강조 복구 및 리스트 초기화.
                if(focusGameobject)               // 포커스가 있으면, 포커스 아웃 애니메이션 실행.
                    focusGameobject.GetComponent<Animator>().SetTrigger("focusOutOn"); 
                currentNum = 0;                   // 숫자 초기화
            }
            
            // 화면 스캔(방향키 일정시간 입력하지 않고 있으면, 화면 스캔)
            scanStateAndDownLineWaitTime += Time.deltaTime;
            if (isScanCameraStop && !thisCameraSeenFindTarget && scanStateAndDownLineWaitTime > 0.5f)
            {
                // 카메라 안 찾기.
                scanRenewalTimeCount += Time.deltaTime;
                
                UIController.instance.scanStateText.text = "스캔 중...";     // 텍스트
                if (!CameraController.instance.moveScanLineList[0].activeSelf) // 스캔라인이 켜져 있으면 끄기.(1회 켜기 및 실행)
                {
                    AudioManager.instance.DirectingPlay(6); // 1회 재생
                    foreach (var moveScanLineLists in CameraController.instance.moveScanLineList)
                        moveScanLineLists.SetActive(true);
                }
                
                if (scanRenewalTimeCount > scanRenewalTime)
                {
                    thisCameraSeenFindTarget = true;
                    FindTarget();
                }
            }
            else if(scanStateAndDownLineWaitTime < 0.5f)
            {
                UIController.instance.scanStateText.text = "부팅 중...";      // 텍스트
                if (CameraController.instance.moveScanLineList[0].activeSelf) // 스캔라인이 켜져 있으면 끄기.(1회 켜기 및 실행)
                {
                    foreach (var moveScanLineLists in CameraController.instance.moveScanLineList)
                        moveScanLineLists.SetActive(false);
                }
            }
            
            // 이동 및 조작(멈춰있을 때 가능)
            if (targetObjectList.Count != 0 && isScanCameraStop)
            {
                // 이동(좌)
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (currentNum + 1 < targetObjectList.Count)
                    {
                        currentNum++;
                        focusGameobject.transform.position = targetObjectList[currentNum].transform.position;
                    }
                    else
                    {
                        currentNum = 0;
                        focusGameobject.transform.position = targetObjectList[currentNum].transform.position;
                    }
                }
                // 이동(우)                                            
                else if (Input.GetKeyDown(KeyCode.W))
                {
                    if (currentNum - 1 >= 0)
                    {
                        currentNum--;
                        focusGameobject.transform.position = targetObjectList[currentNum].transform.position;
                    }
                    else
                    {
                        currentNum = targetObjectList.Count -1;
                        focusGameobject.transform.position = targetObjectList[currentNum].transform.position;
                    }
                }
                // 조작
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    currentConnectionLifeNum = connectionLifeNum;                          // 목숨 수 초기화
                    foreach (var scanLifeLists in CameraController.instance.scanLifeList) // 목숨 보이기
                        scanLifeLists.gameObject.SetActive(true);
                
                    UIController.instance.scanStateText.text = "해킹 시도 중...";   // 텍스트
                    if (CameraController.instance.moveScanLineList[0].activeSelf) // 스캔라인이 켜져 있으면 끄기.(1회 켜기 및 실행)
                    {
                        foreach (var moveScanLineLists in CameraController.instance.moveScanLineList)
                            moveScanLineLists.SetActive(false);
                    }
                
                    HackingPossibleTerminal hackingPossibleTerminal = targetObjectList[currentNum].GetComponent<HackingPossibleTerminal>();
                    currentScanningObejectTrans                     = hackingPossibleTerminal.transform;  // 현재 스캔하는 오브젝트의 위치값을 받아오기 위함.
                    
                    if (hackingPossibleTerminal.isScanPossible)
                    {
                        isConnection = true;

                        // 미니게임 세팅
                        CameraController.instance.myConnectionIcon.transform.localPosition = Vector3.zero;
                        CameraController.instance.connectionFrame.SetActive(true); 
                        
                        // 아이템 생성(해당 터미널의 아이템 숫자만큼)
                        for (int i = 0; i < hackingPossibleTerminal.itemNumber; i++)
                        {
                            float createTransX = Random.Range(-myIconConstrainX * 0.8f, myIconConstrainX * 0.8f);   // 벽과의 간격 0.2f를 남겨둠.
                            float createTransY = Random.Range(-myIconConstrainY * 0.8f, myIconConstrainY * 0.8f);   // 벽과의 간격 0.2f를 남겨둠.
                            
                            // 간격 조정(X Y)
                            if (createTransX <= 0f)
                                createTransX += -myIconConstrainX * 0.15f;   // 플레이어 아이콘과의 거리 0.15을 벌림.
                            else
                                createTransX +=  myIconConstrainX * 0.15f;    
                            
                            if (createTransY <= 0f)
                                createTransY += -myIconConstrainY * 0.15f;   // 플레이어 아이콘과의 거리 0.15을 벌림.
                            else
                                createTransY +=  myIconConstrainY * 0.15f;    
                                
                            GameObject item = Instantiate(CameraController.instance.itemIcon, Vector3.zero, Quaternion.identity);
                            
                            item.transform.position = new Vector3(CameraController.instance.connectionFrame.transform.position.x + createTransX, CameraController.instance.connectionFrame.transform.position.y + createTransY, 0f);
                            item.transform.parent   = CameraController.instance.connectionFrame.transform;
                            
                            itemList.Add(item);
                        }
                        
                        // 적 생성
                        // 아이템 생성(해당 터미널의 아이템 숫자만큼)
                        for (int i = 0; i < hackingPossibleTerminal.antiVirusNumber; i++)
                        {
                            float createTransX = Random.Range(-myIconConstrainX * 0.5f, myIconConstrainX * 0.5f);   
                            float createTransY = Random.Range(-myIconConstrainY * 0.5f, myIconConstrainY * 0.5f);  
                            
                            // 간격 조정(X Y)
                            if (createTransX <= 0f)
                                createTransX += -myIconConstrainX * 0.45f;   // 플레이어 아이콘과의 거리 0.45을 벌림.
                            else
                                createTransX +=  myIconConstrainX * 0.45f;    
                            
                            if (createTransY <= 0f)
                                createTransY += -myIconConstrainY * 0.45f;   // 플레이어 아이콘과의 거리 0.45을 벌림.
                            else
                                createTransY +=  myIconConstrainY * 0.45f;    
                                
                            GameObject anti = Instantiate(CameraController.instance.antiIcon, Vector3.zero, Quaternion.identity);
                            
                            anti.transform.position = new Vector3(CameraController.instance.connectionFrame.transform.position.x + createTransX, CameraController.instance.connectionFrame.transform.position.y + createTransY, 0f);
                            anti.transform.parent   = CameraController.instance.connectionFrame.transform;
                            
                            antiList.Add(anti);
                        }
                        
                        // 컨트롤 중.
                        while (isConnection)
                        {
                            ConnectionMode();
                            
                            // 강제 종료(스캔인데, 히트 or 쫓아오는 적이 있음 or 남은 목숨이 없음.)
                            if (isScan && (PlayerHp.instance.isHit || EnemyDistanceActive.instance.enemyChaseList.Count != 0))
                            {
                                StartCoroutine(ScanEnd());
                                break;
                            }
                            
                            // 모두 먹음.
                            if (obtainedItemNumber == hackingPossibleTerminal.itemNumber)
                            {
                                if(focusGameobject)               // 성공하면, 포커스 아웃 
                                    focusGameobject.GetComponent<Animator>().SetTrigger("focusOutOn"); 
                            
                                ColorHighlightRestoration();            // 나가기 전 라이트 강조 끄기.
                                
                                UIController.instance.scanStateText.text = "구조물 해킹 성공.";
                                
                                obtainedItemNumber = 0;                                     // 초기화
                                itemList.Clear();                                           // 비우기
                                foreach (var antiLists in antiList)                // 제거 및 비우기
                                    Destroy(antiLists);
                                antiList.Clear();
                                foreach (var scanLifeLists in CameraController.instance.scanLifeList) // 목숨 숨기기
                                    scanLifeLists.gameObject.SetActive(false);
                                
                                CameraController.instance.connectionFrame.SetActive(false); // 창끄기
                                
                                // 트리거 발동
                                if(hackingPossibleTerminal.specialGate) // 특수 게이트
                                    hackingPossibleTerminal.SpecialGateScanTrigger();                      
                                else                                    // 레이저 장애물 
                                    hackingPossibleTerminal.ObstacleScanTrigger();      
                                
                                scanRenewalTimeCount = 0f;        // 스캔 갱신 시간 초기화
                                thisCameraSeenFindTarget = false; // 파인드 타겟 false
                                
                                yield return new WaitForSeconds(0.5f);
                                
                                isConnection = false;             // 연결 종료
                                
                                break;
                            }
                            yield return null;
                        }
                    }
                }
                // 나가기
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(ScanEnd()); 
                    break;
                }
            }
            // 나가기(움직이는 중, 가능하게)
            else if (targetObjectList.Count == 0)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(ScanEnd());
                    break;
                }
            }
            
            // 강제 종료(스캔인데, 히트 or 쫓아오는 적이 있음 or 타겟 하나 해킹 성공)
            if (isScan && (PlayerHp.instance.isHit || EnemyDistanceActive.instance.enemyChaseList.Count != 0))
            {
                StartCoroutine(ScanEnd());
                break;
            }
            
            yield return null;  // IEnumerator의 While의 Update
        }
    }

    private void FindTarget()
    {
        // 보이는 스캔오브젝트 찾기(카메라 위치 기준으로, 범위안에 있는 적 저장)
        float cameraHeight     = CameraController.instance.originOrthographicSize * 2f;                 // 카메라 크기 y
        float cameraWidth      = cameraHeight     * Camera.main.aspect; // 카메라 크기 x
        RaycastHit2D[] hitList = Physics2D.BoxCastAll(CameraController.instance.transform.position, new Vector2(cameraWidth, cameraHeight * 0.9f), 
                                                          0f, Vector2.zero, 0f, scanTargetLayer);
        
        // 카메라에 들어온 scanTargetLayer 구분하기.
        foreach (var hitLists in hitList)
        {
            HackingPossibleTerminal hackingPossibleTerminal = hitLists.collider.GetComponent<HackingPossibleTerminal>();
            VentLight               ventLight               = hitLists.collider.GetComponent<VentLight>();
            
            // 옵스타클 터미널(해킹 안된, 가능한 것만 잡기)
            if(hackingPossibleTerminal && hackingPossibleTerminal.isScanPossible)
                targetObjectList.Add(hackingPossibleTerminal.gameObject);
            else if(ventLight)
                ventLightList.Add(ventLight.gameObject);
        }
        
        // (해킹안된)터미널 밝히기
        foreach (var targetObjectLists in targetObjectList)
        {
            HackingPossibleTerminal hackingPossibleTerminal = targetObjectLists.GetComponent<HackingPossibleTerminal>();

            // 오브젝트(메터리얼 홀로효과 on)
            if (hackingPossibleTerminal)
            {
                foreach (var bodyPart in hackingPossibleTerminal.terminalBodySpriteRendererList)
                {
                    bodyPart.material.SetFloat(hologramFadeID,     0.5f);
                    bodyPart.material.SetFloat(innerOutlineFadeID, 1f);
                    
                    bodyPart.GetComponent<HackingPossibleTerminal>().inAccelerationOrderLayer.GroupHackingLayerEnable(); // 레이어 앞으로
                }
            }
        }
        
        foreach (var ventLists in ventLightList)
        {
            VentLight ventLight = ventLists.GetComponent<VentLight>();
            
            ventLight.isScanCameraTouch = true;
        }

        // 가까운 순서로 0번으로 정렬
        targetObjectList = targetObjectList.OrderBy(enemy => Vector2.Distance(PlayerController.instance.transform.position, enemy.transform.position)).ToList();
        if(targetObjectList.Count != 0f)
            focusGameobject = Instantiate(focusImagePrefabs, targetObjectList[0].transform.position, Quaternion.identity);   // 포커스 생성   
        
        if (targetObjectList.Count == 0)
            UIController.instance.scanStateText.text = "스캔 완료. 타겟 없음.";
        else
            UIController.instance.scanStateText.text = "스캔 완료. 타겟 " + targetObjectList.Count + "개 발견.";
    }

    private void ConnectionMode()
    {
        Vector3 moveDirection = Vector3.zero;

        // 위 아래
        if (Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
            moveDirection += Vector3.up;
        else if (!Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
            moveDirection -= Vector3.up;
        
        // 왼쪽 오른쪽
        if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            moveDirection -= Vector3.right;
        else if (!Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
            moveDirection += Vector3.right;

        Vector3 newPosition = CameraController.instance.myConnectionIcon.transform.position + moveDirection.normalized * myConnectionIconSpeed * Time.deltaTime;
        float absoluteDifferenceX = Mathf.Abs(newPosition.x - CameraController.instance.connectionFrame.transform.position.x);
        float absoluteDifferenceY = Mathf.Abs(newPosition.y - CameraController.instance.connectionFrame.transform.position.y);
        
        // 위치 이동
        // 위아래 좌우 둘다 만족한다면
        if (absoluteDifferenceX <= myIconConstrainX && absoluteDifferenceY <= myIconConstrainY)
        {
            newPosition = CameraController.instance.myConnectionIcon.transform.position + moveDirection.normalized * myConnectionIconSpeed * Time.deltaTime;  // 좌우이동 다 하는 경우, 노멀라이즈 값으로 다시 계산한다.
            CameraController.instance.myConnectionIcon.transform.position = newPosition;
        }
        // 좌우만 이동 가능
        else if(absoluteDifferenceX <= myIconConstrainX)
            CameraController.instance.myConnectionIcon.transform.position = new Vector3(newPosition.x,CameraController.instance.myConnectionIcon.transform.position.y,CameraController.instance.myConnectionIcon.transform.position.z);
        // 위아래만 이동 가능
        else if(absoluteDifferenceY <= myIconConstrainY)
            CameraController.instance.myConnectionIcon.transform.position = new Vector3(CameraController.instance.myConnectionIcon.transform.position.x,newPosition.y,CameraController.instance.myConnectionIcon.transform.position.z);
        
        // 보는 방향 회전
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, moveDirection.normalized);
            CameraController.instance.myConnectionIcon.transform.rotation = Quaternion.Lerp(CameraController.instance.myConnectionIcon.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private IEnumerator ScanEnd()
    {
        isScanCameraStop     = false;
        isScanCameraMoveMode = false;
        
        // 오디오 변경
        AudioManager.instance.playerListener.enabled = true;   // 플레이어 오디오 켜기
        AudioManager.instance.cameraListener.enabled = false;  // 카메라 오디오 끄기
    
        // 상태 애니메이션 전환(바로 실행되는 것 방지)
        if (!PlayerHp.instance.isHit)
        {
            switch (PlayerFloorCollider.instance.isGrounded)
            {
                case true:
                    PlayerController.instance.playerAnim.SetTrigger("scan1Off");
                    break;
                case false:
                    PlayerController.instance.playerAnim.SetTrigger("scan2Off");
                    break;
            }
        }
        // 히트 중 이면, on 트리거의 피라미터 제거.
        else if(PlayerHp.instance.isHit)
        {
            PlayerController.instance.playerAnim.ResetTrigger("scan1On");
            PlayerController.instance.playerAnim.ResetTrigger("scan2On");
            PlayerController.instance.playerAnim.ResetTrigger("scan1Off");
            PlayerController.instance.playerAnim.ResetTrigger("scan2Off");
        }
        
        UIController.instance.UISeeState(true);                                // UI 보이기
        
        hackingOverlayEffectShader.transform.localScale = Vector3.zero;        // 크기 복구
        
        CameraController.instance.hackingPanel.SetActive(false);               // 해킹 패널 off
        AudioManager.instance.DirectingStop(5);                         // 루프 멈추기
        
        if(focusGameobject)               // 포커스가 있으면, 포커스 아웃 애니메이션 실행.
            focusGameobject.GetComponent<Animator>().SetTrigger("focusOutOn"); 
        
        CameraController.instance.mainCam.orthographicSize = CameraController.instance.originOrthographicSize;                                            // 카메라 크기 복구
        CameraController.instance.transform.position       = new Vector3(inputVector2.x, inputVector2.y, CameraController.instance.transform.position.z); // 위치 복구
        
        UIController.instance.scanKeyExUI.SetActive(false);                    // 스캔 키 설명 ui 끄기
        UIController.instance.scanStateText.gameObject.SetActive(false);       // 스캔 상태 텍스트 켜기(스캔 및 전투 해킹에서 켜기)

        ColorHighlightRestoration();
        
        isConnection       = false;
        obtainedItemNumber = 0;
        CameraController.instance.connectionFrame.SetActive(false); 
        // 아이템 및 안티 다 지우기 클리어
        foreach (var itemLists in itemList)
            Destroy(itemLists);
        itemList.Clear();
        foreach (var antiLists in antiList)
            Destroy(antiLists);
        antiList.Clear();
        foreach (var scanLifeLists in CameraController.instance.scanLifeList) // 목숨 숨기기
            scanLifeLists.gameObject.SetActive(false);
        
        if (CameraController.instance.moveScanLineList[0].activeSelf) // 스캔라인이 켜져 있으면 끄기.(1회 켜기 및 실행)
        {
            foreach (var moveScanLineLists in CameraController.instance.moveScanLineList)
                moveScanLineLists.SetActive(false);
        }
        
        while (true)
        {
            if (PlayerController.instance.playerAnimStateInfo.IsName("Idle") || PlayerController.instance.playerAnimStateInfo.IsName("Hang") || PlayerHp.instance.isHit)
            {
                isScan        = false;
                break;
            }
            yield return null;
        }
        
        StopAllCoroutines();
    }

    private void ColorHighlightRestoration()
    {
        // 색 강조 복구(해킹안된)
        foreach (var targetObjectLists in targetObjectList)
        {
            HackingPossibleTerminal hackingPossibleTerminal = targetObjectLists.GetComponent<HackingPossibleTerminal>();
        
            if (hackingPossibleTerminal)
            {
                foreach (var bodyPart in hackingPossibleTerminal.terminalBodySpriteRendererList)
                {
                    bodyPart.material.SetFloat(hologramFadeID, 0);
                    bodyPart.material.SetFloat(innerOutlineFadeID, 0);
                    
                    bodyPart.GetComponent<HackingPossibleTerminal>().inAccelerationOrderLayer.GroupHackingLayerDisable();               // 레어이 뒤로 복구
                }
            }
        }
        targetObjectList.Clear(); // 비우기

        // 벤트 라이트 복구
        foreach (var ventLists in ventLightList)
        {
            VentLight ventLight = ventLists.GetComponent<VentLight>();
            
            ventLight.isScanCameraTouch = false;
        }
        ventLightList.Clear(); // 비우기
    }

    public void ScanFailDamage()
    {
        PlayerHp.instance.DamagePlayer(currentScanningObejectTrans,scanFailDamage);
    }
}