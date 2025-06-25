using UnityEngine;

public class UITracker : MonoBehaviour
{
    public static UITracker Instance;

    public bool IsPauseOpen { get; private set; }
    public bool IsOptionOpen { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
