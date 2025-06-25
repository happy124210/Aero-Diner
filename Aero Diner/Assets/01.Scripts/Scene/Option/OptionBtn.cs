using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionBtn : MonoBehaviour
{
    [SerializeField] private SavePopupFader popupFader;
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
    public void OnSaveClick()
    {
        popupFader.ShowPopup("설정이 저장되었습니다!");
    }
}
