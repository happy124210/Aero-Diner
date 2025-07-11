using UnityEngine;
using System.Collections;

public class DailyLifeManager : Singleton<DailyLifeManager>
{
    private void OnEnable()
    {
        EventBus.OnBGMRequested(BGMEventType.PlayLifeTheme);
    }
    private void Start()
    {
        StartCoroutine(ResendEarningsAfterDelay());
    }
    

    private IEnumerator ResendEarningsAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        EventBus.Raise(UIEventType.UpdateEarnings, GameManager.Instance.TotalEarnings);
    }
    public void OnOpenButtonClick()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
    }
}
