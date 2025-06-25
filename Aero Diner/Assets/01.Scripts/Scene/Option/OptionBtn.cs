using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionBtn : MonoBehaviour
{
    private bool IsKeyChanged() => KeyRebindManager.Instance?.HasUnsavedChanges() ?? false;
    private bool IsVolumeChanged() => VolumeHandler.Instance?.HasUnsavedChanges() ?? false;
    private bool IsVideoChanged() => VideoSettingPanel.Instance?.HasUnsavedChanges() ?? false;
    [SerializeField] private SavePopupFader popupFader;
    public void OnClickOption()
    {
        EventBus.Raise(UIEventType.OpenOption);
    }

    public void OnClickSoundTab()
    {
        if (IsKeyChanged() || IsVideoChanged())
        {
            UIExitPopup.Instance?.Show();
        }
        else { EventBus.Raise(UIEventType.ShowSoundTab); }
    }

    public void OnClickVideoTab()
    {
        Debug.Log("비디오탭 눌림");
        if (IsKeyChanged() || IsVolumeChanged()) { Debug.Log("검사필"); UIExitPopup.Instance?.Show(); }
        else { EventBus.Raise(UIEventType.ShowVideoTab); }
    }

    public void OnClickControlTab()
    {
        if(IsVideoChanged() || IsVolumeChanged()) { UIExitPopup.Instance?.Show(); }
       else { EventBus.Raise(UIEventType.ShowControlTab); }
    }
    public void OnSaveClick()
    {
        popupFader.ShowPopup("설정이 저장되었습니다!");
    }
    public void GotoStartScene()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("StartScene");
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
