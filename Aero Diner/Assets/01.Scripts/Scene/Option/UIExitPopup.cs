using System;
using UnityEngine;

public class UIExitPopup : MonoBehaviour
{

    [SerializeField] private GameObject popupRoot;

    private Action pendingAction;



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
        UIRoot.Instance.keyRebindManager?.CancelAll();
        UIRoot.Instance.volumeHandler?.RollbackVolumes();
        UIRoot.Instance.videoSettingPanel?.RollbackPending();

        Hide();

        pendingAction?.Invoke();
    }
}

