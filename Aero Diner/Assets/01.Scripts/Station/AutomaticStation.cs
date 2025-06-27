using System.Collections.Generic;
using System.Linq;               // LINQ 확장 메서드를 사용하기 위해
using UnityEngine;
using TMPro;
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
    public FoodData selectedIngredient;       // 플레이어가 내려놓은 재료 데이터 기반으로 갱신

    [Header("재료 생성 위치")]
    public Transform spawnPoint;

    [Header("가공 허용 재료 그룹")]
    public PassiveSOGroup neededIngredients;  // 플레이어가 내려놓은 재료를 기반으로 동적으로 채워짐

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f;            // 전체 조리 시간

    [Header("조리 시간 표시용 UI 텍스트")]
    public TextMeshProUGUI cookingTimeText;   // 남은 조리 시간을 표시할 UI 요소

    [Header("스테이션 데이터")]
    public StationData stationData;           // 이 스테이션의 종류 및 지원 레시피 정보

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new List<string>(); // 현재 올려진 재료 ID 목록

    // 내부 상태 변수
    private float currentCookingTime;       // 남은 조리 시간
    private GameObject placedIngredientObj; // 화면에 표시되는 재료 오브젝트
    private FoodData currentFoodData;       // 현재 가공 대상 재료 데이터
    private MenuData currentMenuData;       // 가공 된 대상 재료 데이터
    private bool isCooking = false;         // 현재 조리 중인지 여부
    public MenuData cookedIngredient;       // 조리 시작 시 TrySetRecipe() 결과 저장

    private OutlineShaderController outline;

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>();
    }


    private void Start()
    {
        // 타이머 초기화 및 UI 갱신
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    private void Update()
    {
        if (!isCooking || placedIngredientObj == null || currentFoodData == null)
            return;

        currentCookingTime -= Time.deltaTime;
        UpdateCookingTimeText();

        if (currentCookingTime <= 0f)
        {
            ProcessIngredient(currentMenuData);
            ResetStation();
        }
    }

    // 자동 스테이션은 플레이어 입력 없이 동작하므로 비워둠
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType) { }

    /// <summary>
    /// 플레이어가 재료를 내려놓았을 때 호출됨
    /// </summary>
    public void PlaceIngredient(FoodData data)
    {
        if (currentFoodData != null)
        {
            Debug.Log("이미 재료가 배치되어 있습니다.");
            return;
        }

        if (neededIngredients != null && !neededIngredients.Contains(data))
        {
            Debug.Log("제공된 재료가 요구되는 그룹에 속하지 않습니다.");
            return;
        }

        // 1) 데이터 갱신
        currentFoodData = data;
        selectedIngredient = data;
        placedIngredientObj = CreateIngredientDisplay(data);

        // 2) ID 목록에 추가
        if (!currentIngredients.Contains(data.id))
            currentIngredients.Add(data.id);

        // 3) neededIngredients 그룹에 추가
        if (neededIngredients != null &&
            (neededIngredients.GetCount() == 0 || !neededIngredients.Contains(data)))
        {
            neededIngredients.AddIngredient(data);
            Debug.Log($"가공 허용 재료 그룹에 '{data.displayName}' 추가됨.");
        }

        // 4) RecipeManager로 레시피 결정
        currentMenuData = RecipeManager.Instance.TrySetRecipe(
            stationData,
            currentIngredients,
            SetRecipe.Instance.selectedRecipes
        );

        if (currentMenuData != null)
            Debug.Log($"레시피 '{currentMenuData.menuName}' 가능!");
        else
            Debug.Log("조건에 맞는 레시피 없음");

        // 5) 타이머 시작
        currentCookingTime = cookingTime;
        isCooking = currentMenuData != null;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 화면에 보여질 재료 오브젝트 생성
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
        foodDisplay.originAutomatic = this;

        return ingredientObj;
    }

    /// <summary>
    /// 조리 완료 시 호출되어, 결과물 생성
    /// </summary>
    private void ProcessIngredient(MenuData data)
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        if (data == null)
        {
            Debug.LogWarning("가공된 레시피 데이터가 없습니다.");
            return;
        }

        Debug.Log("가공 완료된 재료 생성됨: " + data.menuName);
        CreateProcessedIngredientDisplay(data);
    }

    /// <summary>
    /// 플레이어 인벤토리로부터 재료를 놓을 수 있는지 검사
    /// </summary>
    public bool CanPlaceIngredient(FoodData data)
    {
        if (currentFoodData != null)
        {
            Debug.Log("[Shelf] 현재 선반에 이미 재료가 배치되어 있어 추가할 수 없습니다.");
            return false;
        }

        //허용 목록이 비어있거나 포함되어 있으면 배치 허용
        if (neededIngredients == null || neededIngredients.GetCount() == 0 || neededIngredients.Contains(data))
            return true;

        return false;
    }

    /// <summary>
    /// 최종 결과물 오브젝트 생성
    /// </summary>
    private GameObject CreateProcessedIngredientDisplay(MenuData data)
    {
        if (passiveGroup == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        // 필수 데이터 누락 확인
        if (passiveGroup == null || selectedIngredient == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        GameObject ingredientObj = new GameObject(data.menuName);
        ingredientObj.transform.SetParent(transform); // 스테이션의 자식으로 배치
        ingredientObj.transform.localPosition = Vector3.zero;
        ingredientObj.tag = "Ingredient";
        ingredientObj.layer = 6;

        // SpriteRenderer 추가하여 processedIcon 적용 및 sortingOrder 55로 설정
        SpriteRenderer spriteRenderer = ingredientObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 55;
        if (cookedIngredient.menuIcon != null)
            spriteRenderer.sprite = cookedIngredient.menuIcon;
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
        foodDisplay.originAutomatic = this;

        return ingredientObj;
    }

    /// <summary>
    /// 스테이션을 초기 상태로 리셋
    /// </summary>
    private void ResetStation()
    {
        isCooking = false;
        currentFoodData = null;
        currentMenuData = null;
        currentCookingTime = cookingTime;
        currentIngredients.Clear();
        UpdateCookingTimeText();
    }

    /// <summary>
    /// UI 텍스트에 남은 시간 반영
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
            cookingTimeText.text = currentCookingTime.ToString("F1");
    }

    /// <summary>
    /// 플레이어가 제품을 들어가질 때 스테이션 초기화
    /// </summary>
    public void OnPlayerPickup()
    {
        if (placedIngredientObj != null)
        {
            placedIngredientObj = null;
        }

        ResetStation();
        Debug.Log("플레이어가 재료를 들었고, 스테이션이 초기화되었습니다.");
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