using UnityEditor;
using UnityEngine;
using System.IO;

public class FoodSOGroupPopulator
{
    [MenuItem("Tools/Editor/Populate FoodSOGroup")]
    public static void PopulateGroup()
    {
        // FoodSOGroup 에셋 불러오기
        var group = AssetDatabase.LoadAssetAtPath<FoodSOGroup>("Assets/03.Datas/SOGroup/FoodGroup.asset");
        if (group == null)
        {
            Debug.LogError("FoodGroup.asset 파일을 찾을 수 없습니다!");
            return;
        }

        // FoodData 에셋들 불러오기
        string[] guids = AssetDatabase.FindAssets("t:FoodData", new[] { "Assets/03.Datas/Food" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FoodData foodData = AssetDatabase.LoadAssetAtPath<FoodData>(path);
            if (foodData != null)
            {
                group.AddIngredient(foodData);
            }
        }

        EditorUtility.SetDirty(group);
        AssetDatabase.SaveAssets();
        Debug.Log($"FoodSOGroup에 푸드 데이터를 추가했습니다.");
    }
}