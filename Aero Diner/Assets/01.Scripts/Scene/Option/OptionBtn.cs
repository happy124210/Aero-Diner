using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionBtn : MonoBehaviour
{
    private bool IsKeyChanged() => UIRoot.Instance.keyRebindManager?.HasUnsavedChanges() ?? false;
    private bool IsVolumeChanged() => UIRoot.Instance.volumeHandler?.HasUnsavedChanges() ?? false;
    private bool IsVideoChanged() => UIRoot.Instance.videoSettingPanel?.HasUnsavedChanges() ?? false;
    [SerializeField] private SavePopupFader popupFader;
    public GameObject saveWarningPanel;
    public void OnClickOption()
    {
        EventBus.Raise(UIEventType.OpenOption);
    }

    public void OnClickSoundTab()
    {
        if (UIRoot.Instance.volumeHandler.HasUnsavedChanges() ||
            UIRoot.Instance.videoSettingPanel.HasUnsavedChanges())
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(0);

            UIRoot.Instance.uiExitPopup.Show(() =>
            {
                EventBus.Raise(UIEventType.ShowSoundTab);
                UIRoot.Instance.tabButtonController.ApplyTabSelectionVisuals();
            });
        }
        else
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(0);
            EventBus.Raise(UIEventType.ShowSoundTab);
            UIRoot.Instance.tabButtonController.ApplyTabSelectionVisuals();
        }
    }


    public void OnClickVideoTab()
    {
        if (UIRoot.Instance.volumeHandler.HasUnsavedChanges() ||
            UIRoot.Instance.keyRebindManager.HasUnsavedChanges())
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(1);

            UIRoot.Instance.uiExitPopup.Show(() =>
            {
                EventBus.Raise(UIEventType.ShowVideoTab);
                UIRoot.Instance.tabButtonController.ApplyTabSelectionVisuals();
            });
        }
        else
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(1);
            EventBus.Raise(UIEventType.ShowVideoTab);
            UIRoot.Instance.tabButtonController.ApplyTabSelectionVisuals();
        }
    }


    public void OnClickControlTab()
    {
        if (UIRoot.Instance.volumeHandler.HasUnsavedChanges() ||
            UIRoot.Instance.videoSettingPanel.HasUnsavedChanges())
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(2);

            UIRoot.Instance.uiExitPopup.Show(() =>
            {
                EventBus.Raise(UIEventType.ShowControlTab);
                UIRoot.Instance.tabButtonController.ApplyTabSelectionVisuals();
            });
        }
        else
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(2);
            EventBus.Raise(UIEventType.ShowControlTab);
            UIRoot.Instance.tabButtonController.ApplyTabSelectionVisuals();
        }
    }


    public void OnSaveClick()
    {
        popupFader.ShowPopup("설정이 저장되었습니다!");
    }
    public void GotoStartScene()
    {
        EventBus.Raise(UIEventType.ClosePause);
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
    public void OnClickNewGame()
    {
        saveWarningPanel.SetActive(true);
    }
    public void OnClickCancel()
    {
        saveWarningPanel.SetActive(false);
    }
    public void NewGameNoSave()
    {
        EventBus.Raise(UIEventType.OnClickNewGame);
    }
}
