using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverSceneUIHandler : IUIEventHandler
{
    public bool Handle(UIEventType type, object payload)
    {
        var pausePanel = UIRoot.Instance.pausePanel?.GetComponent<PausePanelEffecter>();
        switch (type)
        {
            case UIEventType.OpenPause:
                UIRoot.Instance.pausePanel?.SetActive(true);
                GameManager.Instance.PauseGame();
                pausePanel?.PlaySequentialIntro(); // 칼 등장 애니메이션
                break;

            case UIEventType.ClosePause:
                GameManager.Instance.ContinueGame();
                pausePanel?.HideWithPushEffect();  // 위로 밀려서 사라지는 연출
                break;
            case UIEventType.OpenOption:
                UIRoot.Instance.pausePanel.SetActive(false);
                UIRoot.Instance.optionPanel.SetActive(true);
                UIRoot.Instance.optionPanel.GetComponent<OptionPanelEffecter>()?.PlayFadeIn();
                UIRoot.Instance.volumePanel.gameObject.SetActive(true);
                break;
            case UIEventType.CloseOption:
                UIRoot.Instance.optionPanel.GetComponent<OptionPanelEffecter>()?.PlayFadeOut();
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
            case UIEventType.LoadDayScene:
                EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(1f, 1f, scene: "DayScene"));
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
