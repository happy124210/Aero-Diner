using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionBtn : MonoBehaviour
{
    public void OnClickOption()
    {
        EventBus.Raise(UIEventType.OpenOption);
    }

    public void OnClickSoundTab()
    {
        EventBus.Raise(UIEventType.ShowSoundTab);
    }

    public void OnClickVideoTab()
    {
        EventBus.Raise(UIEventType.ShowVideoTab);
    }

    public void OnClickControlTab()
    {
        EventBus.Raise(UIEventType.ShowControlTab);
    }
}
