using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneUI : MonoBehaviour
{
    public void OnClickPause()
    {
        EventBus.Raise(UIEventType.OpenPause);
    }
    public void OnClickInventory()
    {
        EventBus.Raise(UIEventType.ShowInventory);
    }
}
