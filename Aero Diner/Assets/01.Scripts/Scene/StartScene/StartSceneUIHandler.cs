using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneUIHandler : IUIEventHandler
{
    private readonly List<GameObject> sceneUIs;
    public StartSceneUIHandler(List<GameObject> sceneUIs)
    {
        this.sceneUIs = sceneUIs;
    }
    public bool Handle(UIEventType type, object payload)
    {
        switch (type)
        {
            case UIEventType.ShowStartMenuWithSave:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<MenuPanel4>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.ShowStartMenuNoSave:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<MenuPanel3>(true)?.gameObject.SetActive(true);
                break;
            case UIEventType.OnClickNewGame:
                SaveLoadManager.DeleteSave(); // 모든 저장 삭제

                // 씬 전환
                EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(alpha: 1f, duration: 1f, scene: "MainScene"));
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
