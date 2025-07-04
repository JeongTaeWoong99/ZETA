using UnityEngine;
using UnityEngine.Rendering;

public class SettingManager : MonoBehaviour
{
    public static SettingManager instance;
    
    public  Volume          volume;
    [HideInInspector] 
    public LimitlessGlitch6 glitch6;   // 히트 효과
    [HideInInspector] 
    public LimitlessGlitch8 glitch8;   // 나레이션 오류 효과

    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        volume.profile.TryGet(out glitch6);
        volume.profile.TryGet(out glitch8);
    }
}