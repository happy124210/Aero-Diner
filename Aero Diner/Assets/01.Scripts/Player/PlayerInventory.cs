using UnityEngine;
using UnityEngine.Serialization;


//플레이어가 재료를 들고, 내려놓는 기능을 담당하는 인벤토리
public class PlayerInventory : MonoBehaviour
{
    public Transform GetItemSlotTransform() => itemSlotTransform;
    
    [Header("아이템 슬롯 위치")]
    [SerializeField] private Transform itemSlotTransform;
    [SerializeField] private TilemapController tilemapController;

    public bool ShowDebugInfo;


    
    ///현재 들고 있는 재료
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
            return;
        }

        // Operation 페이즈 전용 처리
        if (GameManager.Instance.CurrentPhase == GamePhase.Operation)
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
            }
        }
    }

    public void DropItem(IInteractable target)
    {
        if (GameManager.Instance.CurrentPhase == GamePhase.EditStation && heldStation != null)
        {
            var targetCell = PlayerController.Instance.FindGridCellInFront();
            if (target is GridCellStatus gridCell)
            {
                Transform stationTr = heldStation.GetTransform();
                stationTr.SetParent(null);
                stationTr.position = gridCell.transform.position;

                var rb = stationTr.GetComponent<Rigidbody2D>();
                if (rb) rb.simulated = true;

                var col = stationTr.GetComponent<Collider2D>();
                if (col) col.enabled = true;

                heldStation = null;

                if (tilemapController != null)
                {
                    tilemapController.HideAllCells(); // 여기!
                }

                return;
            }
        }
        if (GameManager.Instance.CurrentPhase != GamePhase.Operation)
        {
            if (ShowDebugInfo)
                Debug.Log("[Inventory] Operation 페이즈가 아니므로 재료를 배치할 수 없습니다.");
            return;
        }

        if (ShowDebugInfo)
            Debug.Log($"[Inventory] target 타입: {target.GetType().Name}");

        if (!IsHoldingItem || target == null)
        {
            if (ShowDebugInfo)
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
                    if (ShowDebugInfo)
                        Debug.Log($"[Inventory] 재료 일치 - 재료 소비됨");
                    placed = true;
                }
                else
                {
                    if (ShowDebugInfo)
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
                        if (ShowDebugInfo)
                            Debug.Log("[Inventory] 선반에 아이템 배치됨");
                        placed = true;
                    }
                    else
                    {
                        if (ShowDebugInfo)
                            Debug.Log("[Inventory] 선반에 재료를 배치할 수 없습니다.");
                    }
                    break;
                }
            
            case Trashcan:
                Destroy(holdingItem.gameObject);
                holdingItem = null;
                if (ShowDebugInfo)
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
                    if (ShowDebugInfo)
                        Debug.Log("[Inventory] PassiveStation에 아이템 배치됨");
                    placed = true;
                }

                else
                {
                    if (ShowDebugInfo)
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
                    if (ShowDebugInfo)
                        Debug.Log("[Inventory] AutoStation에 아이템 배치됨");
                    placed = true;
                }
                else
                {
                    if (ShowDebugInfo)
                        Debug.Log("[Inventory] AutoStation에 배치 불가");
                }
                break;
            
            case Table table:
                if (table.CanPlaceFood)
                {
                    table.PlaceObject(holdingItem.foodData);
                    Destroy(holdingItem.gameObject);
                    holdingItem = null;
                    if (ShowDebugInfo)
                        Debug.Log("[Inventory] 테이블에 아이템 배치됨");
                    placed = true;
                }
                else
                {
                    if (ShowDebugInfo)
                        Debug.Log("[Inventory] 테이블에 재료를 배치할 수 없습니다.");
                }
                break;
            
        }

        if (!placed)
        {
            if (ShowDebugInfo)
                Debug.Log("감지는 되었으나 배치 실패");
        }
    }

    public void SetHeldItem(FoodDisplay item)
    {
        holdingItem = item;
    }
}