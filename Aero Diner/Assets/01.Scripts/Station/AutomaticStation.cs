using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어가 올바른 재료를 스테이션에 배치하면
/// 자동으로 조리 타이머가 시작되고, 시간이 다 되면 결과물이 생성되는 자동 조리 스테이션
/// </summary>
public class AutomaticStation : MonoBehaviour
{
    [Header("가공 할 재료 SG (요구되는 재료 그룹)")]
    public IngredientSOGroup NeededIngredients;

    [Header("요리 시간 (초)")]
    public float cookingTime = 5f;

    [Header("Cooking Time UI Text")]
    public Text cookingTimeText;

    [Header("스테이션 타입")]
    public CookingStation stationType;

    [Header("현재 등록된 재료 ID 목록")]
    public List<string> currentIngredients = new List<string>();

    // 현재 조리 타이머 값
    private float currentCookingTime;

    // 현재 조리 중인지 여부
    private bool isCooking = false;

    // 시각적으로 생성된 재료 오브젝트
    private GameObject placedIngredientObj;

    // 현재 조리 대상이 되는 재료 데이터
    private FoodData currentFoodData;

    private void Start()
    {
        // 타이머 초기화 및 UI 갱신
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();
    }

    private void Update()
    {
        // 조리 중이며 재료가 올려져 있을 때만 타이머 진행
        if (isCooking && placedIngredientObj != null && currentFoodData != null)
        {
            // 남은 시간 감소
            currentCookingTime -= Time.deltaTime;
            UpdateCookingTimeText();

            // 조리 완료 시 가공 처리 후 스테이션 리셋
            if (currentCookingTime <= 0f)
            {
                ProcessIngredient(currentFoodData);
                ResetStation();
            }
        }
    }

    /// <summary>
    /// 플레이어가 재료를 내려놓았을 때 호출됨
    /// 재료 오브젝트를 생성하고 조리를 시작함
    /// </summary>
    public void PlaceIngredient(FoodData data)
    {
        if (currentFoodData != null)
        {
            Debug.Log("이미 재료가 배치되어 있습니다.");
            return;
        }

        if (NeededIngredients != null && !NeededIngredients.Contains(data))
        {
            Debug.Log("제공된 재료가 요구되는 그룹에 속하지 않습니다.");
            return;
        }

        currentFoodData = data;
        placedIngredientObj = CreateIngredientDisplay(data);

        if (!currentIngredients.Contains(data.id))
            currentIngredients.Add(data.id);

        // 레시피 판단을 직접 싱글톤 RecipeManager에서 실행
        RecipeData selectedRecipe = RecipeManager.Instance.TrySetRecipe(stationType, currentIngredients);
        if (selectedRecipe != null)
        {
            Debug.Log($"레시피 '{selectedRecipe.recipeName}' 가능!");
        }
        else
        {
            Debug.Log("조건에 맞는 레시피 없음");
        }

        currentCookingTime = cookingTime;
        isCooking = true;
        UpdateCookingTimeText();
    }


    /// <summary>
    /// 재료 데이터를 기반으로 화면에 보여질 재료 오브젝트 생성
    /// </summary>
    private GameObject CreateIngredientDisplay(FoodData data)
    {
        GameObject obj = new GameObject(data.displayName);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = data.icon ?? null;

        if (data.icon == null)
            sr.color = Color.gray;

        obj.AddComponent<BoxCollider2D>();
        Rigidbody2D rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        return obj;
    }

    /// <summary>
    /// 조리 완료 시 호출됨. 기존 오브젝트 제거 및 결과 출력
    /// </summary>
    private void ProcessIngredient(FoodData data)
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        Debug.Log("가공 완료된 재료 생성됨: " + data.displayName);
        // TODO: 실제 결과 아이템 Instantiate 가능
    }

    /// <summary>
    /// 조리 완료 또는 리셋 조건일 때 스테이션 상태 초기화
    /// </summary>
    private void ResetStation()
    {
        isCooking = false;
        currentFoodData = null;
        currentCookingTime = cookingTime;
        currentIngredients.Clear(); // 재료 목록도 초기화
        UpdateCookingTimeText();
    }

    /// <summary>
    /// 남은 조리 시간을 UI에 반영
    /// </summary>
    private void UpdateCookingTimeText()
    {
        if (cookingTimeText != null)
            cookingTimeText.text = currentCookingTime.ToString("F1");
    }

    /// <summary>
    /// 플레이어가 재료를 들었을 때 스테이션 상태 초기화
    /// </summary>
    public void OnPlayerPickup()
    {
        if (placedIngredientObj != null)
        {
            Destroy(placedIngredientObj);
            placedIngredientObj = null;
        }

        currentFoodData = null;
        isCooking = false;
        currentCookingTime = cookingTime;
        UpdateCookingTimeText();

        Debug.Log("플레이어가 물체를 들었습니다. 스테이션 상태 초기화됨.");
    }

    public void OnHoverEnter() { }  // 커서 올릴 때 시각 효과 필요 시 구현
    public void OnHoverExit() { }   // 커서 빠질 때 효과 제거 처리
}