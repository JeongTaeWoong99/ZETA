using UnityEngine;

public class ObjectRotation : MonoBehaviour
{
    public float speed;
    public bool  plusOrMinus;
    
    private void Update()
    {
        if (plusOrMinus)
            transform.Rotate(0f, 0f, speed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
        else
            transform.Rotate(0f, 0f, -speed * Time.deltaTime * PlayerAcceleration.instance.accelerationChangedTimeValue);
    }
}
