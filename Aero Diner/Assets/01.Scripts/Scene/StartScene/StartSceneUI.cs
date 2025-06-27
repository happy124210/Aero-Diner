
using UnityEngine;


public class StartSceneUI : MonoBehaviour
{
    [SerializeField] GameObject startWarningPanel;
    public void OnClickStartGame()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("MainScene");
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료 요청됨");

#if UNITY_EDITOR
        // 에디터에서 실행 중일 경우, 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}
