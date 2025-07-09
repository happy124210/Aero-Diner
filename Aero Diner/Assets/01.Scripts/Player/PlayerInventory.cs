using UnityEngine;
using UnityEngine.Serialization;


//플레이어가 재료를 들고, 내려놓는 기능을 담당하는 인벤토리
public class PlayerInventory : MonoBehaviour
{
    public Transform GetItemSlotTransform() => itemSlotTransform;
    [Header("아이템 슬롯 위치")]
    [SerializeField] private Transform itemSlotTransform;

    ///현재 들고 있는 재료
    public FoodDisplay holdingItem;

    public bool IsHoldingItem => holdingItem != null;

    //아이템을 들기 시도
    public void TryPickup(IInteractable target)
    {
        if (IsHoldingItem || target == null) return;

        // 재료만 집을 수 있음
        FoodDisplay food = target as FoodDisplay;
        if (!food || !food.CanPickup()) return;

        holdingItem = food;

        // 시각적으로 들고 있는 위치로 이동
        holdingItem.transform.SetParent(itemSlotTransform);
        holdingItem.transform.localPosition = Vector3.zero;
        holdingItem.transform.localRotation = Quaternion.identity;

        // 충돌 및 중력 제거
        var rb = holdingItem.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        var col = holdingItem.GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Debug.Log($"[Inventory] {holdingItem.foodData.foodName} 획득");
        // 마지막에 스테이션 초기화 호출 (재료 오브젝트 파괴 방지)
        if (food.originPlace != null) { food.originPlace.OnPlayerPickup(); }
    }

    //아이템을 내려놓기 시도
    public void DropItem(IInteractable target)
    {
        Debug.Log($"[Inventory] target 타입: {target.GetType().Name}");
        if (!IsHoldingItem)
        {
            Debug.Log("[Inventory] 들고 있는 아이템이 없습니다.");
            return;
        }

        if (target == null)
        {
            Debug.Log("[Inventory] 상호작용 대상이 없습니다.");
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
                    Debug.Log($"[Inventory] 재료 일치 - 재료 소비됨");
                    placed = true;
                }
                else
                {
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
                        Debug.Log("[Inventory] 선반에 아이템 배치됨");
                        placed = true;
                    }
                    else
                    {
                        Debug.Log("[Inventory] 선반에 재료를 배치할 수 없습니다.");
                    }
                    break;
                }
            
            case Trashcan:
                Destroy(holdingItem.gameObject);
                holdingItem = null;
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
                    Debug.Log("[Inventory] PassiveStation에 아이템 배치됨");
                    placed = true;
                }

                else
                {
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
                    Debug.Log("[Inventory] AutoStation에 아이템 배치됨");
                    placed = true;
                }
                else
                {
                    Debug.Log("[Inventory] AutoStation에 배치 불가");
                }
                break;
            
            case Table table:
                if (table.CanPlaceFood)
                {
                    table.PlaceObject(holdingItem.foodData);
                    Destroy(holdingItem.gameObject);
                    holdingItem = null;
                    Debug.Log("[Inventory] 테이블에 아이템 배치됨");
                    placed = true;
                }
                else
                {
                    Debug.Log("[Inventory] 테이블에 재료를 배치할 수 없습니다.");
                }
                break;
            
        }

        if (!placed)
        {
            Debug.Log("감지는 되었으나 배치 실패");
        }
    }

    public void SetHeldItem(FoodDisplay item)
    {
        holdingItem = item;
    }
}