using UnityEngine;

public class UIInputHandler : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var tracker = UITracker.Instance;
            if (tracker == null) return;

            if (tracker.IsOptionOpen)
            {
                // 옵션과 하위 패널 모두 닫기
                EventBus.Raise(UIEventType.CloseOption);
                EventBus.Raise(UIEventType.CloseSound);
                EventBus.Raise(UIEventType.CloseVideo);
                EventBus.Raise(UIEventType.CloseControl);
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
    }
}
