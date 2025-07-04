using System;
using UnityEngine;

public class CustomMaterialObject : MonoBehaviour
{
    [HideInInspector]
    public SpriteRenderer spriteRenderer;
    
    // Start보다 먼저, Awake()에서 실행.
    private void Awake()
    {
        // 독립적인 메터리얼 새로 할당
        spriteRenderer          = GetComponent<SpriteRenderer>();
        spriteRenderer.material = new Material(spriteRenderer.material);
    }
    
}
