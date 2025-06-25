using UnityEngine;

public class UIExitPopup : MonoBehaviour
{
    public static UIExitPopup Instance;

    [SerializeField] private GameObject popupRoot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Show()
    {
        popupRoot.SetActive(true);
    }

    public void Hide()
    {
        popupRoot.SetActive(false);
    }

    public void OnClickBack()
    {
        Hide();
    }

    public void OnClickRevertChanges()
    {
        // 각 시스템에 롤백 요청
        KeyRebindManager.Instance?.CancelAll();
        VolumeHandler.Instance?.RollbackVolumes();
        VideoSettingPanel.Instance?.RollbackPending();

        Hide();

        // 옵션 패널 닫기 이벤트 (EventBus 구조 가정)
        EventBus.Raise(UIEventType.CloseControl);
        EventBus.Raise(UIEventType.CloseOption);
        EventBus.Raise(UIEventType.CloseSound);
        EventBus.Raise(UIEventType.CloseVideo);
    }
}

