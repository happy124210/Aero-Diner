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
    private List<IIngredientData> placedIngredientList = new();
    private List<GameObject> placedIngredients = new();
    private List<MenuData> availableMatchedRecipes = new();
    private float currentCookingTime;
    private MenuData cookedIngredient;
    private bool isCooking = false;
    private OutlineShaderController outline;

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
    public void PlaceIngredient(ScriptableObject obj)
    {
        if (obj is not IIngredientData data || !CanPlaceIngredient(data))
        {
            Debug.LogWarning($"'{obj?.name}'은 등록할 수 없는 재료입니다.");
            return;
        }

        RegisterIngredient(data);
        UpdateCandidateRecipes();

        if (cookedIngredient != null &&
            cookedIngredient.ingredients.All(id => currentIngredients.Contains(id)))
        {
            StartCooking();
        }
        else
        {
            isCooking = false;
            Debug.Log($"조리 대기 중...");
        }

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
        //    display.foodData = selectedIngredient;
        //    display.originAutomatic = this;
        //    placedIngredients.Add(obj);
        //}
    }

    /// <summary>
    /// 현재 재료 조합에 일치하는 레시피 후보 탐색
    /// </summary>
    private void UpdateCandidateRecipes()
    {
        var candidateRecipes = stationData.availableRecipes
            .Where(r => r.ingredients != null && currentIngredients.Any(id => r.ingredients.Contains(id)))
            .ToList();

        var matches = RecipeManager.Instance.FindMatchingRecipes(candidateRecipes, currentIngredients);

        if (matches != null && matches.Count > 0)
        {
            cookedIngredient = matches[0].recipe;
            availableMatchedRecipes = matches.Select(m => m.recipe).ToList();
            Debug.Log($"{matches.Count}개 일치 — '{cookedIngredient.menuName}' 선택됨");
        }
        else
        {
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
            display.foodData = selectedIngredient;
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
        isCooking = false;
        cookedIngredient = null;
        ResetCookingTimer();
        currentIngredients.Clear();
        placedIngredientList.Clear();
        ClearPlacedObjects();
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
        if (currentIngredients.Contains(data.GetID()))
            return false;

        if (neededIngredients == null || neededIngredients.GetCount() == 0)
            return true;

        if (data is FoodData food && neededIngredients.Contains(food))
            return true;

        return false;
    }

    /// <summary>
    /// 플레이어가 결과물을 픽업할 때 호출됨
    /// </summary>
    public void OnPlayerPickup(PlayerInventory playerInventory)
    {
        // 타이머 및 내부 상태 초기화
        ResetCookingTimer();
        ClearPlacedObjects();
        currentIngredients.Clear();
        placedIngredientList.Clear();

        // 조리 결과물 혹은 기본 재료명/아이콘 결정
        string name = cookedIngredient?.menuName ?? selectedIngredient?.GetDisplayName();
        Sprite icon = cookedIngredient?.menuIcon ?? selectedIngredient?.foodIcon;

        if (string.IsNullOrEmpty(name) || icon == null)
        {
            Debug.LogWarning("들어올 수 있는 조리 결과물이 없습니다.");
            return;
        }

        // 손에 들릴 오브젝트 생성
        GameObject pickupObj = VisualObjectFactory.CreateIngredientVisual(transform, name, icon);
        if (pickupObj != null)
        {
            // FoodDisplay 컴포넌트 연결
            var display = pickupObj.AddComponent<FoodDisplay>();
            display.foodData = selectedIngredient;
            display.originAutomatic = this;

            // 충돌 처리 제거
            var col = pickupObj.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // 플레이어 손에 위치시키고 등록
            Transform slot = playerInventory.GetItemSlotTransform();
            pickupObj.transform.SetParent(slot);
            pickupObj.transform.localPosition = Vector3.zero;
            pickupObj.transform.localRotation = Quaternion.identity;

            playerInventory.SetHeldItem(display);

            Debug.Log($"플레이어가 '{name}' 획득");
        }
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