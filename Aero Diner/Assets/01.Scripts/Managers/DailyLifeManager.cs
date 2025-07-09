using UnityEngine;

public class DailyLifeManager : Singleton<DailyLifeManager>
{
    private void OnEnable()
    {
        EventBus.OnBGMRequested(BGMEventType.PlayLifeTheme);
    }


    public void OnOpenButtonClick()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
    }
}
