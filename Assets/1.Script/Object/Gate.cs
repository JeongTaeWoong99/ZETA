using System.Collections.Generic;
using Calcatz.MeshPathfinding;
using UnityEngine;

public class Gate : MonoBehaviour
{
    [Header("------Common------")]
    public  BoxCollider2D     box2D;
    public  Animator          anim;
    [HideInInspector]
    public  AnimatorStateInfo gateAnimStateInfo;
    [HideInInspector]
    public  bool              openState;
    
    [Header("------EnemyControlGate------")]
    public  bool              isEnemyControlGate;                             
    public  List<EnemyHp>     enemyInControlList = new List<EnemyHp>();  // 죽여야 열리는 적들

    [Header("------Event------")]
    public bool isEventControl;                 // 이벤트를 통해서, 제어를 하는 문
    [HideInInspector] 
    public bool isElevatorControl;              // 엘리베이터에 의해서, 컨트롤 중 인지
    
    public Node gateNode;                       // 열렸을 때    : 노드의 traversable = true
                                                // 닫혀 있을 때 : 노드의 traversable = false

    private void Start()
    {
        // 문을 관리하는 적이 있는 경우, 빨간색컬러
        if (isEventControl)
            anim.SetTrigger("baseCloseOn");
    }
    
    private void FixedUpdate()
    {
        // 애니메이션 상태 체크
        gateAnimStateInfo = anim.GetCurrentAnimatorStateInfo(0);
    
        // 감속상태 변화
        if (!PlayerAcceleration.instance.isAcceleration)
            anim.speed = 1;
        else
            anim.speed = PlayerAcceleration.instance.accelerationChangedTimeValue;

        // 제어 노드의 상태 전환
        if (gateNode)
        {
            if (gateAnimStateInfo.IsName("Open"))
                gateNode.traversable = true;
            else if (gateAnimStateInfo.IsName("Close") || gateAnimStateInfo.IsName("Base"))
                gateNode.traversable = false;
        }
        
        // 관리하는 적이 살아있지 않으면, 리스트에서 빼기
        for (var index = 0; index < enemyInControlList.Count; index++)
        {
            var enemyInControlLists = enemyInControlList[index];
            if (!enemyInControlLists.liveState || enemyInControlLists == null)
                enemyInControlList.Remove(enemyInControlLists);
        }
        
        // 관리하는 적이 다 죽으면, 문 컬러 변경
        if (enemyInControlList.Count == 0 && isEnemyControlGate)
        {
            isEnemyControlGate = false;
            anim.SetTrigger("eventOpenOn");
            //AudioManager.instance.ObjectSfxCreate(6,true,gameObject); // open 사운드 생성
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 기본 문
        if (other.CompareTag("Player") && !openState && !isElevatorControl && !isEventControl && enemyInControlList.Count == 0 && !PlayerAcceleration.instance.isAcceleration)
        {
            box2D.enabled = false;
            openState     = true;
            anim.SetTrigger("baseToOpenOn");
            //AudioManager.instance.ObjectSfxCreate(6,true,gameObject); // open 사운드 생성
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        // 기본 문
        if (other.CompareTag("Player") && !openState && !isElevatorControl && !isEventControl && enemyInControlList.Count == 0 && !PlayerAcceleration.instance.isAcceleration)
        {
            box2D.enabled = false;
            openState     = true;
            anim.SetTrigger("baseToOpenOn");
            //AudioManager.instance.ObjectSfxCreate(6,true,gameObject); // open 사운드 생성
        }
    }

    public void GateOpenSound()
    {
        AudioManager.instance.ObjectSfxCreate(6,true,gameObject); // close 사운드 생성
    }
    
    public void GateCloseSound()
    {
        AudioManager.instance.ObjectSfxCreate(5,true,gameObject); // open 사운드 생성
    }
}