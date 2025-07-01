using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 플레이어가 상호작용하면 재료를 생성해주는 스테이션
/// </summary>
public class IngredientStation : MonoBehaviour, IInteractable
{
    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient;

    private OutlineShaderController outline;

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>();
    }

    /// <summary>
    /// 플레이어와 상호작용하면 재료 생성 + 즉시 인벤토리로 들어감
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (!selectedIngredient || !playerInventory)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return;
        }

        if (playerInventory.IsHoldingItem)
        {
            Debug.Log("플레이어가 이미 아이템을 들고 있음");
            return;
        }
        
        string displayName = selectedIngredient.foodName;
        Sprite displayIcon = selectedIngredient.foodIcon;

        // VisualObjectFactory로 시각 오브젝트 생성 (부모: 플레이어 손 슬롯)
        Transform slot = playerInventory.GetItemSlotTransform();
        GameObject pickupObj = VisualObjectFactory.CreateIngredientVisual(
            parent: slot,
            name: displayName,
            icon: displayIcon
        );
        if (!pickupObj)
        {
            Debug.LogError("비주얼 오브젝트 생성 실패");
            return;
        }

        // FoodDisplay 세팅
        var display = pickupObj.AddComponent<FoodDisplay>();
        display.foodData = selectedIngredient;

        // Collider / Rigidbody 비활성화
        var col = pickupObj.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        var rb = pickupObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // 인벤토리에 등록
        playerInventory.SetHeldItem(display);

        Debug.Log($"[{name}] {displayName} 생성 → 즉시 플레이어 손으로 이동");
    }

    public bool PlaceIngredient(FoodData data)
    {
        if (data == null || selectedIngredient == null)
        {
            Debug.Log("유효하지 않은 재료입니다.");
            return false;
        }

        if (data.id == selectedIngredient.id)
        {
            Debug.Log("재료 일치: 내려놓기 허용");
            return true;
        }
        else
        {
            Debug.Log("재료 불일치: 내려놓기 차단");
            return false;
        }
    }
    
    public void OnHoverEnter()
    {
        outline?.EnableOutline();
    }
    public void OnHoverExit()
    {
        outline?.DisableOutline();
    }
}