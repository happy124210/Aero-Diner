using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Recipe Group", menuName = "Game Data/Recipe Data SO Group")]
public class RecipeDataSOGroup : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public RecipeData recipeData; // 여기서 RecipeData로 캐스팅할 수 있도록!
    }

    public List<Entry> entries; // <- 실제 데이터 리스트

    /// <summary>
    /// 간편하게 레시피 리스트로 접근할 수 있도록 헬퍼 프로퍼티
    /// </summary>
    public List<RecipeData> Recipes => entries
        .Where(e => e.recipeData != null)
        .Select(e => e.recipeData)
        .ToList();
}
