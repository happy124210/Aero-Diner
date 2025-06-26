
using UnityEngine;


public class StartSceneUI : MonoBehaviour
{
    [SerializeField] GameObject startWarningPanel;
    public void OnClickStartGame()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
    }


}
