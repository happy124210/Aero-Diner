using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionBtn : MonoBehaviour
{
    private bool IsKeyChanged() => UIRoot.Instance.keyRebindManager?.HasUnsavedChanges() ?? false;
    private bool IsVolumeChanged() => UIRoot.Instance.volumeHandler?.HasUnsavedChanges() ?? false;
    private bool IsVideoChanged() => UIRoot.Instance.videoSettingPanel?.HasUnsavedChanges() ?? false;
    [SerializeField] private SavePopupFader popupFader;
    public void OnClickOption()
    {
        EventBus.Raise(UIEventType.OpenOption);
    }

    public void OnClickSoundTab()
    {
        if (IsKeyChanged() || IsVideoChanged())
        {
               UIRoot.Instance.uiExitPopup?.Show(() =>
            {
                EventBus.Raise(UIEventType.ShowSoundTab);
            });
        }
        else
        {
            EventBus.Raise(UIEventType.ShowSoundTab);
        }
    }

    public void OnClickVideoTab()
    {
        if (IsKeyChanged() || IsVolumeChanged())
        {
            UIRoot.Instance.uiExitPopup?.Show(() =>
            {
                EventBus.Raise(UIEventType.ShowVideoTab);
            });
        }
        else
        {
            EventBus.Raise(UIEventType.ShowVideoTab);
        }
    }

    public void OnClickControlTab()
    {
        if (IsVolumeChanged() || IsVideoChanged())
        {
            UIRoot.Instance.uiExitPopup?.Show(() =>
            {
                EventBus.Raise(UIEventType.ShowControlTab);
            });
        }
        else
        {
            EventBus.Raise(UIEventType.ShowControlTab);
        }
    }
    public void OnSaveClick()
    {
        popupFader.ShowPopup("설정이 저장되었습니다!");
    }
    public void GotoStartScene()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("StartScene");
    }
    public void OnClickStartGame()
    {
        EventBus.Raise(UIEventType.LoadMainScene);
    }

    public void QuitGame()
    {
        EventBus.Raise(UIEventType.QuitGame);
    }
}
