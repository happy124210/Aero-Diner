using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어가 올바른 재료를 스테이션에 배치하면
/// 자동으로 조리 타이머가 시작되고, 시간이 다 되면 결과물이 생성되는 자동 조리 스테이션
/// </summary>
public class AutomaticStation : MonoBehaviour, IInteractable, IPlaceableStation
{
    [Header("재료 데이터 그룹")]
    public PassiveSOGroup passiveGroup;

    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient; // 플레이어가 내려놓은 재료 데이터 기반으로 갱신

    [Header("재료 생성 위치")]
    public Transform spawnPoint;

    [Header("가공 허용 재료 그룹")]
    public PassiveSOGroup NeededIngredients; // 플레이어가 내려놓은 재료를 기반으로 동적으로 채워짐

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f; // 전체 조리 시간

    [Header("조리 시간 표시용 UI 텍스트")]
    public TextMeshProUGUI cookingTimeText; // 남은 조리 시간을 표시할 UI 요소

    [Header("스테이션 데이터")]
    public StationData stationData; // 이 스테이션의 종류 및 지원 레시피 정보

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new List<string>(); // 현재 올려진 재료 ID 목록


    // 내부 상태 변수
    private float currentCookingTime;       // 남은 조리 시간
    private GameObject placedIngredientObj; // 화면에 표시되는 재료 오브젝트
    private FoodData currentFoodData;       // 현재 가공 대상 재료 데이터
    private bool isCooking = false;         // 현재 조리 중인지 여부

    private void Start()
    {
        // 타이머 초기화 및 UI 갱신
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    private void Update()
    {
        // 조리 중이며 재료가 올려져 있을 때만 타이머 진행
        if (isCooking && placedIngredientObj != null && currentFoodData != null)
        {
            // 남은 시간 감소
            currentCookingTime -= Time.deltaTime;
            UpdateCookingTimeText();

            // 조리 완료 시 가공 처리 후 스테이션 리셋
            if (currentCookingTime <= 0f)
            {
                ProcessIngredient(currentFoodData);
                ResetStation();
            }
        }
    }

    public void Interact(PlayerInventory playerInventory)
    {

    }

    /// <summary>
    /// 플레이어가 재료를 내려놓았을 때 호출됨
    /// 재료 오브젝트를 생성하고 조리를 시작함
    /// </summary>
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

        // 스테이션에서 가능한 레시피를 RecipeManager를 통해 검사
        MenuData selectedRecipe = RecipeManager.Instance.TrySetRecipe(
            stationData,
            currentIngredients,
            SetRecipe.Instance.selectedRecipes
        );

        if (selectedRecipe != null)
            Debug.Log($"레시피 '{selectedRecipe.menuName}' 가능!");
        else
            Debug.Log("조건에 맞는 레시피 없음");

        // 조리 타이머 초기화 및 UI 업데이트
        currentCookingTime = cookingTime;
        isCooking = true;
        UpdateCookingTimeText();

        ProcessIngredient(data);
    }

    /// <summary>
    /// 재료 데이터를 기반으로 화면에 보여질 재료 오브젝트를 생성
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        // 필수 데이터 누락 확인
        if (passiveGroup == null || selectedIngredient == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        GameObject ingredientObj = new GameObject(data.foodName);
        ingredientObj.transform.SetParent(transform); // 스테이션의 자식으로 배치
        ingredientObj.transform.localPosition = Vector3.zero;
        ingredientObj.tag = "Ingredient";

        // SpriteRenderer 추가하여 processedIcon 적용 및 sortingOrder 55로 설정
        SpriteRenderer spriteRenderer = ingredientObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 55;
        if (selectedIngredient.foodIcon != null)
            spriteRenderer.sprite = selectedIngredient.foodIcon;
        else
            spriteRenderer.color = Color.gray;

        // 충돌 감지를 위한 Collider와 Rigidbody2D 추가
        ingredientObj.AddComponent<BoxCollider2D>();
        Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        FoodDisplay foodDisplay = ingredientObj.AddComponent<FoodDisplay>();
        foodDisplay.foodData = selectedIngredient;

        return ingredientObj;
    }

    /// <summary>
    /// 조리 완료 시 호출되어, 재료 오브젝트를 제거하고 결과 처리를 진행
    /// </summary>
    private void ProcessIngredient(FoodData data)
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        Debug.Log("가공 완료된 재료 생성됨: " + data.displayName);

        // 결과물 생성: selectedIngredient(동적으로 할당된) 기반으로 처리
        GameObject processedObj = CreateProcessedIngredientDisplay(data);
    }

    /// <summary>
    /// 가공된 재료 오브젝트를 생성하는 함수
    /// 플레이어가 내려놓은 재료 데이터(selectedIngredient)를 사용
    /// </summary>
    private GameObject CreateProcessedIngredientDisplay(FoodData data)
    {
        // 필수 데이터 누락 확인
        if (passiveGroup == null || selectedIngredient == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        GameObject ingredientObj = new GameObject(data.foodName);
        ingredientObj.transform.SetParent(transform); // 스테이션의 자식으로 배치
        ingredientObj.transform.localPosition = Vector3.zero;
        ingredientObj.tag = "Ingredient";

        // SpriteRenderer 추가하여 processedIcon 적용 및 sortingOrder 55로 설정
        SpriteRenderer spriteRenderer = ingredientObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 55;
        if (selectedIngredient.processedIcon != null)
            spriteRenderer.sprite = selectedIngredient.processedIcon;
        else
            spriteRenderer.color = Color.gray;

        FoodDisplay foodDisplay = ingredientObj.AddComponent<FoodDisplay>();
        foodDisplay.foodData = selectedIngredient;

        return ingredientObj;
    }

    /// <summary>
    /// 조리 완료 또는 리셋 조건일 때 스테이션 상태를 초기화
    /// </summary>
    private void ResetStation()
    {
        isCooking = false;
        currentFoodData = null;
        currentCookingTime = cookingTime;
        currentIngredients.Clear(); // 재료 목록도 초기화
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 남은 조리 시간을 UI에 반영
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
            cookingTimeText.text = currentCookingTime.ToString("F1");
    }

    /// <summary>
    /// 플레이어가 재료를 들었을 때 스테이션 상태를 초기화
    /// </summary>
    public void OnPlayerPickup()
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        currentFoodData = null;
        isCooking = false;
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();

        Debug.Log("플레이어가 물체를 들었습니다. 스테이션 상태 초기화됨.");
    }

    public void OnHoverEnter() { }
    public void OnHoverExit() { }
}