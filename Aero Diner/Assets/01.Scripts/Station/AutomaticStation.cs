using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static CookingSOGroup;

/// <summary>
/// 자동 조리 스테이션: 재료를 놓으면 자동으로 조리되고, 완료 시 결과물이 생성됨
/// </summary>
public class AutomaticStation : MonoBehaviour, IInteractable, IPlaceableStation
{
    [Header("재료 데이터 그룹")]
    public PassiveSOGroup passiveGroup;

    [Header("생성할 재료 SO")]
    public FoodData selectedIngredient;

    [Header("재료 생성 위치")]
    public Transform spawnPoint;

    [Header("가공 허용 재료 그룹")]
    public CookingSOGroup neededIngredients;

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f;

    [Header("조리 시간 표시용 UI 텍스트")]
    public TextMeshProUGUI cookingTimeText;

    [Header("스테이션 데이터")]
    public StationData stationData;

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new();

    // 내부 상태
    private List<IIngredientData> placedIngredientList = new();      // 실제 등록된 재료의 데이터 목록
    private List<GameObject> placedIngredients = new();              // 화면에 보여지는 재료 오브젝트들
    private List<MenuData> availableMatchedRecipes = new();          // 현재 조건에서 가능한 레시피 리스트
    private float currentCookingTime;                                // 남은 조리 시간
    private MenuData cookedIngredient;                               // 조리 완료 시 결과가 되는 레시피
    private bool isCooking = false;                                  // 현재 조리 중인지 여부 플래그
    private OutlineShaderController outline;                         // 외곽선 효과를 제어하는 컴포넌트

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>();
    }

    private void Start()
    {
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
    public void PlaceObject(ScriptableObject obj)
    {
        // 잘못된 데이터거나 등록 불가한 재료일 경우 경고 출력 후 무시
        if (obj is not IIngredientData data || !CanPlaceIngredient(data))
        {
            Debug.LogWarning($"'{obj?.name}'은 등록할 수 없는 재료입니다.");
            return;
        }

        RegisterIngredient(data);     // 재료 등록
        UpdateCandidateRecipes();     // 현재 재료 기반 가능한 요리 찾기

        // 모든 재료가 준비되었다면 즉시 조리 시작
        if (cookedIngredient != null &&
            cookedIngredient.ingredients.All(id => currentIngredients.Contains(id)))
        {
            StartCooking(); // 자동 조리 시작
        }
        else
        {
            // 준비는 되었지만 아직 요리 시작 조건이 안 맞음
            isCooking = false;
            Debug.Log("조리 대기 중...");
        }
    }

    public void OnPlayerPickup()
    {
    }

    /// <summary>
    /// 재료를 등록하고 시각화 오브젝트를 생성
    /// </summary>
    private void RegisterIngredient(IIngredientData data)
    {
        // 재료 고유 ID 등록
        string id = data.GetID();
        currentIngredients.Add(id);
        placedIngredientList.Add(data);

        //// 아이콘 가져와 시각화 오브젝트 생성
        //GameObject obj = VisualObjectFactory.CreateIngredientVisual(transform, data.GetDisplayName(), GetIconFromData(data));
        //if (obj != null)
        //{
        //    // 시각화 오브젝트에 FoodDisplay 연결 및 목록 추가
        //    var display = obj.AddComponent<FoodDisplay>();
        //    display.rawData = data as ScriptableObject;
        //    display.originAutomatic = this;
        //    placedIngredients.Add(obj);
        //}
    }

    /// <summary>
    /// 현재 재료 조합에 일치하는 레시피 후보 탐색
    /// </summary>
    /// <summary>
    /// 현재 스테이션에 놓인 재료 목록을 기반으로 가능한 레시피 후보를 탐색하고
    /// 가장 일치하는 요리를 선정함
    /// </summary>
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
            Debug.Log($"{matches.Count}개 일치 — '{cookedIngredient.menuName}' 선택됨");
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
        // 현재 화면에 있던 재료 오브젝트 제거
        ClearPlacedObjects();

        // 매칭된 결과물이 없다면 경고 후 종료
        if (cookedIngredient == null)
        {
            Debug.LogWarning("조리 결과 레시피가 없습니다.");
            return;
        }

        // 결과물 오브젝트 생성
        Debug.Log($"조리 완료: '{cookedIngredient.menuName}' 생성");
        GameObject result = VisualObjectFactory.CreateIngredientVisual(transform, cookedIngredient.menuName, cookedIngredient.menuIcon);
        if (result != null)
        {
            // 결과물에도 FoodDisplay 구성
            var display = result.AddComponent<FoodDisplay>();
            display.rawData = cookedIngredient;
            display.originAutomatic = this;
        }
    }

    /// <summary>
    /// 재료 데이터로부터 아이콘을 가져옴
    /// </summary>
    private Sprite GetIconFromData(IIngredientData data)
    {
        return data switch
        {
            FoodData fd => fd.foodIcon,
            MenuData md => md.menuIcon,
            _ => null
        };
    }

    /// <summary>
    /// 스테이션을 초기 상태로 리셋
    /// </summary>
    private void ResetStation()
    {
        isCooking = false;              // 조리 상태 해제
        cookedIngredient = null;        // 조리된 레시피 정보 제거
        ResetCookingTimer();            // 타이머 초기화
        currentIngredients.Clear();     // 재료 ID 초기화
        placedIngredientList.Clear();   // 재료 객체 리스트 초기화
        ClearPlacedObjects();           // 화면에서 제거
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
        {
            if (obj != null) Destroy(obj);
        }
        placedIngredients.Clear();
    }

    /// <summary>
    /// 남은 조리 시간을 UI에 표시
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
            cookingTimeText.text = Mathf.Max(currentCookingTime, 0f).ToString("F1");
    }

    /// <summary>
    /// 특정 재료를 등록할 수 있는지 검증
    /// </summary>
    public bool CanPlaceIngredient(IIngredientData data)
    {
        if (!(data is FoodData food))
        {
            Debug.LogWarning("[Station] 유효하지 않은 데이터 타입입니다.");
            return false;
        }

        // 첫 번째 재료
        if (currentIngredients.Count == 0)
        {
            bool typeMatch = food.requireStation == stationData.stationType;
            Debug.Log($"[Station] 첫 번째 재료 시도됨: {food.foodName} | 스테이션 타입 일치 여부: {typeMatch}");
            return typeMatch;
        }

        if (cookedIngredient == null)
        {
            Debug.LogWarning("[Station] 현재 설정된 레시피가 없습니다.");
            return false;
        }

        if (currentIngredients.Contains(data.GetID()))
        {
            Debug.LogWarning($"[Station] 중복 재료: {data.GetID()} 이미 추가됨");
            return false;
        }

        bool isInRecipe = cookedIngredient.ingredients.Contains(data.GetID());
        Debug.Log($"[Station] 레시피 유효성 검사: {data.GetID()} 포함 여부: {isInRecipe}");

        return isInRecipe;
    }

    /// <summary>
    /// 플레이어가 결과물을 픽업할 때 호출됨
    /// </summary>
    public void OnPlayerPickup(PlayerInventory playerInventory)
    {
        // 조리 관련 상태 초기화
        ResetCookingTimer();
        ClearPlacedObjects();
        currentIngredients.Clear();
        placedIngredientList.Clear();

        // 결과물이 있으면 사용하고, 없으면 마지막 등록 재료 사용
        ScriptableObject dataRaw = cookedIngredient != null
            ? (ScriptableObject)cookedIngredient
            : (ScriptableObject)selectedIngredient;

        // 유효한 데이터인지 검사
        if (dataRaw is not IIngredientData ingredientData)
            return;

        string name = ingredientData.GetDisplayName();
        Sprite icon = ingredientData.Icon;
        if (string.IsNullOrEmpty(name) || icon == null)
            return;

        // 비주얼 오브젝트 생성 및 플레이어 인벤토리에 배치
        Transform slot = playerInventory.GetItemSlotTransform();
        GameObject pickupObj = VisualObjectFactory.CreateIngredientVisual(slot, name, icon);
        if (pickupObj == null)
            return;

        var display = pickupObj.AddComponent<FoodDisplay>();
        display.rawData = dataRaw;
        display.originAutomatic = this;

        // 충돌/물리 엔진 비활성화 (UI 용도)
        Collider2D col = pickupObj.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Rigidbody2D rb = pickupObj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = false;

        // 인벤토리 등록 처리
        playerInventory.SetHeldItem(display);

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