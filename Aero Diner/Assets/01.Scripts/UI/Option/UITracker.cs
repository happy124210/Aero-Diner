using UnityEngine;

public class UITracker : MonoBehaviour
{

    public bool IsPauseOpen { get; private set; }
    public bool IsOptionOpen { get; private set; }


    private void OnEnable()
    {
        EventBus.OnUIEvent += OnUIEvent;
    }

    private void OnDisable()
    {
        EventBus.OnUIEvent -= OnUIEvent;
    }

    private void OnUIEvent(UIEventType eventType, object payload)
    {
        switch (eventType)
        {
            case UIEventType.OpenPause: IsPauseOpen = true; break;
            case UIEventType.ClosePause: IsPauseOpen = false; break;

            case UIEventType.OpenOption: IsOptionOpen = true; break;
            case UIEventType.CloseOption: IsOptionOpen = false; break;
        }
    }
}
