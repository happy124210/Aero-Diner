using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어가 상호작용하면 재료를 가공하여 가공된 재료를 생성하는 스테이션
/// 
/// 주요 기능:
/// - PlaceIngredient(): 재료 오브젝트를 생성하고,  
///   현재 등록된 재료 목록을 기반으로 RecipeManager와 SetRecipe의 레시피 리스트에서  
///   조건에 맞는 레시피를 판단한다.
/// - Interact(): J 키를 누르는 동안 조리 타이머가 감소하며, 타이머가 다 되면 가공 처리.
/// - ProcessIngredient(): 조리가 완료되면 재료 오브젝트를 제거하고 결과 처리를 수행.
/// </summary>
public class PassiveStation : MonoBehaviour, IInteractable
{
    [Header("가공 허용 재료 그룹")]
    public IngredientSOGroup NeededIngredients; // 이 스테이션에서 가공할 수 있는 재료 그룹

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f; // 전체 조리 시간

    [Header("조리 시간 표시용 UI 텍스트")]
    public Text cookingTimeText; // 남은 조리 시간을 표시할 UI 요소

    [Header("스테이션 타입")]
    public CookingStation stationType; // 이 스테이션의 조리 종류

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new List<string>(); // 현재 올려진 재료 ID 목록

    // 내부 상태 변수
    private float currentCookingTime;        // 남은 조리 시간
    private GameObject placedIngredientObj;  // 화면에 표시되는 재료 오브젝트
    private FoodData currentFoodData;        // 현재 가공 대상 재료 데이터

    private void Start()
    {
        // 조리 타이머 초기화 및 UI 갱신
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 플레이어가 재료를 스테이션에 올려놓으면 호출
    /// 재료 오브젝트를 생성하고,  
    /// 현재 등록된 재료 목록에 추가한 후, 해당 목록을 바탕으로 
    /// RecipeManager의 TrySetRecipe()를 통해 가능한 레시피를 판단
    /// </summary>
    public void PlaceIngredient(FoodData data)
    {
        currentFoodData = data;

        // 시각적 재료 오브젝트 생성 및 스테이션에 배치
        placedIngredientObj = CreateIngredientDisplay(data);

        // 재료 ID를 목록에 추가 (중복 등록 방지)
        if (!currentIngredients.Contains(data.id))
            currentIngredients.Add(data.id);

        // SetRecipe의 선택된 레시피 리스트를 넘겨 RecipeManager가 가능한 레시피를 판단하게 함
        RecipeData selectedRecipe = RecipeManager.Instance.TrySetRecipe(
            stationType,
            currentIngredients,
            SetRecipe.Instance.selectedRecipes
        );
        if (selectedRecipe != null)
        {
            Debug.Log($"레시피 '{selectedRecipe.recipeName}' 가능!");
        }
        else
        {
            Debug.Log("조건에 맞는 레시피 없음");
        }

        // 조리 타이머 초기화 및 UI 업데이트
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 현재 가공 중인 재료 데이터를 바탕으로 시각적 재료 오브젝트를 생성
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        GameObject ingredientObj = new GameObject(data.displayName);
        ingredientObj.transform.SetParent(transform); // 스테이션의 자식으로 배치
        ingredientObj.transform.localPosition = Vector3.zero;

        // SpriteRenderer를 추가하여 아이콘 표시 
        SpriteRenderer sr = ingredientObj.AddComponent<SpriteRenderer>();
        sr.sprite = data.icon ?? null;
        if (data.icon == null)
            sr.color = Color.gray;

        // 충돌 감지를 위한 콜라이더 및 Rigidbody2D 추가
        ingredientObj.AddComponent<BoxCollider2D>();
        Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        return ingredientObj;
    }

    /// <summary>
    /// 플레이어가 J 키를 누르고 있는 동안 호출됨
    /// 조리 타이머를 감소시키며, 타이머가 다 되면 가공 처리하는 함수
    /// </summary>
    public void Interact(PlayerInventory playerInventory)
    {
        // 재료가 없거나 시각 오브젝트가 없으면 타이머 초기화 후 종료
        if (currentFoodData == null || placedIngredientObj == null)
        {
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            Debug.Log("재료가 없어 조리 타이머가 리셋되었습니다.");
            return;
        }

        // 제공된 재료가 이 스테이션에서 가공 가능한 재료 그룹에 속하는지 확인
        if (NeededIngredients != null && !NeededIngredients.Contains(currentFoodData))
        {
            Debug.Log("요구된 재료가 아님. 타이머 리셋됨.");
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            return;
        }

        // 조리 타이머 감소 및 UI 업데이트
        currentCookingTime -= Time.deltaTime;
        UpdateCookingTimeText();

        // 타이머가 다 되면 가공 처리
        if (currentCookingTime <= 0f)
        {
            ProcessIngredient(currentFoodData);
            currentFoodData = null;
            // 타이머 리셋 
            currentCookingTime = cookingTime;
        }
    }

    /// <summary>
    /// 조리 완료 시 호출됨
    /// 재료 오브젝트를 제거하고 결과 처리를 진행
    /// </summary>
    private void ProcessIngredient(FoodData data)
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        Debug.Log("가공 완료된 재료 생성됨: " + data.displayName);
        //CreateprocessedDisplay(data);
    }

    //private GameObject CreateprocessedDisplay(FoodData data)
    //{
    //    GameObject ingredientObj = new GameObject(data.displayName);
    //    ingredientObj.transform.SetParent(transform); // 스테이션의 자식으로 배치
    //    ingredientObj.transform.localPosition = Vector3.zero;

    //    // SpriteRenderer를 추가하여 아이콘 표시 
    //    SpriteRenderer sr = ingredientObj.AddComponent<SpriteRenderer>();
    //    sr.sprite = data.processedIcon ?? null;
    //    if (data.processedIcon == null)
    //        sr.color = Color.gray;
    //        sr.color = Color.gray;

    //    // 충돌 감지를 위한 콜라이더 및 Rigidbody2D 추가
    //    ingredientObj.AddComponent<BoxCollider2D>();
    //    Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
    //    rb.bodyType = RigidbodyType2D.Kinematic;
    //    rb.gravityScale = 0f;

    //    return ingredientObj;
    //}

    /// <summary>
    /// 조리 타이머 UI를 업데이트한다. (소수점 첫째 자리까지 표시)
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
            cookingTimeText.text = currentCookingTime.ToString("F1");
    }

    /// <summary>
    /// 플레이어가 재료를 들었을 때 호출됨.
    /// 스테이션에 남아있는 재료 오브젝트들을 제거하고 상태를 초기화
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

    public void OnHoverEnter()  
    {

    }
    public void OnHoverExit()   

    {

    } 
}