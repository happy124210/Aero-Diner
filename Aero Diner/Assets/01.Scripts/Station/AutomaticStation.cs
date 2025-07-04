using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/// <summary>
/// 자동 조리 스테이션: 재료를 놓으면 자동으로 조리되고, 완료 시 결과물이 생성됨
/// </summary>
public class AutomaticStation : MonoBehaviour, IInteractable, IPlaceableStation
{
    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient;

    [Header("재료 생성 위치")]
    public Transform spawnPoint;

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f;

    [Header("조리 시간 표시용 UI 텍스트")]
    public TextMeshProUGUI cookingTimeText;

    [Header("스테이션 데이터")]
    public StationData stationData;

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new();

    [Header("아이콘 디스플레이")] // 추가됨
    [SerializeField] private FoodSlotIconDisplay iconDisplay;

    [SerializeField] private float slotSpacing = 0.5f;

    private List<FoodData> placedIngredientList = new();             // 실제 등록된 재료의 데이터 목록
    private List<GameObject> placedIngredients = new();              // 화면에 보여지는 재료 오브젝트들
    private List<FoodData> availableMatchedRecipes = new();          // 현재 조건에서 가능한 레시피 리스트
    private float currentCookingTime;                                // 남은 조리 시간
    private FoodData cookedIngredient;                               // 조리 완료 시 결과가 되는 레시피
    private bool isCooking = false;                                  // 현재 조리 중인지 여부 플래그
    private OutlineShaderController outline;                         // 외곽선 효과를 제어하는 컴포넌트
    private bool hasInitialized = false;                             // 아이콘 초기화 플래그

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>();
    }

    private void Start()
    {
        if (stationData != null && stationData.slotDisplays != null)
        {
            var types = stationData.slotDisplays.ConvertAll(s => s.foodType);
            iconDisplay.Initialize(stationData.slotDisplays); // 슬롯 전체 전달
            iconDisplay.ResetAll();
            hasInitialized = true;
        }
        else
        {
            Debug.LogWarning("stationData 또는 slotDisplays가 할당되지 않았습니다.");
        }

        ResetCookingTimer();
    }

    private void Update()
    {
        // 조리 중이 아니라면 아무 작업도 하지 않음
        if (!isCooking) return;

        // 경과 시간 만큼 남은 조리 시간 차감
        currentCookingTime -= Time.deltaTime;

        // 남은 시간 UI에 갱신
        UpdateCookingTimeText();

        // 시간이 다 되었으면 결과 처리 및 스테이션 초기화
        if (currentCookingTime <= 0f)
        {
            ProcessCookingResult();
            ResetStation();
        }
    }

    /// <summary>
    /// 자동 스테이션은 플레이어와 상호작용하지 않음
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType) { }

    /// <summary>
    /// 플레이어가 재료를 놓았을 때 호출됨
    /// </summary>
    public void PlaceObject(FoodData data)
    {
        if (!CanPlaceIngredient(data))
        {
            Debug.LogWarning($"[PlaceObject] '{data.foodName}' 등록 실패");
            return;
        }

        iconDisplay.UseSlot(data.foodType); // 오직 아이콘 사용만 처리

        RegisterIngredient(data);
        UpdateCandidateRecipes();

        if (cookedIngredient != null && cookedIngredient.ingredients.All(id => currentIngredients.Contains(id)))
        {
            StartCooking();
        }
        else
        {
            isCooking = false;
        }
    }

    /// <summary>
    /// 재료를 등록하고 시각화 오브젝트를 생성
    /// </summary>
    private void RegisterIngredient(FoodData data)
    {
        if (data == null)
        {
            Debug.LogError("[RegisterIngredient] 등록 시도된 데이터가 null입니다!");
            return;
        }

        Debug.Log($"[RegisterIngredient] ID: {data.id}, Name: {data.foodName}");
        currentIngredients.Add(data.id);
        placedIngredientList.Add(data);
    }

    /// <summary>
    /// 현재 스테이션에 놓인 재료 목록을 기반으로 가능한 레시피 후보를 탐색하고
    /// 가장 일치하는 요리를 선정함
    /// </summary>
    private void UpdateCandidateRecipes()
    {
        Debug.Log($"[UpdateCandidateRecipes] 현재 재료 목록: {string.Join(", ", currentIngredients)}");

        var candidateRecipes = stationData.availableRecipes
            .Where(r => r.ingredients != null && currentIngredients.Any(id => r.ingredients.Contains(id)))
            .ToList();

        Debug.Log($"[UpdateCandidateRecipes] 후보 레시피 수: {candidateRecipes.Count}");

        foreach (var r in candidateRecipes)
        {
            Debug.Log($" - 후보 레시피: {r.foodName} | 필요 재료: {string.Join(", ", r.ingredients)}");
        }

        var matches = RecipeManager.Instance.FindMatchingRecipes(candidateRecipes, currentIngredients);

        if (matches != null && matches.Count > 0)
        {
            cookedIngredient = matches[0].recipe;
            availableMatchedRecipes = matches.Select(m => m.recipe).ToList();
            Debug.Log($"{matches.Count}개 일치 — '{cookedIngredient.foodName}' 선택됨");
        }
        else
        {
            cookedIngredient = null;
            availableMatchedRecipes.Clear();
            Debug.LogWarning("조건에 맞는 레시피가 없습니다.");
        }
    }

    /// <summary>
    /// 조리를 시작하고 타이머 설정
    /// </summary>
    private void StartCooking()
    {
        currentCookingTime = cookingTime;
        isCooking = true;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 조리 완료 시 결과물 생성
    /// </summary>
    private void ProcessCookingResult()
    {
        ClearPlacedObjects();

        if (!cookedIngredient)
        {
            Debug.LogWarning("조리 결과 레시피가 없습니다.");
            return;
        }

        Debug.Log($"조리 완료: '{cookedIngredient.foodName}' 생성");
        GameObject result = VisualObjectFactory.CreateIngredientVisual(transform, cookedIngredient.foodName, cookedIngredient.foodIcon);
        if (result)
        {
            var display = result.AddComponent<FoodDisplay>();
            display.foodData = cookedIngredient;
        }
    }

    /// <summary>
    /// 스테이션을 초기 상태로 리셋
    /// </summary>
    private void ResetStation()
    {
        isCooking = false;
        cookedIngredient = null;
        ResetCookingTimer();
        currentIngredients.Clear();
        placedIngredientList.Clear();
        ClearPlacedObjects();
        iconDisplay?.ResetAll();
    }

    /// <summary>
    /// 요리 시간 초기화 및 UI 반영
    /// </summary>
    private void ResetCookingTimer()
    {
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 화면에 놓인 재료 오브젝트 제거
    /// </summary>
    private void ClearPlacedObjects()
    {
        foreach (var obj in placedIngredients)
            if (obj) Destroy(obj);
        placedIngredients.Clear();
    }

    /// <summary>
    /// 남은 조리 시간을 UI에 표시
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText)
            cookingTimeText.text = Mathf.Max(currentCookingTime, 0f).ToString("F1");
    }

    /// <summary>
    /// 특정 재료를 등록할 수 있는지 검증
    /// </summary>
    public bool CanPlaceIngredient(FoodData food)
    {
        if (currentIngredients.Count == 0)
        {
            bool typeMatch = food.stationType.Any(type => type == stationData.stationType);
            Debug.Log($"[Debug] Food station types: {string.Join(",", food.stationType)}");
            Debug.Log($"[Debug] Station type: {stationData.stationType}");
            return typeMatch;
        }

        if (cookedIngredient == null)
        {
            Debug.LogWarning("[Station] 현재 설정된 레시피가 없습니다.");
            return false;
        }

        if (currentIngredients.Contains(food.id))
        {
            Debug.LogWarning($"[Station] 중복 재료: {food.id} 이미 추가됨");
            return false;
        }

        bool isInRecipe = cookedIngredient.ingredients.Contains(food.id);
        Debug.Log($"[Station] 레시피 유효성 검사: {food.id} 포함 여부: {isInRecipe}");

        return isInRecipe;
    }


    /// <summary>
    /// 플레이어가 결과물을 픽업할 때 호출됨
    /// </summary>
    public void OnPlayerPickup()
    {
        // 배치된 재료 시각 오브젝트 모두 제거
        foreach (var obj in placedIngredients)
            if (obj) Destroy(obj);

        // 관련 리스트와 변수 초기화
        placedIngredients.Clear();
        currentIngredients.Clear();
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();

        // 아이콘 초기화만 수행
        iconDisplay?.ResetAll();

        // 조리 완료된 데이터가 존재하면 그걸 사용, 없으면 마지막 선택된 재료 사용
        FoodData data = cookedIngredient ?? selectedIngredient;
        string name = data.displayName;
        Sprite icon = data.foodIcon;

        if (string.IsNullOrEmpty(name) || !icon)
            return;

        Debug.Log($"플레이어가 '{name}' 획득");
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
