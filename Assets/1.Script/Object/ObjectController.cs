using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public Animator animator;
    
    private void FixedUpdate()
    {
        animator.speed = PlayerAcceleration.instance.accelerationChangedTimeValue;
    }
}
