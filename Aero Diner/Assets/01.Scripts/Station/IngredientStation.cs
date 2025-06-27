using UnityEngine;

/// <summary>
/// 플레이어가 상호작용하면 재료를 생성해주는 스테이션
/// </summary>
public class IngredientStation : MonoBehaviour, IInteractable
{
    [Header("재료 데이터 그룹")]
    public IngredientSOGroup ingredientGroup;

    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient;

    /// <summary>
    /// 플레이어와 상호작용하면 재료 생성 + 즉시 인벤토리로 들어감
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (ingredientGroup == null || selectedIngredient == null || playerInventory == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return;
        }

        if (playerInventory.IsHoldingItem)
        {
            Debug.Log("플레이어가 이미 아이템을 들고 있음");
            return;
        }

        // GameObject 생성
        GameObject ingredientObj = new GameObject(selectedIngredient.foodName);
        ingredientObj.tag = "Ingredient";
        ingredientObj.layer = 6;

        // SpriteRenderer 추가
        SpriteRenderer spriteRenderer = ingredientObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 55;
        spriteRenderer.sprite = selectedIngredient.foodIcon != null ? selectedIngredient.foodIcon : null;
        if (spriteRenderer.sprite == null)
            spriteRenderer.color = Color.gray;

        // Collider2D 추가 및 설정
        CircleCollider2D collider = ingredientObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.7f;

        // Rigidbody2D 추가 및 설정
        Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        // FoodDisplay 설정
        FoodDisplay foodDisplay = ingredientObj.AddComponent<FoodDisplay>();
        foodDisplay.foodData = selectedIngredient;

        // new! 플레이어 인벤토리에 바로 등록 
        Transform slot = playerInventory.GetItemSlotTransform();
        ingredientObj.transform.SetParent(slot);
        ingredientObj.transform.localPosition = Vector3.zero;
        ingredientObj.transform.localRotation = Quaternion.identity;

        rb.simulated = false;
        collider.enabled = false;

        playerInventory.SetHeldItem(foodDisplay); // heldItem에 등록

        Debug.Log($"[{name}] {selectedIngredient.foodName} 생성 → 즉시 플레이어 손으로 이동");
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

        //플레이어가 호출 할 때 참고용 코드
        // bool canPlace = station.PlaceIngredient(playerHoldingData);
        //if (canPlace)
        //{
        //    playerInventory.DropItem();
        //}
        //else
        //{
        //    Debug.Log("이 장소에는 해당 재료를 놓을 수 없습니다!");
        //}
    
    public void OnHoverEnter()
    {

    }
    public void OnHoverExit()
    {

    }
}