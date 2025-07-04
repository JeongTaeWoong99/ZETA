using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [Header("------Particle------")]
    public  List<ParticleSystem> particles                   = new List<ParticleSystem>(); // 각 파티클
    private List<float>          originCustomSimulationSpeed = new List<float>();          // 각 파티클의 내가 임의로 조정한 시뮬레이션 스피드
    
    [Header("------Trail------")]
    public  List<TrailRenderer>  trailRenderers            = new List<TrailRenderer>();    // 각 트레일
    private List<float>          originTrailRenderersTimes = new List<float>();            // 각 트레일의 오리지널 트레일 시간
    private List<float>          originTrailminVertexDistanceTimes = new List<float>();    // 각 트레일의 오리지널 originTrailRenderersTimes 시간

    [Header("------Sound------")] 
    public  bool        isAffectedTime;     // 시간 변화에 영향을 받는지 여부
    public  bool        isLoopSound;        // 루프인지 1회 재생인지
    private float       originPitch;        // 오리지널 속도
    private AudioSource soundSource;
    private float       soundLength;
    private float       soundlengthCount;

    private void Start()
    {
        // 오리지널 파티클 시간 저장
        foreach (var particles in particles)
        {
            originCustomSimulationSpeed.Add(particles.main.simulationSpeed);
        }
        
        // 오리지널 트레일 시간 저장
        foreach (var trails in trailRenderers)
        {
            originTrailRenderersTimes.Add(trails.time);
            originTrailminVertexDistanceTimes.Add(trails.minVertexDistance);
        }
        
        // 사운드의 경우
        soundSource = GetComponent<AudioSource>(); // 오디오 소스
        if (soundSource)
        {
            originPitch = soundSource.pitch;       // 클립의 속도
            soundLength = soundSource.clip.length; // 클립의 재생 길이
            soundSource.Play();                    // 재생
        }
    }
    
    private void Update()   // 헤킹이 null로 느려지기 때문에, Update가 맞음.
    {
        if (!MenuManager.instance.isNormalMenu)
        {
            // 파티클(곱 *)
            for (int i = 0; i < originCustomSimulationSpeed.Count; i++)
            {
                var mainModule             = particles[i].main;
                mainModule.simulationSpeed = originCustomSimulationSpeed[i] * PlayerAcceleration.instance.accelerationChangedTimeValue;
            }
            
            // 트레일 렌더러(나누기 /)
            for (int i = 0; i < originTrailRenderersTimes.Count; i++)
            {
                trailRenderers[i].time              = originTrailRenderersTimes[i]         / PlayerAcceleration.instance.accelerationChangedTimeValue;
                trailRenderers[i].minVertexDistance = originTrailminVertexDistanceTimes[i] / PlayerAcceleration.instance.accelerationChangedTimeValue;
            }
            
            // 사운드(곱 *)
            if (soundSource)
            {
                // 해킹 속도에 따라 피치 변경(모든 사운드 적용)
                if(PlayerHacking.instance.isHacking)
                    soundSource.pitch = originPitch * Time.timeScale;   // 타임 스케일에 따라
                // 엑셀의 영향을 받는 경우
                else if (!PlayerHacking.instance.isHacking)
                {
                    if (isAffectedTime)
                        soundSource.pitch = originPitch * PlayerAcceleration.instance.accelerationChangedTimeValue * Time.timeScale; // 엑셀에 따라, 속도 변경
                    else
                        soundSource.pitch = originPitch;                                                            // 똑같이 유지
                }

                // 루프사운드인지
                if (!isLoopSound)
                {
                    soundlengthCount += Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue;   // 재생 길이가 지나면 삭제.

                    // 재생시간이 넘어감녀 삭제
                    if (soundlengthCount > soundLength)
                        Destroy(gameObject);
                }
            }
        }
        else if(MenuManager.instance.isNormalMenu)
        {
            // 파티클(곱 *)
            // for (int i = 0; i < originCustomSimulationSpeed.Count; i++)
            // {
            //     var mainModule             = particles[i].main;
            //     mainModule.simulationSpeed = 0f;
            // }
            
            // 트레일 렌더러(나누기 /)
            // for (int i = 0; i < originTrailRenderersTimes.Count; i++)
            // {
            //     trailRenderers[i].time              = 0f;
            //     trailRenderers[i].minVertexDistance = 0f;
            // }
            
            // 사운드(곱 *)
            if (soundSource)
            {
                soundSource.pitch = 0f;
            }
        }
    }
}
