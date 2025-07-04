using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class InAccelerationOrderLayer : MonoBehaviour
{
    private SortingGroup   sortingGroup;
    private SpriteRenderer spriteRenderer;
    private Canvas         canvas;
    private LineRenderer   lineRenderer;
           
    private int            originalSortingLayerID;
    private int            originalSortingOrder;
    
    [SerializeField] 
    private bool isNotOperationHackingOrScan;       // 해킹 때, 자동 레이어 교체가 일어나지 않도록 하는 것(플레이어 / 일반적 / 미사일 / 마인 등등)
    [SerializeField] 
    private bool isNotOperationPlayerAcceleration; // 해킹이 완료된 특수 게이트
    private int  newSortingLayerID;
    [SerializeField] 
    private int  newSortingOrder;       // 엑셀 중, 레이어 순서 (레인건 포커스 프레임 등등 = 1000 / 플레이어 맨 앞 =2 / 레이저/이펙트 등등 중간 = 0 / 일반적 캐릭터 뒤 = -1 / 보스 제일 뒤 = -2)
    
    private void Start()
    {
        sortingGroup   = GetComponent<SortingGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        canvas         = GetComponent<Canvas>();
        lineRenderer   = GetComponent<LineRenderer>();
        
        // 스파인 / 오브젝트가 많은 녀석 / 파티클이 많은 것 통합
        if (sortingGroup)
        {
            originalSortingLayerID = sortingGroup.sortingLayerID;
            originalSortingOrder   = sortingGroup.sortingOrder;
        }
        // 랜더러 1개만 있는 것
        else if (spriteRenderer)
        {
            originalSortingLayerID = spriteRenderer.sortingLayerID;
            originalSortingOrder   = spriteRenderer.sortingOrder;
        }
        // 캔버스(일반적 캔버스)
        else if (canvas)
        {
            originalSortingLayerID = canvas.sortingLayerID;
            originalSortingOrder   = canvas.sortingOrder;
        }
        else if (lineRenderer)
        {
            originalSortingLayerID = lineRenderer.sortingLayerID;
            originalSortingOrder   = lineRenderer.sortingOrder;
        }

        // <ID값>
        // 디폴트       0
        // 오브젝트    -457987577
        // 에너미       1404182279     
        // 플레이어    -1181832937
        // 엑세레이션   1468566761
        // 해킹        1526059097     
        // 플랫폼      1420125439
        newSortingLayerID = 1468566761;
    }

    private void FixedUpdate()
    {
        LayerHighLightChange();
    }
    
    private void LayerHighLightChange()
    {
        // 해킹 or 스캔 때, 작동하지 않아야 하는 것 이고, 해킹 or 스캔이면 return;
        if (isNotOperationHackingOrScan && (PlayerHacking.instance.isHacking || PlayerScan.instance.isScan))
            return;
        
        if(isNotOperationPlayerAcceleration && PlayerAcceleration.instance.isAcceleration)
            return;
        
        // 정체실 작동 X (-> 캡슐에서 레이어 관리를 함.)
        if (!EventController.instance.isStasisChamber)
        {
            if(!PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking)
                SetSortingLayer(originalSortingLayerID, originalSortingOrder);
            else if (PlayerAcceleration.instance.isAcceleration || PlayerHacking.instance.isHacking)
                SetSortingLayer(newSortingLayerID, newSortingOrder);
        }
    }
    
    private void SetSortingLayer(int layerID, int order)
    {
        if (sortingGroup)
        {
            if (sortingGroup.sortingLayerID != layerID)
            {
                sortingGroup.sortingLayerID = layerID;
                sortingGroup.sortingOrder   = order;
            }
        }
        else if (spriteRenderer)
        {
            if (spriteRenderer.sortingLayerID != layerID)
            {
                spriteRenderer.sortingLayerID = layerID;
                spriteRenderer.sortingOrder   = order;
            }
        }
        else if (canvas)
        {
            if (canvas.sortingLayerID != layerID)
            {
                canvas.sortingLayerID = layerID;
                canvas.sortingOrder   = order;
            }
        }
        else if (lineRenderer)
        {
            if (lineRenderer.sortingLayerID != layerID)
            {
                lineRenderer.sortingLayerID = layerID;
                lineRenderer.sortingOrder   = order;
            }
        }
    }
    
    // 강조되어야 오브젝트 레이어 체인지(레이저 / 칼 이펙트 /등등)
    // private void LayerHighLightChange()
    // {
    //     // 해킹 때, 작동하지 않아야 하는 것 이고, 해킹이면 return;
    //     if(isNotOperationHacking && PlayerHacking.instance.isHacking)
    //         return;
    //     
    //     // 정체실 작동 X (-> 캡슐에서 레이어 관리를 함.)
    //     if (!EventController.instance.isStasisChamber)  
    //     {
    //         if (sortingGroup)
    //         {
    //             if ((PlayerAcceleration.instance.isAcceleration || PlayerHacking.instance.isHacking) && sortingGroup.sortingLayerID != newSortingLayerID)
    //             {
    //                 sortingGroup.sortingLayerID = newSortingLayerID;
    //                 sortingGroup.sortingOrder   = newSortingOrder;
    //             }
    //             else if(!PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking && sortingGroup.sortingLayerID != originalSortingLayerID)
    //             {
    //                 sortingGroup.sortingLayerID = originalSortingLayerID;
    //                 sortingGroup.sortingOrder   = originalSortingOrder;
    //             }
    //         }
    //         else if (spriteRenderer)
    //         {
    //             if ((PlayerAcceleration.instance.isAcceleration || PlayerHacking.instance.isHacking) && spriteRenderer.sortingLayerID != newSortingLayerID)
    //             {
    //                 spriteRenderer.sortingLayerID = newSortingLayerID;
    //                 spriteRenderer.sortingOrder   = newSortingOrder;
    //             }
    //             else if(!PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking && spriteRenderer.sortingLayerID != originalSortingLayerID)
    //             {
    //                 spriteRenderer.sortingLayerID = originalSortingLayerID;
    //                 spriteRenderer.sortingOrder   = originalSortingOrder;
    //             }
    //         }
    //         else if (canvas)
    //         {
    //             if ((PlayerAcceleration.instance.isAcceleration || PlayerHacking.instance.isHacking) && canvas.sortingLayerID != newSortingLayerID)
    //             {
    //                 canvas.sortingLayerID = newSortingLayerID;
    //                 canvas.sortingOrder   = newSortingOrder;
    //             }
    //             else if(!PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking && canvas.sortingLayerID != originalSortingLayerID)
    //             {
    //                 canvas.sortingLayerID = originalSortingLayerID;
    //                 canvas.sortingOrder   = originalSortingOrder;
    //             }
    //         }
    //     }
    // }
    
    // 해킹 강조되야 하는 타이밍에 맞춰서 실행
    public void GroupHackingLayerEnable()
    {
        // 바꾸기
        sortingGroup.sortingLayerID = newSortingLayerID;
        sortingGroup.sortingOrder   = newSortingOrder;
    }
    
    // 해킹 키입력이 끝나면 실행
    public void GroupHackingLayerDisable()
    {
        // 바꾸기(정상화)
        sortingGroup.sortingLayerID = originalSortingLayerID;
        sortingGroup.sortingOrder   = originalSortingOrder;
    }
}