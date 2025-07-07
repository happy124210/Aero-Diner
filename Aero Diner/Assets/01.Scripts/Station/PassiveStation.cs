using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEditor;

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

    [Header("레시피 매칭 결과 (읽기 전용)")]
    [SerializeField] private string bestMatchedRecipe;

    [Header("모든 매칭 레시피 목록 (읽기 전용)")]
    [SerializeField] private List<string> matchedRecipeNames = new();

    [Header("아이콘 디스플레이")]
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
    private FoodData selectedIngredient;

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>(); // 외곽선 컴포넌트 연결
    }

    private void Start()
    {
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();

        if (stationData != null && stationData.slotDisplays != null)
        {
            iconDisplay.Initialize(stationData.slotDisplays);
            iconDisplay.ResetAll();
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning("stationData 또는 slotDisplays가 null입니다.");
        }

        currentCookingTime = cookingTime;
        UpdateCookingTimeText(); // UI 초기화
    }

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

    private void StartCooking()
    {
        currentCookingTime = cookingTime;  // 타이머 초기화
        UpdateCookingTimeText();           // UI 갱신
        if (showDebugInfo) Debug.Log($"'{cookedIngredient.foodName}' 조리 시작!");
    }

    /// <summary>
    /// 현재 스테이션에 놓인 재료 목록을 기반으로 가능한 레시피 후보를 탐색하고
    /// 가장 일치하는 요리를 선정함
    /// </summary>
    private void UpdateCandidateRecipes()
    {
        availableMatchedRecipes.Clear();
        matchedRecipeNames.Clear();

        // 모든 재료를 포함한 레시피만 후보로 선정
        var candidateRecipes = stationData.availableRecipes
            .Where(r => r.ingredients != null && currentIngredients.All(id => r.ingredients.Contains(id)))
            .ToList();

        // 현재 재료 기반으로 가장 많이 일치하는 레시피들 검색
        var matches = RecipeManager.Instance.FindMatchingRecipes(candidateRecipes, currentIngredients);

        if (matches != null && matches.Count > 0)
        {
            // 전체 매칭된 레시피 저장
            availableMatchedRecipes = matches.Select(m => m.recipe).ToList();
            cookedIngredient = matches[0].recipe;

            // bestMatchedRecipe도 갱신!
            var best = matches[0];
            bestMatchedRecipe = $"{best.recipe.id} ({best.matchedCount}/{best.totalRequired})";

            foreach (var recipe in availableMatchedRecipes)
            {
                foreach (var ingredientId in recipe.ingredients)
                {
                    matchedRecipeNames.Add(ingredientId);
                }
            }

            if (showDebugInfo) Debug.Log($"{matches.Count}개 일치 — '{cookedIngredient.foodName}' 선택됨");
        }
        else
        {
            cookedIngredient = null;
            availableMatchedRecipes.Clear();
            matchedRecipeNames.Clear();

            if (showDebugInfo) Debug.Log("조건에 맞는 레시피가 없습니다.");
        }
    }

    /// <summary>
    /// 플레이어가 J 키 등으로 상호작용할 때 호출
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (currentIngredients.Count == 0)
        {
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            if (showDebugInfo) Debug.Log("재료가 없어 조리 타이머가 리셋되었습니다.");
            return;
        }

        if (currentIngredients.Any())
        {
            bool hasInvalid = currentIngredients.Any(id => !IsIngredientIDAllowed(id));

            if (hasInvalid)
            {
                var invalids = currentIngredients
                    .Where(id => !IsIngredientIDAllowed(id))
                    .ToList();

                if (showDebugInfo) Debug.Log($"요구되지 않은 재료 포함됨: {string.Join(", ", invalids)} → 타이머 리셋");
                currentCookingTime = cookingTime;
                UpdateCookingTimeText();
                return;
            }
        }

        if (interactionType == InteractionType.Use)
        {
            currentCookingTime -= Time.deltaTime;
            UpdateCookingTimeText();

            if (currentCookingTime <= 0f)
            {
                ProcessIngredient(cookedIngredient);
                currentIngredients.Clear();
                currentCookingTime = cookingTime;
            }
        }
        else
        {
            if (showDebugInfo) Debug.Log("[PassiveStation] InteractionType.Use가 아니므로 무시됩니다.");
        }
    }

    // 요리 유효성 검사
    private bool IsIngredientIDAllowed(string id)
    {
        return currentIngredients.Contains(id);
    }

    /// <summary>
    /// 조리 완료 시 호출되어 결과 재료 생성
    /// </summary>
    private void ProcessIngredient(FoodData data)
    {
        // 기존 재료 오브젝트 제거
        foreach (var obj in placedIngredients)
        {
            if (obj)
                Destroy(obj);
        }

        placedIngredients.Clear(); // 리스트 초기화

        // 레시피가 유효하지 않을 경우 경고
        if (!data)
        {
            if (showDebugInfo) Debug.LogWarning("가공된 레시피 데이터가 없습니다.");
            return;
        }

        if (showDebugInfo) Debug.Log("가공 완료된 재료 생성됨: " + data.foodName);

        // 결과물 시각화 생성
        CreateProcessedIngredientDisplay(data);
    }

    /// <summary>
    /// 현재 재료를 추가할 수 있는지 검사
    /// </summary>
    public bool CanPlaceIngredient(FoodData food)
    {
        // 첫 번째 재료일 경우: 해당 스테이션 타입이 맞는지 검사
        if (currentIngredients.Count == 0)
        {
            bool typeMatch = food.stationType.Any(type => type == stationData.stationType);
            if (showDebugInfo) Debug.Log($"[Debug] 첫 재료: 스테이션 타입 확인 — 재료: {string.Join(",", food.stationType)} / 스테이션: {stationData.stationType}");
            return typeMatch;
        }

        // 중복 재료 확인
        if (currentIngredients.Contains(food.id))
        {
            if (showDebugInfo) Debug.LogWarning($"[Station] 중복 재료: {food.id} 이미 추가됨");
            return false;
        }

        // 유효 재료 ID 목록이 없는 경우
        if (matchedRecipeNames == null || matchedRecipeNames.Count == 0)
        {
            if (showDebugInfo) Debug.LogWarning("[Station] 현재 설정된 유효 재료 ID가 없습니다.");
            return false;
        }

        // 유효 재료 ID에 포함되는지 검사
        bool isValidIngredient = matchedRecipeNames.Contains(food.id);
        if (showDebugInfo) Debug.Log($"[Station] 재료 유효성 검사: {food.id} → {isValidIngredient}");

        return isValidIngredient;
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

        var recipes = stationData?.availableRecipes ?? new List<FoodData>();

        var matches = RecipeManager.Instance.FindMatchingRecipes(recipes, currentIngredients);

        // 매칭 목록 초기화
        matchedRecipeNames.Clear();

        if (matches.Count > 0)
        {
            // 가장 일치하는 레시피 저장
            var best = matches[0];
            bestMatchedRecipe = $"{best.recipe.displayName} ({best.matchedCount}/{best.totalRequired})";

            // 전체 매칭 리스트 업데이트
            foreach (var m in matches)
            {
                matchedRecipeNames.Add($"{m.recipe.displayName} ({m.matchedCount}/{m.totalRequired})");
            }

            if (showDebugInfo)
            {
                var previewList = string.Join("\n", matchedRecipeNames.Select(r => "- " + r));
                Debug.Log("[레시피 미리보기]\n" + previewList);
            }
        }
        else
        {
            bestMatchedRecipe = "매칭되는 레시피 없음";
            matchedRecipeNames.Add("없음");

            if (showDebugInfo) Debug.Log("[PassiveStation] 일치하는 레시피 없음");
        }
    }

    /// <summary>
    /// 조리 결과물 오브젝트 생성
    /// </summary>
    private GameObject CreateProcessedIngredientDisplay(FoodData data)
    {
        // 필수 데이터가 누락되었는지 확인
        if (!data || !spawnPoint)
        {
            if (showDebugInfo) Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        if (showDebugInfo) Debug.Log($"조리 완료: '{data.foodName}' 생성");

        // 시각 오브젝트 생성
        GameObject result = VisualObjectFactory.CreateIngredientVisual(transform, data.foodName, data.foodIcon);

        if (result)
        {
            var display = result.AddComponent<FoodDisplay>();
            display.foodData = data;
        }

        return result;
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
    /// UI에 남은 조리 시간 업데이트
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText)
            cookingTimeText.text = currentCookingTime.ToString("F1");
    }


    /// <summary>
    /// 플레이어가 재료를 들 때 호출
    /// </summary>
    public void OnPlayerPickup()
    {
        // 배치된 재료 시각 오브젝트 모두 제거
        foreach (var obj in placedIngredients)
        {
            if (obj) Destroy(obj);
        }

        // 관련 리스트와 변수 초기화
        placedIngredients.Clear();          // 시각 오브젝트 리스트 초기화
        currentIngredients.Clear();         // 현재 재료 목록 초기화
        currentCookingTime = cookingTime;   // 타이머 초기화
        UpdateCookingTimeText();            // UI 갱신

        iconDisplay?.ResetAll(); // 아이콘 리셋

        // 조리 완료된 데이터가 존재하면 그걸 사용, 없으면 마지막 선택된 재료 사용
        FoodData data = cookedIngredient
            ? cookedIngredient
            : selectedIngredient;
        
        // 이름이나 아이콘이 비어있으면 중단
        string name = data.displayName;
        Sprite icon = data.foodIcon;
        if (string.IsNullOrEmpty(name) || !icon)
            return;

        if (showDebugInfo) Debug.Log($"플레이어가 '{name}' 획득");
    }


    public void OnHoverEnter()
    {
        if (CompareTag("Ingredient")) return; // 재료는 아웃라인 적용 안 함
        outline?.EnableOutline();
    }
    public void OnHoverExit()
    {
        outline?.DisableOutline();
    }
}