using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

/// <summary>
/// 플레이어가 상호작용하면 재료를 생성해주는 스테이션
/// </summary>
public class IngredientStation : MonoBehaviour, IInteractable
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
    public void Interact(PlayerInventory playerInventory)
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
        ingredientObj.layer = 6;

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

    }
    public void OnHoverExit()
    {

    }
}