using System.Collections.Generic;
using UnityEngine;

public class OneOffUI : MonoBehaviour
{
    [Header("------Hacking------")]
    public List<SpriteRenderer> arrowSpriteRendererList = new List<SpriteRenderer>(); // 눌러야 할 때, 메터리얼 밝기 조절

    // 전투 해킹 타겟의 End 애니메이션 마지막에 실행
    public void DestroyPlayEnd()
    {
        Destroy(gameObject);
    }
}
