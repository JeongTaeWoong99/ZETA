using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PlayerHacking : MonoBehaviour
{
    public static PlayerHacking instance;
    
    [Header("------HackingStart------")] 
    public float      hackingGageValue;                           // 필요 게이지
    public float      filterChangeSpeed;                          
    [HideInInspector] 
    public bool       isHacking;
    
    public  float hackingTimePerGage;          // hackingTimePerGage 시간이 지나면, 게이지가 0.1씩 빠르게 감소하고, 다 감소되면, UI가 사라지도록 함.
    private float hackingTimePerGageCount; 

    [Header("------FindObject------")] 
    public LayerMask  hackingTargetLayer;
    
    [HideInInspector]
    public List<GameObject> targetObjectList = new List<GameObject>();
    
    private List<GameObject> ventLightList = new List<GameObject>();  // 찾은 뒷길 라이트 리스트
    
    private float pivotMaxX;
    private float pivotMinX;
    private float pivotMaxY;
    private float pivotMinY;
    
    private int hologramFadeID;     // 홀로그렘 페이드 ID
    private int innerOutlineFadeID; // 이너라인 페이드 ID
    [HideInInspector]
    public  int  strongTintFadeID;  // 스트롱 틴트 페이드 아이디

    [Header("------TakeControlMode------")]
    public  GameObject       hackingLine;
    public  GameObject       focusCirclePrefabs;
    [HideInInspector]
    public  GameObject       focusCircleGameObject; // 튜토리얼 때, 위치를 알 수 있도록 public
    
    public  KeyCode[]        keysToChooseFrom  = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };
    public  List<GameObject> keysPrefabs       = new List<GameObject>();
    
    private int brightnessFadeID;     // 홀로그렘 페이드 ID(키를 눌러야 할 때, 빛나기)
    
    private List<GameObject> successList       = new List<GameObject>();            // 해킹성공 리스트
    public  float            successValueAdd;                                       // 키입력 성공 게이지 증가
    public  float            failValueMinus;                                        // 키입력 실패 게이지 감소

    public  float            startHackingTime;
    public  float            multiplicationValuePerHackingTarget;                   // 해킹타겟당곱값
    private float            hackingHoldTime;                                       // 해킹 지속시간
    private float            hackingHoldTimeCount;
    
    public  int              hackingDamage;
    public  float            stunTime;

    public GameObject hamilton;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        hologramFadeID      = Shader.PropertyToID("_HologramFade");
        innerOutlineFadeID  = Shader.PropertyToID("_InnerOutlineFade");
        strongTintFadeID    = Shader.PropertyToID("_StrongTintFade");
        brightnessFadeID    = Shader.PropertyToID("_Brightness");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)                && PlayerHp.instance.liveState          && !PlayerHp.instance.isHit     
        && !PlayerAcceleration.instance.isAcceleration && !isHacking                           && !PlayerHp.instance.isRecovery
        && !PlayerScan.instance.isScan                 && !EventController.instance.eventState && !MenuManager.instance.isNormalMenu 
        && PlayerAcceleration.instance.accelerationChangedTimeValue == 1 && PlayerAcceleration.instance.colorFilterMaterial.GetFloat("_Contrast_Intensity") == 2f)
        {
            if (!EventController.instance.hackingLock && UIController.instance.gageSlider.value >= hackingGageValue)
            {
                isHacking = true;

                // 시간 멈추기
                Time.timeScale      = 0f;
                
                AudioManager.instance.currentAmbientSoundNum = 999; // BGM 복구
                
                StartCoroutine(HackingStart());
            }
            else
            {
                AudioManager.instance.UISoundPlay(4);
            }
        }
    }

    // 줌과 타임스케일 제로
    private IEnumerator HackingStart()
    {
        // 카운트 초기화
        hackingTimePerGageCount = 0f;
        
        AudioManager.instance.DirectingPlay(3);
        
        while (true)
        {
            // 게이지 감소
            hackingTimePerGageCount += Time.unscaledDeltaTime;
            if (hackingTimePerGageCount > hackingTimePerGage)
            {
                hackingTimePerGageCount                 = 0f;   // 초기화
                UIController.instance.gageSlider.value -= 0.1f; // 게이지 1칸 = 0.1감소
            }
            
            // 가속필터 켜지기
            PlayerAcceleration.instance.colorFilterMaterial.SetFloat("_Contrast_Intensity",
                                                                     Mathf.Lerp(PlayerAcceleration.instance.colorFilterMaterial.GetFloat("_Contrast_Intensity"), 0f, Time.unscaledDeltaTime * filterChangeSpeed));
            
            if (PlayerAcceleration.instance.colorFilterMaterial.GetFloat("_Contrast_Intensity") <= 0.01f && UIController.instance.gageSlider.value <= 0f)
            {
                // UI 숨기기
                UIController.instance.UISeeState(false);
                PlayerAcceleration.instance.colorFilterMaterial.SetFloat("_Contrast_Intensity",0f);
                
                StartCoroutine(AreaExpansion());
                break;
            }
            
            yield return null;
        }
    }
    
    private IEnumerator AreaExpansion()
    {
        CameraController.instance.hackingPanel.SetActive(true); // 해킹 패널 on
        AudioManager.instance.DirectingPlay(4);                 // 1회 재생
        AudioManager.instance.DirectingPlay(5);                 // 루프
        
        PlayerScan.instance.hackingOverlayEffectShader.transform.localScale = Vector3.zero; // 해킹 이펙트 쉐이더 최소크기
        
        UIController.instance.scanStateText.gameObject.SetActive(true); // 스캔 상태 텍스트 켜기(스캔 및 전투 해킹에서 켜기)
        UIController.instance.scanStateText.text = "부팅 중...";         // 텍스트
        
        // 후처리
        float percent = 0.0f;
        while (true)
        {
            percent +=  Time.unscaledDeltaTime * PlayerScan.instance.hackingOverLaySpeed;
            
            // 카메라 크기 키우기
            CameraController.instance.mainCam.orthographicSize = Mathf.Lerp(CameraController.instance.originOrthographicSize,PlayerScan.instance.hackingStateCameraSize, percent);
            
            // 켄버스 사이즈 조정(= 시간이 멈추면, FixedUpdate에서 켄버스 사이즈 반영이 안됨.)
            CameraController.instance.CanvasSizeToOrthographicSize();
            
            // 해킹 영역 전개
            PlayerScan.instance.hackingOverlayEffectShader.transform.localScale 
                = Vector3.Lerp(Vector3.zero, new Vector3(PlayerScan.instance.hackingOverLayMaxSize,PlayerScan.instance.hackingOverLayMaxSize,PlayerScan.instance.hackingOverLayMaxSize), percent);
            
            if (percent >= 1f) 
            {
                StartCoroutine(FindTarget());
                yield break;
            }
            
            yield return null;
        }
    }

    private IEnumerator FindTarget()
    {
        // 보이는 적 찾기(카메라 위치 기준으로, 범위안에 있는 적 저장)
        float cameraHeight  = CameraController.instance.originOrthographicSize * 2f; // 카메라 y 크기 
        float cameraWidth   = cameraHeight * Camera.main.aspect;                     // 카메라 x 크기 
        RaycastHit2D[] hits = Physics2D.BoxCastAll(CameraController.instance.transform.position, new Vector2(cameraWidth, cameraHeight * 0.9f), 
                                                          0f, Vector2.zero, 0f, hackingTargetLayer);
        // 벽으로 막혀 있는 적 빼기(범위 안에 있지만, 벽으로 막혀 있으면, 리스트에서 제거)
        targetObjectList.Clear();
        
        foreach (var hit in hits)
        {
            EnemyHp       enemyHp       = hit.collider.GetComponent<EnemyHp>();
            GuidedMissile guidedMissile = hit.collider.GetComponent<GuidedMissile>();
            Mine          mine          = hit.collider.GetComponent<Mine>();
            
            VentLight     ventLight     = hit.collider.GetComponent<VentLight>();
            
            // 일반적
            if (enemyHp)
            {
                if (enemyHp.liveState && !enemyHp.isStun && hit.collider.GetComponent<EnemyLightController>().isAppear)
                        targetObjectList.Add(hit.collider.gameObject);
            }
            // 유도미사일
            else if (guidedMissile)
            {
                if (guidedMissile.isTracking && guidedMissile.isHackingPossible)
                    targetObjectList.Add(hit.collider.gameObject);
            }
            // 마인
            else if (mine)
            {
                if (!mine.isTriggerOn && mine.isHackingPossible && mine.isAppear)
                    targetObjectList.Add(hit.collider.gameObject);
            }
            
            // 벤트 라이트
            if (ventLight)
            {
                ventLight.isScanCameraTouch = true;      // 라이트 밝히기
                ventLightList.Add(ventLight.gameObject); // 리스트 저장
            }
        }

        UIController.instance.scanStateText.text = "스캔 중...";        // 텍스트
        if (!CameraController.instance.moveScanLineList[0].activeSelf) // 스캔라인이 켜져 있으면 끄기.(1회 켜기 및 실행)
        {
            AudioManager.instance.DirectingPlay(6); // 1회 재생
            foreach (var moveScanLineLists in CameraController.instance.moveScanLineList)
                moveScanLineLists.SetActive(true);
        }
        
        yield return new WaitForSecondsRealtime(PlayerScan.instance.scanRenewalTime);   // 스캔라인이 내려오는 리뉴얼 시간은 공통으로 사용.
        
        // 적이 없음 -> 바로 종료
        if (targetObjectList.Count == 0)
        {
            UIController.instance.scanStateText.text = "스캔 완료. 타겟 없음.";
            
            yield return new WaitForSecondsRealtime(0.5f);
            
            StartCoroutine(HackingEnd());
        }
        else
        {
            // 0. 겹치는 타겟 미리 빼버리기
            RemoveCloseObjects();
        
            // 1. 첫번째 피봇값 찾기(왼쪽 위)
            CalculatePivot();
            MoveClosestToFront(targetObjectList, pivotMinX, pivotMaxY);
            
            // 헤밀턴 체크
            // 체크 전 리스트 관리
            // n = 1: 1! = 1
            // n = 2: 2! = 2
            // n = 3: 3! = 6
            // n = 4: 4! = 24
            // n = 5: 5! = 120
            // n = 6: 6! = 720
            // n = 7: 7! = 5040
            // n = 8: 8! = 40320
            // n = 9: 9! = 362880
            // n = 10: 10! = 3628800
            if (targetObjectList.Count >= 8)
            {
                while (targetObjectList.Count >= 8)
                {
                    // 리스트의 맨 뒤의 값을 제거
                    // 8이 될 때 까지
                    targetObjectList.RemoveAt(targetObjectList.Count - 1);
                    yield return null;
                }
            }
            
            GameObject hamiltonCheckObject = Instantiate(hamilton, transform.position, Quaternion.identity);
            while (true)
            {
                if (hamiltonCheckObject.IsDestroyed()) // 오브젝트가 파괴되면, 체크가 완료된 것.
                    break;
                yield return null;
            }
            
            // 4. 찾은 적 색 강조
            foreach (var enemyObject in targetObjectList)
            {
                GuidedMissile guidedMissile = enemyObject.GetComponent<GuidedMissile>();
                Mine          mine          = enemyObject.GetComponent<Mine>(); 
                EnemyHp       enemyHp       = enemyObject.GetComponent<EnemyHp>();
            
                // 적
                if (enemyHp)
                {
                    enemyHp.skeletonCustom.changeIndependentMat3.SetFloat(hologramFadeID,     0.5f);    // 바디 홀로 켜기
                    enemyHp.skeletonCustom.changeIndependentMat3.SetFloat(innerOutlineFadeID, 1f);      // 바디 이너 켜기
            
                    enemyHp.inAccelerationOrderLayer.GroupHackingLayerEnable(); // 레이어 앞으로
                }
                // 유도미사일
                else if(guidedMissile)
                {
                    guidedMissile.GetComponent<SpriteRenderer>().material.SetFloat(hologramFadeID,     0.5f);
                    guidedMissile.GetComponent<SpriteRenderer>().material.SetFloat(innerOutlineFadeID, 1f);
            
                    guidedMissile.inAccelerationOrderLayer.GroupHackingLayerEnable(); // 레이어 앞으로
                }
                // 마인
                else if (mine)
                {
                    mine.GetComponent<SpriteRenderer>().material.SetFloat(hologramFadeID,     0.5f);
                    mine.GetComponent<SpriteRenderer>().material.SetFloat(innerOutlineFadeID, 1f);
            
                    mine.inAccelerationOrderLayer.GroupHackingLayerEnable(); // 레이어 앞으로
                }
            }
            
            UIController.instance.scanStateText.text = "스캔 완료. 타겟 " + targetObjectList.Count + "개 발견.";

            yield return new WaitForSecondsRealtime(0.5f);
            
            StartCoroutine(TakeControlMode());
        }
    }
    
    // 겹치는 타겟은 빼버리기
    public void RemoveCloseObjects()
    {
        for (int i = 0; i < targetObjectList.Count - 1; i++)
        {
            GameObject currentObject = targetObjectList[i];

            for (int j = i + 1; j < targetObjectList.Count; j++)
            {
                GameObject nextObject = targetObjectList[j];
                float distance = Vector3.Distance(currentObject.transform.position, nextObject.transform.position);

                if (distance < 0.5f)
                {
                    targetObjectList.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    private void CalculatePivot()
    {
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float minX = float.MaxValue;
        float minY = float.MaxValue;
    
        foreach (GameObject obj in targetObjectList)
        {
            Vector3 position = obj.transform.position;
            maxX = Mathf.Max(maxX, position.x);
            maxY = Mathf.Max(maxY, position.y);
            minX = Mathf.Min(minX, position.x);
            minY = Mathf.Min(minY, position.y);
        }
    
        pivotMaxX = maxX;
        pivotMinX = minX;
        pivotMaxY = maxY;
        pivotMinY = minY;
    }
    
    private void MoveClosestToFront(List<GameObject> list, float targetX, float targetY)
    {
        if (list.Count == 0)
        {
            // 리스트가 비어있으면 아무 작업도 수행하지 않음
            return;
        }
    
        Vector2 targetPosition = new Vector2(targetX, targetY);
    
        // 리스트에서 가장 가까운 값 찾기
        int closestIndex = 0;
        float closestDistance = Vector2.Distance(list[0].transform.position, targetPosition);
    
        for (int i = 1; i < list.Count; i++)
        {
            float distance = Vector2.Distance(list[i].transform.position, targetPosition);
            if (distance < closestDistance)
            {
                closestIndex = i;
                closestDistance = distance;
            }
        }
    
        // 가장 가까운 값 맨 앞으로 이동
        if (closestIndex != 0)
        {
            GameObject closestObject = list[closestIndex];
            list.RemoveAt(closestIndex);
            list.Insert(0, closestObject);
        }
    }

    private IEnumerator TakeControlMode()
    {
        // 키할당
        int numKeysToSelect             = targetObjectList.Count;                // 포착된 적 만큼 반복
        KeyCode[] selectedKeys          = new KeyCode[numKeysToSelect];          // numKeysToSelect 크기만큼, 키코드 Array 생성
        KeyCode[] keysPool              = (KeyCode[])keysToChooseFrom.Clone();   // 똑같은 클론을 복사
        List<GameObject> keyPrefabsList = new List<GameObject>(numKeysToSelect); // 키 프리팹 리스트

        for (int i = 0; i < numKeysToSelect; i++)
        {
            int randomIndex = Random.Range(0, keysPool.Length); // 랜덤 숫자 선택
            selectedKeys[i] = keysPool[randomIndex];            // 랜덤 값 저장
            GameObject currentKeyPrefabs;                       // 현새 키생성 프리팹 게임오브젝트
            
            EnemyHp       enemyHp       = targetObjectList[i].GetComponent<EnemyHp>();
            GuidedMissile guidedMissile = targetObjectList[i].GetComponent<GuidedMissile>();
            Mine          mine          = targetObjectList[i].GetComponent<Mine>();
            
            // 일반적
            if (enemyHp)
            {
                currentKeyPrefabs = Instantiate(keysPrefabs[randomIndex], enemyHp.bodyHackingTransform.transform.position, Quaternion.identity); // 생성저장(생성 포지션 = bodyHackingTransform)
                currentKeyPrefabs.GetComponent<SortingGroup>().sortingOrder = 1000 - i;                               // 레이어값 변경
                keyPrefabsList.Add(currentKeyPrefabs);                                                                // 키 가이드 프리팹 생성 및 저장(일반 적)
            }
            // 유도미사일
            else if (guidedMissile)
            {
                currentKeyPrefabs = Instantiate(keysPrefabs[randomIndex], guidedMissile.transform.position, Quaternion.identity); // 생성저장
                currentKeyPrefabs.GetComponent<SortingGroup>().sortingOrder = 1000 - i;                                           // 레이어값 변경
                keyPrefabsList.Add(currentKeyPrefabs);                                                                            // 키 가이드 프리팹 생성 및 저장(미사일)
            }
            // 마인
            else if (mine)
            {
                currentKeyPrefabs = Instantiate(keysPrefabs[randomIndex], mine.transform.position, Quaternion.identity); // 생성저장
                currentKeyPrefabs.GetComponent<SortingGroup>().sortingOrder = 1000 - i;                                  // 레이어값 변경
                keyPrefabsList.Add(currentKeyPrefabs);                                                                   // 키 가이드 프리팹 생성 및 저장(마인)
            }
            
            AudioManager.instance.DirectingSfxCreate(13);    // 포착 사운드 재생
            
            yield return new WaitForSecondsRealtime(0.1f);
        }

        yield return new WaitForSecondsRealtime(0.5f);
        
        // 경로(점선)생성
        GameObject hackingLineClone = Instantiate(hackingLine, targetObjectList[0].transform.position, Quaternion.identity);
        LineRenderer lineRenderer   = hackingLineClone.GetComponent<LineRenderer>();
        lineRenderer.positionCount  = targetObjectList.Count;
        for (int i = 0; i < targetObjectList.Count; i++)
        {
            EnemyHp       enemyHp       = targetObjectList[i].GetComponent<EnemyHp>();
            GuidedMissile guidedMissile = targetObjectList[i].GetComponent<GuidedMissile>();
            Mine          mine          = targetObjectList[i].GetComponent<Mine>();
        
            // 일반적
            if (enemyHp)
                lineRenderer.SetPosition(i,enemyHp.bodyHackingTransform.transform.position);
            // 유도미사일
            else if (guidedMissile)
                lineRenderer.SetPosition(i,guidedMissile.transform.position);
            // 마인
            else if (mine)
                lineRenderer.SetPosition(i,mine.transform.position);
        }
        
        // 포커스 생성
        if(targetObjectList[0].GetComponent<EnemyHp>())
            focusCircleGameObject = Instantiate(focusCirclePrefabs, targetObjectList[0].GetComponent<EnemyHp>().bodyHackingTransform.transform.position, Quaternion.identity); // 포커스 프리팹 생성(일반적)
        else if(targetObjectList[0].GetComponent<GuidedMissile>())
            focusCircleGameObject = Instantiate(focusCirclePrefabs, targetObjectList[0].transform.position                                             , Quaternion.identity); // 포커스 프리팹 생성(유도미사일)
        else if(targetObjectList[0].GetComponent<Mine>())
            focusCircleGameObject = Instantiate(focusCirclePrefabs, targetObjectList[0].transform.position                                             , Quaternion.identity); // 포커스 프리팹 생성(마인)

        // 눌러야 하는 키 빛나기
        foreach (var arrowList in keyPrefabsList[0].gameObject.GetComponent<OneOffUI>().arrowSpriteRendererList)
            arrowList.material.SetFloat(brightnessFadeID,3);

        UIController.instance.scanStateText.text = "해킹 시도 중...";                  // 텍스트
        
        // 해킹 홀드타임 값 할당
        var activeHackingHoldTimeValue = startHackingTime;                       // 시작값 넣어주기
        for (int i = 0; i < targetObjectList.Count; i++)                             // 곱값 만큼 시간 증가
            activeHackingHoldTimeValue *= multiplicationValuePerHackingTarget;
        hackingHoldTime = activeHackingHoldTimeValue;                                // 값 할당
        UIController.instance.hackingTimeRemainingSlider.maxValue = hackingHoldTime; // UI 최고값 변경
        
        // 입력모드 시작
        UIController.instance.hackingTimeRemainingSlider.gameObject.SetActive(true); // 타임리메인 슬라이더 켜기
        int currentNum  = 0;                                                         // 현재 해킹 번호
        
        while (currentNum < selectedKeys.Length || hackingHoldTimeCount > hackingHoldTime)
        {
            if (!EventController.instance.eventState ||
                (EventController.instance.eventState && hackingHoldTime/2 > hackingHoldTimeCount)) // 튜토리얼 W 이벤트 모드이면,
                                                                                                   // hackingHoldTimeCount의 값이 hackingHoldTime/2 보다 작을 때 까지만 감소하기
                hackingHoldTimeCount += Time.unscaledDeltaTime; // 남은시간 감소
            
            if(hackingHoldTimeCount > hackingHoldTime)
                break;
            UIController.instance.hackingTimeRemainingSlider.value = hackingHoldTime - hackingHoldTimeCount;  // 남은시간 표시
            
            if (Input.anyKeyDown)
            {
                AudioManager.instance.DirectingSfxCreate(14);    // 키 입력 기본 사운드
                // 검사
                KeyCode pressedKey = GetAnyPressedKey();
                // 키입력 성공
                if (pressedKey == selectedKeys[currentNum])
                {
                    AudioManager.instance.DirectingSfxCreate(16);    // 키 입력 성공 사운드
                
                    hackingHoldTimeCount -= successValueAdd;       // 홀드타임 증가
                    successList.Add(targetObjectList[currentNum]); // 성공 리스트에 추가
                    
                    // 해킹 성공시 파랑색으로 변경.
                    EnemyHp       enemyHp       = targetObjectList[currentNum].GetComponent<EnemyHp>();
                    GuidedMissile guidedMissile = targetObjectList[currentNum].GetComponent<GuidedMissile>();
                    Mine          mine          = targetObjectList[currentNum].GetComponent<Mine>();
                    
                    if (enemyHp)
                    {
                        enemyHp.skeletonCustom.changeIndependentMat3.SetFloat(strongTintFadeID, 1f); // 바디 스트롱 틴트 켜기.
                        enemyHp.skeletonCustom.changeIndependentMat2.SetFloat(strongTintFadeID, 1f); // 바디 라이트 스트롱 틴트 켜기.
                    }
                    else if (guidedMissile)
                    {
                        guidedMissile.GetComponent<SpriteRenderer>().material.SetFloat(strongTintFadeID, 1f); // 바디 스트롱 틴트 켜기.
                        guidedMissile.guidedMissileLightRenderer.color = guidedMissile.seizingControlColor;        // 라이트 색변경(파랑)
                    }
                    else if (mine)
                    {
                        mine.GetComponent<SpriteRenderer>().material.SetFloat(strongTintFadeID, 1f);  // 바디 스트롱 틴트 켜기.
                        mine.lightGameObject.GetComponent<SpriteRenderer>().color = mine.mineToEnemyColor; // 라이트 색변경(파랑)
                    }
                }
                // 키입력 실패
                else
                {
                    // W 이벤트 중, 뒷 부분을 실행하지 않고, while 처음으로 이동
                    if (EventController.instance.eventState)
                    {
                        yield return null; // 넘거가기 되는 경우, 해당 부분 까지의 while의 yield가 필요함.(없으면 버그 걸림.)
                        continue;          // 실패시 넘어가기
                    }
                    AudioManager.instance.DirectingSfxCreate(15); // 키 입력 실패 사운드
                    hackingHoldTimeCount += failValueMinus;  // 홀드타임 감소
                }
                
                // 키 가이드 프리팹 삭제
                keyPrefabsList[currentNum].gameObject.GetComponent<Animator>().SetTrigger("endOn");
                
                // 지나간 키 밝기 없애기 (밝기 3 -> 0)
                foreach (var arrowList in keyPrefabsList[currentNum].gameObject.GetComponent<OneOffUI>().arrowSpriteRendererList)
                    arrowList.material.SetFloat(brightnessFadeID,0);
                
                currentNum++;   // 현재숫자 증가(레이다시 그리기용)
                
                // 레이 다시 그리기    
                lineRenderer.positionCount = targetObjectList.Count - currentNum;
                for (int i = currentNum; i < targetObjectList.Count; i++)
                {
                    EnemyHp       enemyHp_Ray       = targetObjectList[i].GetComponent<EnemyHp>();
                    GuidedMissile guidedMissile_Ray = targetObjectList[i].GetComponent<GuidedMissile>();
                    Mine          mine_Ray          = targetObjectList[i].GetComponent<Mine>();
                
                    // 일반적
                    if (enemyHp_Ray)
                        lineRenderer.SetPosition(i-currentNum,enemyHp_Ray.bodyHackingTransform.transform.position);
                    // 유도미사일
                    else if (guidedMissile_Ray)
                        lineRenderer.SetPosition(i-currentNum,guidedMissile_Ray.transform.position);
                    // 마인
                    else if (mine_Ray)
                        lineRenderer.SetPosition(i-currentNum,mine_Ray.transform.position);
                    
                    // 포커스 이동 + 눌러야 하는 키 빛나기
                    if (currentNum == i && currentNum < selectedKeys.Length)
                    {
                        // 포커스 이동
                        if(enemyHp_Ray)
                            focusCircleGameObject.transform.position = enemyHp_Ray.bodyHackingTransform.transform.position;
                        else if(guidedMissile_Ray)
                            focusCircleGameObject.transform.position = guidedMissile_Ray.transform.position;
                        else if(mine_Ray)
                            focusCircleGameObject.transform.position = mine_Ray.transform.position;
                        
                        // 다음 키 빛나기 (밝기 0.5 -> 3)
                        foreach (var arrowList in keyPrefabsList[currentNum].gameObject.GetComponent<OneOffUI>().arrowSpriteRendererList)
                            arrowList.material.SetFloat(brightnessFadeID,3);
                    }
                }
            }
            yield return null;
        }
        
        Destroy(focusCircleGameObject);                      // 포커스 삭제
        Destroy(hackingLineClone);                           // 라인렌더러 삭제
        foreach (var keyPrefabs in keyPrefabsList) // 남아있는 키 가이드 삭제
        {
            if(keyPrefabs != null)
                keyPrefabs.gameObject.GetComponent<Animator>().SetTrigger("endOn");
        }
        
        UIController.instance.hackingTimeRemainingSlider.gameObject.SetActive(false);   // 타임리메인 슬라이더 끄기
        hackingHoldTimeCount = 0f;                                                      // 카운트 초기화

        UIController.instance.scanStateText.text = successList.Count + "개 해킹 성공.";

        yield return new WaitForSecondsRealtime(0.5f);
        
        StartCoroutine(HackingEnd());
    }
    
    private KeyCode GetAnyPressedKey()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                return key;
            }
        }
        return KeyCode.None;
    }
    
    private IEnumerator HackingEnd()
    {
        if (CameraController.instance.moveScanLineList[0].activeSelf) // 스캔라인이 켜져 있으면 끄기.(1회 켜기 및 실행)
        {
            foreach (var moveScanLineLists in CameraController.instance.moveScanLineList)
                moveScanLineLists.SetActive(false);
        }
    
        // 해킹 성공한 적, 트리거 발동
        foreach (var successEnemyObject in successList)
        {
            EnemyHp       enemyHp       = successEnemyObject.GetComponent<EnemyHp>();
            GuidedMissile guidedMissile = successEnemyObject.GetComponent<GuidedMissile>();
            Mine          mine          = successEnemyObject.GetComponent<Mine>();
        
            // 일반적
            if (enemyHp)
            {
                enemyHp.hitAnimNum = 3;
                enemyHp.DamageEnemy(hackingDamage,PlayerController.instance.transform,stunTime);
                enemyHp.inAccelerationOrderLayer.GroupHackingLayerDisable(); // 레이어 뒤로 (복구)
            }
            // 유도미사일
            else if (guidedMissile)
            {
                guidedMissile.SeizingControlTrigger();
                guidedMissile.inAccelerationOrderLayer.GroupHackingLayerDisable(); // 레이어 뒤로 (복구)
            }
            // 마인
            else if (mine)
            {
                mine.HackingTrigger();
                mine.inAccelerationOrderLayer.GroupHackingLayerDisable(); // 레이어 뒤로 (복구)
            }
        
        }
        successList.Clear();

        // 색강조 복구
        // 정삭 색 = 2 // 필터 색 최대 = 0
        PlayerAcceleration.instance.colorFilterMaterial.SetFloat("_Contrast_Intensity", 2f);
        
        // 시간 정상화
        Time.timeScale      = 1.0f;
        //Time.fixedDeltaTime = Time.timeScale * 0.02f;
        
        // 게이지 초기화
        UIController.instance.gageSlider.value = 0f;
        
        // UI 보이기
        UIController.instance.UISeeState(true);
        
        // 컬러 필트 강조
        PlayerAcceleration.instance.colorFilterMaterial.SetFloat("_Contrast_Intensity", 2f);
        
        CameraController.instance.hackingPanel.SetActive(false);                            // 해킹 패널 off
        AudioManager.instance.DirectingStop(5);                                      // 루프 멈추기
        
        PlayerScan.instance.hackingOverlayEffectShader.transform.localScale = Vector3.zero; // 해킹 오버레이 크기 조정
        UIController.instance.scanStateText.gameObject.SetActive(false);                    // 스캔 상태 텍스트 켜기(스캔 및 전투 해킹에서 켜기)
    
        CameraController.instance.mainCam.orthographicSize = CameraController.instance.originOrthographicSize; // 카메라 복구
        CameraController.instance.CanvasSizeToOrthographicSize();                                              // 복구
        
        // 색 강조 복구(전체)
        foreach (var enemyObject in targetObjectList)
        {
            if(enemyObject == null)
                continue;
            EnemyHp       enemyHp       = enemyObject.GetComponent<EnemyHp>();
            GuidedMissile guidedMissile = enemyObject.GetComponent<GuidedMissile>();
            Mine          mine          = enemyObject.GetComponent<Mine>();
            
            if (enemyHp)
            {
                enemyHp.skeletonCustom.changeIndependentMat3.SetFloat(hologramFadeID, 0);     // 홀로 끄기
                enemyHp.skeletonCustom.changeIndependentMat3.SetFloat(innerOutlineFadeID, 0); // 이너 끄기
                
                enemyHp.skeletonCustom.changeIndependentMat3.SetFloat(strongTintFadeID, 0f);  // 바디의 스트롱 틴트만 끄기(-> 라이트의 스트롱 틴트는 스턴이 끝나면, 꺼지도록 함.)
                
                enemyHp.inAccelerationOrderLayer.GroupHackingLayerDisable();                       // 레이어 뒤로 (복구)
            }
            else if (guidedMissile)
            {
                guidedMissile.GetComponent<SpriteRenderer>().material.SetFloat(hologramFadeID, 0);
                guidedMissile.GetComponent<SpriteRenderer>().material.SetFloat(innerOutlineFadeID, 0);
                
                guidedMissile.GetComponent<SpriteRenderer>().material.SetFloat(strongTintFadeID, 0f); // 바디의 스트롱 틴트만 끄기(-> 라이트의 스트롱 틴트는 스턴이 끝나면, 꺼지도록 함.)
                
                guidedMissile.inAccelerationOrderLayer.GroupHackingLayerDisable();
            }
            else if (mine)
            {
                mine.GetComponent<SpriteRenderer>().material.SetFloat(hologramFadeID, 0);
                mine.GetComponent<SpriteRenderer>().material.SetFloat(innerOutlineFadeID, 0);
                
                mine.GetComponent<SpriteRenderer>().material.SetFloat(strongTintFadeID, 0f); // 바디의 스트롱 틴트만 끄기(-> 라이트의 스트롱 틴트는 스턴이 끝나면, 꺼지도록 함.)
                
                mine.inAccelerationOrderLayer.GroupHackingLayerDisable();
            }
        }
        targetObjectList.Clear();

        // 뒷길 라이트 정상화
        foreach (var ventLists in ventLightList)
        {
            VentLight ventLight = ventLists.GetComponent<VentLight>();
            
            ventLight.isScanCameraTouch = false;
        }
        ventLightList.Clear();   // 비우기
        
        AudioManager.instance.currentAmbientSoundNum = EventController.instance.BGMnum; // BGM 복구
        
        isHacking = false;
        
        yield return null;
        
        StopAllCoroutines();
    }
}