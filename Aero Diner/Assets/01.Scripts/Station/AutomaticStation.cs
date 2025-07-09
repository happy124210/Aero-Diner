using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

/// <summary>
/// 자동 조리 스테이션: 재료를 놓으면 자동으로 조리되고, 완료 시 결과물이 생성됨
/// </summary>
public class AutomaticStation : MonoBehaviour, IInteractable, IPlaceableStation
{
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

    [Header("레시피 매칭 결과")]
    [SerializeField, ReadOnly] private string bestMatchedRecipe;
    
    [Header("가능한 레시피에 포함된 음식 ID들")]
    [SerializeField, ReadOnly] private List<string> availableFoodIds = new();

    [Header("아이콘 디스플레이")] // 추가됨
    [SerializeField] private FoodSlotIconDisplay iconDisplay;
    [SerializeField] private float slotSpacing = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

    private List<FoodData> placedIngredientList = new();             // 실제 등록된 재료의 데이터 목록
    private List<GameObject> placedIngredients = new();              // 화면에 보여지는 재료 오브젝트들
    private List<FoodData> availableMatchedRecipes = new();          // 현재 조건에서 가능한 레시피 리스트
    private float currentCookingTime;                                // 남은 조리 시간
    private FoodData cookedIngredient;                               // 조리 완료 시 결과가 되는 레시피
    private bool isCooking = false;                                  // 현재 조리 중인지 여부 플래그
    private OutlineShaderController outline;                         // 외곽선 효과를 제어하는 컴포넌트
    private bool hasInitialized = false;                             // 아이콘 초기화 플래그
    private FoodData selectedIngredient;

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
            if (showDebugInfo) Debug.LogWarning("stationData 또는 slotDisplays가 할당되지 않았습니다.");
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
            if (showDebugInfo) Debug.LogWarning($"[PlaceObject] '{data.foodName}' 등록 실패");
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
            if (showDebugInfo) Debug.LogError("[RegisterIngredient] 등록 시도된 데이터가 null입니다!");
            return;
        }

        if (showDebugInfo) Debug.Log($"[RegisterIngredient] ID: {data.id}, Name: {data.foodName}");
        currentIngredients.Add(data.id);
        placedIngredientList.Add(data);
    }

    /// <summary>
    /// 현재 스테이션에 놓인 재료 목록을 기반으로 가능한 레시피 후보를 탐색하고
    /// 가장 일치하는 요리를 선정함
    /// </summary>
    /// <summary>
    /// 현재 스테이션에 놓인 재료 목록을 기반으로 가능한 레시피 후보를 탐색하고
    /// 가장 일치하는 요리를 선정함
    /// </summary>
    private void UpdateCandidateRecipes()
    {
        availableMatchedRecipes.Clear();
        availableFoodIds.Clear();

        // 오늘 메뉴 기준으로 레시피 매칭 진행
        var matches = RecipeManager.Instance.FindMatchingTodayRecipes(currentIngredients);

        // 모든 재료를 포함한 레시피만 후보로 선정
        if (matches != null && matches.Count > 0)
        {
            // bestMatchedRecipe도 갱신
            var best = matches
                .OrderByDescending(m => m.MatchRatio)
                .ThenByDescending(m => m.matchedCount)
                .First();

            // 전체 매칭된 레시피 저장
            availableMatchedRecipes = matches.Select(m => m.recipe).ToList();
            cookedIngredient = best.recipe;

            bestMatchedRecipe = $"{best.recipe.id} ({best.matchedCount}/{best.totalRequired})";

            foreach (var recipe in availableMatchedRecipes)
            {
                foreach (var ingredientId in recipe.ingredients)
                {
                    if (!availableFoodIds.Contains(ingredientId))
                    {
                        availableFoodIds.Add(ingredientId);
                    }
                }
            }

            if (showDebugInfo)
                Debug.Log($"{matches.Count}개 일치 — '{cookedIngredient.foodName}' 선택됨");
        }
        else
        {
            cookedIngredient = null;
            availableMatchedRecipes.Clear();
            availableFoodIds.Clear();

            if (showDebugInfo)
                Debug.Log("조건에 맞는 레시피가 없습니다.");
        }
    }

    /// <summary>
    /// 현재 재료를 기반으로 가장 적합한 레시피를 찾아서 저장
    /// </summary>
    public void UpdateRecipePreview()
    {
        if (RecipeManager.Instance == null)
        {
            if (showDebugInfo) Debug.LogWarning("[PassiveStation] RecipeManager 인스턴스가 없음");
            return;
        }

        var matches = RecipeManager.Instance.FindMatchingTodayRecipes(currentIngredients);

        // 매칭 목록 초기화
        availableFoodIds.Clear();

        if (matches.Count > 0)
        {
            // 가장 일치하는 레시피 저장
            var best = matches
                .OrderByDescending(m => m.MatchRatio)
                .ThenByDescending(m => m.matchedCount)
                .First();

            bestMatchedRecipe = $"{best.recipe.displayName} ({best.matchedCount}/{best.totalRequired})";

            // 전체 매칭 리스트 업데이트
            foreach (var recipe in matches.Select(m => m.recipe))
            {
                foreach (var id in recipe.ingredients)
                {
                    availableFoodIds.Add(id);
                }
            }

            if (showDebugInfo)
            {
                var previewList = string.Join("\n", availableFoodIds.Select(r => "- " + r));
                Debug.Log("[레시피 미리보기]\n" + previewList);
            }
        }
        else
        {
            bestMatchedRecipe = "매칭되는 레시피 없음";
            availableFoodIds.Clear();

            if (showDebugInfo) Debug.Log("[PassiveStation] 일치하는 레시피 없음");
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
            if (showDebugInfo) Debug.LogWarning("조리 결과 레시피가 없습니다.");
            return;
        }

        if (showDebugInfo) Debug.Log($"조리 완료: '{cookedIngredient.foodName}' 생성");
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
    public bool CanPlaceIngredient(FoodData data)
    {
        // 중복 방지
        if (currentIngredients.Contains(data.id))
        {
            if (showDebugInfo) Debug.LogWarning($"[Station] 중복 재료: {data.id} 이미 추가됨");
            return false;
        }

        // 첫 번째 재료
        if (currentIngredients.Count == 0)
        {
            bool typeMatch = data.stationType.Any(type => type == stationData.stationType);
            
            if (showDebugInfo) Debug.Log($"[Debug] Food station types: {string.Join(",", data.stationType)}");
            if (showDebugInfo) Debug.Log($"[Debug] Station type: {stationData.stationType}");
            
            return typeMatch;
        }

        // 이후에는 matchedRecipeNames에 포함된 재료만 허용
        if (availableFoodIds.Contains(data.id))
        {
            if (showDebugInfo) Debug.Log($"[Station] '{data.id}' matchedRecipeNames에 포함 → 등록 가능");
            return true;
        }

        if (showDebugInfo) Debug.Log($"[Station] '{data.id}'은 현재 어떤 레시피에도 포함되지 않음");
        return false;
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

        if (showDebugInfo) Debug.Log($"플레이어가 '{name}' 획득");
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