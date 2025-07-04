using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    public static BossUI instance;

    public Slider bossHealthSlider;               // UI의 Slider    참조

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (BossHP.instance.isLive && !EventController.instance.eventState)
        {
            if ((PlayerAcceleration.instance.isAcceleration || PlayerHacking.instance.isHacking) && bossHealthSlider.gameObject.activeInHierarchy)
                bossHealthSlider.gameObject.SetActive(false);
            else if (!PlayerAcceleration.instance.isAcceleration && !PlayerHacking.instance.isHacking && !bossHealthSlider.gameObject.activeInHierarchy)
                bossHealthSlider.gameObject.SetActive(true);
        }
        else
        {
            bossHealthSlider.gameObject.SetActive(false);
        }
    }
}
