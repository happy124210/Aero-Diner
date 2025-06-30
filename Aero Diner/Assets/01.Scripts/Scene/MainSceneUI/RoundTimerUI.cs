using UnityEngine;
using UnityEngine.UI;

public class RoundTimerUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;

    private void Update()
    {
        if (!RestaurantGameManager.Instance || !timerImage) return;

        float time = RestaurantGameManager.Instance.CurrentGameTime;
        float limit = RestaurantGameManager.Instance.GameTimeLimit;

        float percent = Mathf.Clamp01(time / limit);
        timerImage.fillAmount = 1f - percent;
    }
}
