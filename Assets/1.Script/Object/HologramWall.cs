using UnityEngine;

public class HologramWall : MonoBehaviour
{
    public Material hologramWallMat; // 자체 홀로그램
    public float    hologramLineSpeed;
    
    private void FixedUpdate()
    {
        // 서서히 바뀌는 경우, 이상하게 역으로 올라가는 문제 때문에, accelerationChangedTimeValue로 변하지 않게 함.
        // 한번에 전환
        if (PlayerAcceleration.instance.isAcceleration)
        {
            hologramWallMat.SetFloat("_HologramLineSpeed",hologramLineSpeed * PlayerAcceleration.instance.accelerationStatScale);
        }
        else
        {
            hologramWallMat.SetFloat("_HologramLineSpeed",hologramLineSpeed);
        }
    }
}
