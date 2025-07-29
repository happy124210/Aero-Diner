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
            case UIEventType.tu1:
                foreach (var ui in sceneUIs)
                {
                    Debug.Log("TU 호출됨");

                    ui?.GetComponentInChildren<Tu1>(true)?.gameObject.SetActive(true);
                }
                return true;
            case UIEventType.tu2:
                foreach (var ui in sceneUIs)
                {
                    Debug.Log("TU 호출됨");
                    ui?.GetComponentInChildren<Tu1>(true)?.gameObject.SetActive(false);
                    ui?.GetComponentInChildren<Tu2>(true)?.gameObject.SetActive(true);
                }
                return true;
            case UIEventType.tu3:
                foreach (var ui in sceneUIs)
                {
                    Debug.Log("TU 호출됨");

                    ui?.GetComponentInChildren<Tu3>(true)?.gameObject.SetActive(true);
                }
                return true;
            case UIEventType.tu3_step2:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<Tu3>(true)?.ShowTutorialStep2();
                return true;
            case UIEventType.tu3_step3:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<Tu3>(true)?.ShowTutorialStep3();
                return true;
            case UIEventType.tu3_step4:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<Tu3>(true)?.ShowTutorialStep4();
                return true;
            case UIEventType.tu3_step5:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<Tu3>(true)?.ShowTutorialStep5();
                return true;
            case UIEventType.tu3_step6:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<Tu3>(true)?.ShowTutorialStep6();
                return true;
            case UIEventType.tu3_step7:
                foreach (var ui in sceneUIs)
                    ui?.GetComponentInChildren<Tu3>(true)?.ShowTutorialStep7();
                return true;
            case UIEventType.tu4:
                foreach (var ui in sceneUIs)
                {
                    Debug.Log("TU 호출됨");

                    ui?.GetComponentInChildren<Tu4>(true)?.gameObject.SetActive(true);
                }
                return true;
        }
        
        return false;
    }

}
