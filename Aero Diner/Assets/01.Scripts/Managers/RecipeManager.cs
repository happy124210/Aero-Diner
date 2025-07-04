using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeManager : Singleton<RecipeManager>
{
    public List<RecipeMatchResult> FindMatchingRecipes(List<FoodData> candidateRecipes, List<string> ingredientIds)
    {
        var matches = candidateRecipes
            .Where(r => r.ingredients != null && r.ingredients.Length > 0)
            .Select(r =>
            {
                int matchedCount = r.ingredients.Count(ingredientIds.Contains);
                return new RecipeMatchResult
                {
                    recipe = r,
                    matchedCount = matchedCount,
                    totalRequired = r.ingredients.Length
                };
            })
            .Where(r => r.matchedCount > 0)
            .ToList();

        return matches
            .OrderByDescending(r => r.MatchRatio)
            .ThenByDescending(r => r.matchedCount)
            .ToList();
    }

    public class RecipeMatchResult
    {
        public FoodData recipe;
        public int matchedCount;
        public int totalRequired;
        public float MatchRatio => totalRequired == 0 ? 0f : (float)matchedCount / totalRequired;
    }
}