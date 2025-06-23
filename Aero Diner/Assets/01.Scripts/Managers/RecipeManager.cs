using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SetRecipe로부터 넘겨받은 레시피 리스트를 바탕으로,
/// 현재 스테이션과 제공된 재료 목록으로 조리 가능한 레시피를 판단하는 매니저
/// </summary>
public class RecipeManager : Singleton<RecipeManager>
{
    /// <summary>
    /// 사용 가능한 레시피 리스트(selectedRecipes)에서,  
    /// 해당 스테이션에 맞고 (또는 None인 경우 허용)  
    /// 모든 필요한 재료가 현재 등록된 재료 목록에 포함되어 있는 레시피들을 반환
    /// </summary>
    public List<RecipeData> GetCookableRecipes(CookingStation station, List<string> ingredients, List<RecipeData> selectedRecipes)
    {
        List<RecipeData> cookableRecipes = new List<RecipeData>();

        foreach (var recipe in selectedRecipes)
        {
            // 스테이션이 일치하거나 None이면 허용
            if (recipe.requireStation != CookingStation.None && recipe.requireStation != station)
                continue;

            // 레시피에 필요한 모든 재료가 ingredients에 포함되어 있어야 함
            if (recipe.ingredients.All(id => ingredients.Contains(id)))
            {
                cookableRecipes.Add(recipe);
            }
        }
        return cookableRecipes;
    }

    /// <summary>
    /// 사용 가능한 레시피 리스트(selectedRecipes) 중에서,
    /// 현재 등록된 재료와의 일치도를 기준으로 가장 많은 재료가 일치하는 레시피를 반환
    /// 부분 일치 기반으로 후보를 정렬하여 첫 번째 후보를 선택
    /// </summary>
    public RecipeData TrySetRecipe(CookingStation station, List<string> ingredients, List<RecipeData> selectedRecipes)
    {
        return selectedRecipes
            .Where(recipe => recipe.requireStation == CookingStation.None || recipe.requireStation == station)
            .OrderByDescending(recipe => recipe.ingredients.Count(id => ingredients.Contains(id)))
            .FirstOrDefault();
    }
}