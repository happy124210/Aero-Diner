using System;
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

    [Header("Movement")]
    [SerializeField] private NavMeshAgent navAgent;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

    // NavMesh 관련 상수
    private const float AGENT_DRIFT = 0.0001f;
    private const float ARRIVAL_THRESHOLD = 0.5f;
    private const float VELOCITY_THRESHOLD = 0.1f;

    #region Initialization
    public void Initialize(float speed)
    {
        SetupComponents();
        SetupNavMeshAgent(speed);
    }

    private void SetupComponents()
    {
        if (!navAgent) navAgent = GetComponent<NavMeshAgent>();
        
        // UI 컴포넌트들을 찾기
        if (!customerUI) customerUI = transform.FindChild<Canvas>("Group_Customer");
        if (!orderBubble) orderBubble = transform.FindChild<Image>("Img_OrderBubble");
        if (!patienceTimer) patienceTimer = transform.FindChild<Image>("Img_PatienceTimer");

        if (!navAgent)
        {
            Debug.LogError($"[CustomerView]: {gameObject.name} NavMeshAgent 없음!");
        }
        
        HideAllUI();
    }

    private void SetupNavMeshAgent(float speed)
    {
        if (!navAgent) return;

        // 2D NavMesh 설정
        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
        navAgent.speed = speed;
        navAgent.stoppingDistance = 0.1f;
        navAgent.angularSpeed = 120f;
        navAgent.acceleration = 8f;
    }
    #endregion

    #region Movement & Animation
    public void SetDestination(Vector3 destination)
    {
        if (!navAgent || !navAgent.isOnNavMesh)
        {
            if (showDebugInfo) Debug.LogWarning($"[CustomerView]: {gameObject.name} NavMesh 문제!");
            return;
        }

        navAgent.isStopped = false;
        
        // NavMeshPlus Y축 버그 방지
        if (Mathf.Abs(transform.position.x - destination.x) < AGENT_DRIFT)
        {
            destination.x += AGENT_DRIFT;
        }

        navAgent.SetDestination(destination);
        
        if (showDebugInfo) Debug.Log($"[CustomerView]: {gameObject.name} 목적지 설정: {destination}");
    }

    public void StopMovement()
    {
        if (!navAgent) return;
        
        navAgent.isStopped = true;
        navAgent.ResetPath();
        navAgent.velocity = Vector3.zero;
    }

    public bool HasReachedDestination()
    {
        if (!navAgent || !navAgent.isOnNavMesh) return false;

        bool reached = !navAgent.pathPending && 
                      navAgent.remainingDistance < ARRIVAL_THRESHOLD && 
                      navAgent.velocity.sqrMagnitude < VELOCITY_THRESHOLD;

        if (reached && showDebugInfo) 
            Debug.Log($"[CustomerView]: {gameObject.name} 목적지 도착!");

        return reached;
    }

    public void SetAnimationState(CustomerAnimState state)
    {
        // TODO: 실제 애니메이터 연동
        if (showDebugInfo) Debug.Log($"[CustomerView]: {gameObject.name} 애니메이션: {state}");
    }

    public void AdjustSeatPosition(Vector3 seatPosition)
    {
        transform.position = seatPosition;
        SetAnimationState(CustomerAnimState.Idle);
    }
    #endregion

    #region UI Updates (Controller로부터 호출)
    public void UpdatePatienceUI(float currentPatience, float maxPatience)
    {
        if (!patienceTimer) return;

        float patienceRatio = currentPatience / maxPatience;
        patienceTimer.fillAmount = patienceRatio;

        // 색상 변경
        Color timerColor = patienceRatio switch
        {
            > 0.66f => Color.green,
            > 0.33f => Color.yellow,
            _ => Color.red
        };
        patienceTimer.color = timerColor;
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
        if (!orderBubble || order == null) return;

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
        if (!patienceTimer) return;
        
        patienceTimer.gameObject.SetActive(false);
    }

    private void HideOrderBubble()
    {
        if (!orderBubble) return;
        
        orderBubble.gameObject.SetActive(false);
    }

    public void HideAllUI()
    {
        if (!customerUI) return;
        
        customerUI.gameObject.SetActive(false);
    }
    public void OnServedStateChanged(bool isServed)
    {
        if (isServed)
        {
            HideOrderBubble();
        }
    }

    public void OnEatingStateChanged(bool isEating)
    {
        // 식사 시작 시 UI 정리
        if (isEating)
        {
            HideOrderBubble();
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

    #region Cleanup
    public void Cleanup()
    {
        HideAllUI();
        
        if (navAgent && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    #endregion
}