using UnityEngine;
using UnityEngine.UIElements;

public class UIInputHandler : MonoBehaviour
{
    public void HandleEscapeLikeAction()
    {
        var tracker = UITracker.Instance;
        if (tracker == null) return;

        if (tracker.IsOptionOpen)
        {
            bool keyChanged = KeyRebindManager.Instance?.HasUnsavedChanges() ?? false;
            bool volumeChanged = VolumeHandler.Instance?.HasUnsavedChanges() ?? false;
            bool videoChanged = VideoSettingPanel.Instance?.HasUnsavedChanges() ?? false;

            if (keyChanged || volumeChanged || videoChanged)
            {
                UIExitPopup.Instance?.Show(); // 변경 사항 있음 → 팝업 노출
            }
            else
            {
                EventBus.Raise(UIEventType.CloseOption); // 변경 없음 → 바로 닫기
                EventBus.Raise(UIEventType.CloseSound);
                EventBus.Raise(UIEventType.CloseVideo);
                EventBus.Raise(UIEventType.CloseControl);
            }
        }
        else if (tracker.IsPauseOpen)
        {
            EventBus.Raise(UIEventType.ClosePause);
        }
        else
        {
            EventBus.Raise(UIEventType.OpenPause);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC 눌렸음");
            HandleEscapeLikeAction();
        }
    }
}
