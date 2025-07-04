using UnityEngine;
using Random = UnityEngine.Random;

public class BoomBody : MonoBehaviour
{
    private  Rigidbody2D      rb2D;		
    private  SpriteRenderer   theSR;        // 투명도 참조 
    private  CircleCollider2D circle2D;

    public  bool  isDestoryBody;  // 바로 삭제 바디
    private bool  isBodyBoom;
    private bool  isGround;
    public  float moveSpeed;

    private float randomX;
    public  float decelerationX;
    public  float perFixedUpdateDecelerationX;
    
    private float randomY;
    public  float decelerationY;
    public  float perFixedUpdateDecelerationY;

    private int   fadePropertyID_1; // 글로우 페이드 이름
    private float fadeValue_1;      // 페이드값
    
    private int   fadePropertyID_2; // 일반 페이드 이름
    private float fadeValue_2;      // 페이드값
    
    public  bool  isNotGlowFade;  // 글로우 페이드가 아닌 일반 페이드
    
    public  float lifetime;	      // 잔여 시간   
    public  float fadeSpeed;      // 사라지는 시간
    
    
    [HideInInspector]
    public  bool       isSlope;            // 평지판단
    public  float      slopeCheckDistance; // 표시해줄 선 거리
    private float      angle;
    private Vector2    perepndi;
    public  LayerMask  floorLayer;
    public  LayerMask  wallLayer;

    public  bool       isRotation = true;
    private float      randomRotation;
    
    private bool       slopeCheckActive;   // down에 닿으면, 그 때 슬로프 체크 작동

    private void Start()
    {
        rb2D     = GetComponent<Rigidbody2D>();
        theSR    = GetComponent<SpriteRenderer>();
        circle2D = GetComponent<CircleCollider2D>();
        
        fadePropertyID_1               = Shader.PropertyToID("_FullGlowDissolveFade");
        fadeValue_1                    = 1;
        
        fadePropertyID_2               = Shader.PropertyToID("_FullAlphaDissolveFade");
        fadeValue_2                    = 1;
    }

    private void Update()
    {
        if (isBodyBoom)
        {
            FadeBody();
        }
    }

    private void FixedUpdate()
    {
        if (isBodyBoom)
        {
            Slop();

            BoomBodyMove();
        }
    }
    
    public void BodyBoom()
    {
        // 폭파 트리거 작동
        if (!isDestoryBody)
        {
             isBodyBoom             = true;
             rb2D.bodyType          = RigidbodyType2D.Dynamic;        // 바디타입 변경
             circle2D.enabled       = true;                           // 충돌활성화
             theSR.sortingLayerName = "Object";                       // 정렬레이어 변경
            
             // 방향
             randomX        = Random.Range(1f,  -1f);                 // 좌우
             randomY        = Random.Range(1f, 0.5f);                 // 무조건 일단 위로
             randomRotation = Random.Range(1f, -1f);                  //
        }
        // 바로삭제
        else
        {
            Destroy(gameObject);
        }
    }

    private void BoomBodyMove()
    {
        // 바디폭파 및 파편 땅에 안 닿음
        // X이동값은 1과 -1에서 +값소값과 -감소값까지
        if (randomX >= decelerationX)
        {
            randomX -= Time.fixedDeltaTime * perFixedUpdateDecelerationX * PlayerAcceleration.instance.accelerationChangedTimeValue;
        }
        else if(randomX <= -decelerationX)
        {
            randomX += Time.fixedDeltaTime * perFixedUpdateDecelerationX * PlayerAcceleration.instance.accelerationChangedTimeValue;
        }
        
        // Y이동값은, 최대 1에서 -1까지 
        if (randomY >= decelerationY)
        {
            randomY -= Time.fixedDeltaTime * perFixedUpdateDecelerationY * PlayerAcceleration.instance.accelerationChangedTimeValue;
        }
        
        // 공중 이동
        if (!isSlope && !isGround)
        {
            // 이동
            rb2D.MovePosition(rb2D.position + new Vector2(randomX * moveSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue, 
                                                          randomY * moveSpeed * PlayerAcceleration.instance.accelerationChangedTimeValue) * Time.fixedDeltaTime);
        }
        // 바닥 슬로프 이동
        else if(isSlope && isGround)
        {
            if(perepndi.y < 0)
                rb2D.velocity    = perepndi * PlayerAcceleration.instance.accelerationChangedTimeValue;
            else
                rb2D.velocity    = perepndi * PlayerAcceleration.instance.accelerationChangedTimeValue * -1f;
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        // 바닥 평지 이동 잠금
        else if (!isSlope && isGround)
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation; 
        }
        
        //회전
        if (isRotation)
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            currentRotation.z      += randomRotation * (moveSpeed * 50f) * PlayerAcceleration.instance.accelerationChangedTimeValue * Time.fixedDeltaTime;
            transform.rotation      = Quaternion.Euler(currentRotation);
        }
    }
    private void Slop()
    {
        if (slopeCheckActive)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, slopeCheckDistance, floorLayer); // 플레이어 몸 기준으로 아래로 레이를 쏴서, 닿은 플로어 반사각 판단

            if (hit)
            {
                perepndi   = Vector2.Perpendicular(hit.normal).normalized; // 경사판단
                angle      = Vector2.Angle(hit.normal, Vector2.up);        // 경사 angle판단            
                isGround   = true;
                isRotation = false;
                
                if (angle != 0) // 언덕 판단
                    isSlope = true;
                else
                    isSlope = false;
            }
        }
    }
    
    private void FadeBody()
    {
        // 바디폭파 및 파편 땋에 닿음
        if (isGround)
        {
            // 조각 사라짐
            lifetime -= Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
            
            if (lifetime <= 0)
            {
                if (isNotGlowFade)
                {
                    fadeValue_2          -= fadeSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
                    theSR.material.SetFloat(fadePropertyID_2, fadeValue_2);                                                              // 개인 메터리얼 값변환
                    
                    if (fadeValue_2 <= 0)
                        Destroy(gameObject);
                }
                else
                {
                    fadeValue_1          -= fadeSpeed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;
                    theSR.material.SetFloat(fadePropertyID_1, fadeValue_1);                                                              // 개인 메터리얼 값변환
                    
                    if (fadeValue_1 <= 0)
                        Destroy(gameObject);
                }
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb2D.constraints  = RigidbodyConstraints2D.FreezeRotation;   // 강제잠금
        
        RaycastHit2D up   = Physics2D.Raycast(transform.position, Vector2.up,    slopeCheckDistance, wallLayer);    
                  
        RaycastHit2D down = Physics2D.Raycast(transform.position, Vector2.down,  slopeCheckDistance, floorLayer);

        if (up)
        {
            randomY          *= -1;
            isRotation        = false;
            rb2D.constraints  = RigidbodyConstraints2D.FreezeRotation;
        }
            
        if (down)
        {
            slopeCheckActive  = true;
            rb2D.gravityScale = 5f;
        }

        if ((collision.gameObject.CompareTag("Platform") || collision.gameObject.CompareTag("MovingPlatform") || collision.gameObject.CompareTag("Gate")))
        {
            // 부딪칠 때, 오른쪽벽 왼쪽벽과 닿았을 시, 이동방향 변경
            RaycastHit2D right = Physics2D.Raycast(transform.position, Vector2.right, slopeCheckDistance, wallLayer);
            RaycastHit2D left  = Physics2D.Raycast(transform.position, Vector2.left,  slopeCheckDistance, wallLayer);      
            
            if (right || left)
            {
                randomX         *= -1;
                isRotation       = false;
                rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
}