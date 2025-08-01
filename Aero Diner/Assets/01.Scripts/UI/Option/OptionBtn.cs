using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionBtn : MonoBehaviour
{
    [SerializeField] private SavePopupFader popupFader;
    public GameObject saveWarningPanel;
    public void OnClickOption()
    {
        EventBus.Raise(UIEventType.OpenOption);
        EventBus.PlaySFX(SFXType.ButtonClick);
    }

    public void OnClickSoundTab()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        if (UIRoot.Instance.volumeHandler.HasUnsavedChanges() ||
            UIRoot.Instance.videoSettingPanel.HasUnsavedChanges())
        {
            UIRoot.Instance.uiExitPopup.Show(() =>
            {
                UIRoot.Instance.tabButtonController.RequestSelectTab(0);
            });
        }
        else
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(0);
        }
    }

    public void OnClickVideoTab()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        if (UIRoot.Instance.volumeHandler.HasUnsavedChanges() ||
            UIRoot.Instance.keyRebindManager.HasUnsavedChanges())
        {
            UIRoot.Instance.uiExitPopup.Show(() =>
            {
                UIRoot.Instance.tabButtonController.RequestSelectTab(1);
            });
        }
        else
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(1);
        }
    }

    public void OnClickControlTab()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        if (UIRoot.Instance.volumeHandler.HasUnsavedChanges() ||
            UIRoot.Instance.videoSettingPanel.HasUnsavedChanges())
        {
            UIRoot.Instance.uiExitPopup.Show(() =>
            {
                UIRoot.Instance.tabButtonController.RequestSelectTab(2);
            });
        }
        else
        {
            UIRoot.Instance.tabButtonController.RequestSelectTab(2);
        }
    }


    public void OnSaveClick()
    {
        popupFader.ShowPopup("설정이 저장되었습니다!");
        EventBus.PlaySFX(SFXType.ButtonClick);
    }
    public void GotoStartScene()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.ClosePause);
        EventBus.PlayBGM(BGMEventType.StopBGM);
        GameManager.Instance.RestoreEarningsToBeforeDay();
        EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(scene: "StartScene"));
        EventBus.PlayBGM(BGMEventType.StopBGM);
    }
    public void OnClickStartGame()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);

        EventBus.Raise(UIEventType.LoadIntroScene);
    }
    public void OnClickloadGame()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);

        EventBus.Raise(UIEventType.LoadDayScene);
    }
    public void QuitGame()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.QuitGame);
    }
    public void OnClickNewGame()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        saveWarningPanel.SetActive(true);
    }
    public void OnClickCancel()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        saveWarningPanel.SetActive(false);
    }
    public void NewGameNoSave()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.OnClickNewGame);
    }
}
