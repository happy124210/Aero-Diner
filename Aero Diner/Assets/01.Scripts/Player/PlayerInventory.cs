using UnityEngine;


//플레이어가 재료를 들고, 내려놓는 기능을 담당하는 인벤토리
public class PlayerInventory : MonoBehaviour
{
    public Transform GetItemSlotTransform() => itemSlotTransform;
    
    [Header("아이템 슬롯 위치")]
    [SerializeField] private Transform itemSlotTransform;
    [SerializeField] private TilemapController tilemapController;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo;
    
    // 인벤토리 정보
    public FoodDisplay holdingItem;
    public IMovableStation heldStation;
    
    public bool IsHoldingItem => holdingItem != null || heldStation != null;

    private void Start()
    {
        if (tilemapController == null)
        {
            tilemapController = FindObjectOfType<TilemapController>();
        }
    }

    public void TryPickup(IInteractable target)
    {
        if (IsHoldingItem || target == null) return;

        //Station 들기: 편집 모드에서만
        if (GameManager.Instance.CurrentPhase == GamePhase.EditStation && target is IMovableStation movable)
        {
            heldStation = movable;

            var tr = movable.GetTransform();
            tr.SetParent(itemSlotTransform);
            tr.localPosition = Vector3.zero;

            var rb = tr.GetComponent<Rigidbody2D>();
            if (rb) rb.simulated = false;

            var col = tr.GetComponent<Collider2D>();
            if (col) col.enabled = false;

            tilemapController?.ShowAllCells();
            EventBus.Raise(GameEventType.PlayerPickedUpItem, heldStation.StationData);
            return;
        }

        // Operation, Closing 페이즈 전용 처리
        if (GameManager.Instance.CurrentPhase == GamePhase.Operation || GameManager.Instance.CurrentPhase == GamePhase.Closing)
        {
            // IngredientStation → Interact 호출로 재료 생성
            if (target is IngredientStation station)
            {
                station.Interact(this, InteractionType.Pickup);
                return;
            }

            // FoodDisplay → 직접 들기
            if (target is FoodDisplay food && food.CanPickup())
            {
                holdingItem = food;

                food.transform.SetParent(itemSlotTransform);
                food.transform.localPosition = Vector3.zero;

                var rb = food.GetComponent<Rigidbody2D>();
                if (rb) rb.simulated = false;

                var col = food.GetComponent<Collider2D>();
                if (col) col.enabled = false;

                food.originPlace?.OnPlayerPickup();
                EventBus.Raise(GameEventType.PlayerPickedUpItem, holdingItem.foodData);
            }
        }
    }

    public void DropItem(IInteractable target)
    {
        if (GameManager.Instance.CurrentPhase == GamePhase.EditStation && heldStation != null)
        {
            if (GameManager.Instance.CurrentPhase == GamePhase.EditStation && heldStation != null)
            {
                if (target is GridCellStatus gridCell)
                {
                    // 배치 성공 여부 체크
                    bool success = PlacementManager.Instance.TryPlaceStationAt(gridCell.gameObject, heldStation);

                    if (success)
                    {
                        heldStation = null; // 성공 시에만 손에서 내려놓음

                        if (tilemapController != null)
                            tilemapController.HideAllCells();
                    }

                    return;
                }
            }
        }

        if (GameManager.Instance.CurrentPhase != GamePhase.Operation)
        {
            if (showDebugInfo)
                Debug.Log("[Inventory] Operation 페이즈가 아니므로 재료를 배치할 수 없습니다.");
            return;
        }

        if (showDebugInfo)
            Debug.Log($"[Inventory] target 타입: {target.GetType().Name}");

        if (!IsHoldingItem || target == null)
        {
            if (showDebugInfo)
                Debug.Log("[Inventory] 아이템이 없거나 타겟이 없습니다.");
            return;
        }

        bool placed = false;

        switch (target)
        {
            case IngredientStation station:
                FoodData dataToStation = holdingItem.foodData;
                if (dataToStation != null && station.PlaceIngredient(dataToStation))
                {
                    Destroy(holdingItem.gameObject);
                    holdingItem = null;
                    if (showDebugInfo)
                        Debug.Log($"[Inventory] 재료 일치 - 재료 소비됨");
                    placed = true;
                }
                else
                {
                    if (showDebugInfo)
                        Debug.Log("[Inventory] 재료 불일치 - 내려놓을 수 없음");
                }
                break;
            
            case Shelf shelf:
                {
                    if (shelf.CanPlaceIngredient(holdingItem.foodData))
                    {
                        shelf.PlaceObject(holdingItem.foodData);
                        Destroy(holdingItem.gameObject);
                        holdingItem = null;
                        if (showDebugInfo)
                            Debug.Log("[Inventory] 선반에 아이템 배치됨");
                        placed = true;
                    }
                    else
                    {
                        if (showDebugInfo)
                            Debug.Log("[Inventory] 선반에 재료를 배치할 수 없습니다.");
                    }
                    break;
                }
            
            case Trashcan:
                Destroy(holdingItem.gameObject);
                holdingItem = null;
                if (showDebugInfo)
                    Debug.Log("[Inventory] 아이템을 쓰레기통에 버렸습니다.");
                placed = true;
                break;
            
            case PassiveStation station:
                if (holdingItem.foodData
                    && station.CanPlaceIngredient(holdingItem.foodData))
                {
                    // ScriptableObject 원본(rawData)으로 배치 호출
                    station.PlaceObject(holdingItem.foodData);

                    Destroy(holdingItem.gameObject);
                    holdingItem = null;
                    if (showDebugInfo)
                        Debug.Log("[Inventory] PassiveStation에 아이템 배치됨");
                    placed = true;
                }

                else
                {
                    if (showDebugInfo)
                        Debug.Log("[Inventory] PassiveStation에 재료 배치 실패");
                }
                break;
                
            case AutomaticStation automatic:
                if (holdingItem.foodData
                    && automatic.CanPlaceIngredient(holdingItem.foodData))
                {
                    // 실제 배치 호출
                    automatic.PlaceObject(holdingItem.foodData);

                    Destroy(holdingItem.gameObject);
                    holdingItem = null;
                    if (showDebugInfo)
                        Debug.Log("[Inventory] AutoStation에 아이템 배치됨");
                    placed = true;
                }
                else
                {
                    if (showDebugInfo)
                        Debug.Log("[Inventory] AutoStation에 배치 불가");
                }
                break;
            
            case Table table:
                if (table.CanPlaceFood)
                {
                    table.PlaceObject(holdingItem.foodData);
                    Destroy(holdingItem.gameObject);
                    holdingItem = null;
                    if (showDebugInfo)
                        Debug.Log("[Inventory] 테이블에 아이템 배치됨");
                    placed = true;
                }
                else
                {
                    if (showDebugInfo)
                        Debug.Log("[Inventory] 테이블에 재료를 배치할 수 없습니다.");
                }
                break;
            
        }

        if (!placed)
        {
            if (showDebugInfo)
                Debug.Log("감지는 되었으나 배치 실패");
        }
    }

    public void SetHeldItem(FoodDisplay item)
    {
        holdingItem = item;
        EventBus.Raise(GameEventType.PlayerPickedUpItem, holdingItem.foodData);
    }
}