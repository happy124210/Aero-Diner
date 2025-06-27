using UnityEngine;


//플레이어가 재료를 들고, 내려놓는 기능을 담당하는 인벤토리
public class PlayerInventory : MonoBehaviour
{
    public Transform GetItemSlotTransform() => itemSlotTransform;
    [Header("아이템 슬롯 위치")]
    [SerializeField] private Transform itemSlotTransform;

    ///현재 들고 있는 재료
    public FoodDisplay heldItem;
    public bool IsHoldingItem => heldItem != null;

    //아이템을 들기 시도
    public void TryPickup(IInteractable target)
    {
        if (IsHoldingItem || target == null) return;

        // 재료만 집을 수 있음
        FoodDisplay food = target as FoodDisplay;
        if (food == null) return;

        heldItem = food;

        // 시각적으로 들고 있는 위치로 이동
        heldItem.transform.SetParent(itemSlotTransform);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;

        // 충돌 및 중력 제거
        var rb = heldItem.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        var col = heldItem.GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Debug.Log($"[Inventory] {heldItem.foodData.foodName} 획득");

        // 마지막에 스테이션 초기화 호출 (재료 오브젝트 파괴 방지)
        if (food.originShelf) { food.originShelf.OnPlayerPickup(); }

        if (food.originPassive) { food.originPassive.OnPlayerPickup(); }
        if (food.originAutomatic) {food.originAutomatic.OnPlayerPickup(); }
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
                if (station.PlaceIngredient(heldItem.foodData))
                {
                    Destroy(heldItem.gameObject);
                    heldItem = null;
                    Debug.Log($"[Inventory] 재료 일치 - 재료 소비됨");
                    placed = true;
                }
                else
                {
                    Debug.Log("[Inventory] 재료 불일치 - 내려놓을 수 없음");
                }
                break;

            case Shelf shelf:
                if (shelf.CanPlaceIngredient(heldItem.foodData))
                {
                    FoodData dataToPlace = heldItem.foodData;
                    Destroy(heldItem.gameObject); // 손에 들고 있는 아이템 제거
                    heldItem = null;

                    shelf.PlaceIngredient(dataToPlace); // 선반에 새로운 오브젝트 생성
                    Debug.Log("[Inventory] 선반에 재료 배치됨");
                    placed = true;
                }
                else
                {
                    Debug.Log("[Inventory] 선반에 재료를 배치할 수 없습니다.");
                }
                break;

            case Trashcan trashcan:
                if (trashcan.PlaceIngredient(heldItem.foodData))
                {
                    Destroy(heldItem.gameObject);
                    heldItem = null;
                    Debug.Log("[Inventory] 재료를 쓰레기통에 버렸습니다.");
                    placed = true;
                }
                else
                {
                    Debug.Log("쓰레기통이 아닐지도?");
                }
                break;
            case PassiveStation station:
                if (station.CanPlaceIngredient(heldItem.foodData))
                {
                    FoodData dataToPlace = heldItem.foodData;
                    Destroy(heldItem.gameObject);
                    heldItem = null;
                    station.PlaceIngredient(dataToPlace);
                    Debug.Log("[Inventory] PassiveStation에 재료 배치됨");
                    placed = true;
                }
                else
                {
                    Debug.Log("[Inventory] PassiveStation에 재료 배치 실패");
                }
                break;
                case AutomaticStation automatic:
                if(automatic.CanPlaceIngredient(heldItem.foodData))
                {
                    FoodData dataToPlace = heldItem.foodData;
                    Destroy(heldItem.gameObject);
                    heldItem = null;
                    automatic.PlaceIngredient(dataToPlace);
                    Debug.Log("[Inventory] AutoStation에 재료 배치됨");
                    placed = true;
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
        heldItem = item;
    }
}