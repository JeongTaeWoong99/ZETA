using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerWallSensor : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HangPlatform"))
        {
            PlayerController.instance.hangWallSensorCount++;
            
            // 코너를 오르는 중, 잡히지 않도록 하기.
            if(PlayerController.instance.hangWallSensorCount == 2 && !PlayerController.instance.playerAnimStateInfo.IsName("CornerClimb") && !other.GetComponent<TilemapCollider2D>())
                PlayerController.instance.gameObject.transform.parent = other.gameObject.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("HangPlatform"))
        {
            PlayerController.instance.hangWallSensorCount--;

            if (PlayerController.instance.hangWallSensorCount != 2)
            {
                Transform objectTransform = PlayerController.instance.gameObject.transform;         // 부모에서 벗어나기
                objectTransform.SetParent(null);                                                  // 부모에서 벗어나기
            }
        }
    }
}