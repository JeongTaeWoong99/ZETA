using System.Collections.Generic;
using UnityEngine;

public class BigGate : MonoBehaviour
{
    public List<SpriteRenderer> gateSpriteRendererList = new List<SpriteRenderer>();
    public GameObject soundTransGameObject; // 사운드 생성 위치 고정

    // 플렛폼 레이어 -> 디폴트 레이어로 전환
    public void LGateOpenLayerChange()
    {
        foreach (var gateSpriteRendererLists in gateSpriteRendererList)
        {
            gateSpriteRendererLists.sortingLayerID = -457987577;	
        }
    }
    
    // 오브젝트 레이어 -> 플렛폼 레이어로 전환
    public void RGateOpenLayerChange()
    {
        foreach (var gateSpriteRendererLists in gateSpriteRendererList)
        {
            gateSpriteRendererLists.sortingLayerID = 1420125439;	
        }
    }

    public void OpenSound()
    {
        AudioManager.instance.ObjectSfxCreate(12,true,soundTransGameObject);
    }
}
