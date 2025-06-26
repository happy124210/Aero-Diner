using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour, IInteractable, IPlaceableStation
{
    [Header("재료 데이터 그룹")]
    public ShelfSOGroup shelfGroup;

    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient; // 플레이어가 내려놓은 재료 데이터 기반으로 갱신

    [Header("재료 생성 위치")]
    public Transform spawnPoint;

    [Header("가공 허용 재료 그룹")]
    public ShelfSOGroup NeededIngredients; // 플레이어가 내려놓은 재료를 기반으로 동적으로 채워짐

    // 내부 상태 변수
    private GameObject placedIngredientObj; // 화면에 표시되는 재료 오브젝트
    private FoodData currentFoodData;       // 현재 가공 대상 재료 데이터

    public void Interact(PlayerInventory playerInventory)
    {

    }


    public void PlaceIngredient(FoodData data)
    {
        if (currentFoodData != null)
        {
            Debug.Log("이미 재료가 배치되어 있습니다.");
            return;
        }

        if (NeededIngredients != null && !NeededIngredients.Contains(data))
        {
            Debug.Log("제공된 재료가 요구되는 그룹에 속하지 않습니다.");
            return;
        }

        // 현재 재료 데이터 갱신
        currentFoodData = data;

        // 플레이어가 내려놓은 재료 정보를 바탕으로 가공 결과를 동적으로 결정
        selectedIngredient = currentFoodData;

        // 시각적 재료 오브젝트 생성 및 배치
        placedIngredientObj = CreateIngredientDisplay(data);

        // 만약 가공 허용 재료 그룹이 비어있거나 현재 재료가 포함되지 않은 경우 추가
        if (NeededIngredients != null && (NeededIngredients.GetCount() == 0 || !NeededIngredients.Contains(currentFoodData)))
        {
            NeededIngredients.AddIngredient(currentFoodData);
            Debug.Log($"가공 허용 재료 그룹에 '{currentFoodData.displayName}' 추가됨.");
        }
    }
    //플레이어 인벤토리와 상호작용을 위한 체크함수.
    public bool CanPlaceIngredient(FoodData data)
    {
        if (currentFoodData)
        {
            Debug.Log("[Shelf] 현재 선반에 이미 재료가 배치되어 있어 추가할 수 없습니다.");
            return false;
        }

        if (NeededIngredients && !NeededIngredients.Contains(data))
        {
            Debug.Log($"[Shelf] '{data.displayName}'는 요구되는 재료 그룹에 포함되어 있지 않아 배치할 수 없습니다.");
            return false;
        }

        Debug.Log($"[Shelf] '{data.displayName}' 배치 가능");
        return true;
    }
    /// <summary>
    /// 현재 가공 중인 재료 데이터를 바탕으로 시각적 재료 오브젝트를 생성
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        // 필수 데이터 누락 확인
        if (shelfGroup == null || selectedIngredient == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        GameObject ingredientObj = new GameObject(data.foodName);
        ingredientObj.transform.SetParent(transform); // 스테이션의 자식으로 배치
        ingredientObj.transform.localPosition = Vector3.zero;
        ingredientObj.tag = "Ingredient";
        ingredientObj.layer = 6;

        // SpriteRenderer 추가하여 processedIcon 적용 및 sortingOrder 55로 설정
        SpriteRenderer spriteRenderer = ingredientObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 55;
        if (selectedIngredient.foodIcon != null)
            spriteRenderer.sprite = selectedIngredient.foodIcon;
        else
            spriteRenderer.color = Color.gray;

        // 충돌 감지를 위한 Collider와 Rigidbody2D 추가
        CircleCollider2D collider = ingredientObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.7f;
        Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        FoodDisplay foodDisplay = ingredientObj.AddComponent<FoodDisplay>();
        foodDisplay.foodData = selectedIngredient;
        foodDisplay.originShelf = this;

        return ingredientObj;
    }

    /// <summary>
    /// 플레이어가 재료를 들 때 호출됨.
    /// 스테이션에 남아있는 재료 오브젝트들을 제거하고 상태를 초기화
    /// </summary>
    public void OnPlayerPickup()
    {
        if (placedIngredientObj != null)
        {
            placedIngredientObj = null;
        }

        currentFoodData = null;

        Debug.Log("플레이어가 재료를 들었고, 스테이션이 초기화되었습니다.");
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
