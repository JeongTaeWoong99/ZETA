using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAnimatorFunction : MonoBehaviour
{
    [Header("------Common------")] 
    public EnemyController enemyCon;
    public EnemyHp         enemyHp;
    public EnemyAttack     enemyAttack;
    public List<BoomBody>  boomBodyList = new List<BoomBody>();
    public GameObject      bodyBoomEffect;

    [Header("------Laser------")] 
    public  AlterLine alterLine;
    private int       laserRotationChangeSymbol;   // 불릿 증감값 -10 0 10 순으로 들어갈 수 있도록, -1 0 1 순으로 곱해주도록 함.

    private void Start()
    {
        laserRotationChangeSymbol = 1;
    }

    public void EnableBlade()
    {
        enemyAttack.distanceToPlayer          = Vector2.Distance(PlayerController.instance.transform.position, transform.position);  // 켜질 때에 거리체크
        enemyAttack.meleeWeapon.enemyBladeCollider.enabled = true;                                                                                             // 칼 히트 on
        enemyAttack.meleeWeapon.isAttackPlayerThisTime     = false;                                                                                            // 켜진 직후 히트한적 없음
    }
    
    public void DisableBlade()
    {
        enemyAttack.meleeWeapon.enemyBladeCollider.enabled  = false; // 칼 히트 off
    }

    // 모션락 해제
    public void AttackMotionLockFalse()
    {
        enemyAttack.attackMotionLock    = false;                       // 모션락 off
        enemyCon.hasPath                = false;                       // 멈춤
        enemyCon.pathResultGrounds      = null;                        // 길초기화
        enemyCon.pathResultFlys         = null;                        // 길초기화
    }

    public void EnableFirearmRecoil()
    {
        enemyAttack.isFirearmRecoil = true;
    }
    
    public void DisableFirearmRecoil()
    {
        enemyAttack.isFirearmRecoil = false;
        enemyCon.rb2D.velocity      = Vector2.zero;
    }
    
    public void EnableIsRotation()
    {
        enemyAttack.isRotation         = true;  // 회전 on
        enemyAttack.isRotationRecovery = false; // 복구 off
        enemyCon.isAimingSound         = true;  // 조준 사운드 on
    }
    
    public void DisableIsRotation()
    {
        enemyAttack.isRotation = false;
    }

    public void EnableRotationRecovery()
    {
        enemyAttack.isRotationRecovery = true;  // 복구 on
    }
    
    // Shoot전용
    public void MakeLaser()
    {
        enemyCon.isAimingSound = false; // 조준 사운드 off
    
        int bodyDirection = enemyCon.bodyObject.transform.localScale.x == 1 ? 1 : -1;                // 오른쪽인지 왼쪽인지에 따라 곱값 설정
        
        // 드론과 같이 총구가 2개의 경우
        int randomNum     = Random.Range(0, enemyAttack.laserBulletTrans.Count);
        
        // 슛 사운드 생성
        // 스나
        if(enemyAttack.laserBulletTrans.Count == 1)
            AudioManager.instance.EnemySfxCreate(5, true, enemyCon.gameObject);
        // 드론
        else if(enemyAttack.laserBulletTrans.Count != 1)
            AudioManager.instance.EnemySfxCreate(6, true, enemyCon.gameObject);
        
        // 레이저 불릿
        // 스나
        if (enemyAttack.laserBulletTrans.Count == 1)
        {
            if (bodyDirection == 1)
                Instantiate(enemyAttack.laserPrefabs, enemyAttack.laserBulletTrans[randomNum].position, enemyAttack.laserBulletTrans[randomNum].rotation);
            else
                Instantiate(enemyAttack.laserPrefabs, enemyAttack.laserBulletTrans[randomNum].position, enemyAttack.laserBulletTrans[randomNum].rotation * Quaternion.Euler(0f, 0f, 180f));
        }
        // 드론
        else if (enemyAttack.laserBulletTrans.Count != 1)
        {
            GameObject clone;
            if (bodyDirection == 1)
                clone = Instantiate(enemyAttack.laserPrefabs, enemyAttack.laserBulletTrans[randomNum].position, enemyAttack.laserBulletTrans[randomNum].rotation);
            else
                clone = Instantiate(enemyAttack.laserPrefabs, enemyAttack.laserBulletTrans[randomNum].position, enemyAttack.laserBulletTrans[randomNum].rotation * Quaternion.Euler(0f, 0f, 180f));
            clone.GetComponent<Projectle>().laserRotationChangeSymbol = laserRotationChangeSymbol;
            laserRotationChangeSymbol++;
            if (laserRotationChangeSymbol >= 1)
                laserRotationChangeSymbol = -1;
        }

        // 슛 이펙트
        // 3D 이펙트 angle에 따라 x값 회전
        // AlterLine의 pointCircle의 위치를 보고 슛 이펙트의 앵글을 구하고, 만든다
        Vector2 direction = new Vector2(enemyAttack.rotationMain.transform.position.x - alterLine.pointCircle.transform.position.x, 
                                        enemyAttack.rotationMain.transform.position.y - alterLine.pointCircle.transform.position.y);
                                        
        direction *= -1;                                                                                    // 방향 반전 조정
        float      angle = Mathf.Atan2(direction.normalized.y, direction.normalized.x) * Mathf.Rad2Deg;     // 회전하는 앵글값
        Instantiate(enemyAttack.laserShootEffect, enemyAttack.shootEffectTrans[randomNum].position, Quaternion.Euler(-angle , 90f, 0f));
    }
    
    public void MakeSlashEffect()
    {
        // 슬레쉬 사운드 생성
        // 가드
        if(!enemyAttack.rotationMain)
            AudioManager.instance.EnemySfxCreate(0, true, enemyCon.gameObject);
        // 스나
        else if (enemyAttack.rotationMain)
            AudioManager.instance.EnemySfxCreate(7, true, enemyCon.gameObject);
    
        // 스나이퍼 (스나이퍼는 라이트세이버 + 슛터이기 때문에, 우선순위 enemyCon.isShooter를 먼저 if문에 걸리도록 함.)
        if (enemyAttack.rotationMain)
        {
            if (enemyCon.bodyObject.transform.localScale.x == 1)
                enemyAttack.currentSlashEffect = Instantiate(enemyAttack.attackSlashEffect, enemyAttack.rotationMain.transform.position, 
                                                      Quaternion.Euler(enemyAttack.attackSlashEffect.transform.eulerAngles.x, 180f, enemyAttack.attackSlashEffect.transform.eulerAngles.z));
            else
                enemyAttack.currentSlashEffect = Instantiate(enemyAttack.attackSlashEffect, enemyAttack.rotationMain.transform.position, 
                                                      Quaternion.Euler(enemyAttack.attackSlashEffect.transform.eulerAngles.x, 0f, enemyAttack.attackSlashEffect.transform.eulerAngles.z)); 
        }
        // 가드 이펙트
        else if(enemyAttack.meleeWeapon)
        {
            if (enemyCon.bodyObject.transform.localScale.x == 1)
                enemyAttack.currentSlashEffect = Instantiate(enemyAttack.attackSlashEffect, enemyHp.bodyBoomTrans.transform.position, Quaternion.Euler(0f, 0f, 180f));
            else
                enemyAttack.currentSlashEffect = Instantiate(enemyAttack.attackSlashEffect, enemyHp.bodyBoomTrans.transform.position, Quaternion.Euler(180f, 0f, 0f));
        }
    }

    public void SetBodyBoom()
    {
        Instantiate(bodyBoomEffect,enemyHp.bodyBoomTrans.transform.position,Quaternion.identity); // 폭발이펙트
        AudioManager.instance.EnemySfxCreate(3, false, enemyCon.gameObject);        // 폭발사운드(부모 X)
        
        // 트리거 작동 및 종속관계 제거
        foreach (var boom in boomBodyList)
        {
            boom.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);    // 투명도 보이기
            Transform objectTransform = boom.gameObject.transform;                                // 부모에서 벗어나기
            objectTransform.SetParent(null);                                                    // 부모에서 벗어나기
            boom.BodyBoom();                                                                      // 날라가기
        }
        
        Destroy(enemyCon.gameObject);                                                             // 본체제거
        
        Destroy(enemyCon.spawnPointGameObject);                                                   // 스폰위치 게임오브젝트 삭제
    }

    //공격 중 잠깐 멈추기 기능
    public void AttackPause()
    {
        // 트윈클이 enemyAttack.attackPauseEffectMakeTrans를 부모로 생성하여, 적의 몸체가 이동해서, 트윈클이 따라가기
        GameObject newObj                = Instantiate(enemyAttack.attackPauseEffect, enemyAttack.attackPauseEffectMakeTrans.position, Quaternion.identity);
        newObj.transform.parent          = enemyAttack.attackPauseEffectMakeTrans;
        enemyAttack.isAttackPause        = true;
        enemyAttack.attackPauseTimeCount = enemyAttack.attackPauseTime;
    }
}
