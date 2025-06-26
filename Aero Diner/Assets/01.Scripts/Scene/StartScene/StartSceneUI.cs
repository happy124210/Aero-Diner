
using UnityEngine;


public class StartSceneUI : MonoBehaviour
{
    public void OnClickStartGame()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
    }
}
