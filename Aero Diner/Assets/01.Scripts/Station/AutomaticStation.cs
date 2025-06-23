using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어가 올바른 재료(FoodDataSO)를 스테이션에 내려놓으면
/// 자동으로 조리 타이머가 시작되어 cookingTime이 0 이하가 되었을 때 가공된 재료를 생성하는 스테이션
/// 
/// 기능:
/// 1. PlaceIngredient() 함수에서 내려놓은 재료의 FoodData를 기반으로 자식 오브젝트 생성
/// 2. 올바른 재료가 배치되면 자동으로 타이머가 시작
/// 3. 조리 시간이 다 지나면 ProcessIngredient() 호출 후 원재료 표시 오브젝트를 삭제
/// </summary>
public class AutomaticStation : MonoBehaviour
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

    // 자동 조리 시작 플래그
    private bool isCooking = false;

    private void Start()
    {
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    private void Update()
    {
        // 재료가 배치되어 올바른 재료라면 자동으로 조리 진행
        if (isCooking && placedIngredientObj != null && currentFoodData != null)
        {
            currentCookingTime -= Time.deltaTime;
            UpdateCookingTimeText();

            if (currentCookingTime <= 0f)
            {
                ProcessIngredient(currentFoodData);
                ResetStation();
            }
        }
    }

    /// <summary>
    /// 플레이어(또는 다른 스크립트)가 올바른 재료를 스테이션에 내려놓을 때 호출
    /// 내려놓은 재료의 FoodData를 기반으로 자식 오브젝트를 생성하고 자동 조리를 시작함
    /// </summary>
    /// <param name="data">내려놓은 재료의 FoodData</param>
    public void PlaceIngredient(FoodData data) 
    {
        // 스테이션에 이미 재료가 배치되어 있으면 추가 배치를 막음
        if (currentFoodData != null)
        {
            Debug.Log("이미 재료가 배치되어 있습니다.");
            return;
        }

        // 재료가 요구 그룹에 포함되어 있는지 확인
        if (NeededIngredients != null && !NeededIngredients.Contains(data))
        {
            Debug.Log("제공된 재료가 요구되는 그룹에 속하지 않습니다.");
            return;
        }

        currentFoodData = data;
        placedIngredientObj = CreateIngredientDisplay(data);
        // 재료 배치와 동시에 타이머 리셋 및 자동 조리 시작
        currentCookingTime = cookingTime;
        isCooking = true;
        UpdateCookingTimeText();
    }

    /// <summary>
    /// FoodData 기반의 원재료 표시 오브젝트를 생성하는 함수
    /// 생성된 오브젝트는 스테이션의 자식으로 배치됨
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        GameObject ingredientObj = new GameObject(data.displayName);
        // 스테이션 오브젝트의 자식으로 설정
        ingredientObj.transform.SetParent(transform);
        ingredientObj.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = ingredientObj.AddComponent<SpriteRenderer>();
        if (data.icon != null)
        {
            sr.sprite = data.icon;
        }
        else
        {
            sr.color = Color.gray;  // 스프라이트가 없는 경우 회색으로 표시
        }

        // 박스 콜라이더2D 추가
        ingredientObj.AddComponent<BoxCollider2D>();

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
    /// 이후 원재료 표시 오브젝트는 삭제됨
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
    /// 조리 완료 후 스테이션 리셋 처리 (다음 재료 수령을 위해)
    /// </summary>
    private void ResetStation()
    {
        isCooking = false;
        currentFoodData = null;
        currentCookingTime = cookingTime;
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }
        UpdateCookingTimeText();
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
        isCooking = false;
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