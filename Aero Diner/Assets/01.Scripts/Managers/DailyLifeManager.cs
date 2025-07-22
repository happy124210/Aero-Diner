using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class DailyLifeManager : Singleton<DailyLifeManager>
{
    private void OnEnable()
    {
        EventBus.OnBGMRequested(BGMEventType.PlayLifeTheme);
    }
    private async void Start()
    {
        await Task.Delay(1000);
        EventBus.RaiseFadeEvent(FadeEventType.FadeIn, new FadeEventPayload(0f, 1f));
        StartCoroutine(ResendEarningsAfterDelay());
    }

    private IEnumerator ResendEarningsAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
    }
    public void OnOpenButtonClick()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.ShowMenuPanel);
        EventBus.PlayBGM(BGMEventType.PlayRecipeChoice);
    }
}
