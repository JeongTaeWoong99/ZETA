using System.Collections;
using System.Collections.Generic;
using Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HackingPossibleTerminal : MonoBehaviour
{
    [Header("------Common------")]
    public InAccelerationOrderLayer inAccelerationOrderLayer;
    
    [HideInInspector]
    public bool isScanPossible = true;                                                  // 스캔 가능 여부(true가능 false불가)

    public  List<SpriteRenderer> terminalBodySpriteRendererList;                        // 스캔에서 색 강조할 바디 리스트
    public  SpriteRenderer       terminalLightRenderer;                                 // 터미널의 라이트 렌더러
    [HideInInspector]
    public  int                  strongTintFadeID;                                      // 페이드 이름(해킹 성공시 색 변환을 위해)
    
    private bool      isTouchoperationPossible; // 터미널에 닿아 있는지
    public  Transform designatedLocation;
    
    [HideInInspector]
    public bool                  isControlMode;                          // 컨트롤 모드
    public GameObject            controlUI;
    public Image                 focusImage;
    public List<TextMeshProUGUI> selectTextList = new List<TextMeshProUGUI>();
    
    [Header("------Highlight Text------")] 
    public TMP_FontAsset highlightFont;
    public Color         highlightTextColor;

    [Header("------Normal Text------")]
    public TMP_FontAsset normalFont;
    public Color         normalTextColor;

    [Header("------Anti------")]
    public float antiVirusNumber; // 안티 바이러스 숫자
    public float itemNumber;      // 먹어야 하는 아이템 수
    
    [Header("------Obstacle Terminal------")]
    public List<GameObject>     laserBeamGameObjects      = new List<GameObject>();     // box콜리더와 레이저 엑티브 끄기
    public List<SpriteRenderer> obstacleBodyLightMaterial = new List<SpriteRenderer>(); // 레이저 장애물 본체의 라이트의 렌더러
    public float                obstacleBodyLightChangeSpeed;

    public List<Turret>         controlledTurretList = new List<Turret>();              // 괸라하는 터렛

    public FreightGenerator     freightGenerator;                                       // 관리하는 화물 콘솔
    
    [Header("------SpecialGate Terminal------")]
    public Gate      specialGate;
    public VentLight ventLight;     // 해킹에 성공하면, 밝아지는 라이트

    private void Start()
    {
        isScanPossible = true;   // 해킹 가능으로 시작
        
        strongTintFadeID = Shader.PropertyToID("_StrongTintFade");
    }
    
    public void Update()
    {   
        // 쫒을 때, 작동불가
        if (Input.GetKeyDown(KeyCode.F)&& !isControlMode && isScanPossible && EnemyDistanceActive.instance.enemyChaseList.Count == 0 && !PlayerHp.instance.isHit &&
            isTouchoperationPossible                     && !PlayerScan.instance.isScan       && !PlayerHp.instance.isRecovery     && PlayerFloorCollider.instance.isGrounded  &&
            !PlayerAcceleration.instance.isAcceleration  && !PlayerHacking.instance.isHacking && !PlayerDash.instance.isDash)
        {
            isControlMode                       = true;
            EventController.instance.eventState = true; 
            
            UIController.instance.UISeeState(false);
            
            StartCoroutine(MoveDesignatedLocation());   // 이동
        }
    }

    private void FixedUpdate()
    {
        // 레이저 장애물 바디 라이트 관리(->독립메터리얼로 교체가 완료되면 작동하도록)
        foreach (var obstacleBodyLightMaterials in obstacleBodyLightMaterial)
        {
            // 라이트 UP
            if (isScanPossible && obstacleBodyLightMaterials.material.GetFloat("_Brightness") < 5f)
            {
                obstacleBodyLightMaterials.material.SetFloat("_Brightness", Mathf.MoveTowards(obstacleBodyLightMaterials.material.GetFloat("_Brightness"),5f,
                                                                                          Time.fixedDeltaTime * obstacleBodyLightChangeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue));
            }
            // 라이트 Down
            else if (!isScanPossible && obstacleBodyLightMaterials.material.GetFloat("_Brightness") > 0f)
            {
                obstacleBodyLightMaterials.material.SetFloat("_Brightness", Mathf.MoveTowards(obstacleBodyLightMaterials.material.GetFloat("_Brightness"),0f,
                                                                                          Time.fixedDeltaTime * obstacleBodyLightChangeSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue));
            }
        }
    }

    public void ObstacleScanTrigger()
    {
        isScanPossible = false;
        
        // 레이저 장애물 트리거 발동 
        foreach (var laserBeam in laserBeamGameObjects)
        {
            laserBeam.GetComponent<BoxCollider2D>().enabled                = false; // 충돌 끄기
            laserBeam.GetComponent<LaserBeamController>().isLaserActivated = false; // 레이저 끄기
            
            terminalLightRenderer.material.SetFloat(strongTintFadeID, 1f);     // 터미널 라이트 틴트 페이드
        }
        AudioManager.instance.ObjectSfxCreate(8,true,gameObject); // 사운드

        // 터렛 장애물 트리거 발동
        foreach (var controlledTurretLists in controlledTurretList)
        {
            controlledTurretLists.isControlTerminalScanned = true;
        }
        
        // 관리하는 화물 이동 작동
        if(freightGenerator)
            freightGenerator.isOperation = true;
    }
    
    public void SpecialGateScanTrigger()
    {
        isScanPossible = false;
        
        specialGate.anim.SetTrigger("openOn");
        AudioManager.instance.ObjectSfxCreate(6,true,gameObject); // open 사운드 생성
        
        if(ventLight)
            ventLight.isScan = true; // 스페셜 게이가 관리하는 라이트
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
                PlayerController.instance.bodyGameObject.transform.localScale 
                    = PlayerController.instance.transform.position.x < transform.position.x ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);

                // 정확한 x위치 이동
                PlayerController.instance.gameObject.transform.position = new Vector3(designatedLocation.position.x,PlayerController.instance.gameObject.transform.position.y);
                
                StartCoroutine(ControlMode());
                break;
            }       
            
            // 이동
            if (PlayerController.instance.transform.position.x < designatedLocation.position.x)
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
                    AudioManager.instance.ObjectSfxCreate(3,true,gameObject);   // 인터렉션 사운드(1회 재생)
                    
                    StartCoroutine(EscapeControlMode());
                    
                    ObstacleScanTrigger();                      // 터미널 조작 장애물 끄기 실행
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
        AudioManager.instance.ObjectSfxCreate(3,true,gameObject);   // 인터렉션 사운드

        while (true)
        {
            if (PlayerController.instance.playerAnimStateInfo.IsName("Idle"))
                break;

            yield return new WaitForFixedUpdate();
        }
        
        UIController.instance.UISeeState(true);
                
        controlUI.SetActive(false);
        MenuManager.instance.menuKeyExUI.gameObject.SetActive(false);
                
        isControlMode                       = false;
        EventController.instance.eventState = false;
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
}
