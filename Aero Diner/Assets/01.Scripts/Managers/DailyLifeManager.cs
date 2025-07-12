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
        EventBus.RaiseFadeEvent(FadeEventType.FadeIn, new FadeEventPayload(0f, 1f));
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
        EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(alpha: 1f, duration: 1f, scene: "MainScene"));
    }
}
