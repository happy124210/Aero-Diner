using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Table : MonoBehaviour, IInteractable
{
    [Header("테이블 설정")]
    [SerializeField] private Transform seatPosition;
    
    [Header("현재 설정")]
    [SerializeField] private FoodDisplay currentFood;
    [SerializeField] private CustomerController assignedCustomer; // 이 테이블에 앉아있는 손님
    [SerializeField] private int seatIndex = -1;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

#region UnityEvents

    private void Reset()
    {
        seatPosition = transform.Find("Seat Position");
    }

#endregion
    
#region TableManager 연동

    public void SetSeatIndex(int index)
    {
        seatIndex = index;
    }

    public void AssignCustomer(CustomerController customer)
    {
        this.assignedCustomer = customer;
        if (showDebugInfo) Debug.Log("[Table]: 손님 할당");
    }

    public void ReleaseCustomer()
    {
        assignedCustomer = null;
    }

    public Vector3 GetSeatPosition()
    {
        return seatPosition.position;
    }
    
    #endregion

#region IInteractable

    public void Interact(PlayerInventory inventory, InteractionType interactionType)
    {
        switch (interactionType)
        {
            case InteractionType.Pickup:
                if (currentFood != null && !inventory.IsHoldingItem)
                {
                    
                }
                break;
            
            case InteractionType.Use:
                break;
        }
    }

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }

    #endregion

    /// <summary>
    /// 현재 테이블이 음식 서빙 가능 상태인지
    /// 손님 존재 여부는 고려하지 않음
    /// </summary>
    public bool CanPlaceFood()
    {
        return currentFood == null;
    }

    public bool PlaceFood()
    {
        if (!CanPlaceFood())
        {
            return false;
        }
        
        // TODO: 배치 로직
        return true;
    }

    public void CheckOrderMatch()
    {
        if (assignedCustomer == null || currentFood == null) return;
        
        // TODO: 주문 시스템 연동
    }

    public void OnOrderMatch()
    {
        // TODO: 주문 일치 시 식사 시작
    }

    /// <summary>
    /// 식사 대기 후 음식 제거
    /// </summary>
    public IEnumerator ClearFood()
    {
        while (assignedCustomer != null)
        {
            float eatTime = assignedCustomer.CurrentData.eatTime;
            yield return new WaitForSeconds(eatTime);
        }
        
        // TODO: 임시 Destroy
        Destroy(currentFood.gameObject);
        currentFood = null;
    }

    /// <summary>
    /// 테이블에서 음식 가져가기
    /// </summary>
    public void PickupFood(PlayerInventory inventory)
    {
        if (currentFood == null) return;
        
        if (assignedCustomer != null && assignedCustomer.IsFoodServed()) return;
        
        currentFood.transform.SetParent(inventory.GetItemSlotTransform());
        currentFood.transform.localPosition = Vector3.zero;
        
        // 물리 효과 정리
        var rb = currentFood.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;
        var col = currentFood.GetComponent<Collider2D>();
        if (col) col.enabled = false;

        inventory.SetHeldItem(currentFood);
        currentFood = null;
    }
    
#region Public getters
    
    public bool HasFood => currentFood != null;
    public bool HasCustomer => assignedCustomer != null;
    public FoodDisplay CurrentFood => currentFood;
    public CustomerController AssignedCustomer => assignedCustomer;
    
#endregion
}
