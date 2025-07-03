using UnityEngine;

/// <summary>
/// 플레이어가 올바른 재료(또는 메뉴)를 선반에 놓으면 저장되는 스테이션
/// </summary>
public class Shelf : MonoBehaviour, IInteractable, IPlaceableStation
{
    [Header("생성할 Food/Menu SO")]
    public FoodData currentData; // 선반에 올려진 음식 데이터

    [Header("비주얼 오브젝트 생성 위치")]
    public Transform spawnPoint;

    [Header("가공 허용 재료 그룹 (FoodData만 관리)")] // 플레이어가 내려놓은 재료를 기반으로 동적으로 채워짐
    public FoodSOGroup neededIngredients;

    // 내부 상태
    private GameObject placedIngredientObj;

    private OutlineShaderController outline;

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>();
    }

    // 선반 자체는 직접 Interact 불필요
    public void Interact(PlayerInventory playerInventory, InteractionType interactionType) { }

    // IPlaceableStation 인터페이스 구현
    public void PlaceObject(FoodData data)
    {

        // 이미 올려진 게 있으면 차단
        if (currentData != null)
        {
            Debug.Log("이미 항목이 배치되어 있습니다.");
            return;
        }

        // FoodData인 경우 그룹 검사
        if (data is FoodData food && neededIngredients != null && !neededIngredients.Contains(food))
        {
            Debug.Log($"'{food.displayName}'는 허용되지 않은 재료입니다.");
            return;
        }

        // 상태 갱신
        currentData = data;

        // 비주얼 생성
        placedIngredientObj = CreateIngredientDisplay(data);

        // 만약 FoodData이고, 그룹에 없으면 추가
        if (neededIngredients != null &&
            !neededIngredients.Contains(data))
        {
            neededIngredients.AddIngredient(data);
            Debug.Log($"가공 허용 재료 그룹에 '{data.displayName}' 추가됨.");
        }
    }

    // IIngredientData 전반을 검사
    public bool CanPlaceIngredient(FoodData data)
    {
        if (currentData != null)
        {
            Debug.Log("[Shelf] 이미 항목이 배치되어 있습니다.");
            return false;
        }

        if (neededIngredients != null && !neededIngredients.Contains(data))
        {
            Debug.Log($"[Shelf] '{data.displayName}'는 허용되지 않은 재료입니다.");
            return false;
        }

        Debug.Log($"[Shelf] '{data.displayName}' 배치 가능");
        return true;
    }

    /// <summary>
    /// rawData(FoodData or MenuData) 기반으로 VisualObjectFactory 사용해 오브젝트 생성
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        if (data == null || spawnPoint == null)
        {
            Debug.LogError("필수 데이터가 누락되었습니다.");
            return null;
        }

        // VisualObjectFactory 호출
        GameObject obj = VisualObjectFactory.CreateIngredientVisual(
            parent: spawnPoint,
            name: data.foodName,
            icon: data.foodIcon
        );
        if (obj == null) return null;

        // FoodDisplay 세팅 (원본 데이터만 연결)
        var display = obj.AddComponent<FoodDisplay>();
        display.foodData = data;
        display.originPlace = this;
        return obj;
    }

    /// <summary>
    /// 플레이어가 재료를 들 때 호출됨.
    /// 스테이션에 남아있는 재료 오브젝트들을 제거하고 상태를 초기화
    /// </summary>
    public void OnPlayerPickup()
    {
        placedIngredientObj = null;
        currentData = null;
        Debug.Log("플레이어가 재료를 들었고, 스테이션이 초기화되었습니다.");
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