using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

public class Table : MonoBehaviour, IInteractable, IPlaceableStation
{
    [Header("테이블 설정")] 
    [SerializeField] private Transform seatPosition;
    [SerializeField] private Transform menuSpawnPoint;
    [SerializeField] private int seatIndex = -1;
    
    [Header("현재 설정 - 확인용")]
    [SerializeField, ReadOnly] private GameObject currentFoodObj; // 테이블에 놓여있는 음식
    [SerializeField, ReadOnly] private CustomerController assignedCustomer; // 테이블에 앉아있는 손님

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private FoodDisplay currentData;

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

    public void Interact(PlayerInventory inventory, InteractionType interactionType) { }

    public void OnHoverEnter() { }

    public void OnHoverExit() { }

    #endregion
    
    #region IPlaceableStation
    
    public void PlaceObject(FoodData data)
    {
        if (currentFoodObj) return;
        currentFoodObj = CreateMenuDisplay(data);
        currentData = currentFoodObj.GetComponent<FoodDisplay>();
        
        CheckOrderMatch();
    }

    public void OnPlayerPickup()
    {
        currentFoodObj = null;
    }

    #endregion
    
    private GameObject CreateMenuDisplay(FoodData data)
    {
        if (data == null || menuSpawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        // VisualObjectFactory 호출
        GameObject obj = VisualObjectFactory.CreateIngredientVisual(
            parent: menuSpawnPoint,
            name: data.foodName,
            icon: data.foodIcon
        );
        if (obj == null) return null;

        // FoodDisplay 세팅 (원본 데이터만 연결)
        var display = obj.AddComponent<FoodDisplay>();
        display.foodData = data;
        display.originPlace = this;
        return obj;
    }

    public void CheckOrderMatch()
    {
        if (assignedCustomer == null || !currentFoodObj) return;
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
        Destroy(currentFoodObj);
        currentFoodObj = null;
    }

    #region Public getters

    public bool HasFood => currentFoodObj != null;
    public bool HasCustomer => assignedCustomer != null;
    public bool CanPlaceFood => currentFoodObj == null;
    public CustomerController AssignedCustomer => assignedCustomer;

    #endregion

}