using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.Serialization;
using System.Collections;
using UnityEngine.UI;

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

    [Header("가공 허용 재료 그룹")]
    public FoodData[] neededIngredients;

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f;

    [Header("스테이션 데이터")]
    public StationData stationData;

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new();

    [Header("레시피 매칭 결과")]
    [SerializeField] private string bestMatchedRecipe;
    
    [Header("가능한 레시피에 포함된 음식 ID들")]
    [SerializeField] private List<string> availableFoodIds = new();

    [Header("아이콘 디스플레이")]
    [SerializeField] private FoodSlotIconDisplay iconDisplay;
    [SerializeField] private float slotSpacing = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

    [Header("조리 타이머 UI 컨트롤러")]
    [SerializeField] private StationTimerController timerController; // 타이머 UI 컨트롤러

    private List<FoodData> placedIngredientList = new();             // 실제 등록된 재료의 데이터 목록
    private List<GameObject> placedIngredients = new();              // 화면에 보여지는 재료 오브젝트들
    private List<FoodData> availableMatchedRecipes = new();          // 현재 조건에서 가능한 레시피 리스트
    private float currentCookingTime;                                // 남은 조리 시간
    private FoodData cookedIngredient;                               // 조리 완료 시 결과가 되는 레시피
    private bool isCooking = false;                                  // 현재 조리 중인지 여부 플래그
    private OutlineShaderController outline;                         // 외곽선 효과를 제어하는 컴포넌트
    private bool hasInitialized = false;                             // 아이콘 초기화 플래그
    private bool timerVisible = false;                               // 타이머 UI가 켜졌는지 여부

    private void Awake()
    {
        if (timerController != null)
            timerController.gameObject.SetActive(false);
        else
            Debug.LogWarning("TimerController가 연결되어 있지 않습니다.");

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
            else
            {
                Debug.LogWarning($"[IconLoader] SpriteRenderer가 없거나 stationIcon이 null입니다. 오브젝트: '{objName}'");
            }
        }
        else
        {
            Debug.LogError($"[IconLoader] StationData를 찾을 수 없습니다: 경로 = '{resourcePath}'");
        }

    }

    private void Start()
    {
        UpdateCookingProgress();

        if (stationData != null && stationData.slotDisplays != null)
        {
            var types = stationData.slotDisplays.ConvertAll(s => s.foodType);
            iconDisplay.Initialize(stationData.slotDisplays);
            iconDisplay.ResetAll();
            hasInitialized = true;
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning("stationData 또는 slotDisplays가 null입니다.");
        }

        if (timerController != null)
        {
            timerController.gameObject.SetActive(false); // 외부에서 꺼줌
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
            if (showDebugInfo) Debug.Log("재료가 없어 조리 타이머가 리셋되었습니다.");
            return;
        }

        // 필요한 재료 그룹이 지정되어 있다면 유효성 검사
        if (neededIngredients.Any())
        {
            // 허용되지 않은 재료가 들어있는지 검사
            bool hasInvalid = currentIngredients.Any(id => !IsIngredientIDAllowed(id));
            if (hasInvalid)
            {
                if (showDebugInfo) Debug.Log("요구된 재료가 아닌 항목이 포함되어 있어 타이머가 리셋됨.");
                currentCookingTime = cookingTime;
                return;
            }

            // 모든 필요한 재료가 있는지 검사
            List<string> neededIds = neededIngredients.Select(n => n.id).ToList();
            bool allRequiredPresent = neededIds.All(id => currentIngredients.Contains(id));
            if (!allRequiredPresent)
            {
                if (showDebugInfo) Debug.Log("모든 재료가 준비되지 않아 조리가 시작되지 않습니다.");
                currentCookingTime = cookingTime;
                return;
            }
        }

        // 실제 상호작용이 'Use'일 때만 처리
        if (interactionType == InteractionType.Use)
        {
            // 조리 중이 아니면 이제 시작
            if (!isCooking)
            {
                if (stationData != null && stationData.workType == WorkType.Passive)
                {
                    var sfx = StationSFXResolver.GetSFXFromStationData(stationData);
                    EventBus.PlayCookingLoop(sfx);
                }

                StartCooking();
                return;
            }

            // 이미 조리 중이면 진행
            currentCookingTime -= Time.deltaTime;
            UpdateCookingProgress();

            if (currentCookingTime <= 0f)
            {
                ProcessCookingResult();
                currentIngredients.Clear();
                currentCookingTime = cookingTime;
            }
        }
        else
        {
            // J키에서 손을 뗀 경우
            if (isCooking)
            {
                isCooking = false; // 시간 정지
                EventBus.StopCookingLoop(); // 사운드 정지
                if (showDebugInfo) Debug.Log("[PassiveStation] 조리 중단됨");
            }
        }
    }


    private bool IsIngredientIDAllowed(string id)
    {
        if (neededIngredients == null || !neededIngredients.Any())
            return true;

        return neededIngredients.Any(entry => entry.id == id);
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

        isCooking = false;
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

    private void StartCooking()
    {
        currentCookingTime = cookingTime;
        isCooking = true;

        UpdateCookingProgress();
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
        EventBus.PlaySFX(SFXType.DoneCooking);

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
        iconDisplay?.ResetAll();
        ClearPlacedObjects();

        // 타이머 UI 숨기기
        if (timerController != null)
        {
            timerController.gameObject.SetActive(false);
            timerVisible = false;
        }

        EventBus.StopCookingLoop();
    }

    /// <summary>
    /// 요리 시간 초기화 및 UI 반영
    /// </summary>
    private void ResetCookingTimer()
    {
        currentCookingTime = cookingTime;
        UpdateCookingProgress(); ;
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
    /// 조리 시간 감소, 게이지 Fill, 텍스트 업데이트, 완료 처리까지 전반적 시간 진행 처리
    /// </summary>
    private void UpdateCookingProgress()
    {
        if (!isCooking)
        {
            EventBus.StopCookingLoop();
            return;
        }
        // 처음 조리 시작 시, 타이머 UI가 보이도록 설정
        if (!timerVisible)
        {
            timerController.gameObject.SetActive(true);
            timerVisible = true;
        }

        currentCookingTime -= Time.deltaTime;

        timerController.UpdateTimer(currentCookingTime, cookingTime);

        if (currentCookingTime <= 0f)
        {
            ProcessCookingResult();
            ResetStation();
        }
    }

    /// <summary>
    /// 현재 재료를 추가할 수 있는지 검사
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

    public static void AttachFoodIcon(GameObject obj, FoodData foodData)
    {
        if (obj == null || foodData == null) return;

        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = foodData.foodIcon;
            if (foodData.foodIcon == null)
                renderer.color = Color.gray;
        }
    }

    /// <summary>
    /// 플레이어가 결과물을 픽업할 때 호출됨
    /// </summary>
    public void OnPlayerPickup()
    {
        //currentIngredients.Clear();         // 현재 재료 목록 초기화
        currentCookingTime = cookingTime;   // 타이머 초기화
        UpdateCookingProgress();            

        StartCoroutine(HandlePickup());
    }

    /// <summary>
    /// 플레이어가 재료 또는 요리 결과물을 픽업할 때 실행되는 코루틴 핸들러
    /// - 조리된 결과물이 있는 경우: 아이콘 초기화 후 종료
    /// - 조리되지 않은 경우: 기존 재료 오브젝트 제거 후 딜레이를 두고, 선택된 재료를 시각화 오브젝트로 재생성
    /// - 해당 재료가 등록된 재료인지 확인한 뒤, UI 상태 및 아이콘 복구를 수행
    /// 프레임 딜레이를 통해 생성/삭제 타이밍 간 충돌을 방지하고 상호작용 안정성을 확보
    /// </summary>
    private IEnumerator HandlePickup()
    {
        if (cookedIngredient != null)
        {
            // 마지막으로 사용된 재료의 타입으로 아이콘 복구
            if (placedIngredientList.Count > 0)
            {
                var lastIngredient = placedIngredientList.Last();

                // 아이콘 다시 표시
                iconDisplay?.ShowSlot(lastIngredient.foodType);

                // currentIngredients에서 ID 제거
                if (currentIngredients.Contains(lastIngredient.id))
                {
                    currentIngredients.Remove(lastIngredient.id);
                    if (showDebugInfo) Debug.Log($"currentIngredients에서 '{lastIngredient.id}' 제거됨");
                }

                // placedIngredientList에서도 제거
                placedIngredientList.RemoveAt(placedIngredientList.Count - 1);

                // 레시피 후보 다시 계산
                UpdateCandidateRecipes();
            }

            cookedIngredient = null;
            yield break;
        }

        if (placedIngredientList.Count > 0)
        {
            FoodData food = placedIngredientList.Last(); // 마지막 재료 꺼냄

            GameObject result = placedIngredients.Last();

            //if (result)
            //{
            //    // 플레이어의 아이템 슬롯 위치 가져오기
            //    Transform itemSlot = GameObject.FindGameObjectWithTag("Player")?.transform.Find("Itemslot");

            //    if (itemSlot != null)
            //    {
            //        result.transform.SetParent(itemSlot);
            //        result.transform.localPosition = Vector3.zero;
            //        result.transform.localRotation = Quaternion.identity;

            //        // 충돌 제거 및 중력 비활성화
            //        var rb = result.GetComponent<Rigidbody2D>();
            //        if (rb) rb.simulated = false;

            //        var col = result.GetComponent<Collider2D>();
            //        if (col) col.enabled = false;

            //        // 식별 정보
            //        var display = result.AddComponent<FoodDisplay>();
            //        display.foodData = food;
            //        display.originPlace = this;

            //        if (showDebugInfo)
            //            Debug.Log($"[HandlePickup] 플레이어 손에 재료 '{food.foodName}' 생성 및 이동 완료");
            //    }
            //    else
            //    {
            //        if (showDebugInfo) Debug.LogWarning("[HandlePickup] Player 또는 Itemslot을 찾을 수 없습니다.");
            //    }
            //}

            AttachFoodIcon(result, food);

            // 아이콘 복구
            iconDisplay?.ShowSlot(food.foodType);

            // 목록 제거
            currentIngredients.Remove(food.id);
            placedIngredientList.Remove(food);

            // 레시피 갱신
            UpdateCandidateRecipes();
        }
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