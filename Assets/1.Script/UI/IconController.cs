using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class IconController : MonoBehaviour
{
    [Header("------Common------")] 
    private SpriteRenderer spriteRenderer;
    private Color          originColor;

    [Header("------MyIcon------")] 
    public  bool  isMyIcon;
    
    public  float invincibilityTime;      // 히트 후 무적시간
    private float invincibilityTimeCount; 
    
    [Header("------AntiIcon------")] 
    public  bool isAntiIcon;
    
    private Vector3 moveDirection;
    
    private float xMoveValue;
    private float yMoveValue;
    
    public float antiMoveSpeed;

    public float laserCreateTime;
    [HideInInspector]
    public float laserCreateTimeCount;

    private List<GameObject> laserList = new List<GameObject>();

    [Header("------AntiLaser------")]
    public bool  isAntiLaserIcon;
    public float laserMoveSpeed;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originColor    = spriteRenderer.color;
        
        // 마이 아이콘
        if (isMyIcon)
            invincibilityTimeCount = 0f;

        // 안티 바이러스 (랜덤방향 이동)
        if (isAntiIcon)
        {
            xMoveValue = Random.Range(-1f,1f);
            yMoveValue = Random.Range(-1f,1f);
            
            moveDirection = new Vector3(xMoveValue, yMoveValue, 0f);
        }
        
        // 레이저(쏨플레이어를 향해)
        if (isAntiLaserIcon)
        {
            xMoveValue = CameraController.instance.myConnectionIcon.transform.position.x - transform.position.x;
            yMoveValue = CameraController.instance.myConnectionIcon.transform.position.y - transform.position.y;
            
            moveDirection = new Vector3(xMoveValue, yMoveValue, 0f);
        }
    }

    private void Update()
    {
        // 무적시간 카운트
        if (isMyIcon)
        {
            invincibilityTimeCount -= Time.deltaTime;
            if (invincibilityTimeCount > 0f) // 무적시간이 남아 있으면, 아이콘 희리게 보이기.
            {
                spriteRenderer.color = new Color(0f, 1f, 0f, 0.5f);
                SettingManager.instance.glitch8.enable.value = true;
            }
            else
            {
                spriteRenderer.color = originColor;
                SettingManager.instance.glitch8.enable.value = false;
            }
        }

        // 안티 아이콘 이동 + 마이아이콘 바라보기
        if (isAntiIcon)
        {
            // 레이저 생성
            laserCreateTimeCount += Time.deltaTime;
            if (laserCreateTimeCount > laserCreateTime)
            {
                AudioManager.instance.DirectingPlay(12);            // 바이러스 슛 사운드
            
                laserList.Add(Instantiate(CameraController.instance.antiLaserObject, transform.position, Quaternion.identity));
                laserCreateTimeCount = 0f;
            }
            
            //이동
            Vector3 newPosition       = transform.position + moveDirection.normalized * (antiMoveSpeed * Time.deltaTime);
            float absoluteDifferenceX = Mathf.Abs(newPosition.x - CameraController.instance.connectionFrame.transform.position.x);
            float absoluteDifferenceY = Mathf.Abs(newPosition.y - CameraController.instance.connectionFrame.transform.position.y);

            // 위아래 좌우 둘다 만족한다면
            if (absoluteDifferenceX < PlayerScan.instance.myIconConstrainX && absoluteDifferenceY < PlayerScan.instance.myIconConstrainY)
                transform.position = newPosition;
            // 좌우만 이동 가능
            else if (absoluteDifferenceX < PlayerScan.instance.myIconConstrainX)
            {
                moveDirection.y   *= -1;
                newPosition        = transform.position + moveDirection.normalized * (antiMoveSpeed * Time.deltaTime);
                transform.position = newPosition;
            }
            // 위아래만 이동 가능
            else if (absoluteDifferenceY < PlayerScan.instance.myIconConstrainY)
            {
                moveDirection.x   *= -1;
                newPosition        = transform.position + moveDirection.normalized * (antiMoveSpeed * Time.deltaTime);
                transform.position = newPosition;
            }
            // 둘다 만족 안함.
            else
            {   
                moveDirection.x   *= -1; //값 반전
                moveDirection.y   *= -1; //값 반전
                newPosition       = transform.position + moveDirection.normalized * (antiMoveSpeed * Time.deltaTime);
                transform.position = newPosition;
            }
            
            // 마이 아이콘 바라보기
            Vector3 direction = CameraController.instance.myConnectionIcon.transform.position - transform.position;
            float   angle     = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg;
            
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + 90f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        
        // 안티 레이저 이동
        if (isAntiLaserIcon)
        {
            Vector3 newPosition       = transform.position + moveDirection.normalized * (laserMoveSpeed * Time.deltaTime);
            float absoluteDifferenceX = Mathf.Abs(newPosition.x - CameraController.instance.connectionFrame.transform.position.x);
            float absoluteDifferenceY = Mathf.Abs(newPosition.y - CameraController.instance.connectionFrame.transform.position.y);

            // 이동(위아래 좌우 둘다 만족한다면, 이 경우에만 이동 함.)
            if (absoluteDifferenceX < PlayerScan.instance.myIconConstrainX && absoluteDifferenceY < PlayerScan.instance.myIconConstrainY)
                transform.position = newPosition;
            else if (absoluteDifferenceX < PlayerScan.instance.myIconConstrainX)
                Destroy(gameObject);
            else if (absoluteDifferenceY < PlayerScan.instance.myIconConstrainY)
                Destroy(gameObject);
            else
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 마이 아이콘
        if (isMyIcon)
        {
            if (other.CompareTag("Item"))
            {
                AudioManager.instance.DirectingPlay(11);        // 아이템 획득 사운드
                PlayerScan.instance.obtainedItemNumber += 1;
                Destroy(other.gameObject);
            }
            
            if (other.CompareTag("Anti") && invincibilityTimeCount < 0f)  // 남은 무적시간이 없는 경우.  
            {
                AudioManager.instance.DirectingPlay(10);            // 히트 사운드
            
                PlayerScan.instance.currentConnectionLifeNum -= 1;  // 목숨 감소

                invincibilityTimeCount = invincibilityTime;         // 무적 시간 초기화
                
                // 남은 목숨 == 0
                // 스캔 실패 함수 실행
                if(PlayerScan.instance.currentConnectionLifeNum == 0)
                    PlayerScan.instance.ScanFailDamage();
                // 남은 목숨 != 0
                else
                {
                    // 전체 안보이게 하고
                    foreach (var scanLifeLists in CameraController.instance.scanLifeList)
                        scanLifeLists.gameObject.SetActive(false);

                    // 남은 목숨 만큼 보이게 하기.
                    for (int i = 0; i < PlayerScan.instance.currentConnectionLifeNum; i++)
                        CameraController.instance.scanLifeList[i].gameObject.SetActive(true);
                }
            }
        }
        
        // 레이저
        if (isAntiLaserIcon)
        {
            if (other.CompareTag("MyIcon"))
                Destroy(gameObject);
        }
    }

    // 파괴될 때 실행
    private void OnDestroy()
    {
        // 레이저 모두 파괴
        foreach (var laserLists in laserList)
            Destroy(laserLists);
    }

    private void OnDisable()
    {
        if (isMyIcon)
        {
            spriteRenderer.color   = originColor; // 다시 활성화 될 때, 색 초기화
            invincibilityTimeCount = 0f;          // 남은 무적시간 초기화
            SettingManager.instance.glitch8.enable.value = false;
        }
    }
}
