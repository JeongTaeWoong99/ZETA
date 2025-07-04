using System.Collections;
using TMPro;
using UnityEngine;

public class ExplanationOnEnable : MonoBehaviour
{
    public TextMeshProUGUI explanationText;     // 텍스트
    public string          explanationString;

    private void OnEnable()
    {
        StartCoroutine(ExplanationString());
    }
    
    private IEnumerator ExplanationString()
    {
        for (int j = 0; j < explanationString.Length + 1; j++)
        {
            explanationText.text = explanationString.Substring(0, j);
            yield return new  WaitForSecondsRealtime(0.05f);
        }
    }
}
