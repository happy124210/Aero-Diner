using UnityEngine;

public class Table : MonoBehaviour, IInteractable, IPlaceableStation
{
    [Header("테이블 설정")] 
    [SerializeField] private Transform menuSpawnPosition;
    [SerializeField] private Transform stopPoint;
    [SerializeField] private Transform seatPoint;// 메뉴가 생성되는 위치
    [SerializeField] private int seatIndex = -1;
    
    [Header("현재 설정 - 확인용")]
    [SerializeField, ReadOnly] private FoodData currentFoodData; // 테이블에 놓여있는 음식 정보
    [SerializeField, ReadOnly] private CustomerController assignedCustomer; // 테이블에 앉아있는 손님

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    private GameObject currentFoodObj;
    
    #region UnityEvents

    private void Reset()
    {
        menuSpawnPosition = transform.Find("Menu Spawn Position");
    }

    #endregion

    #region TableManager 연동

    public void SetSeatIndex(int index)
    {
        seatIndex = index;
    }

    public void AssignCustomer(CustomerController customer)
    {
        assignedCustomer = customer;
        if (showDebugInfo) Debug.Log("[Table]: 손님 할당");
    }

    public void ReleaseCustomer()
    {
        ClearFood();
        assignedCustomer = null;
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
        currentFoodData = currentFoodObj.GetComponent<FoodDisplay>().foodData;
        
        // 앉아있는 고객에게 메뉴 전달
        if (assignedCustomer)
            assignedCustomer.ReceiveFood(data);
    }

    public void OnPlayerPickup()
    {
        currentFoodObj = null;
    }

    #endregion
    
    private GameObject CreateMenuDisplay(FoodData data)
    {
        if (data == null || menuSpawnPosition == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        // VisualObjectFactory 호출
        GameObject obj = VisualObjectFactory.CreateIngredientVisual(
            parent: menuSpawnPosition,
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

    public void OnOrderMatch()
    {
        // TODO: 주문 일치 시 FoodDisplay 비활성화
        // 이벤트 연결 필요
    }

    /// <summary>
    /// 식사 대기 후 음식 제거
    /// </summary>
    public void ClearFood()
    {
        Destroy(currentFoodObj);
        currentFoodObj = null;
    }
    
    #region Public getters & methods
    
    public Vector3 GetSeatPoint() => seatPoint.position;
    public Vector3 GetStopPoint() => stopPoint.position;
    
    public bool HasFood => currentFoodObj != null;
    public bool HasCustomer => assignedCustomer != null;
    public bool CanPlaceFood => currentFoodObj == null;
    public CustomerController AssignedCustomer => assignedCustomer;
    
    public FoodDisplay GetCurrentFood() => currentFoodObj?.GetComponent<FoodDisplay>();

    #endregion
    
}