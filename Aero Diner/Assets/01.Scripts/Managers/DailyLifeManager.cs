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
        await Task.Delay(1600);
        EventBus.RaiseFadeEvent(FadeEventType.FadeIn, new FadeEventPayload(0f, 1f));
        StartCoroutine(ResendEarningsAfterDelay());
        
        GameManager.Instance.ChangePhase(GamePhase.Day);
        StationManager.Instance.InitializeStations();
    }

    private IEnumerator ResendEarningsAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
    }

}
