using System.Collections.Generic;
using System.Linq;
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
    public FoodData selectedIngredient;

    [Header("재료 생성 위치")]
    public Transform spawnPoint;
    
    [Header("가공 허용 재료 그룹")]
    public FoodData[] neededIngredients;

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f;

    [Header("조리 시간 표시용 UI 텍스트")]
    public TextMeshProUGUI cookingTimeText;

    [Header("스테이션 데이터")]
    public StationData stationData;

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new();

    private List<FoodData> placedIngredientList = new();      // 실제 등록된 재료의 데이터 목록
    private List<GameObject> placedIngredients = new();              // 화면에 보여지는 재료 오브젝트들
    private List<FoodData> availableMatchedRecipes = new();          // 현재 조건에서 가능한 레시피 리스트
    private float currentCookingTime;                                // 남은 조리 시간
    private FoodData cookedIngredient;                               // 조리 완료 시 결과가 되는 레시피
    private bool isCooking = false;                                  // 현재 조리 중인지 여부 플래그
    private OutlineShaderController outline;                         // 외곽선 효과를 제어하는 컴포넌트
    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>(); // 외곽선 컴포넌트 연결
    }

    private void Start()
    {
        currentCookingTime = cookingTime;
        UpdateCookingTimeText(); // UI 초기화
    }

    public void PlaceObject(FoodData data)
    {
        // 유효하지 않은 데이터거나 등록 불가한 재료일 경우 경고 출력 후 무시
        if (!CanPlaceIngredient(data))
        {
            Debug.LogWarning($"'{data.foodName}'은 등록할 수 없는 재료입니다.");
            return;
        }

        RegisterIngredient(data);     // 재료 등록 및 선택 처리
        UpdateCandidateRecipes();     // 현재 재료 조합으로 가능한 레시피 탐색

        // 모든 필요한 재료가 충족되었는지 확인 후 조리 시작
        if (cookedIngredient != null &&
            cookedIngredient.ingredients.All(id => currentIngredients.Contains(id)))
        {
            StartCooking(); // 타이머 초기화
        }
        else
        {
            isCooking = false;
            Debug.Log("조건에 맞는 레시피가 부족하여 대기 중...");
        }
    }

    private void RegisterIngredient(FoodData data)
    {
        // 재료 고유 ID 등록
        string id = data.id;
        currentIngredients.Add(id);
        placedIngredientList.Add(data);
    }

    private void StartCooking()
    {
        currentCookingTime = cookingTime;  // 타이머 초기화
        UpdateCookingTimeText();           // UI 갱신
        Debug.Log($"'{cookedIngredient.foodName}' 조리 시작!");
    }

    private void UpdateCandidateRecipes()
    {
        // 전체 레시피 중, 현재 등록된 재료 ID 중 하나라도 포함하는 후보 레시피 필터링
        var candidateRecipes = stationData.availableRecipes
            .Where(r => r.ingredients != null && currentIngredients.Any(id => r.ingredients.Contains(id)))
            .ToList();

        // 후보 중에서 현재 재료와 정확히 일치하는 레시피를 RecipeManager를 통해 찾음
        var matches = RecipeManager.Instance.FindMatchingRecipes(candidateRecipes, currentIngredients);

        // 일치하는 레시피가 하나라도 있을 경우
        if (matches != null && matches.Count > 0)
        {
            cookedIngredient = matches[0].recipe; // 가장 먼저 일치한 요리 데이터를 선택
            availableMatchedRecipes = matches.Select(m => m.recipe).ToList(); // 전체 일치 목록 저장
            Debug.Log($"{matches.Count}개 일치 — '{cookedIngredient.foodName}' 선택됨");
        }
        else
        {
            // 일치하는 레시피가 없으면 초기화
            cookedIngredient = null;
            availableMatchedRecipes.Clear();
            Debug.Log("조건에 맞는 레시피가 없습니다.");
        }
    }

    /// <summary>
    /// 플레이어가 J 키 등으로 상호작용할 때 호출
    /// </summary>
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        // 재료가 없는 경우 조리 타이머 초기화 후 종료
        if (currentIngredients.Count == 0)
        {
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            Debug.Log("재료가 없어 조리 타이머가 리셋되었습니다.");
            return;
        }

        // 필요한 재료 그룹이 지정되어 있다면 유효성 검사
        if (neededIngredients.Any())
        {
            bool hasInvalid = currentIngredients.Any(id => !IsIngredientIDAllowed(id));

            if (hasInvalid)
            {
                Debug.Log("요구된 재료가 아닌 항목이 포함되어 있어 타이머가 리셋됨.");
                currentCookingTime = cookingTime;
                UpdateCookingTimeText();
                return;
            }
        }

        // 실제 상호작용이 'Use'일 때만 처리
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
            Debug.Log("[PassiveStation] InteractionType.Use가 아니므로 무시됩니다.");
        }
    }

    private bool IsIngredientIDAllowed(string id)
    {
        if (neededIngredients == null || !neededIngredients.Any())
            return true;

        return neededIngredients.Any(entry => entry.id == id);
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
            Debug.LogWarning("가공된 레시피 데이터가 없습니다.");
            return;
        }

        Debug.Log("가공 완료된 재료 생성됨: " + data.foodName);

        // 결과물 시각화 생성
        CreateProcessedIngredientDisplay(data);
    }

    /// <summary>
    /// 현재 재료를 추가할 수 있는지 검사
    /// </summary>
    public bool CanPlaceIngredient(FoodData data)
    {
        // 첫 번째 재료
        if (currentIngredients.Count == 0)
        {
            bool typeMatch = data.stationType == stationData.stationType;
            Debug.Log($"[Station] 첫 번째 재료 시도됨: {data.foodName} | 스테이션 타입 일치 여부: {typeMatch}");
            return typeMatch;
        }

        if (cookedIngredient == null)
        {
            Debug.LogWarning("[Station] 현재 설정된 레시피가 없습니다.");
            return false;
        }

        if (currentIngredients.Contains(data.id))
        {
            Debug.LogWarning($"[Station] 중복 재료: {data.id} 이미 추가됨");
            return false;
        }

        bool isInRecipe = cookedIngredient.ingredients.Contains(data.id);
        Debug.Log($"[Station] 레시피 유효성 검사: {data.id} 포함 여부: {isInRecipe}");

        return isInRecipe;
    }

    /// <summary>
    /// 조리 결과물 오브젝트 생성
    /// </summary>
    private GameObject CreateProcessedIngredientDisplay(FoodData data)
    {
        // 필수 데이터가 누락되었는지 확인
        if (!passiveGroup || !data || !spawnPoint)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        Debug.Log($"조리 완료: '{data.foodName}' 생성");

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

        // 조리 완료된 데이터가 존재하면 그걸 사용, 없으면 마지막 선택된 재료 사용
        FoodData data = cookedIngredient
            ? cookedIngredient
            : selectedIngredient;
        
        // 이름이나 아이콘이 비어있으면 중단
        string name = data.displayName;
        Sprite icon = data.foodIcon;
        if (string.IsNullOrEmpty(name) || !icon)
            return;
        
        Debug.Log($"플레이어가 '{name}' 획득");
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