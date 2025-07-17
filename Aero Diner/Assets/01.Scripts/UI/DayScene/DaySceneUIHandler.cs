using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaySceneUIHandler : IUIEventHandler
{
    private readonly List<GameObject> sceneUIs;

    public DaySceneUIHandler(List<GameObject> sceneUIs)
    {
        this.sceneUIs = sceneUIs;
    }
    public bool Handle(UIEventType type, object payload)
    {
        switch (type)
        {
            case UIEventType.ShowMenuPanel:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<MenuPanel>(true)?.gameObject.SetActive(true);

                //EventBus.OnBGMRequested?.Invoke(BGMEventType.PlayRecipeChoice);
                return true;

            case UIEventType.UpdateMenuPanel:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<MenuPanel>(true)?.GenerateFoodList();
                return true;

            case UIEventType.HideMenuPanel:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<MenuPanel>(true)?.gameObject.SetActive(false);
                return true;

        }
        return false;
    }
}
