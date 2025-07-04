using System;
using System.Collections;
using UnityEngine;

public class PlayerFloorCollider : MonoBehaviour
{
    public static PlayerFloorCollider instance;
    
    [HideInInspector]
    public bool isGrounded;
    
    private bool isPlatformContact; 
    private bool isDownPlatformContact;
    
    private GameObject currentDownPlatform;
    
    [SerializeField] 
    private CapsuleCollider2D playerCollider;
    [SerializeField] 
    private CapsuleCollider2D playerFloorCollider;
    
    private bool onPlatBool;
    private bool onDownPlatBool;

    public  float playerVelocityCheckYTime;      // 0.04초 전의 값을 체크 및 갱신. (OnTriggerEnter2D에서 바로 0이 되버려서, 떨어지는 중에 닿은지 알 수 없기 때문이다.)
    private float playerVelocityCheckYTimeCount;
    private float playerBeforeVelocityY;         // playerVelocityCheckYTime전 벨로시티 값

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // 다운 플랫폼 내려가기
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.Space) && PlayerHp.instance.liveState && !PlayerDash.instance.isDash && !PlayerAttack.instance.isAttackState)
        {
            if (currentDownPlatform != null && currentDownPlatform.CompareTag("DownPlatform"))
                StartCoroutine(DisableCollision());
        }
        
        // 플레이어 Velocity Y값 체크 
        playerVelocityCheckYTimeCount += Time.deltaTime;
        if (playerVelocityCheckYTimeCount > playerVelocityCheckYTime) // 시간이 지날 때 마다,
        {
            playerVelocityCheckYTimeCount = 0f;
            playerBeforeVelocityY         = PlayerController.instance.rb2D.velocity.y;  // Y값 갱신
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Platform"))             
        {
            onPlatBool = true;
            
            isGrounded = true;
            PlayerController.instance.playerAnim.SetBool("isGrounded", isGrounded); // landing state
            PlayerController.instance.floorJumpState   = false;
            PlayerController.instance.currentJumpCount = 2;
            
            // 사운드
            if(playerBeforeVelocityY < -1f)
                AudioManager.instance.PlayerSfxCreate(10,true); // 사운드 생성
        }
        
        if ((col.CompareTag("DownPlatform") && (Mathf.Abs(PlayerController.instance.rb2D.velocity.y) <= 0.001f || PlayerController.instance.rb2D.velocity.y < 0f)) || col.CompareTag("MovingPlatform"))
        {
            onDownPlatBool = true;
            
            if(col.CompareTag("DownPlatform"))
                currentDownPlatform = col.gameObject;

            isGrounded = true;
            PlayerController.instance.playerAnim.SetBool("isGrounded", isGrounded); // landing state
            PlayerController.instance.floorJumpState   = false;
            PlayerController.instance.currentJumpCount = 2;
            
            //사운드
            if(playerBeforeVelocityY < -1f)
                AudioManager.instance.PlayerSfxCreate(10,true); // 사운드 생성
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Platform"))
        {
            onPlatBool = false;
            if (!onDownPlatBool)
            {
                isGrounded = false;
                PlayerController.instance.playerAnim.SetBool("isGrounded", isGrounded); // landing state
                PlayerController.instance.floorJumpState = false;
            }
        }
        
        if (col.CompareTag("DownPlatform") || col.CompareTag("MovingPlatform"))
        {
            onDownPlatBool = false;
            if (!onPlatBool)
            {
                if(col.CompareTag("DownPlatform"))
                    currentDownPlatform = null;

                isGrounded = false;
                PlayerController.instance.playerAnim.SetBool("isGrounded", isGrounded); // landing state
                PlayerController.instance.floorJumpState = false;
            }
        }
    }

    private IEnumerator DisableCollision()
    {
        EdgeCollider2D platformCollider = currentDownPlatform.GetComponent<EdgeCollider2D>();
        Physics2D.IgnoreCollision(playerCollider,      platformCollider);
        Physics2D.IgnoreCollision(playerFloorCollider, platformCollider);

        while (true)
        {
            int count = 0;

            Collider2D[] hit = Physics2D.OverlapBoxAll(
                   PlayerController.instance.transform.position + new Vector3(PlayerController.instance.cap2D.offset.x, PlayerController.instance.cap2D.offset.y, 0f),
                new Vector2(PlayerController.instance.cap2D.size.x, PlayerController.instance.cap2D.size.y), 0f);
            
            foreach (var t in hit)
            {
                if (t.CompareTag("DownPlatform"))
                    count++;
            }
            
            if (count == 0)
                break;
            yield return new WaitForSeconds(0.1f);
        }

        Physics2D.IgnoreCollision(playerCollider,      platformCollider, false);
        Physics2D.IgnoreCollision(playerFloorCollider, platformCollider, false);
    }

}