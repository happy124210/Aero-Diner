using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 재료와 레시피 후보들을 바탕으로 적합한 레시피들을 찾는 매니저
/// </summary>
public class RecipeManager : Singleton<RecipeManager>
{
    public List<RecipeMatchResult> FindMatchingRecipes(List<FoodData> candidateRecipes, List<string> ingredientIds)
    {
        if (candidateRecipes == null || ingredientIds == null)
        {
            Debug.LogWarning("[RecipeManager] 입력값이 null입니다.");
            return new List<RecipeMatchResult>();
        }

        return candidateRecipes
            .Where(r => r.ingredients != null)
            .Select(r => new RecipeMatchResult
            {
                recipe = r,
                matchedCount = r.ingredients.Count(ingredientIds.Contains),
                totalRequired = r.ingredients.Length
            })
            .Where(r => r.matchedCount > 0)
            .OrderByDescending(r => r.MatchRatio)
            .ThenByDescending(r => r.matchedCount)
            .ToList();
    }
}

public class RecipeMatchResult
{
    public FoodData recipe;
    public int matchedCount;
    public int totalRequired;
    public float MatchRatio => totalRequired == 0 ? 0f : (float)matchedCount / totalRequired;
}