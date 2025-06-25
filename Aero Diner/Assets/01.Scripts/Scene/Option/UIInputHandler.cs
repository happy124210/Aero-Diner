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
            UIExitPopup.Instance?.Show(); // 팝업 노출
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
