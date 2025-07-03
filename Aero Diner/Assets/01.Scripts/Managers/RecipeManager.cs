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

        var matches = candidateRecipes
            .Where(r => r.ingredients != null)
            .Select(r =>
            {
                var matchedCount = r.ingredients.Count(ingredientIds.Contains);
                var result = new RecipeMatchResult
                {
                    recipe = r,
                    matchedCount = matchedCount,
                    totalRequired = r.ingredients.Length
                };

                Debug.Log($"[RecipeManager] Checking recipe: {r.foodName}");
                Debug.Log($" - Required: {string.Join(", ", r.ingredients)}");
                Debug.Log($" - Given:   {string.Join(", ", ingredientIds)}");
                Debug.Log($" - Match: {matchedCount}/{r.ingredients.Length} | MatchRatio: {result.MatchRatio:F2}");

                return result;
            })
            .Where(r => r.matchedCount > 0)
            .ToList();

        var fullMatches = matches
            .Where(r => r.matchedCount == r.totalRequired && ingredientIds.Count == r.totalRequired)
            .ToList();

        if (fullMatches.Count > 0)
        {
            Debug.Log($"[RecipeManager] 정확 일치 레시피 {fullMatches.Count}개");
            return fullMatches;
        }

        Debug.Log("[RecipeManager] 정확 일치 없음 — 유사도 기반 결과 반환");
        return matches
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