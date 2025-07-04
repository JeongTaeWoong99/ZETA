using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserObstacle : MonoBehaviour
{
    public int damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHp>().afterHitInvincibleCount = 0; // 레이저 장애물은 무적 타임으로 통과 할 수 없도록 초기화
            
            if(other.GetComponent<PlayerHp>().liveState)
                other.GetComponent<PlayerHp>().DamagePlayer(transform,damage);
        }
    }
}
