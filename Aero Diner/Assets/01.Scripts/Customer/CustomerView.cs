using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum CustomerAnimState
{
    Idle,
    Walking,
    Sit
}

/// <summary>
/// 시각적 표현을 담당하는 View
/// </summary>
public class CustomerView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Canvas customerUI;
    [SerializeField] private Image orderBubble;
    [SerializeField] private Image patienceTimer;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

    #region Initialization
    public void Initialize(float speed)
    {
        SetupComponents();
        HidePatienceTimer();
    }

    private void SetupComponents()
    {
        if (!customerUI) customerUI = transform.FindChild<Canvas>("Group_Customer");
        if (!orderBubble) orderBubble = transform.FindChild<Image>("Img_OrderBubble");
        if (!patienceTimer) patienceTimer = transform.FindChild<Image>("Img_PatienceTimer");
    }
    
    #endregion

    #region UI Updates (Controller로부터 호출됨)
    public void UpdatePatienceUI(float patienceRatio)
    {
        if (!patienceTimer) return;
        patienceTimer.fillAmount = patienceRatio;
        patienceTimer.color = Util.ChangeColorByRatio(patienceRatio);
    }

    public void UpdatePatienceVisibility(bool isDecreasing)
    {
        if (isDecreasing)
        {
            ShowPatienceTimer();
        }
        else
        {
            HidePatienceTimer();
        }
    }

    public void ShowOrderBubble(FoodData order)
    {
        if (!orderBubble || !order) return;

        customerUI.gameObject.SetActive(true);
        orderBubble.gameObject.SetActive(true);
        orderBubble.sprite = order.foodIcon;
    } 

    private void ShowPatienceTimer()
    {
        if (!customerUI || !patienceTimer) return;

        customerUI.gameObject.SetActive(true);
        patienceTimer.gameObject.SetActive(true);
    }

    private void HidePatienceTimer()
    {
        if (!customerUI || !patienceTimer) return;
        
        customerUI.gameObject.SetActive(false);
        orderBubble.gameObject.SetActive(false);
        patienceTimer.gameObject.SetActive(false);
    }
    
    public void OnServedStateChanged(bool isServed)
    {
        if (isServed)
        {
            HidePatienceTimer();
        }
    }

    public void OnPaymentStateChanged(bool isCompleted)
    {
        if (isCompleted)
        {
            // TODO: 결제 이펙트 표시
            if (showDebugInfo) Debug.Log($"[CustomerView]: {gameObject.name} 결제 완료 이펙트");
        }
    }
    #endregion

    #region Animations

    public void SetAnimationState(CustomerAnimState state)
    {
        // TODO: 실제 애니메이터 연동
        
    }

    #endregion
    
    #region Cleanup
    public void Cleanup()
    {
        HidePatienceTimer();

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    #endregion
}