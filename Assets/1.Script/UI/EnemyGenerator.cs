using System.Collections;
using System.Collections.Generic;
using Calcatz.MeshPathfinding;
using Unity.Mathematics;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    public static EnemyGenerator instance;

    [Header("------Common------")] 
    public EnemyDistanceActive enemyDistanceActive;
    public Transform           temporaryCreateTrans;

    [Header("------GroundEnemy------")] 
    public Waypoints waypoints;
    
    [Header("------Guard------")]
    public GameObject      guardPrefabs;                               // 생성드론
    public List<Transform> guardMakeTransList = new List<Transform>(); // 생성위치
    
    [Header("------Sniper------")]
    public GameObject      sniperPrefabs;                               // 생성드론
    public List<Transform> sniperMakeTransList = new List<Transform>(); // 생성위치
    
    [Header("------Drone------")]
    public GameObject      dronePrefabs;                               // 생성드론
    public List<Transform> droneMakeTransList = new List<Transform>(); // 생성위치

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator CreateGuard(int num)
    {
        // 가드 생성
        GameObject guardClone = Instantiate(guardPrefabs, temporaryCreateTrans.transform.position, quaternion.identity,
                                                  enemyDistanceActive.transform); // 안보이는 위치 생성(+enemyDistanceActive에 자식으로 생성.)
        guardClone.GetComponent<EnemyLightController>().isAppear = false;                                                                      // 안보이기 상태로 설정
        
        // 좌우
        guardClone.GetComponent<EnemyController>().bodyObject.transform.localScale = new Vector3(guardMakeTransList[num].localScale.x, 1, 1);       
        
        yield return new WaitForSecondsRealtime(0.1f);                          // 모습이 완전히 보이는 것에서, isAppear = false의 전환 시간을 기다리는 것
        guardClone.transform.position = guardMakeTransList[num].transform.position; // 원래 생성 포지션 이동
        
        AudioManager.instance.EnemySfxCreate(9,true,guardClone);
        
        // 체크 넣어주기
        enemyDistanceActive.enemyList.Add(guardClone);
        EnemyHp         enemyHp         = guardClone.GetComponent<EnemyHp>();
        EnemyController enemyController = guardClone.GetComponent<EnemyController>();
        if (enemyHp && enemyController)
        {
            enemyDistanceActive.enemyHpList.Add(enemyHp);
            enemyDistanceActive.enemyControllerList.Add(enemyController);
        }
        
        // 모습이 다 등장하면, 추적 시작
        while (true)
        {
            if (guardClone.GetComponent<EnemyLightController>().isAppear)
            {
                guardClone.GetComponent<Pathfinding>().waypoints = waypoints;                                         // 길설정
                
                if (PlayerHp.instance.liveState)
                {
                    guardClone.GetComponent<Pathfinding>().SetTarget(PlayerController.instance.gameObject.transform); // 타겟설정
                    guardClone.GetComponent<EnemyController>().bodyAnim.SetTrigger("found");
                    guardClone.GetComponent<EnemyController>().isChasePlayer = true;
                }
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    
    public IEnumerator CreateSniper(int num)
    {
        // 가드 생성
        GameObject sniperClone = Instantiate(sniperPrefabs, temporaryCreateTrans.transform.position, quaternion.identity,
                                                  enemyDistanceActive.transform); // 안보이는 위치 생성(+enemyDistanceActive에 자식으로 생성.)
        sniperClone.GetComponent<EnemyLightController>().isAppear = false;                                                                      // 안보이기 상태로 설정
        
        // 좌우
        sniperClone.GetComponent<EnemyController>().bodyObject.transform.localScale = new Vector3(sniperMakeTransList[num].localScale.x, 1, 1);                     
        
        yield return new WaitForSecondsRealtime(0.1f);                           // 모습이 완전히 보이는 것에서, isAppear = false의 전환 시간을 기다리는 것
        sniperClone.transform.position = sniperMakeTransList[num].transform.position; // 원래 생성 포지션 이동
        
        AudioManager.instance.EnemySfxCreate(9,true,sniperClone);
        
        // 체크 넣어주기
        enemyDistanceActive.enemyList.Add(sniperClone);
        EnemyHp         enemyHp         = sniperClone.GetComponent<EnemyHp>();
        EnemyController enemyController = sniperClone.GetComponent<EnemyController>();
        if (enemyHp && enemyController)
        {
            enemyDistanceActive.enemyHpList.Add(enemyHp);
            enemyDistanceActive.enemyControllerList.Add(enemyController);
        }
        
        // 모습이 다 등장하면, 추적 시작
        while (true)
        {
            if (sniperClone.GetComponent<EnemyLightController>().isAppear)
            {
                sniperClone.GetComponent<Pathfinding>().waypoints = waypoints;                                         // 길설정
                
                if (PlayerHp.instance.liveState)
                {
                    sniperClone.GetComponent<Pathfinding>().SetTarget(PlayerController.instance.gameObject.transform); // 타겟설정
                    sniperClone.GetComponent<EnemyController>().bodyAnim.SetTrigger("found");
                    sniperClone.GetComponent<EnemyController>().isChasePlayer = true;
                }
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
    
    public IEnumerator CreateDrone(int num)
    {
        // 드론 생성
        GameObject droneClone = Instantiate(dronePrefabs, temporaryCreateTrans.transform.position, quaternion.identity, enemyDistanceActive.transform); // 안보이는 위치 생성(+enemyDistanceActive에 자식으로 생성.)
        droneClone.GetComponent<EnemyLightController>().isAppear = false;                                                                // 안보이기 상태로 설정
        
        // 좌우
        droneClone.GetComponent<EnemyController>().bodyObject.transform.localScale = new Vector3(droneMakeTransList[num].localScale.x, 1, 1);                     
        
        yield return new WaitForSecondsRealtime(0.1f);  // 모습이 완전히 보이는 것에서, isAppear = false의 전환 시간을 기다리는 것
        droneClone.transform.position = droneMakeTransList[num].transform.position;                                                      // 원래 생성 포지션 이동
        
        AudioManager.instance.EnemySfxCreate(9,true,droneClone);
        
        // 체크 넣어주기
        enemyDistanceActive.enemyList.Add(droneClone);
        EnemyHp         enemyHp         = droneClone.GetComponent<EnemyHp>();
        EnemyController enemyController = droneClone.GetComponent<EnemyController>();
        if (enemyHp && enemyController)
        {
            enemyDistanceActive.enemyHpList.Add(enemyHp);
            enemyDistanceActive.enemyControllerList.Add(enemyController);
        }
        
        // 모습이 다 등장하면, 추적 시작
        while (true)
        {
            if (droneClone.GetComponent<EnemyLightController>().isAppear)
            {
                if (PlayerHp.instance.liveState)
                {
                    droneClone.GetComponent<EnemyController>().bodyAnim.SetTrigger("found");
                    droneClone.GetComponent<EnemyController>().isChasePlayer = true;
                }
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}