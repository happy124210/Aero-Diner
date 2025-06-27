using System.Collections.Generic;
using System.Linq;           // LINQ 사용을 위한 네임스페이스
using UnityEngine;
using TMPro;

/// <summary>
/// 플레이어가 상호작용하면 재료를 가공하여 가공된 재료를 생성하는 스테이션
/// 주요 기능:
/// - PlaceIngredient(): 재료 오브젝트를 생성하고, 플레이어가 내려놓은 재료의 데이터를 바탕으로
///   생성할 재료(selectedIngredient)와 가공 허용 재료 그룹(neededIngredients)을 동적으로 채움
/// - Interact(): J 키를 누르는 동안 조리 타이머가 감소하며, 타이머가 다 되면 가공 처리
/// - ProcessIngredient(): 조리가 완료되면 재료 오브젝트를 제거하고 결과 처리를 수행
/// </summary>
public class PassiveStation : MonoBehaviour, IInteractable, IPlaceableStation
{
    [Header("재료 데이터 그룹")]
    public PassiveSOGroup passiveGroup;

    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient;          // 플레이어가 내려놓은 재료 데이터 기반으로 갱신

    [Header("재료 생성 위치")]
    public Transform spawnPoint;

    [Header("가공 허용 재료 그룹")]
    public PassiveSOGroup neededIngredients;     // 플레이어가 내려놓은 재료를 기반으로 동적으로 채워짐

    [Header("가공 된 재료 SO")]
    public MenuData cookedIngredient;

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f;               // 전체 조리 시간

    [Header("조리 시간 표시용 UI 텍스트")]
    public TextMeshProUGUI cookingTimeText;      // 남은 조리 시간을 표시할 UI 요소

    [Header("스테이션 데이터")]
    public StationData stationData;              // 이 스테이션의 종류 및 지원 레시피 정보

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new List<string>();

    // 내부 상태 변수
    private float currentCookingTime;       // 남은 조리 시간
    private GameObject placedIngredientObj; // 화면에 표시되는 재료 오브젝트
    private FoodData currentFoodData;       // 현재 가공 대상 재료 데이터
    private MenuData currentMenuData;       // 가공 된 대상 재료 데이터

    private void Start()
    {
        // 조리 타이머 초기화 및 UI 갱신
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 플레이어가 재료를 스테이션에 내려놓을 때 호출됨
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

        // 데이터 갱신
        currentFoodData = data;

        // 플레이어가 내려놓은 재료 정보를 바탕으로 가공 결과를 동적으로 결정
        selectedIngredient = currentFoodData;

        // 시각적 재료 오브젝트 생성 및 배치
        placedIngredientObj = CreateIngredientDisplay(data);

        // ID 목록에 추가
        if (!currentIngredients.Contains(data.id))
            currentIngredients.Add(data.id);

        // neededIngredients 그룹에 추가
        if (neededIngredients != null &&
            (neededIngredients.GetCount() == 0 || !neededIngredients.Contains(currentFoodData)))
        {
            neededIngredients.AddIngredient(currentFoodData);
            Debug.Log($"가공 허용 재료 그룹에 '{currentFoodData.displayName}' 추가됨.");
        }

        // 가능한 레시피 선택
        MenuData selectedRecipe = RecipeManager.Instance.TrySetRecipe(
            stationData,
            currentIngredients,
            SetRecipe.Instance.selectedRecipes
        );

        if (selectedRecipe != null)
        {
            Debug.Log($"레시피 '{selectedRecipe.menuName}' 가능!");
            currentMenuData = selectedRecipe;
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
    /// 재료 디스플레이용 게임오브젝트 생성
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
        foodDisplay.originPassive = this;

        return ingredientObj;
    }

    /// <summary>
    /// J 키를 누르는 동안 호출
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        // 재료가 없거나 화면상의 오브젝트가 없으면 타이머 리셋 후 종료
        if (currentFoodData == null || placedIngredientObj == null)
        {
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            Debug.Log("재료가 없어 조리 타이머가 리셋되었습니다.");
            return;
        }

        if (neededIngredients != null &&
            neededIngredients.GetCount() > 0 &&
            !neededIngredients.Contains(currentFoodData))
        {
            Debug.Log("요구된 재료가 아님. 타이머 리셋됨.");
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            return;
        }

        if (interactionType == InteractionType.Use)
        {
            // 조리 타이머 감소 후 UI 업데이트
            currentCookingTime -= Time.deltaTime;
            UpdateCookingTimeText();

            // 타이머 종료 시 재료 가공 처리
            if (currentCookingTime <= 0f)
            {
                ProcessIngredient(currentMenuData);
                currentFoodData = null;
                currentCookingTime = cookingTime;
                currentIngredients.Clear();
            }
        }
        else
        {
            Debug.Log("[PassiveStation] InteractionType.Use가 아니므로 무시됩니다.");
        }
    }

    /// <summary>
    /// 조리 완료 시 호출
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

    //플레이어 인벤토리와 상호작용을 위한 체크함수
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
    /// 가공된 재료 디스플레이용 게임오브젝트 생성
    /// </summary>
    private GameObject CreateProcessedIngredientDisplay(MenuData data)
    {
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
        foodDisplay.originPassive = this;

        return ingredientObj;
    }

    /// <summary>
    /// 조리 타이머 UI 갱신
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
            cookingTimeText.text = currentCookingTime.ToString("F1");
    }

    /// <summary>
    /// 플레이어가 재료를 들 때 호출
    /// </summary>
    public void OnPlayerPickup()
    {
        if (placedIngredientObj != null)
        {
            placedIngredientObj = null;
        }

        currentFoodData = null;
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
        currentIngredients.Clear();

        Debug.Log("플레이어가 재료를 들었고, 스테이션이 초기화되었습니다.");
    }

    public void OnHoverEnter() 
    {

    }

    public void OnHoverExit() 
    {

    }
}