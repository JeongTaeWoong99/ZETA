using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{  
    public static AudioManager instance;

    [HideInInspector]
    public AudioListener  playerListener; // 플레이어 리스너 (기본)
    [HideInInspector]
    public  AudioListener cameraListener; // 카메라 리스터 (플레이어 사망 교체 + 스캔 이동)

    public AudioMixer mixer;

    [Header("------Ambient------")]
    public  AudioSource[] ambientSoundList;
    private List<float>   originVolumeValueList = new List<float>();
    public  List<float>   volumeUpSpeed         = new List<float>();
    [HideInInspector] 
    public int            currentAmbientSoundNum; // 현재 재생중인 엠비언트 사운드 넘
    
    [Header("------One-Off Create Sound------")]
    public AudioSource[] playerCreateSFX; // 1회 재생하는 sfx의 배열(루프 X)
    public AudioSource[] objectCreateSFX;
    public AudioSource[] enemyCreateSFX;
    public AudioSource[] bossCreateSFX;
    
    [Header("------One-Off UI Or Directing------")]
    public AudioSource[] UiSFX;
    public AudioSource[] directingSFX;

    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        playerListener = PlayerController.instance.GetComponent<AudioListener>();
        cameraListener = CameraController.instance.GetComponent<AudioListener>();
        
        // Sound
        // 오리지널 볼륨 길이 저장 및 볼륨 초기화
        currentAmbientSoundNum = 999;
        foreach (var ambientSoundLists in ambientSoundList)
        {
            if (ambientSoundLists != null)
            {
                originVolumeValueList.Add(ambientSoundLists.volume); // 오리지널값 넣기
                ambientSoundLists.Stop();                            // 멈추기
                ambientSoundLists.volume = 0f;                       // 볼륨값 없애기
            }
            else
            {
                originVolumeValueList.Add(0f);
            }
        }
    }
    
    private void Update()
    {
        // 엔비언트 사운드 재생
        // unscaledDeltaTime으로, 해킹 때, 제어하기 위해서 Update에서 컨트롤 함.
        for (var i = 0; i < ambientSoundList.Length; i++)
        {
            HandleBGMSound(i,currentAmbientSoundNum);
        }
    }
    
    private void HandleBGMSound(int soundIndex,int currentAmbientSoundNum)
    {
        // 번호가 현재 사운드 번호와 같으면, 사운드 소리 높히기
        if (soundIndex == currentAmbientSoundNum)
        {
            // 소리 높히기
            if (!ambientSoundList[soundIndex].isPlaying)
                ambientSoundList[soundIndex].Play();
            
            if(originVolumeValueList[soundIndex] > ambientSoundList[soundIndex].volume)
                ambientSoundList[soundIndex].volume += volumeUpSpeed[soundIndex] * Time.unscaledDeltaTime;
        }
        else
        {
            // 소리 줄이기
            ambientSoundList[soundIndex].volume -= volumeUpSpeed[soundIndex] * Time.unscaledDeltaTime;
        }
    }
    
    // 1회성 재생이기 때문에, 스탑 and 플레이
    public void UISoundPlay(int playNum)
    {
        // 해당 번호 종료 및 다시 재생
        UiSFX[playNum].Stop();
        UiSFX[playNum].Play();
    }
    
    // 디렉팅은 루프를 쓰기 때문에, 스탑 and 플레이
    public void DirectingPlay(int playNum)
    {
        // 해당 번호 종료 및 다시 재생
        directingSFX[playNum].Stop();
        directingSFX[playNum].Play();
    }
    
    // 디렉팅 루프 사운드 멈추기
    public void DirectingStop(int playNum)
    {
        // 해당 번호 종료 및 다시 재생
        directingSFX[playNum].Stop();
    }
    
    public void DirectingSfxCreate(int playNum) // 1회 재생하는 SFX 생성
    {
        Instantiate(directingSFX[playNum],transform.position, Quaternion.identity);
    }
    
    public void PlayerSfxCreate(int playNum, bool isParent) // 1회 재생하는 SFX 생성
    {
        AudioSource newObj = Instantiate(playerCreateSFX[playNum], new Vector3(PlayerController.instance.transform.position.x, PlayerController.instance.transform.position.y, 0f), Quaternion.identity);
        if(isParent)    // 주인공을 부모로 할지 여부 true이면
            newObj.transform.parent = PlayerController.instance.transform;
    }
    
    public void ObjectSfxCreate(int playNum, bool isParent,GameObject bodyGameObject) // 1회 재생하는 SFX 생성
    {
        AudioSource newObj = Instantiate(objectCreateSFX[playNum], new Vector3(bodyGameObject.transform.position.x, bodyGameObject.transform.position.y, 0f), Quaternion.identity);
        if(isParent)    // 주인공을 부모로 할지 여부 true이면
            newObj.transform.parent = bodyGameObject.transform;
    }
    
    public void EnemySfxCreate(int playNum, bool isParent,GameObject bodyGameObject) // 1회 재생하는 SFX 생성
    {
        AudioSource newObj = Instantiate(enemyCreateSFX[playNum], new Vector3(bodyGameObject.transform.position.x, bodyGameObject.transform.position.y, 0f), Quaternion.identity);
        if(isParent)    // 주인공을 부모로 할지 여부 true이면
            newObj.transform.parent = bodyGameObject.transform;
    }
    
    public void WardenSfxCreate(int playNum, bool isParent,GameObject bodyGameObject) // 1회 재생하는 SFX 생성
    {
        AudioSource newObj = Instantiate(bossCreateSFX[playNum], new Vector3(bodyGameObject.transform.position.x, bodyGameObject.transform.position.y, 0f), Quaternion.identity);
        if(isParent)    // 주인공을 부모로 할지 여부 true이면
            newObj.transform.parent = bodyGameObject.transform;
    }

}
