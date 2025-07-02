using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class StationRecipeAssigner
{
    [MenuItem("Tools/Editor/Assign Recipes To Stations")]
    public static void AssignRecipes()
    {
        // 모든 StationData 에셋을 불러오기
        string[] stationGUIDs = AssetDatabase.FindAssets("t:StationData", new[] { "Assets/03.Datas/Station" });
        string[] foodGUIDs = AssetDatabase.FindAssets("t:FoodData", new[] { "Assets/03.Datas/Food" });

        // 모든 FoodData 에셋 캐싱
        var allFoods = new List<FoodData>();
        foreach (var guid in foodGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var food = AssetDatabase.LoadAssetAtPath<FoodData>(path);
            if (food != null)
                allFoods.Add(food);
        }

        int totalAssigned = 0;

        // 스테이션 하나씩 처리
        foreach (var stationGUID in stationGUIDs)
        {
            string stationPath = AssetDatabase.GUIDToAssetPath(stationGUID);
            var station = AssetDatabase.LoadAssetAtPath<StationData>(stationPath);
            if (station == null) continue;

            // 매칭되는 레시피 필터링
            var matchingFoods = allFoods.FindAll(f => f.stationType != null && f.stationType.Contains(station.stationType));

            // 레시피 할당
            station.availableRecipes = matchingFoods;
            EditorUtility.SetDirty(station);
            totalAssigned += matchingFoods.Count;

            Debug.Log($"Assigned {matchingFoods.Count} recipes to '{station.stationName}' ({station.stationType})");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Done! Total {totalAssigned} recipes assigned to StationData assets.");
    }
}