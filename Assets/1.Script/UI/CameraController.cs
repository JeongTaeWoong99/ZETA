using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
	public static CameraController instance;
	
	[Header("------Camera------")]
	public Transform target;  // 카메라가 따라다닐 오브젝트
	[HideInInspector] 
	public Camera    mainCam; // 카메라 컴포넌트
	[HideInInspector] 
	public float     originOrthographicSize;
	
	public float     moveSpeed;			// 기본 속도(15f)
	[HideInInspector] 
	public float     originMoveSpeed;	// 오리지널 속도
	public LayerMask blockLayer;

	public float leftAndRightBlockRayDistance;
	public float upBlockRayDistance;
	public float downBlockRayDistance;
	
	private bool upBool;
	private bool downBool;
	private bool leftBool;
	private bool rightBool;

	private RaycastHit2D upHit;
	private RaycastHit2D downHit;
	private RaycastHit2D leftHit;
	private RaycastHit2D rightHit;

	public List<RectTransform> orthographicSizePerCanvasScale = new List<RectTransform>();
	
	[Header("------Hacking And Scan------")]
	public GameObject       hackingPanel;
	public List<GameObject> moveScanLineList = new List<GameObject>();
	
	[Header("------Object Scan------")]
	public GameObject           connectionFrame;
	public List<SpriteRenderer> scanLifeList = new List<SpriteRenderer>();
	
	public GameObject myConnectionIcon;
	public GameObject itemIcon;
	public GameObject antiIcon;	
	public GameObject antiLaserObject;

	private void Awake()	
	{
		instance = this;
		
		mainCam = GetComponent<Camera>();
	}

	private void Start()
	{
		originMoveSpeed        = moveSpeed;				   // 오리지널 스피드 저장
		originOrthographicSize = mainCam.orthographicSize; // 오리지널 카메라 크기 저장
	}

	// Physics2D.Raycast는 fixedUpdate
	private void FixedUpdate()
	{
		// 물리 체크
		upHit    = Physics2D.Raycast(transform.position, Vector2.up,    upBlockRayDistance,  blockLayer);
		downHit  = Physics2D.Raycast(transform.position, Vector2.down,  downBlockRayDistance, blockLayer);
		leftHit  = Physics2D.Raycast(transform.position, Vector2.left,  leftAndRightBlockRayDistance, blockLayer);
		rightHit = Physics2D.Raycast(transform.position, Vector2.right, leftAndRightBlockRayDistance, blockLayer);

		// 카메라 orthographicSize에 맞춰서, 켄버스 사이즈 조정
		CanvasSizeToOrthographicSize();
	}

	// 부드러운 이동을 위해, 동은 update	
	private void Update()
	{
		// 기본 카메라 이동
		if (!CameraShaker.instance.isShack && !PlayerHacking.instance.isHacking && target && !PlayerScan.instance.isScanCameraMoveMode && !PlayerScan.instance.isConnection)
		{
			Vector2 targetDirection = target.position - transform.position;
			
			// 위아래
			if (upHit.collider != null && downHit.collider != null)
			{
				var absoluteYDifference = Mathf.Abs(((upHit.point.y + downHit.point.y) / 2f) - transform.position.y);
				if (absoluteYDifference > 0.1f)
					targetDirection.y = ((upHit.point.y + downHit.point.y) / 2f) - transform.position.y;
				else
					targetDirection.y = 0;
			}
			else if (upHit.collider != null && targetDirection.y > 0)
				targetDirection.y = 0;
			else if (downHit.collider != null && targetDirection.y < 0)
				targetDirection.y = 0;

			// 왼쪽오른쪽
			if (leftHit.collider != null && rightHit.collider != null)
			{
				var absoluteYDifference = Mathf.Abs(((leftHit.point.x + rightHit.point.x) / 2f) - transform.position.x);
				if(absoluteYDifference >0.1f)
					targetDirection.x = ((leftHit.point.x + rightHit.point.x) / 2f) - transform.position.x;
				else
					targetDirection.x = 0;
			}
			else if (leftHit.collider != null && targetDirection.x < 0)
				targetDirection.x = 0;
			else if (rightHit.collider != null && targetDirection.x > 0)
				targetDirection.x = 0;
			
			// 부드러운 이동(Update)
			Vector3 newPosition = transform.position = Vector3.Slerp(transform.position,
																	 new Vector3(transform.position.x + targetDirection.x, transform.position.y + targetDirection.y, -10f), 
																	 moveSpeed * Time.deltaTime);
			newPosition.z      = -10f;		  // z위치 고정
			transform.position = newPosition; 
		}
		// 스캔 중 이동
		else if (PlayerScan.instance.isScanCameraMoveMode && !PlayerScan.instance.isConnection)
		{
			Vector3 moveDirection = Vector3.zero;

			if (Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
			{
				if (upHit.collider == null)
					moveDirection += Vector3.up;
			}
			else if (!Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
			{
				if (downHit.collider == null)
					moveDirection -= Vector3.up;
			}

			if (Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
			{
				if (leftHit.collider == null)
					moveDirection -= Vector3.right;
			}
			else if (!Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
			{
				if (rightHit.collider == null)
					moveDirection += Vector3.right;
			}

			Vector3 newPosition = transform.position + moveDirection.normalized * PlayerScan.instance.scanCameraMoveSpeed * Time.deltaTime;
			float absoluteDifferenceX = Mathf.Abs(newPosition.x - PlayerController.instance.transform.position.x);
			float absoluteDifferenceY = Mathf.Abs(newPosition.y - PlayerController.instance.transform.position.y);
				
			// 위아래 좌우 둘다 만족한다면
			if (absoluteDifferenceX <= PlayerScan.instance.scanRangeConstrain && absoluteDifferenceY <= PlayerScan.instance.scanRangeConstrain)
			{
				newPosition = transform.position + moveDirection.normalized * PlayerScan.instance.scanCameraMoveSpeed * Time.deltaTime;  // 좌우이동 다 하는 경우, 노멀라이즈 값으로 다시 계산한다.
				transform.position = newPosition;
			}
			// 좌우만 이동 가능
			else if(absoluteDifferenceX <= PlayerScan.instance.scanRangeConstrain)
				transform.position = new Vector3(newPosition.x,transform.position.y,transform.position.z);
			// 위아래만 이동 가능
			else if(absoluteDifferenceY <= PlayerScan.instance.scanRangeConstrain)
				transform.position = new Vector3(transform.position.x,newPosition.y,transform.position.z);
		}
	}

	public void CanvasSizeToOrthographicSize()
	{
		// orthographicSize당 켄버스 크기 영향
		foreach (var orthographicSizePerCanvasScales in orthographicSizePerCanvasScale)
			orthographicSizePerCanvasScales.localScale = new Vector3(0.01f, 0.01f, 0.01f) * (mainCam.orthographicSize / originOrthographicSize);
	}
	
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 0f, 0.03f);

		Gizmos.DrawLine(transform.position, transform.position + new Vector3(0f,upBlockRayDistance,0f));
		
		Gizmos.DrawLine(transform.position, transform.position + new Vector3(0f,-downBlockRayDistance,0f));
		
		Gizmos.DrawLine(transform.position, transform.position + new Vector3(leftAndRightBlockRayDistance,0f,0f));
		
		Gizmos.DrawLine(transform.position, transform.position + new Vector3(-leftAndRightBlockRayDistance,0f,0f));
	}
}
