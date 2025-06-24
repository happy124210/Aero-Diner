using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// 플레이어가 상호작용하면 재료를 가공하여 가공된 재료를 생성하는 스테이션
/// 주요 기능:
/// - PlaceIngredient(): 재료 오브젝트를 생성하고, 플레이어가 내려놓은 재료의 데이터를 바탕으로
///   생성할 재료(selectedIngredient)와 가공 허용 재료 그룹(NeededIngredients)을 동적으로 채움
/// - Interact(): J 키를 누르는 동안 조리 타이머가 감소하며, 타이머가 다 되면 가공 처리
/// - ProcessIngredient(): 조리가 완료되면 재료 오브젝트를 제거하고 결과 처리를 수행
/// </summary>
public class PassiveStation : MonoBehaviour, IInteractable, IPlaceableStation
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

    private void Start()
    {
        // 조리 타이머 초기화 및 UI 갱신
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 플레이어가 재료를 스테이션에 내려놓을 때 호출됨
    /// 재료 오브젝트를 생성하고, 플레이어가 내려놓은 재료의 데이터를 기반으로
    /// selectedIngredient를 동적으로 할당하며, NeededIngredients 그룹에도 추가
    /// RecipeManager의 TrySetRecipe()를 통해 가능한 레시피(MenuData)를 판단
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

        // 재료 ID를 목록에 추가
        if (!currentIngredients.Contains(data.id))
            currentIngredients.Add(data.id);

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
        sr.sprite = data.foodIcon ?? null;
        if (data.foodIcon == null)
            sr.color = Color.gray;

        // 충돌 감지를 위한 Collider와 Rigidbody2D 추가
        ingredientObj.AddComponent<BoxCollider2D>();
        Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        return ingredientObj;
    }

    /// <summary>
    /// 플레이어가 J 키를 누르고 있는 동안 호출된다.
    /// 조리 타이머를 감소시키면서, 타이머가 0 이하가 되면 재료를 가공 처리
    /// </summary>
    public void Interact(PlayerInventory playerInventory)
    {
        // 재료가 없거나 화면상의 오브젝트가 없으면 타이머 리셋 후 종료
        if (currentFoodData == null || placedIngredientObj == null)
        {
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            Debug.Log("재료가 없어 조리 타이머가 리셋되었습니다.");
            return;
        }

        // 동적 업데이트 검사:
        // 만약 NeededIngredients 그룹이 비어있지 않고 현재 재료가 포함되지 않았다면 타이머 리셋
        if (NeededIngredients != null && NeededIngredients.GetCount() > 0 && !NeededIngredients.Contains(currentFoodData))
        {
            Debug.Log("요구된 재료가 아님. 타이머 리셋됨.");
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            return;
        }

        // 조리 타이머 감소 후 UI 업데이트
        currentCookingTime -= Time.deltaTime;
        UpdateCookingTimeText();

        // 타이머 종료 시 재료 가공 처리
        if (currentCookingTime <= 0f)
        {
            ProcessIngredient(currentFoodData);
            currentFoodData = null;
            currentCookingTime = cookingTime; // 타이머 리셋
        }
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
        GameObject processedObj = CreateProcessedIngredientDisplay();
    }

    /// <summary>
    /// 가공된 재료 오브젝트를 생성하는 함수
    /// 플레이어가 내려놓은 재료 데이터(selectedIngredient)를 사용
    /// </summary>
    private GameObject CreateProcessedIngredientDisplay()
    {
        // 필수 데이터 누락 확인
        if (passiveGroup == null || selectedIngredient == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        // 새 GameObject 생성, 이름은 FoodData에 정의된 foodName으로 지정
        GameObject ingredientObj = new GameObject(selectedIngredient.foodName);
        ingredientObj.transform.position = spawnPoint.position;

        // SpriteRenderer 추가하여 processedIcon 적용 및 sortingOrder 55로 설정
        SpriteRenderer spriteRenderer = ingredientObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 55;
        if (selectedIngredient.processedIcon != null)
            spriteRenderer.sprite = selectedIngredient.processedIcon;
        else
            spriteRenderer.color = Color.gray;

        return ingredientObj;
    }

    /// <summary>
    /// 조리 타이머 UI를 갱신
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
            cookingTimeText.text = currentCookingTime.ToString("F1");
    }

    /// <summary>
    /// 플레이어가 재료를 들 때 호출됨.
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