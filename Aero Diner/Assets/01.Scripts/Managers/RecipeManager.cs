using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 모든 FoodData와 레시피를 관리.
/// </summary>
public class RecipeManager : Singleton<RecipeManager>
{
    public Dictionary<string, FoodData> FoodDatabase { get; private set; } = new ();

    [Header("Debug")]
    private bool showDebugInfo;
    
    protected override void Awake()
    {
        base.Awake();
        LoadFoodDatabase();
    }

    /// <summary>
    /// 모든 FoodData를 딕셔너리에 로드 (ID, FoodData)
    /// </summary>
    private void LoadFoodDatabase()
    {
        FoodData[] allFoods = Resources.LoadAll<FoodData>("Datas/Food");
    
        FoodDatabase.Clear();
        foreach (var food in allFoods)
        {
            if (!string.IsNullOrEmpty(food.id))
            {
                FoodDatabase[food.id] = food;
            }
        }
    
        if (showDebugInfo) Debug.Log($"[MenuManager] {FoodDatabase.Count}개 음식 데이터 로드 완료");
    }

    #region 레시피 원재료 찾기

    /// <summary>
    /// 현재 메뉴들의 모든 원재료 리스트 반환
    /// </summary>
    public List<FoodData> GetAllRecipe(List<Menu> availableMenus)
    {
        HashSet<string> rawIngredientIds = new HashSet<string>();
    
        // 모든 메뉴에 대해 원재료 수집
        foreach (var menu in availableMenus)
        {
            if (menu != null)
            {
                CollectRawIngredients(menu.foodData.id, rawIngredientIds);
            }
        }
    
        // ID를 실제 FoodData로 변환
        List<FoodData> result = new List<FoodData>();
        foreach (string id in rawIngredientIds)
        {
            if (FoodDatabase.TryGetValue(id, out var food))
            {
                result.Add(food);
            }
        }
    
        return result;
    }

    /// <summary>
    /// 특정 음식의 모든 원재료를 재귀적으로 수집
    /// </summary>
    private void CollectRawIngredients(string foodId, HashSet<string> collectedIds)
    {
        if (string.IsNullOrEmpty(foodId) || !FoodDatabase.TryGetValue(foodId, out var food))
            return;

        // 원재료거나 ingredients가 없으면 최종 재료
        if (food.foodType == FoodType.Raw || 
            food.ingredients == null || 
            food.ingredients.Length == 0)
        {
            collectedIds.Add(foodId);
            return;
        }
    
        // ingredients가 있으면 재귀적으로 탐색
        foreach (string ingredientId in food.ingredients)
        {
            CollectRawIngredients(ingredientId, collectedIds);
        }
    }

    /// <summary>
    /// 특정 메뉴 하나의 원재료만 가져오기
    /// </summary>
    public List<FoodData> GetRawIngredientsForMenu(FoodData menu)
    {
        if (menu == null) return new List<FoodData>();
    
        HashSet<string> rawIngredientIds = new HashSet<string>();
        CollectRawIngredients(menu.id, rawIngredientIds);
    
        List<FoodData> result = new List<FoodData>();
        foreach (string id in rawIngredientIds)
        {
            if (FoodDatabase.TryGetValue(id, out var food))
            {
                result.Add(food);
            }
        }
    
        return result;
    }

    #endregion
    
    #region 레시피 관리

    /// <summary>
    /// 유저가 선택한 재료 ID 리스트를 기반으로, 후보 레시피 중 일치하는 레시피를 찾아 반환
    /// </summary>
    /// <param name="candidateRecipes">레시피 후보 리스트</param>
    /// <param name="ingredientIds">유저가 선택한 재료 ID 리스트</param>
    /// <returns>일치율 순으로 정렬된 레시피 결과 리스트</returns>
    public List<RecipeMatchResult> FindMatchingRecipes(List<FoodData> candidateRecipes, List<string> ingredientIds)
    {
        var matches = candidateRecipes
            // 재료가 null이 아니고 최소 1개 이상일 때만 필터링
            .Where(r => r.ingredients != null && r.ingredients.Length > 0)
            // 각 레시피에 대해 유저 재료와 몇 개나 일치하는지 계산
            .Select(r =>
            {
                int matchedCount = r.ingredients.Count(ingredientIds.Contains); // 재료 일치 개수 계산
                return new RecipeMatchResult
                {
                    recipe = r,
                    matchedCount = matchedCount,
                    totalRequired = r.ingredients.Length
                };
            })
            // 일치하는 재료가 하나라도 있으면 필터링
            .Where(r => r.matchedCount > 0)
            .ToList();

        // 일치율이 높은 순, 그 다음 일치 개수가 많은 순으로 정렬하여 반환
        return matches
            .OrderByDescending(r => r.MatchRatio)
            .ThenByDescending(r => r.matchedCount)
            .ToList();
    }

    /// <summary>
    /// 개별 레시피와 유저 재료의 일치 결과 정보를 담는 클래스
    /// </summary>
    public class RecipeMatchResult
    {
        public FoodData recipe;           // 해당 레시피 데이터
        public int matchedCount;          // 유저 재료와 일치한 재료 개수
        public int totalRequired;         // 해당 레시피가 요구하는 전체 재료 개수

        // 전체 재료 중 일치한 재료의 비율을 계산
        public float MatchRatio => totalRequired == 0 ? 0f : (float)matchedCount / totalRequired;
    }

    #endregion
}