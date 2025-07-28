using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class TutorialUIHandler : IUIEventHandler
{
    private readonly List<GameObject> sceneUIs;
    public TutorialUIHandler(List<GameObject> sceneUIs)
    {
        this.sceneUIs = sceneUIs;
    }
    public bool Handle(UIEventType type, object payload)
    {
        switch (type)
        {
            case UIEventType.Tu1:
                foreach (var ui in sceneUIs)
                {
                    Debug.Log("TU 호출됨");
                    
                    ui?.GetComponentInChildren<Tu1>(true)?.gameObject.SetActive(true);
                }
                return true;

        }
        
        return false;
    }

}
