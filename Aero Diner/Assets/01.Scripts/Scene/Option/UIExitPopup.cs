using System;
using UnityEngine;

public class UIExitPopup : MonoBehaviour
{
    public static UIExitPopup Instance;

    [SerializeField] private GameObject popupRoot;

    private Action pendingAction;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Show(Action onConfirm = null)
    {
        popupRoot.SetActive(true);
        pendingAction = onConfirm;
    }

    public void Hide()
    {
        popupRoot.SetActive(false);
        pendingAction = null;
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

        pendingAction?.Invoke();
    }
}

