using UnityEngine;

public class DailyLifeManager : Singleton<DailyLifeManager>
{
    public void OnOpenButtonClick()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
    }
}
