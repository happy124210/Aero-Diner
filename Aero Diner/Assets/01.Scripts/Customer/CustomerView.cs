using UnityEngine;
using UnityEngine.UI;

public enum CustomerAnimState
{
    Idle,
    Walking,
    Sitting,
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
    
    [Header("Animation Components")]
    [SerializeField] private Animator animator;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    
    // animation hash
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsSitting = Animator.StringToHash("IsSitting");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    
    #region Initialization
    public void Initialize()
    {
        SetupComponents();
        SetPatienceVisibility(false);
    }

    private void SetupComponents()
    {
        if (!customerUI) customerUI = transform.FindChild<Canvas>("Group_Customer");
        if (!orderBubble) orderBubble = transform.FindChild<Image>("Img_OrderBubble");
        if (!patienceTimer) patienceTimer = transform.FindChild<Image>("Img_PatienceTimer");
        
        animator = GetComponentInChildren<Animator>();
    }
    
    #endregion

    #region UI Updates (Controller로부터 호출됨)
    public void UpdatePatienceUI(float patienceRatio)
    {
        if (!patienceTimer) return;
        patienceTimer.fillAmount = patienceRatio;
        patienceTimer.color = Util.ChangeColorByRatio(patienceRatio);
    }

    public void SetPatienceVisibility(bool isActive)
    {
        if (!customerUI || !patienceTimer) return;

        customerUI.gameObject.SetActive(isActive);
        orderBubble.gameObject.SetActive(isActive);
        patienceTimer.gameObject.SetActive(isActive);
    }

    public void ShowOrderBubble(FoodData order)
    {
        if (!orderBubble || !order) return;
        
        orderBubble.gameObject.SetActive(true);
        orderBubble.sprite = order.foodIcon;
    }

    public void ShowServedEffect()
    {
        EventBus.OnSFXRequested(SFXType.CustomerServe);
        if (showDebugInfo) Debug.Log($"[CustomerView]: {gameObject.name} 먹는중 이펙트");
    }
    
    public void ShowEatingEffect()
    {
        // TODO: 먹는중 이펙트 표시
        if (showDebugInfo) Debug.Log($"[CustomerView]: {gameObject.name} 먹는중 이펙트");
    }

    public void ShowPayEffect()
    {
        // TODO: 결제 이펙트 표시
        if (showDebugInfo) Debug.Log($"[CustomerView]: {gameObject.name} 결제 완료 이펙트");
    }

    public void ShowAngryEffect()
    {
        EventBus.OnSFXRequested(SFXType.CustomerAngry);
        if (showDebugInfo) Debug.Log($"[CustomerView]: {gameObject.name} 결제 완료 이펙트");
    }
    
    #endregion

    #region Animations

    public void SetAnimationState(CustomerAnimState state)
    {
        if (!animator) return;
        
        animator.SetBool(IsWalking, state == CustomerAnimState.Walking);
        animator.SetBool(IsSitting, state == CustomerAnimState.Sitting);
    }
    
    public void UpdateAnimationDirection(Vector2 direction)
    {
        if (!animator) return;

        animator.SetFloat(MoveX, direction.x);
        animator.SetFloat(MoveY, direction.y);
    }

    #endregion

    #region Coroutines

    

    #endregion
    
    #region Cleanup
    public void Cleanup()
    {
        SetPatienceVisibility(false);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    #endregion
}