using System.Collections;
using Calcatz.MeshPathfinding;
using UnityEngine;

public class EnemyFloorCollider : MonoBehaviour
{
    public  EnemyController   enemyCon;
    public  Pathfinding       pathfinding;
    
    [SerializeField] 
    private CapsuleCollider2D enemyCollider;
    [SerializeField] 
    private CapsuleCollider2D enemyFloorCollider;
    
    private GameObject       currentDownPlatform;
    
    private bool             coroutineState;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("DownPlatform"))
        {
            currentDownPlatform = col.gameObject;
        }
        // sound
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("DownPlatform"))
        {
            currentDownPlatform = null;
        }
    }
    
    // 지상 hasPath가 true일 때, 호출
    public void CompareNodes()
    {
        if (currentDownPlatform && !coroutineState)
        {
            string nodeName  = enemyCon.pathResultGrounds[enemyCon.currentNodeNum].name;
            Node[] downNodes = pathfinding.waypoints.downNodes.ToArray();
            
            foreach (Node node in downNodes)
            {
                if (node.name == nodeName)
                {
                    coroutineState = true;
                    StartCoroutine(DisableCollision());
                }
            }
        }
    }
    
    private IEnumerator DisableCollision()
    {
        EdgeCollider2D platformCollider = currentDownPlatform.GetComponent<EdgeCollider2D>();
        Physics2D.IgnoreCollision(enemyCollider,      platformCollider);
        Physics2D.IgnoreCollision(enemyFloorCollider, platformCollider);

        while (true)
        {
            int count = 0;

            Collider2D[] hit = Physics2D.OverlapBoxAll(enemyCon.transform.position + new Vector3(enemyCon.cap2D.offset.x, enemyCon.cap2D.offset.y, 0f),
                                                               new Vector2(enemyCon.cap2D.size.x, enemyCon.cap2D.size.y), 0f);
            
            for (var i = 0; i < hit.Length; ++i)
            {
                if (hit[i].CompareTag("DownPlatform"))
                    count++;
            }
            
            if (count == 0)
                break;
            yield return new WaitForSeconds(0.1f);
        }

        Physics2D.IgnoreCollision(enemyCollider,      platformCollider, false);
        Physics2D.IgnoreCollision(enemyFloorCollider, platformCollider, false);
        coroutineState = false;
    }
}
