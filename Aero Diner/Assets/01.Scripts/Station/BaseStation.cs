using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BaseStation : MonoBehaviour, IPlaceableStation, IMovableStation
{
    public Transform GetTransform() => transform;

    public Transform spawnPoint;
    public float cookingTime = 5f;
    [SerializeField] private StationData stationData;
    public StationData StationData => stationData;
    public List<string> currentIngredients = new();
    [SerializeField] protected List<string> availableFoodIds = new();
    [SerializeField] protected string bestMatchedRecipe;
    [SerializeField] protected FoodSlotIconDisplay iconDisplay;
    [SerializeField] protected float slotSpacing = 0.5f;
    [SerializeField] protected StationTimerController timerController; // 타이머 UI 컨트롤러
    [SerializeField] protected bool showDebugInfo;

    protected List<FoodData> placedIngredientList = new();             // 실제 등록된 재료의 데이터 목록
    protected List<GameObject> placedIngredients = new();              // 화면에 보여지는 재료 오브젝트들
    protected List<FoodData> availableMatchedRecipes = new();          // 현재 조건에서 가능한 레시피 리스트
    protected FoodData cookedIngredient;                               // 조리 완료 시 결과가 되는 레시피
    protected OutlineShaderController outline;                         // 외곽선 효과를 제어하는 컴포넌트
    protected bool hasInitialized = false;                             // 아이콘 초기화 플래그
    protected CookingTimer timer;
    protected bool isCooking = false;

    public string StationId => stationData != null ? stationData.id : string.Empty;

    private void Awake()
    {
        timer = new CookingTimer(cookingTime);
        timerController.gameObject.SetActive(false);

        outline = GetComponent<OutlineShaderController>(); // 외곽선 컴포넌트 연결

        string objName = gameObject.name;
        string resourcePath = $"Datas/Station/{objName}Data";

        // SO 로드
        StationData data = Resources.Load<StationData>(resourcePath);
        if (data != null)
        {
            // stationData 필드 연결
            stationData = data;

            // 스프라이트 아이콘 설정
            if (TryGetComponent<SpriteRenderer>(out var sr) && data.stationIcon != null)
            {
                sr.sprite = data.stationIcon;
            }
        }
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
    }

    /// <summary>
    /// 플레이어가 재료를 놓았을 때 호출됨
    /// </summary>
    public void PlaceObject(FoodData data)
    {
        // 유효하지 않은 데이터거나 등록 불가한 재료일 경우 경고 출력 후 무시
        if (!CanPlaceIngredient(data))
        {
            if (showDebugInfo) Debug.LogWarning($"'{data.foodName}'은 등록할 수 없는 재료입니다.");
            return;
        }

        iconDisplay.UseSlot(data.foodType); // 아이콘 끄기

        RegisterIngredient(data);     // 재료 등록 및 선택 처리
        UpdateCandidateRecipes();     // 현재 재료 조합으로 가능한 레시피 탐색

        // 모든 필요한 재료가 충족되었는지 확인 후 조리 시작
        if (cookedIngredient != null && cookedIngredient.ingredients.All(id => currentIngredients.Contains(id)) && stationData.workType == WorkType.Automatic)
        {
            StartCooking();
        }
        else
        {
            // 조리 조건 불충분 시 아무 타이머도 시작하지 않음
            timerController?.gameObject.SetActive(false);
            if (showDebugInfo) Debug.Log("조건에 맞는 레시피가 부족하여 대기 중...");
        }

    }
    public void Interact(PlayerInventory inventory, InteractionType interactionType)
    {
        
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
    /// 재료를 등록하고 시각화 목록 생성
    /// </summary>
    private void RegisterIngredient(FoodData data)
    {
        if (data == null)
        {
            if (showDebugInfo) Debug.LogError("[RegisterIngredient] 등록 시도된 데이터가 null입니다!");
            return;
        }

        // 시각 오브젝트 생성
        if (showDebugInfo) Debug.Log($"[RegisterIngredient] ID: {data.id}, Name: {data.foodName}");
        currentIngredients.Add(data.id);
        placedIngredientList.Add(data);

        GameObject visual = VisualObjectFactory.PlaceIngredientVisual(transform, data.foodName, data.foodIcon);
        if (visual)
        {
            var display = visual.AddComponent<FoodDisplay>();
            display.foodData = data;
            display.originPlace = this;

            placedIngredients.Add(visual); // 오브젝트 추적 리스트에 등록

            if (showDebugInfo) Debug.Log($"'{data.foodName}' 시각 오브젝트 생성 및 배치 완료");
        }
    }

    /// <summary>
    /// 현재 스테이션에 놓인 재료 목록을 기반으로 가능한 레시피 후보를 탐색하고
    /// 가장 일치하는 요리를 선정함
    /// </summary>
    private void UpdateCandidateRecipes()
    {
        // 현재 재료 ID 목록 가져오기
        var currentIds = currentIngredients;

        // 오늘 레시피 중 매칭되는 목록 탐색
        var matches = RecipeManager.Instance.FindMatchingTodayRecipes(currentIds);

        // 초기화
        availableMatchedRecipes = new();
        availableFoodIds = new();
        cookedIngredient = null;

        if (matches is { Count: > 0 })
        {
            // 가장 일치율 높은 레시피 선정
            var best = matches
                .OrderByDescending(m => m.MatchRatio)
                .ThenByDescending(m => m.matchedCount)
                .FirstOrDefault();

            availableMatchedRecipes = matches
                .Select(m => m.recipe)
                .ToList();

            availableFoodIds = availableMatchedRecipes
                .SelectMany(recipe => recipe.ingredients)
                .Distinct()
                .ToList();

            if (best?.MatchRatio >= 1f)
            {
                cookedIngredient = best.recipe;
                bestMatchedRecipe = $"{best.recipe.id} ({best.matchedCount}/{best.totalRequired})";

                if (showDebugInfo) Debug.Log($"완전 일치 — '{cookedIngredient.foodName}' 선택됨");
            }
            else
            {
                bestMatchedRecipe = "요리 대기 중 (재료 부족)";
                if (showDebugInfo) Debug.Log("아직 요리 조건 불충분");
            }
        }
        else
        {
            bestMatchedRecipe = "조건에 맞는 레시피 없음";
            if (showDebugInfo) Debug.Log("조건에 맞는 레시피가 없습니다.");
        }
    }

    /// <summary>
    /// 조리를 시작하고 타이머 설정
    /// </summary>
    public void StartCooking()
    {
        // 만약 타이머가 없거나 0이면 새로 시작
        if (timer == null || timer.Remaining <= 0f)
        {
            timer = new CookingTimer(cookingTime);
            timer.Start();                                                              // Duration으로 리셋 시작
            if (showDebugInfo) Debug.Log("[StartCooking] 새 타이머 시작");
        }
        else if (!timer.IsRunning)
        {
            timer.Start(timer.Remaining);                                               // 중단 시점에서 이어서 시작
            if (showDebugInfo) Debug.Log($"[StartCooking] 이어서 시작 / 남은 시간: {timer.Remaining:F2}");
        }

        if (timerController != null)
            timerController.gameObject.SetActive(true);                                  // UI 활성화

        if (stationData != null)
        {
            var sfx = StationSFXResolver.GetSFXFromStationData(stationData);

            // 루프 사운드는 자동인 경우만 재생
            if (stationData.workType == WorkType.Automatic)
            {
                EventBus.PlayLoopSFX(sfx);
            }
            else if (stationData.workType == WorkType.Passive)
            {
                EventBus.PlayLoopSFX(sfx);
            }
        }
    }

    /// <summary>
    /// 조리 완료 시 결과물 생성
    /// </summary>
    public void ProcessCookingResult()
    {
        ClearPlacedObjects();

        if (!cookedIngredient)
        {
            if (showDebugInfo) Debug.LogWarning("조리 결과 레시피가 없습니다.");
            return;
        }

        if (showDebugInfo) Debug.Log($"조리 완료: '{cookedIngredient.foodName}' 생성");

        GameObject result = VisualObjectFactory.CreateIngredientVisual(transform, cookedIngredient.foodName, cookedIngredient.foodIcon);

        var sfx = StationSFXResolver.GetSFXFromStationData(stationData);
        EventBus.StopLoopSFX(sfx);
        Invoke(nameof(PlayCookingFinishSound), 0.2f);

        if (result)
        {
            var display = result.AddComponent<FoodDisplay>();
            display.foodData = cookedIngredient;
        }
    }

    private void PlayCookingFinishSound()
    {
        EventBus.PlaySFX(SFXType.DoneCooking);
    }


    /// <summary>
    /// 스테이션을 초기 상태로 리셋
    /// </summary>
    public void ResetStation()
    {
        timer = new CookingTimer(cookingTime);
        iconDisplay?.ResetAll();                   // 아이콘 리셋
        ClearPlacedObjects();                      // 오브젝트 제거
        var sfx = StationSFXResolver.GetSFXFromStationData(stationData);
        EventBus.StopLoopSFX(sfx);                 // 사운드 중지

        // 두 컬렉션 모두 초기화
        currentIngredients.Clear();
        placedIngredientList.Clear();

        if (timerController != null)
            timerController.gameObject.SetActive(false);
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
    /// 플레이어가 결과물을 픽업할 때 호출됨
    /// </summary>
    public void OnPlayerPickup()
    {
        StartCoroutine(HandlePickup());                                 // 픽업 처리

        currentIngredients.Clear();                                     // 현재 재료 목록 초기화는 그대로 유지
        placedIngredientList.Clear();                                   // 실재료 목록도 초기화

        timer.Start();                                                  // CookingTimer 객체 기반으로 타이머 시작
        timerController?.UpdateTimer(timer.Remaining, timer.Duration);  // 타이머 UI 갱신
    }

    /// <summary>
    /// 플레이어가 재료 또는 요리 결과물을 픽업할 때 실행되는 코루틴 핸들러
    /// 조리된 결과물이 있는 경우: 아이콘 초기화 후 종료
    /// 조리되지 않은 경우: 기존 재료 오브젝트 제거 후 딜레이를 두고, 선택된 재료를 시각화 오브젝트로 재생성
    /// 해당 재료가 등록된 재료인지 확인한 뒤, UI 상태 및 아이콘 복구를 수행
    /// 프레임 딜레이를 통해 생성/삭제 타이밍 간 충돌을 방지하고 상호작용 안정성을 확보
    /// </summary>

    private IEnumerator HandlePickup()
    {
        if (placedIngredients.Count == 0)
        {
            if (showDebugInfo) Debug.LogWarning("[Pickup] 처리할 오브젝트가 없습니다.");
            yield break;
        }

        // placedIngredients 역순 탐색하며 유효한 재료 찾기
        GameObject targetObject = null;
        FoodDisplay display = null;

        for (int i = placedIngredients.Count - 1; i >= 0; i--)
        {
            var obj = placedIngredients[i];
            var fd = obj?.GetComponent<FoodDisplay>();
            if (fd != null && fd.foodData != null)
            {
                targetObject = obj;
                display = fd;
                break;
            }
        }

        if (targetObject == null || display == null)
        {
            if (showDebugInfo) Debug.LogWarning("[Pickup] 유효한 FoodDisplay를 찾지 못했습니다.");
            yield break;
        }

        var pickedData = display.foodData;

        // 결과물 픽업이면 전체 초기화
        if (cookedIngredient != null && pickedData == cookedIngredient)
        {
            if (showDebugInfo) Debug.Log($"[Pickup] 요리 결과물 픽업: {pickedData.foodName}");

            iconDisplay?.ResetAll();
            ClearPlacedObjects();

            currentIngredients.Clear();            // 결과물 픽업 시에는 전체 클리어
            placedIngredientList.Clear();
            cookedIngredient = null;

            UpdateCandidateRecipes();
            yield break;
        }

        // 재료 픽업: 해당 오브젝트 기준으로 제거
        if (placedIngredients.Contains(targetObject))
        {
            if (showDebugInfo) Debug.Log($"[Pickup] 재료 픽업: {pickedData.foodName}");

            iconDisplay?.ShowSlot(pickedData.foodType);

            // 아이콘 로딩 보장
            if (pickedData.foodIcon == null && !string.IsNullOrEmpty(pickedData.id))
            {
                var loadedIcon = Resources.Load<Sprite>($"Icons/Food/{pickedData.id}");
                if (loadedIcon != null)
                {
                    pickedData.foodIcon = loadedIcon;
                }
            }

            AttachFoodIcon(targetObject, pickedData);

            // 오브젝트 및 데이터 제거
            placedIngredients.Remove(targetObject);
            currentIngredients.Remove(pickedData.id);

            // 딱 하나만 제거
            var targetIndex = placedIngredientList.FindIndex(fd => fd.id == pickedData.id);
            if (targetIndex >= 0)
            {
                placedIngredientList.RemoveAt(targetIndex);
            }

            yield return null;

            // 아이콘 다시 부착
            AttachFoodIcon(targetObject, pickedData);

            if (currentIngredients.Count > 0)
            {
                UpdateCandidateRecipes();
            }
            else
            {
                cookedIngredient = null;
                bestMatchedRecipe = "조건에 맞는 레시피 없음";
                iconDisplay?.ShowSlot(pickedData.foodType);
            }
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning("[Pickup] 선택된 재료가 placedIngredientList에 존재하지 않습니다.");
        }
    }

    /// <summary>
    /// 전달된 GameObject에 해당 FoodData의 아이콘을 SpriteRenderer로 표시
    /// 아이콘이 없을 경우 회색으로 표시
    /// </summary>
    /// <param name="obj">아이콘을 부착할 대상 GameObject</param>
    /// <param name="foodData">표시할 음식 데이터(FoodData)</param>
    public static void AttachFoodIcon(GameObject obj, FoodData foodData)
    {
        if (obj == null || foodData == null) return;

        // 대상 객체에서 SpriteRenderer 컴포넌트를 가져 옴
        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // 음식 데이터에 아이콘이 존재할 경우 해당 스프라이트를 설정
            renderer.sprite = foodData.foodIcon;

            // 만약 아이콘이 null이라면 시각적으로 구분하기 위해 색상을 회색으로 설정
            if (foodData.foodIcon == null)
                renderer.color = Color.gray;
        }
    }

    /// <summary>
    /// 플레이어가 일상씬에서 k키를 사용했을때 설비를 픽업하는 매서드
    /// </summary>
    public void OnStationPickup()
    {

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
