using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 상호작용하면 재료를 생성해주는 스테이션
/// </summary>
public class IngredientStation : ItemSlotStation
{
    [Header("재료 데이터 그룹")]
    public IngredientSOGroup ingredientGroup;

    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient;

    [Header("재료 생성 위치")]
    public Transform spawnPoint;

    /// <summary>
    /// 플레이어가 J 키를 눌렀을 때 실행되는 상호작용 메서드
    /// </summary>
    public override void Interact(PlayerInventory playerInventory)
    {
        if (ingredientGroup == null || selectedIngredient == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return;
        }

        // 해당 위치에 "Ingredient" 태그를 가진 오브젝트가 있는지 확인
        Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPoint.position, 0.1f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Ingredient"))
            {
                Debug.Log("이미 재료가 생성되어 있습니다.");
                return;
            }
        }

        // GameObject 생성
        GameObject ingredientObj = new GameObject(selectedIngredient.foodName);
        ingredientObj.transform.position = spawnPoint.position;
        ingredientObj.tag = "Ingredient";

        // SpriteRenderer 추가
        SpriteRenderer spriteRenderer = ingredientObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 55;
        if (selectedIngredient.foodIcon != null)
        {
            spriteRenderer.sprite = selectedIngredient.foodIcon;
        }
        else
        {
            spriteRenderer.color = Color.gray;
        }

        // Collider2D 추가 및 설정
        CircleCollider2D collider = ingredientObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.1f;

        // Rigidbody2D 추가 및 설정
        Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        // FoodDisplay 설정
        FoodDisplay foodDisplay = ingredientObj.AddComponent<FoodDisplay>();
        foodDisplay.foodData = selectedIngredient;
    }

    public void PlaceIngredient(FoodData data)
    {
        if (data == null)
        {
            Debug.LogWarning("전달된 재료 데이터가 없습니다.");
            return;
        }

        if (data == selectedIngredient)
        {
            Debug.Log("재료가 일치합니다. 내려놓기 허용.");
            // 필요한 추가 동작을 여기에 구현 (예: 플레이어 인벤토리에서 제거 등)
        }

        else
        {
            Debug.Log("재료가 일치하지 않습니다. 내려놓기 불가.");
            // 효과음 또는 피드백 UI 등으로 알릴 수 있음
        }
    }


    public void OnHoverEnter()
    {

    }
    public void OnHoverExit()
    {

    }
}