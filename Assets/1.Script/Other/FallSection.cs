using System.Collections;
using UnityEngine;

public class FallSection : MonoBehaviour
{
    public int damage;
    public Transform respawnTrans;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어
        if (other.CompareTag("Player") && other.GetComponent<PlayerHp>().liveState)
        {
            // 데미지
            other.GetComponent<PlayerHp>().DamagePlayer(transform, damage);

            // 이동 불가 상태 변경
            EventController.instance.eventState = true;
            EventController.instance.AllKeyLockTrue();
            
            StartCoroutine(FallRespawnCoroutine());
            
            // 모든 쫒아오던 적 돌아가기
            EnemyChaseFalse();
        }

        // 적 바로 사망
        if (other.CompareTag("Enemy") && other.GetComponent<EnemyHp>().liveState)
        {
            other.GetComponent<EnemyHp>().hitAnimNum = 3; // 큰 히트 모션
            other.GetComponent<EnemyHp>().DamageEnemy(999, transform, 0f);
        }
    }

    private IEnumerator FallRespawnCoroutine()
    {
        yield return new WaitForFixedUpdate(); // PlayerHp.instance.liveState 바뀌는거 기다리기.

        // 오디오 변경(플레이어 -> 카메라)
        AudioManager.instance.playerListener.enabled = false;
        AudioManager.instance.cameraListener.enabled = true;

        // 살아 있으면, 현재 씬에서 지정된 위치로 리스폰
        if (PlayerHp.instance.liveState)
        {
            yield return StartCoroutine(FadeManager.instance.NextSeenFadeIn()); // 페이드 인

            PlayerController.instance.gameObject.transform.position = respawnTrans.transform.position; // 위치
            PlayerController.instance.bodyGameObject.transform.localScale = respawnTrans.transform.localScale; // 보는 방향

            yield return StartCoroutine(FadeManager.instance.NextSeenFadeOut()); // 페이드 아웃

            // 오디오 변경(카메라 -> 플레이어)
            AudioManager.instance.playerListener.enabled = true;
            AudioManager.instance.cameraListener.enabled = false;

            // 이동 가능 상태 변경
            EventController.instance.eventState = false;
            EventController.instance.AllKeyLockFalse();
        }
        // 사망시 자동으로 원래 코드가 실행됨.
        // else
        // {
        //     
        // }
    }
    
    private void EnemyChaseFalse()
    {
        foreach (var enemyLists in EnemyDistanceActive.instance.enemyList)
        {
            enemyLists.GetComponent<EnemyController>().chaseTimeCount = 0;
        }
    }

}
