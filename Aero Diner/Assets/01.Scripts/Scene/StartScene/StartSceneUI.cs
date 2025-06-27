using UnityEngine;

public class StartSceneUI : MonoBehaviour
{
    public void OnClickStartGame()
    {
        EventBus.Raise(UIEventType.LoadMainScene);
    }

    public void QuitGame()
    {
        EventBus.Raise(UIEventType.QuitGame);
    }
}