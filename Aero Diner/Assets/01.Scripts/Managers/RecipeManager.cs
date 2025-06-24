using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 선택된 레시피 리스트(selectedRecipes)와 현재 스테이션, 제공된 재료 목록을 바탕으로
/// 조리 가능한 레시피를 판단하는 매니저
/// </summary>
public class RecipeManager : Singleton<RecipeManager>
{
    /// <summary>
    /// 스테이션 데이터에 등록된 supported 레시피를 기준으로,
    /// 제공된 재료 목록에 맞춰 조리 가능한 레시피들을 반환
    /// </summary>
    public List<MenuData> GetCookableRecipes(StationData stationData, List<string> ingredients, List<MenuData> selectedRecipes)
    {
        List<MenuData> cookableRecipes = new List<MenuData>();

        foreach (var recipe in selectedRecipes)
        {
            // 현재 스테이션에 해당 레시피가 할당되어 있는지 확인
            if (!stationData.availableRecipes.Contains(recipe))
                continue;

            // 레시피에 필요한 모든 재료가 제공된 재료 목록에 포함되어 있어야 함
            if (recipe.ingredients.All(id => ingredients.Contains(id)))
            {
                cookableRecipes.Add(recipe);
            }
        }
        return cookableRecipes;
    }

    /// <summary>
    /// 선택된 레시피 목록 중에서, 
    /// 스테이션이 지원하는 레시피에 한해 제공된 재료와의 일치도를 기준으로 
    /// 가장 많은 재료가 일치하는 레시피를 반환
    /// </summary>
    public MenuData TrySetRecipe(StationData stationData, List<string> ingredients, List<MenuData> selectedRecipes)
    {
        return selectedRecipes
            .Where(recipe => stationData.availableRecipes.Contains(recipe))
            .OrderByDescending(recipe => recipe.ingredients.Count(id => ingredients.Contains(id)))
            .FirstOrDefault();
    }
}