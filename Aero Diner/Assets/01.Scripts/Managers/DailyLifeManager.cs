using UnityEngine;

public class DailyLifeManager : Singleton<DailyLifeManager>
{
    private void OnEnable()
    {
        EventBus.OnBGMRequested(BGMEventType.PlayLifeTheme);
    }


    public void OnOpenButtonClick()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
    }
}
