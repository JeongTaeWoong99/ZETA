using UnityEngine;
using UnityEngine.Rendering;

public class SortingLayer : MonoBehaviour
{
    public  SortingGroup sortingGroup;

    private void Update()
    {
        if (!PlayerAcceleration.instance.isAcceleration)
        {
            // 실시간 오버값 변경(적 레이어)
            sortingGroup.sortingOrder = Mathf.RoundToInt(transform.position.x * -10.0f); // 왼쪽일 수록 앞으로
        }
    }
    
}