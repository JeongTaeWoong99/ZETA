using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyDistanceActive : MonoBehaviour
{
    public static EnemyDistanceActive instance;

    public float      activeDistance;
    public GameObject spawnGatherTrans;
    
    public  List<GameObject>     enemyList          = new List<GameObject>();
    [HideInInspector]
    public List<EnemyHp>         enemyHpList         = new List<EnemyHp>();
    [HideInInspector]
    public List<EnemyController> enemyControllerList = new List<EnemyController>();
    
    public  List<GameObject>     enemyChaseList     = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 적 모두 넣어주기
        enemyList.Clear();
        foreach (Transform child in transform)
        {
            enemyList.Add(child.gameObject);
        }
        
        // 숫자에 맞춰서, 미리 넣어두기
        foreach (var enemy in enemyList)
        {
            EnemyHp         enemyHp         = enemy.GetComponent<EnemyHp>();
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyHp && enemyController)
            {
                enemyHpList.Add(enemyHp);
                enemyControllerList.Add(enemyController);
            }
        }
    }

    private void FixedUpdate()
    {
        if (PlayerHp.instance.liveState)
        {
            for (int i = enemyList.Count - 1; i >= 0; i--)
            {
                var enemy  = enemyList[i];
                var enemyController  = enemyControllerList[i];
                var enemyHp          = enemyHpList[i];

                if (!enemy)
                {
                    enemyList.RemoveAt(i);
                    enemyHpList.RemoveAt(i);
                    enemyControllerList.RemoveAt(i);
                    continue;
                }

                if (enemyHp.liveState && !enemyController.isChasePlayer)
                {
                    // 쫒기 리스트에 포함되어 있다면, 삭제
                    if (enemyChaseList.Contains(enemy))
                    {
                        enemyChaseList.Remove(enemy);
                    }
                    
                    // 활성화
                    if (Vector2.Distance(PlayerController.instance.gameObject.transform.position, enemy.transform.position) < activeDistance)
                    {
                        if (!enemy.activeInHierarchy)
                            enemy.SetActive(true);
                    }
                    // 비활성화
                    else
                    {
                        if (enemy.activeInHierarchy && enemyController.isSpawnLocationArrive)
                        {
                            enemy.SetActive(false);
                        }
                    }
                }
                else if(enemyHp.liveState && enemyController.isChasePlayer)
                {
                    // 쫒기 리스트에 포함되어 있지 않다면, 넣기
                    if (!enemyChaseList.Contains(enemy))
                    {
                        enemyChaseList.Add(enemy);
                    }
                }
            }
            
            // 추적 등록 후 사망하여, 누락된 리스트 비우기
            for (int i = enemyChaseList.Count - 1; i >= 0; i--)
            {
                if (!enemyChaseList[i])
                {
                    enemyChaseList.RemoveAt(i);
                }
            }
        }
    }

}