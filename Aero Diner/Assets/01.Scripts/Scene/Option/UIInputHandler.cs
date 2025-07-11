using UnityEngine;
using UnityEngine.SceneManagement;

public class UIInputHandler : MonoBehaviour
{
    public void HandleEscapeLikeAction()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        var tracker = UIRoot.Instance.uiTracker;
        if (tracker == null) return;

        string currentScene = SceneManager.GetActiveScene().name;
        bool isStartScene = currentScene == "StartScene";

        if (tracker.IsOptionOpen)
        {
            bool keyChanged = UIRoot.Instance.keyRebindManager?.HasUnsavedChanges() ?? false;
            bool volumeChanged = UIRoot.Instance.volumeHandler?.HasUnsavedChanges() ?? false;
            bool videoChanged = UIRoot.Instance.videoSettingPanel?.HasUnsavedChanges() ?? false;

            if (keyChanged || volumeChanged || videoChanged)
            {
                UIRoot.Instance.uiExitPopup?.Show(); // 변경 사항 있음 → 팝업 노출
            }
            else
            {
                // 옵션창 닫기
                EventBus.Raise(UIEventType.CloseOption);
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
            if (!isStartScene) // StartScene이 아닐 때만 Pause 열기
            {
                EventBus.Raise(UIEventType.OpenPause);
            }
            else
            {
                Debug.Log("StartScene에서는 PausePanel을 열지 않습니다.");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EventBus.PlaySFX(SFXType.ButtonClick);
            Debug.Log("ESC 눌렸음");
            HandleEscapeLikeAction();
        }
    }
}
