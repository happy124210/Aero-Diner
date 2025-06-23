using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어가 상호작용하면 재료를 가공하여 가공된 재료를 생성하는 스테이션
/// 
/// 주요 기능:
/// - PlaceIngredient(): 재료 오브젝트 생성 + 조리 가능한 레시피 판단
/// - Interact(): J 키를 누르면 cookingTime이 감소하며 조리가 진행됨
/// - ProcessIngredient(): 조리가 완료되면 가공된 재료 생성 및 기존 재료 제거
/// </summary>
public class PassiveStation : MonoBehaviour, IInteractable
{
    [Header("가공 허용 재료 그룹")]
    public IngredientSOGroup NeededIngredients; // 이 스테이션에서 가공할 수 있는 재료 그룹

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f; // 전체 조리 시간 (초)

    [Header("조리 시간 표시용 UI 텍스트")]
    public Text cookingTimeText; // 남은 조리 시간을 표시할 UI

    [Header("스테이션 타입")]
    public CookingStation stationType; // 이 스테이션의 조리 종류

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new List<string>(); // 현재 올려진 재료 ID 목록

    // 내부 상태
    private float currentCookingTime;
    private GameObject placedIngredientObj;
    private FoodData currentFoodData;

    private void Start()
    {
        // 조리 타이머 초기화
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 재료를 올려놓았을 때 호출됨
    /// 시각 오브젝트 생성 및 가능한 레시피 판별
    /// </summary>
    public void PlaceIngredient(FoodData data)
    {
        currentFoodData = data;

        // 시각적 재료 오브젝트 생성
        placedIngredientObj = CreateIngredientDisplay(data);

        // 재료 ID 등록
        if (!currentIngredients.Contains(data.id))
            currentIngredients.Add(data.id);

        // 가능한 레시피 판단 (싱글톤 매니저 사용)
        RecipeData selectedRecipe = RecipeManager.Instance.TrySetRecipe(stationType, currentIngredients);
        if (selectedRecipe != null)
        {
            Debug.Log($"레시피 '{selectedRecipe.recipeName}' 가능!");
            // TODO: 애니메이션, 이펙트 등 연동
        }
        else
        {
            Debug.Log("조건에 맞는 레시피 없음");
        }

        // 조리 타이머 리셋
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 시각적으로 스테이션 위에 재료 오브젝트를 생성
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        GameObject ingredientObj = new GameObject(data.displayName);
        ingredientObj.transform.SetParent(transform);
        ingredientObj.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = ingredientObj.AddComponent<SpriteRenderer>();
        sr.sprite = data.icon ?? null;
        if (data.icon == null)
            sr.color = Color.gray;

        ingredientObj.AddComponent<BoxCollider2D>();
        Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        return ingredientObj;
    }

    /// <summary>
    /// 플레이어가 J 키를 누르고 있는 동안 호출됨
    /// 조리 시간이 줄어들고 완료되면 가공 처리
    /// </summary>
    public void Interact(PlayerInventory playerInventory)
    {
        if (currentFoodData == null || placedIngredientObj == null)
        {
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            Debug.Log("재료가 없어 조리 타이머가 리셋되었습니다.");
            return;
        }

        if (NeededIngredients != null && !NeededIngredients.Contains(currentFoodData))
        {
            Debug.Log("요구된 재료가 아님. 타이머 리셋됨.");
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            return;
        }

        currentCookingTime -= Time.deltaTime;
        UpdateCookingTimeText();

        if (currentCookingTime <= 0f)
        {
            ProcessIngredient(currentFoodData);
            currentFoodData = null;
            currentCookingTime = cookingTime;
        }
    }

    /// <summary>
    /// 조리 완료 시 재료 오브젝트 제거 및 로그 출력
    /// </summary>
    private void ProcessIngredient(FoodData data)
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        Debug.Log("가공 완료된 재료 생성됨: " + data.displayName);
        // TODO: 결과 아이템 생성 로직 연결
    }

    /// <summary>
    /// cookingTimeText UI 갱신
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
            cookingTimeText.text = currentCookingTime.ToString("F1");
    }

    /// <summary>
    /// 플레이어가 재료를 들었을 때 호출됨. 상태 초기화
    /// </summary>
    public void OnPlayerPickup()
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        currentFoodData = null;
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();

        Debug.Log("플레이어가 재료를 들었고, 스테이션이 초기화되었습니다.");
    }

    public void OnHoverEnter() { }
    public void OnHoverExit() { }
}