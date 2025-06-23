using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어가 상호작용하면 재료를 가공하여 가공된 재료를 생성하는 스테이션
/// 
/// 기능:
/// 1. 플레이어가 올바른 재료(FoodDataSO)를 스테이션에 내려놓으면
///    → PlaceIngredient() 함수에서 해당 FoodData를 기반으로 자식 오브젝트를 생성
/// 2. 플레이어가 J 키를 누르고 있으면 cookingTime이 감소하며 진행
/// 3. cookingTime이 0 이하가 되면 ProcessIngredient() 함수가 호출되어
///    → 가공된 재료(Processed Icon 사용)를 생성하고, 원재료 표시 오브젝트는 삭제
/// </summary>
public class PassiveStation : MonoBehaviour, IInteractable
{
    [Header("가공 할 재료 SG (요구되는 재료 그룹)")]
    public IngredientSOGroup NeededIngredients;

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f;

    [Header("Cooking Time UI Text")]
    public Text cookingTimeText;

    // 현재 조리 진행 시간
    private float currentCookingTime;

    // 스테이션에 배치된 재료의 시각적 표현
    private GameObject placedIngredientObj;

    // 현재 스테이션에 배치된 재료의 FoodData
    private FoodData currentFoodData;

    private void Start()
    {
        // 타이머 초기화
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 플레이어가 올바른 재료를 스테이션에 내려놓을 때 호출
    /// 내려놓은 재료의 FoodData를 기반으로 자식 오브젝트를 생성
    /// </summary>
    /// <param name="data">내려놓은 재료의 FoodData</param>
    public void PlaceIngredient(FoodData data)
    {
        currentFoodData = data;
        placedIngredientObj = CreateIngredientDisplay(data);

        // 재료가 배치되면 타이머를 초기화
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// FoodData 기반의 원재료 표시 오브젝트를 생성하는 함수
    /// 생성된 오브젝트는 스테이션의 자식으로 배치
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        GameObject ingredientObj = new GameObject(data.displayName);
        // 스테이션 오브젝트의 자식으로 설정
        ingredientObj.transform.SetParent(transform);
        ingredientObj.transform.localPosition = Vector3.zero;  // 필요에 따라 위치 조정 가능

        SpriteRenderer sr = ingredientObj.AddComponent<SpriteRenderer>();
        if (data.icon != null)
        {
            sr.sprite = data.icon;
        }
        else
        {
            sr.color = Color.gray;  // 스프라이트가 없으면 회색으로 표시
        }

        // 박스 콜라이더2D 추가
        BoxCollider2D boxCollider = ingredientObj.AddComponent<BoxCollider2D>();

        // Rigidbody2D 추가 및 설정: 키네마틱, 중력 스케일 0
        Rigidbody2D rb = ingredientObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        return ingredientObj;
    }

    /// <summary>
    /// FoodData 기반의 가공된 재료 표시 오브젝트를 생성하는 함수
    /// 이 함수는 cookingTime이 모두 경과했을 때 호출
    /// </summary>
    //private GameObject CreateProcessedIngredientDisplay(FoodData data)
    //{
    //    GameObject processedObj = new GameObject(data.displayName + " (Processed)");
    //    // 가공된 재료는 스테이션의 위치에 생성 (필요에 따라 부모 지정 가능)
    //    processedObj.transform.position = transform.position;

    //    SpriteRenderer sr = processedObj.AddComponent<SpriteRenderer>();
    //    if (data.processedIcon != null)
    //    {
    //        sr.sprite = data.processedIcon;
    //    }
    //    else
    //    {
    //        sr.color = Color.green;
    //    }

    //    // 박스 콜라이더2D 추가
    //    BoxCollider2D boxCollider = processedObj.AddComponent<BoxCollider2D>();

    //    // Rigidbody2D 추가 및 설정: 키네마틱, 중력 스케일 0
    //    Rigidbody2D rb = processedObj.AddComponent<Rigidbody2D>();
    //    rb.bodyType = RigidbodyType2D.Kinematic;
    //    rb.gravityScale = 0f;

    //    return processedObj;
    //}

    /// <summary>
    /// cookingTime이 0 이하가 되었을 때 호출되어 가공된 재료를 생성
    /// 이후 원재료 표시 오브젝트는 삭제
    /// </summary>
    private void ProcessIngredient(FoodData data)
    {
        // 가공된 재료 표시 오브젝트 생성
        //GameObject processedIngredient = CreateProcessedIngredientDisplay(data);

        // 기존 원재료 표시 오브젝트 삭제
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        Debug.Log("가공 완료된 재료 생성됨: " + data.displayName);
    }

    /// <summary>
    /// 플레이어가 상호작용(J 키)을 누르고 있을 때 호출되는 메서드.
    /// 
    /// 동작:
    /// 1. 스테이션에 재료가 배치되어 있지 않으면 타이머를 초기화
    /// 2. 배치된 재료가 요구되는 재료(NeededIngredients)에 해당하는지 검사
    /// 3. 요구조건을 만족하면 cookingTime을 감소시키고 UI 업데이트
    /// 4. cookingTime이 0 이하가 되면 가공된 재료를 생성
    /// 
    /// J 키가 눌려 있는 동안 이 함수가 지속적으로 호출되며, 키 입력이 취소되면 타이머는 정지
    /// </summary>
    public void Interact(PlayerInventory playerInventory)
    {
        // 재료가 없으면 cookingTime 리셋
        if (currentFoodData == null || placedIngredientObj == null)
        {
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            Debug.Log("재료가 놓여있지 않아 가공 타이머 리셋됨.");
            return;
        }

        // 배치된 재료가 요구되는 재료 그룹에 포함되어 있는지 확인
        if (NeededIngredients != null && !NeededIngredients.Contains(currentFoodData))
        {
            Debug.Log("제공된 재료가 요구되는 재료와 다릅니다. 타이머 리셋됨.");
            currentCookingTime = cookingTime;
            UpdateCookingTimeText();
            return;
        }

        // J 키를 계속 누르고 있으므로 Time.deltaTime만큼 타이머 감소
        currentCookingTime -= Time.deltaTime;
        UpdateCookingTimeText();

        // cookingTime이 0 이하가 되면 가공 완료
        if (currentCookingTime <= 0f)
        {
            ProcessIngredient(currentFoodData);
            // 가공 완료 후 다음 과정을 위해 재료 데이터와 타이머 초기화
            currentFoodData = null;
            currentCookingTime = cookingTime;
        }
    }

    /// <summary>
    /// cookingTime UI 텍스트 업데이트 함수
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
        {
            cookingTimeText.text = currentCookingTime.ToString("F1");
        }
    }

    /// <summary>
    /// 플레이어가 물체를 들었을 때 호출되는 함수
    /// 스테이션에 배치된 자식 오브젝트(재료)를 삭제하고 관련 변수를 리셋
    /// </summary>
    public void OnPlayerPickup()
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        // 재료와 관련 상태 변수 리셋
        currentFoodData = null;
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();

        Debug.Log("플레이어가 물체를 들었습니다. 스테이션의 자식 오브젝트가 삭제되었습니다.");
    }


    public void OnHoverEnter()
    {
        // 스테이션 위에 플레이어 커서가 올려졌을 때의 시각적 효과 추가 가능
    }

    public void OnHoverExit()
    {
        // 스테이션에서 커서가 벗어났을 때의 효과 제거 처리
    }
}