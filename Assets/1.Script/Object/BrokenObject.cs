using System.Collections.Generic;
using UnityEngine;

public class BrokenObject : MonoBehaviour
{
    [Header("------Common------")]
    public List<BoomBody>          brokenBodyList  = new List<BoomBody>();

    [Header("------Mine------")] 
    public Mine mine;
    public void SetNormalBoom()
    {
        // 트리거 작동 및 종속관계 제거
        foreach (var boom in brokenBodyList)
        {
            boom.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);   // 투명도 보이기
            Transform objectTransform = boom.gameObject.transform;                              // 부모에서 벗어나기
            objectTransform.SetParent(null);                                                  // 부모에서 벗어나기
            boom.BodyBoom();                                                                    // 날라가기
        }
        
        Destroy(gameObject);                                                                    // 본체제거
    }

    public void SetMineBoom()
    {
        // 트리거 작동 및 종속관계 제거
        foreach (var boom in brokenBodyList)
        {
            boom.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);   // 투명도 보이기
            Transform objectTransform = boom.gameObject.transform;                              // 부모에서 벗어나기
            objectTransform.SetParent(null);                                                  // 부모에서 벗어나기
            boom.BodyBoom();                                                                    // 날라가기
        }
        
        Destroy(gameObject);                                                                    // 본체제거
        
        if(mine.redRangeGameObject != null)
            Destroy(mine.redRangeGameObject);
        
        if(mine.blueRangeGameObject != null)
            Destroy(mine.blueRangeGameObject);
        
    }
}