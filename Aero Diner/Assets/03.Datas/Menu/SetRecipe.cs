using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 게임에서 사용할 레시피 세트를 보관하는 매니저
/// 싱글톤으로 어디서든 접근 가능
/// </summary>
public class SetRecipe : Singleton<SetRecipe>
{
    [Header("게임에서 사용할 선택된 레시피 리스트")]
    public List<RecipeData> selectedRecipes; // 게임 시작 시 수동 또는 초기 설정
}