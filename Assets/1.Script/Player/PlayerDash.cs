using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public static PlayerDash instance;

    public  GameObject dashEffect;

    [HideInInspector] 
    public  bool  isDash;
    
    public  float dashSpeed;
    public  float dashDelay;
    private float dashDelayTimeCount;

    public float inDashColliderY;   //대쉬 중, 감소할 콜리더의 사이즈와 오프셋 Y값
       
    [HideInInspector]
    public float decelerationValue; // 애니메이션 진행 당 감속 값(공격3에서도 사용)

    private void Awake()
    {
        instance = this;
    }
    
    private void Update()
    {
        // 대쉬(입력)
        if (Input.GetKeyDown(KeyCode.S)   && !Input.GetKeyDown(KeyCode.A)           && !Input.GetKeyDown(KeyCode.Space)   && 
            PlayerHp.instance.liveState   && !PlayerHp.instance.isHit               && !PlayerHacking.instance.isHacking  && !PlayerScan.instance.isScan 
            && !isDash                    && !MenuManager.instance.isNormalMenu     && !PlayerController.instance.isJumpMacro && !PlayerController.instance.isCornerClimb &&
            !PlayerController.instance.playerAnimStateInfo.IsName("Attack3")        && !PlayerController.instance.isHangWall &&
            dashDelayTimeCount>dashDelay  && !EventController.instance.eventState   && !EventController.instance.dashLock)
        {
            // 인풋 Update
            Dash();
        }
        
        // 딜레이 체크
        if(!isDash)
            dashDelayTimeCount += Time.deltaTime;
    }

    private void Dash()
    {
        // 튜튜리얼 인풋 체크
        EventController.instance.tutorialDashCheckCount += 1;
        
        // 상태값 변경
        isDash = true;
        
        PlayerAttack.instance.isAttackState = false;
        PlayerHp.instance.isHit             = false;
        PlayerAttack.instance.enterAhead    = false;
        
        // 좌우반전(누르고 있는 키에 따라서 대쉬하거나, 바라보는 방향)
        if (Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow))
            PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(1, 1, 1);  // 좌우반전
        else if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            PlayerController.instance.bodyGameObject.transform.localScale = new Vector3(-1, 1, 1); // 좌우반전
            
        // 트리거 재생
        PlayerController.instance.playerAnim.SetTrigger("dashOn");
        
        AudioManager.instance.PlayerSfxCreate(5,true);    // 사운드 생성
        
        // 기존 슬레쉬 이펙트 따라오지 않도록 하기.
        PlayerAttack.instance.currentSlashEffect = null;
    }

    private void FixedUpdate()
    {
        // 대쉬 이동(인풋에서 상택값이 변경되고, 애니메이션까지 변경이 되었으면)
        if (isDash && PlayerController.instance.playerAnimStateInfo.IsName("Dash") && !PlayerController.instance.isJumpMacro)
        {
            PlayerController.instance.playerAnim.SetBool("run", false); // 상태값 제거
            dashDelayTimeCount = 0f;                                              // 대쉬 딜레이 카운트 초기화
            
            // CAP2D 사이즈 및 오프셋 변경
            PlayerController.instance.cap2D.size   = new Vector2(PlayerController.instance.cap2D.size.x,   PlayerController.instance.originColliderSizeY   - inDashColliderY);      // Y크기 감소
            PlayerController.instance.cap2D.offset = new Vector2(PlayerController.instance.cap2D.offset.x, PlayerController.instance.originColliderOffsetY - inDashColliderY / 2f); // Y오프셋 변경
            
            // 애니메이션 진행량 (0 -> 1)
            float progress     = PlayerController.instance.playerAnimStateInfo.normalizedTime;
            // 감속값 (1 -> 0)
            // 0에 가까워 질 수록, dashSpeed * productValue으로 대쉬 속도가 작아짐.
            decelerationValue = 1f - progress;
            
            // 경사
            if (PlayerController.instance.isSlope && PlayerFloorCollider.instance.isGrounded)
                PlayerController.instance.rb2D.velocity = PlayerController.instance.perpendicular * (dashSpeed * decelerationValue) * PlayerController.instance.bodyGameObject.transform.localScale.x * -1;
            // 평지(+ 공격하다가 공중으로 간 경우 -> y값은 -1값 적용 = 대쉬와 동일)
            else if (!PlayerController.instance.isSlope && PlayerFloorCollider.instance.isGrounded)
                PlayerController.instance.rb2D.velocity = new Vector2(PlayerController.instance.bodyGameObject.transform.localScale.x * (dashSpeed * decelerationValue), -1);
            // 공중
            else if(!PlayerFloorCollider.instance.isGrounded)
            {
                PlayerController.instance.rb2D.velocity = new Vector2(PlayerController.instance.bodyGameObject.transform.localScale.x * (dashSpeed * decelerationValue), 0f);
            }
        }
        else
        {
            // CAP2D 사이즈 및 오프셋 복구
            PlayerController.instance.cap2D.size   = new Vector2(PlayerController.instance.cap2D.size.x,   PlayerController.instance.originColliderSizeY);     
            PlayerController.instance.cap2D.offset = new Vector2(PlayerController.instance.cap2D.offset.x, PlayerController.instance.originColliderOffsetY);
        }
    }
}
