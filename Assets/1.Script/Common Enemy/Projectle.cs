using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Projectle : MonoBehaviour
{
    private Rigidbody2D rb2D;

    public float       speed;
    public int         damage;
    public GameObject  laserExplosionEffect; // 폭파 이펙트
    public AudioSource laserExplosionSound;  // 폭파 사운드
    
    [Header("------RandomRotationLaser------")] 
    public bool  isRandomRotationLaser;
    public float maxRotationChange;
    public int   laserRotationChangeSymbol;
    
    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // 난사형식 레이저 로테이션 조정
        if (isRandomRotationLaser)
        {
            // //int sign = Random.Range(0, 2) == 0 ? -1 : 1;
            //
            // float rotationChange = Random.Range(0f, maxRotationChange);

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + maxRotationChange * laserRotationChangeSymbol);
        }
    }
    
    private void FixedUpdate()
    {
        Vector2 moveVector = transform.right * (speed * Time.fixedDeltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
        rb2D.MovePosition(rb2D.position + moveVector);
    }
    
    //
    // private void Update()
    // {
    //     gameObject.transform.position += transform.right *
    //                                      (speed * Time.deltaTime *
    //                                       PlayerAcceleration.instance.accelerationChangedTimeValue);
    // }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<PlayerHp>().liveState && !other.GetComponent<PlayerDash>().isDash)
        {
            other.GetComponent<PlayerHp>().DamagePlayer(transform,damage);              // 데미지
            
            Instantiate(laserExplosionEffect, transform.position, Quaternion.identity); // 폭파 이펙트
            
            Instantiate(laserExplosionSound, transform.position, Quaternion.identity);  // 폭파 사운드
            
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Platform") || other.CompareTag("Gate"))
        {
            Instantiate(laserExplosionEffect, transform.position, Quaternion.identity); // 폭파 이펙트
            
            Instantiate(laserExplosionSound,  transform.position, Quaternion.identity);  // 폭파 사운드
            
            Destroy(gameObject);
        }
        
        // UPDATE 타임스케일 1
        // 144 -> 델타타임 0.006666
        // 60  -> 델타타임 0.0161616
        // 50  -> 델타타임 0.02
        
        // UPDATE 타임스케일 2
        // 144 -> 델타타임 0.0121212
        // 60  -> 델타타임 0.0323232
        // 50  -> 델타타임 0.04
        
        // FixedUpdate에서는
        // 프레임이 바뀌어도, 텔타타임이 고정이다.
        // 그래서, 배속을 했을 때, 임의로 델타타임을 바꿔주지 않으면, 2배속으로 물리가 적용되지 않는다.
        // Time.timeScale      = changedTimeScale;
        // Time.fixedDeltaTime = 0.02F * Time.timeScale;
        
        // GameObject.postition += time.deltaTime * 1
        // GameObject.postition += (time.deltaTime * 2) * 1
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        // 대쉬 중 이면 폭파 안 하도록 함.
        if (other.CompareTag("Player") && other.GetComponent<PlayerHp>().liveState && !other.GetComponent<PlayerDash>().isDash)
        {
            other.GetComponent<PlayerHp>().DamagePlayer(transform,damage);              // 데미지
            
            Instantiate(laserExplosionEffect, transform.position, Quaternion.identity); // 폭파 이펙트
            
            Instantiate(laserExplosionSound, transform.position, Quaternion.identity);  // 폭파 사운드
            
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Platform") || other.CompareTag("Gate"))
        {
            Instantiate(laserExplosionEffect, transform.position, Quaternion.identity); // 폭파 이펙트
            
            Instantiate(laserExplosionSound,  transform.position, Quaternion.identity);  // 폭파 사운드
            
            Destroy(gameObject);
        }
    }
}