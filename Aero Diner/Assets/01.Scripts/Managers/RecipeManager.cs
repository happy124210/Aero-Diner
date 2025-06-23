using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 모든 레시피 데이터를 관리하고
/// 조리가 가능한 레시피를 판단하는 매니저
/// </summary>
public class RecipeManager : Singleton<RecipeManager>
{
    [Header("모든 레시피 데이터 그룹")]
    public RecipeDataSOGroup[] allRecipes;

    /// <summary>
    /// 해당 스테이션에서 현재 제공된 재료로 만들 수 있는 레시피들을 반환
    /// </summary>
    public List<RecipeData> GetCookableRecipes(CookingStation station, List<string> availableIngredients)
    {
        List<RecipeData> cookableRecipes = new List<RecipeData>();

        foreach (var group in allRecipes)
        {
            foreach (var recipe in group.Recipes)
            {
                // 1. 스테이션이 일치하거나, None일 경우 허용
                if (recipe.requireStation != CookingStation.None && recipe.requireStation != station)
                    continue;

                // 2. 모든 재료가 사용 가능한 재료 목록에 존재하는지 확인
                if (recipe.ingredients.All(ingredient => availableIngredients.Contains(ingredient)))
                {
                    cookableRecipes.Add(recipe);
                }
            }
        }

        return cookableRecipes;
    }

    /// <summary>
    /// 현재 스테이션 타입과 등록된 재료를 바탕으로
    /// 가능한 레시피 목록을 판단하고 첫 번째 레시피를 반환
    /// </summary>
    public RecipeData TrySetRecipe(CookingStation station, List<string> ingredients)
    {
        var recipes = GetCookableRecipes(station, ingredients);
        return recipes.FirstOrDefault(); // 가능하다면 첫 번째 레시피 반환, 없으면 null
    }
}