using UnityEngine;


//플레이어가 재료를 들고, 내려놓는 기능을 담당하는 인벤토리
public class PlayerInventory : MonoBehaviour
{
    [Header("아이템 슬롯 위치")]
    [SerializeField] private Transform itemSlotTransform;

    ///현재 들고 있는 재료
    private FoodDisplay heldItem;
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
        if (rb != null) rb.simulated = false;

        var col = heldItem.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log($"[Inventory] {heldItem.foodData.foodName} 획득");
    }

    //아이템을 내려놓기 시도
    public void DropItem(IInteractable target)
    {
        if (!IsHoldingItem) return;

        bool placed = false;

        // 스테이션이라면 내려놓기 허용 여부 확인
        if (target is IngredientStation station)
        {
            placed = station.PlaceIngredient(heldItem.foodData);

            if (placed)
            {
                heldItem.transform.SetParent(null);
                heldItem.transform.position = station.spawnPoint.position;

                var rb = heldItem.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = true;

                var col = heldItem.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;

                Debug.Log($"[Inventory] {heldItem.foodData.foodName} 내려놓음");
                heldItem = null;
            }
        }

        if (!placed)
        {
            Debug.Log("[Inventory] 이 위치에는 내려놓을 수 없습니다.");
        }
    }
}
