using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
/// <summary>
/// 게임 데이터베이스 CSV 임포터
/// 손님, 레시피, 음식/재료 데이터를 CSV에서 ScriptableObject로 변환
/// </summary>
public class CSVImporter
{
    #region CustomerData 생성

    [MenuItem("Tools/Import Game Data/Customer Data")]
    public static void ImportCustomerData()
    {
        ImportData("CustomerData", ParseCustomerData, "Customer");
    }

    private static CustomerData ParseCustomerData(string[] cols)
    {
        var data = ScriptableObject.CreateInstance<CustomerData>();
        
        // 손님 데이터 파싱
        data.id = cols[0].Trim();
        data.customerName = cols[1].Trim();
        data.displayName = cols[2].Trim();
        data.speed = float.Parse(cols[3]);
        data.waitTime = float.Parse(cols[4]);
        data.eatTime = float.Parse(cols[5]);
        
        return data;
    }
    
    #endregion

    #region StationData 생성

    [MenuItem("Tools/Import Game Data/Station Data")]
    public static void ImportStationData()
    {
        ImportData("StationData", ParseStationData, "Station");
    }

    private static StationData ParseStationData(string[] cols)
    {
        var data = ScriptableObject.CreateInstance<StationData>();

        // 레시피 데이터 파싱
        data.id = cols[0].Trim();
        data.stationName = cols[1].Trim();
        data.displayName = cols[2].Trim();
        data.stationType = (StationType)Enum.Parse(typeof(StationType), cols[3].Trim());
        data.workType = (WorkType)Enum.Parse(typeof(WorkType), cols[4].Trim());
        data.stationIcon = LoadIcon($"{data.stationName}-icon", "Station");
        data.description = cols[5].Trim();
        data.stationCost = int.Parse(cols[6]);

        return data;
    }

    #endregion

    #region FoodData 생성

    [MenuItem("Tools/Import Game Data/Food Data")]
    public static void ImportFoodData()
    {
        ImportData("FoodData", ParseFoodData, "Food");
    }

    private static FoodData ParseFoodData(string[] cols)
    {
        var data = ScriptableObject.CreateInstance<FoodData>();
        
        // 음식 데이터 파싱
        data.id = cols[0].Trim();
        data.foodName = cols[1].Trim();
        data.displayName = cols[2].Trim();
        data.foodType = (FoodType)Enum.Parse(typeof(FoodType), cols[3].Trim());
        data.foodIcon = LoadIcon($"{data.foodName}-icon", "Food"); // Resources에서 아이콘 로드
        data.description = cols[4].Trim();
        data.stationType = ParseEnumArray<StationType>(cols[5]); // StationType enum값 parse
        data.ingredients = ParseStringArray(cols[6]);
        data.cookTime = float.Parse(cols[7]);
        data.foodCost = int.Parse(cols[8]);
        
        return data;
    }
    
    #endregion
    
    #region 공통 Import 메서드
    
    /// <summary>
    /// 제네릭 데이터 임포트 메서드
    /// </summary>
    private static void ImportData<T>(string csvName, Func<string[], T> parseFunc, string folderName) where T : ScriptableObject
    {
        string path = EditorUtility.OpenFilePanel($"Select {csvName} CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1) 
        {
            Debug.LogWarning("CSV has no data.");
            return;
        }
        
        string targetFolder = $"Assets/03.Datas/{folderName}/";
        if (!Directory.Exists(targetFolder)) 
        {
            Directory.CreateDirectory(targetFolder);
        }
        
        int successCount = 0;
        for (int i = 1; i < lines.Length; i++) 
        {
            try
            {
                string[] cols = lines[i].Split(',');
                if (cols.Length < 2 || string.IsNullOrEmpty(cols[0].Trim())) continue; // 빈 행 스킵
                
                T data = parseFunc(cols);
                
                // name 열을 기반으로 파일명 생성
                string fileName = ToPascalDataName(cols[1].Trim());
                string assetPath = $"{targetFolder}/{fileName}.asset";
                
                // 기존 에셋이 있으면 업데이트, 없으면 새로 생성
                var existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(data, existing);
                    EditorUtility.SetDirty(existing);
                    Debug.Log($"Updated: {fileName}");
                }
                else
                {
                    AssetDatabase.CreateAsset(data, assetPath);
                    Debug.Log($"Created: {fileName}");
                }
                successCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing line {i}: {lines[i]}\nError: {e.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{csvName} CSV import completed! {successCount} items processed.");
    }
    
    #endregion
    
    #region 유틸리티 메서드들
    
    /// <summary>
    /// ID를 PascalCase + "Data" 형태의 파일명으로 변환
    /// ex: "customer-001" -> "Customer001Data"
    /// </summary>
    private static string ToPascalDataName(string id)
    {
        if (string.IsNullOrEmpty(id)) return "UnknownData";
        
        // 하이픈이나 언더스코어로 분리
        string[] parts = id.Split(new [] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        
        // 각 단어의 첫 글자를 대문자로 만들고 이어붙임
        string pascal = string.Concat(parts.Select(part =>
        {
            if (string.IsNullOrEmpty(part)) return "";
            return char.ToUpper(part[0]) + part.Substring(1).ToLower();
        }));

        return pascal + "Data";
    }
    
    /// <summary>
    /// Resources 폴더에서 아이콘 로드
    /// </summary>
    private static Sprite LoadIcon(string iconName, string category = "")
    {
        if (string.IsNullOrEmpty(iconName)) return null;
        
        string path = string.IsNullOrEmpty(category) 
            ? $"Icons/{iconName.Trim()}" 
            : $"Icons/{category}/{iconName.Trim()}";
            
        Sprite icon = Resources.Load<Sprite>(path);
        if (icon == null)
        {
            Debug.LogWarning($"[LoadIcon] Icon 없어요!!!: Resources/{path}");
            //Debug.LogWarning($"Resources/{path}.png 형태 있나 확인해주세요 !!!");
        }
        else
        {
            Debug.Log($"[LoadIcon] 로드 완료: {path}");
        }
        return icon;
    }
    
    /// <summary>
    /// 문자열 배열 파싱 (파이프 구분자 사용)
    /// ex: "carrot|potato|onion" -> ["carrot", "potato", "onion"]
    /// </summary>
    private static string[] ParseStringArray(string value)
    {
        if (string.IsNullOrEmpty(value)) return Array.Empty<string>();
        return value.Split('|').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
    }
    
    /// <summary>
    /// float 배열 파싱 (파이프 구분자 사용)
    /// </summary>
    public static float[] ParseFloatArray(string value)
    {
        if (string.IsNullOrEmpty(value)) return Array.Empty<float>();
        return value.Split('|').Select(s => float.Parse(s.Trim())).ToArray();
    }

    private static TEnum[] ParseEnumArray<TEnum>(string value) where TEnum : struct
    {
        string[] tokens = value.Split('|');
        return tokens.Select(t => t.Trim())
            .Where(t=> Enum.TryParse<TEnum>(t, out _))
            .Select(t => Enum.Parse<TEnum>(t))
            .ToArray();
    }
    
    #endregion
}
#endif