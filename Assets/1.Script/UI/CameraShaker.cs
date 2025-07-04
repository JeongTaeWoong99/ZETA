using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker instance; 
    
    [HideInInspector]
    public Camera mainCam; 

    [HideInInspector] 
    public bool isShack;

    private void Awake()
    {
        instance = this;
        mainCam  = GetComponent<Camera>();
    }

    public void Shake(float duration, float intensity, AnimationCurve intensityCurve)
    {
        if (instance == null)
        {
            Debug.LogWarning("CameraShaker script not found in the scene.");
            return;
        }

        StartCoroutine(ShakeCamera(duration, intensity, intensityCurve));
    }

    IEnumerator ShakeCamera(float duration, float intensity, AnimationCurve intensityCurve)
    {
        isShack = true;
        float elapsed = 0f;

        Vector3 originalCameraPosition = mainCam.transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float percentComplete = elapsed / duration;

            float damper = 1f - Mathf.Clamp(4f * percentComplete - 3f, 0f, 1f);

            float currentIntensity = intensity * intensityCurve.Evaluate(percentComplete);

            float x = Random.Range(-1f, 1f) * currentIntensity * damper;
            float y = Random.Range(-1f, 1f) * currentIntensity * damper;
            float z = Random.Range(-1f, 1f) * currentIntensity * damper;

            mainCam.transform.localPosition = originalCameraPosition + new Vector3(x, y, z);

            yield return null;
        }
        
        isShack = false;
    }
}