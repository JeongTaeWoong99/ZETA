using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SaveTerminal : MonoBehaviour
{
    [Header("------Common------")]
    public Transform designatedLocation;
    [HideInInspector] 
    public  bool isSaving;                 // 저장 중 인지
    private bool isTouchoperationPossible; // 터미널에 닿아 있는지
    
    public GameObject            controlUI;
    public Image                 focusImage;

    public List<TextMeshProUGUI> selectTextList = new List<TextMeshProUGUI>();

    [Header("------Highlight Text------")] 
    public TMP_FontAsset highlightFont;
    public Color         highlightTextColor;

    [Header("------Normal Text------")]
    public TMP_FontAsset normalFont;
    public Color         normalTextColor;
    
    public void Update()
    {                                                                  // 쫒을 때, 작동불가
        if (Input.GetKeyDown(KeyCode.F) && !isSaving    && EnemyDistanceActive.instance.enemyChaseList.Count == 0             && !PlayerHp.instance.isHit                 &&
            isTouchoperationPossible                    && !PlayerScan.instance.isScan       && !PlayerHp.instance.isRecovery && PlayerFloorCollider.instance.isGrounded  &&
            !PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking && !PlayerDash.instance.isDash)
        {
            isSaving                            = true;
            EventController.instance.eventState = true; 
            
            UIController.instance.UISeeState(false);
            
            StartCoroutine(MoveDesignatedLocation());
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
                yield return new WaitForFixedUpdate(); // 이전 입력 겹치기 방지 대기
                PlayerController.instance.rb2D.velocity = Vector2.zero;
                PlayerController.instance.playerAnim.SetBool("run", false);
                
                // 좌우반전(콘솔 바라보기)
                if (PlayerController.instance.transform.position.x < transform.position.x)
                    PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
                else
                    PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                    
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
                    StartCoroutine(Save());
                else if (currentSelectedNum == 1)
                    StartCoroutine(EscapeControlMode());
                
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
                AudioManager.instance.UISoundPlay(0); // UI 이동 사운드       
                       
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
        MenuManager.instance.menuKeyExUI.gameObject.SetActive(false); // 키 설명
                
        isSaving                            = false;
        EventController.instance.eventState = false;
    }
    
    private IEnumerator Save()
    {
        AudioManager.instance.ObjectSfxCreate(3,true,gameObject);   // 인터렉션 사운드
        PlayerController.instance.playerAnim.SetTrigger("interaction1Off"); 
        
        controlUI.SetActive(false);
        MenuManager.instance.menuKeyExUI.gameObject.SetActive(false); // 키 설명
    
        // 현재씬 이름 플레이어프리펩 저장
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SaveSeenName",currentSceneName);
        
        // 세이브 터미널 이름 저장
        string saveTerminalName = gameObject.name;
        PlayerPrefs.SetString("SaveTerminalName",saveTerminalName);
        
        // 페이드
        yield return StartCoroutine(FadeManager.instance.NextSeenFadeIn());
        
        // 체력 및 게이지 MAX값 저장.
        PlayerPrefs.SetInt("currentHP",PlayerHp.instance.maxHealth);                   // 체력 최대값으로 저장
        PlayerPrefs.SetFloat("currentGage",UIController.instance.gageSlider.maxValue); // 게이지 최대값으로 저장
        
        // 씬 재시작
        SceneManager.LoadScene(currentSceneName);

        yield return null;
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
