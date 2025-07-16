using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverSceneUIHandler : IUIEventHandler
{
    public bool Handle(UIEventType type, object payload)
    {
        switch (type)
        {
            case UIEventType.OpenPause:
                UIRoot.Instance.pausePanel?.SetActive(true);
                GameManager.Instance.PauseGame();

                var pause = UIRoot.Instance.pausePanel?.GetComponent<PausePanel>();
                pause?.PlaySequentialIntro(); //칼 등장 애니메이션 호출
                break;
            case UIEventType.ClosePause:
                UIRoot.Instance.pausePanel?.SetActive(false);
                GameManager.Instance.ContinueGame();
                break;
            case UIEventType.OpenOption:
                UIRoot.Instance.pausePanel.SetActive(false);
                UIRoot.Instance.optionPanel.SetActive(true);
                UIRoot.Instance.volumePanel.gameObject.SetActive(true);
                break;
            case UIEventType.CloseOption:
                UIRoot.Instance.optionPanel.SetActive(false);
                if (SceneManager.GetActiveScene().name != "StartScene")
                    UIRoot.Instance.pausePanel.SetActive(true);
                break;
            case UIEventType.ShowSoundTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(true);
                UIRoot.Instance.videoPanel.gameObject.SetActive(false);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(false);
                break;
            case UIEventType.ShowVideoTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(false);
                UIRoot.Instance.videoPanel.gameObject.SetActive(true);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(false);
                break;
            case UIEventType.ShowControlTab:
                UIRoot.Instance.volumePanel.gameObject.SetActive(false);
                UIRoot.Instance.videoPanel.gameObject.SetActive(false);
                UIRoot.Instance.keysettingPanel.gameObject.SetActive(true);
                break;
            case UIEventType.LoadMainScene:
                EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(alpha: 1f, duration: 1f, scene: "MainScene"));
                break;
            case UIEventType.QuitGame:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
        return false;
    }
}
