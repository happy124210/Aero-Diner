using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RecipeManager는 사용자가 선택한 재료에 맞는 레시피를 찾아주는 기능을 제공
/// </summary>
public class RecipeManager : Singleton<RecipeManager>
{
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
}