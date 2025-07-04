using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorFunction : MonoBehaviour
{
    public static PlayerAnimatorFunction instance;
    
    [Header("------BoomBody------")]
    public GameObject     bodyBoomEffect;
    public List<BoomBody> boomBodyList = new List<BoomBody>();

    private void Start()
    {
        instance = this;
    }

    // private void FixedUpdate()
    // {
    //     // 상태 변경 트레일 전환
    //     // nomal      -> acceleration
    //     if (PlayerAcceleration.instance.isAcceleration && PlayerLightSaber.instance.nomalTrail.IsEffectEnabled() && !PlayerLightSaber.instance.accelerationTrail.IsEffectEnabled())
    //     {
    //         DisableTrailFunction();
    //         EnableTrailFunction();
    //     }
    //     // acceleration -> nomal
    //     else if (!PlayerAcceleration.instance.isAcceleration && PlayerLightSaber.instance.accelerationTrail.IsEffectEnabled() && !PlayerLightSaber.instance.nomalTrail.IsEffectEnabled())
    //     {
    //         DisableTrailFunction();
    //         EnableTrailFunction();
    //     }
    // }

    // 공격 1 2 3 발동 타이밍
    // public void EnableTrailFunction()
    // {
    //     PlayerLightSaber.instance.isLightSaberTrail = true;
    //     if(PlayerAcceleration.instance.isAcceleration)
    //         PlayerLightSaber.instance.accelerationTrail.EnableTrail();      
    //     else
    //         PlayerLightSaber.instance.nomalTrail.EnableTrail();             
    // }

    // public void DisableTrailFunction()
    // {
    //     PlayerLightSaber.instance.isLightSaberTrail = false;
    //     PlayerLightSaber.instance.accelerationTrail.DisableTrail();    // 트레일 off
    //     PlayerLightSaber.instance.nomalTrail       .DisableTrail();    // 트레일 off
    // }
    private void HitFunction()
    {
        PlayerAttack.instance.Hit();
    }
    
    public void AttackMoveStart()
    {
        PlayerAttack.instance.isAttackMove = true;
    }
    
    public void AttackMoveEnd()
    {
        PlayerAttack.instance.isAttackMove = false;
    }
    
    public void AttackStateFalse()
    {
        PlayerAttack.instance.isAttackState = false;
    }

    public void SetBodyBoom()
    {
        Instantiate(bodyBoomEffect,PlayerController.instance.transform.position,Quaternion.identity); // 폭발이펙트
        AudioManager.instance.PlayerSfxCreate(7,false);                               // 사운드 생성(제타의 위치에 생성 X -> Destroy때문에, 사운드도 같이 사라져버림)
        
        // 오디오 변경
        AudioManager.instance.playerListener.enabled = false;   // 플레이어 오디오 끄기
        AudioManager.instance.cameraListener.enabled = true;    // 카메라 오디오 켜기
        
        // 트리거 작동 및 종속관계 제거
        foreach (var boom in boomBodyList)
        {
            boom.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);    // 투명도 보이기
            Transform objectTransform = boom.gameObject.transform;                               // 부모에서 벗어나기
            objectTransform.SetParent(null);                                                   // 부모에서 벗어나기
            boom.BodyBoom();                                                                     // 날라가기
        }
        
        Destroy(PlayerController.instance.gameObject);                                           // 본체제거
    }

    public void MakeDashEffect()
    {
        float perendiY;
        if (Mathf.Abs(PlayerController.instance.perpendicular.y) > 0.01f)
            perendiY = PlayerController.instance.perpendicular.y;
        else
            perendiY = 0f;
                
        // 대쉬 이펙트
        if (PlayerController.instance.bodyGameObject.transform.localScale.x == -1) // 오른쪽
        {
            if (perendiY > 0)
                Instantiate(PlayerDash.instance.dashEffect, PlayerController.instance.transform.position,Quaternion.Euler(PlayerController.instance.angle,90f,0f));  // 내리막
            else if(perendiY < 0)
                Instantiate(PlayerDash.instance.dashEffect, PlayerController.instance.transform.position, Quaternion.Euler(-PlayerController.instance.angle,90f,0f)); // 오르막
            else
                Instantiate(PlayerDash.instance.dashEffect, PlayerController.instance.transform.position, Quaternion.Euler(0f,90f,0f));                               // 평지
        }
        else                                                                        // 왼쪽
        {
            if (perendiY < 0) 
                Instantiate(PlayerDash.instance.dashEffect, PlayerController.instance.transform.position,Quaternion.Euler(PlayerController.instance.angle,-90f,0f));  // 내리막
            else if(perendiY > 0)
                Instantiate(PlayerDash.instance.dashEffect, PlayerController.instance.transform.position, Quaternion.Euler(-PlayerController.instance.angle,-90f,0f)); // 오르막
            else
                Instantiate(PlayerDash.instance.dashEffect, PlayerController.instance.transform.position, Quaternion.Euler(0f,-90f,0f));                               // 평지
        }
    }
    
    public void MakeAttackEffect()
    {
        // 공격 이펙트
        if (PlayerController.instance.bodyGameObject.transform.localScale.x == 1)
        {
            switch (PlayerAttack.instance.attackComboNum)
            {
                // 오른쪽 공격 1
                case 1:
                    PlayerAttack.instance.currentSlashEffect = Instantiate(PlayerAttack.instance.attackSlashEffect1_2, PlayerAttack.instance.effectFollowTrans.position, Quaternion.Euler(180f, 0f, 180f));
                    break;
                // 오른쪽 공격 2
                case 2:
                    PlayerAttack.instance.currentSlashEffect = Instantiate(PlayerAttack.instance.attackSlashEffect1_2, PlayerAttack.instance.effectFollowTrans.position , Quaternion.Euler(0f, 0f, 180f));
                    break;
                // 오른쪽 공격 3
                case 3:
                    PlayerAttack.instance.currentSlashEffect = Instantiate(PlayerAttack.instance.attackSlashEffect3, PlayerAttack.instance.effectFollowTrans.position, Quaternion.Euler(0f, 180f, 15f)); 
                    break;
            }
        }
        else
        {
            switch (PlayerAttack.instance.attackComboNum)
            {
                // 왼쪽 공격 1
                case 1:
                    PlayerAttack.instance.currentSlashEffect = Instantiate(PlayerAttack.instance.attackSlashEffect1_2, PlayerAttack.instance.effectFollowTrans.position, Quaternion.Euler(0f, 0f, 0f));
                    break;
                // 왼쪽 공격 2
                case 2:
                    PlayerAttack.instance.currentSlashEffect = Instantiate(PlayerAttack.instance.attackSlashEffect1_2, PlayerAttack.instance.effectFollowTrans.position, Quaternion.Euler(180f, 0f, 0f));
                    break;
                // 왼쪽 공격 3
                case 3:
                    PlayerAttack.instance.currentSlashEffect =Instantiate(PlayerAttack.instance.attackSlashEffect3, PlayerAttack.instance.effectFollowTrans.position, Quaternion.Euler(0f, 0f, 15f));
                    break;
            }
        }
    }

    public void HitGlitchDisable()
    {
        PlayerHp.instance.isHit                      = false;
        SettingManager.instance.glitch6.enable.value = false;
        PlayerHp.instance.afterHitInvincibleCount    = PlayerHp.instance.afterHitInvincible;   // 무적시간 초기화
    }
    
    public void DashDisable()
    {
        PlayerDash.instance.isDash       = false;
    }
    
    public void EnterAheadDisable()
    {
        PlayerAttack.instance.enterAhead = false;
    }
}