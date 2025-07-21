using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
/// <summary>
/// 게임 데이터베이스 CSV 임포터
/// 손님, 레시피, 음식/재료, 이벤트 데이터 등을 CSV에서 ScriptableObject로 변환
/// </summary>
public class CSVImporter
{
    // --- 데이터 타입별 임포트 메뉴 ---

    [MenuItem("Tools/Import Game Data/Customer Data")]
    public static void ImportCustomerData() => ImportData("CustomerData", ParseCustomerData, "Datas/Customer");

    [MenuItem("Tools/Import Game Data/Station Data")]
    public static void ImportStationData() => ImportData("StationData", ParseStationData, "Datas/Station");

    [MenuItem("Tools/Import Game Data/Food Data")]
    public static void ImportFoodData() => ImportData("FoodData", ParseFoodData, "Datas/Food");

    [MenuItem("Tools/Import Game Data/Dialogue Data")]
    public static void ImportDialogueData() => ImportGroupedData<DialogueData>("DialogueData", ParseDialogueLines, "Datas/Dialogue");

    // --- 데이터 타입별 파싱 로직 ---

    #region Parser Functions

    private static CustomerData ParseCustomerData(string[] cols)
    {
        var data = ScriptableObject.CreateInstance<CustomerData>();
        data.id = cols[0].Trim();
        data.customerName = cols[1].Trim();
        data.displayName = cols[2].Trim();
        data.speed = float.Parse(cols[3]);
        data.waitTime = float.Parse(cols[4]);
        data.eatTime = float.Parse(cols[5]);
        return data;
    }

    private static StationData ParseStationData(string[] cols)
    {
        var data = ScriptableObject.CreateInstance<StationData>();
        data.id = cols[0].Trim();
        data.stationName = cols[1].Trim();
        data.displayName = cols[2].Trim();
        data.stationType = (StationType)Enum.Parse(typeof(StationType), cols[3].Trim());
        data.workType = (WorkType)Enum.Parse(typeof(WorkType), cols[4].Trim());
        data.stationIcon = LoadIcon($"{data.stationName}", "Station");
        data.description = cols[5].Trim();
        data.stationCost = int.Parse(cols[6]);
        return data;
    }

    private static FoodData ParseFoodData(string[] cols)
    {
        var data = ScriptableObject.CreateInstance<FoodData>();
        data.id = cols[0].Trim();
        data.foodName = cols[1].Trim();
        data.displayName = cols[2].Trim();
        data.foodType = (FoodType)Enum.Parse(typeof(FoodType), cols[3].Trim());
        data.foodIcon = LoadIcon($"{data.foodName}", "Food");
        data.description = cols[4].Trim();
        data.stationType = ParseEnumArray<StationType>(cols[5]);
        data.ingredients = ParseStringArray(cols[6]);
        data.cookTime = float.Parse(cols[7]);
        data.foodCost = int.Parse(cols[8]);
        return data;
    }

    #endregion
    
    // --- 핵심 임포트 로직 ---

    #region Core Importer Methods

    /// <summary>
    /// 한 줄이 하나의 SO가 되는 일반적인 데이터
    /// </summary>
    private static void ImportData<T>(string csvName, Func<string[], T> parseFunc, string folderName) where T : Object, IData
    {
        string path = EditorUtility.OpenFilePanel($"Select {csvName} CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV has no data.");
            return;
        }

        string targetFolder = $"Assets/Resources/{folderName}/";
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
                if (cols.Length < 1 || string.IsNullOrEmpty(cols[0].Trim())) continue;

                T data = parseFunc(cols);
                
                string fileName = ToPascalDataName(cols[1].Trim());
                string assetPath = $"{targetFolder}/{fileName}.asset";

                var existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(data, existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(data, assetPath);
                }
                EditorUtility.SetDirty(data);
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
    
    /// <summary>
    /// 여러 줄이 하나의 SO가 되는 그룹화된 데이터 임포트
    /// </summary>
    private static void ImportGroupedData<T>(string csvName, Action<T, string[]> parseLineFunc, string folderName) where T : ScriptableObject, IData
    {
        string path = EditorUtility.OpenFilePanel($"Select {csvName} CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV has no data.");
            return;
        }

        string targetFolder = $"Assets/Resources/{folderName}/";
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        // ID를 기준으로 모든 데이터 줄을 그룹화
        var groupedLines = lines.Skip(1)
                                .Select(line => line.Split(','))
                                .Where(cols => cols.Length > 0 && !string.IsNullOrEmpty(cols[0]))
                                .GroupBy(cols => cols[0].Trim());

        int successCount = 0;
        foreach (var group in groupedLines)
        {
            try
            {
                string id = group.Key;
                string assetPath = $"{targetFolder}/{id}.asset";
                
                T data = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<T>();
                    AssetDatabase.CreateAsset(data, assetPath);
                }
                data.id = id;

                // 데이터 파싱 함수 호출
                foreach(var cols in group)
                {
                    parseLineFunc(data, cols);
                }

                EditorUtility.SetDirty(data);
                successCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing group '{group.Key}': {e.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{csvName} grouped CSV import completed! {successCount} groups processed.");
    }
    
    /// <summary>
    /// DialogueData의 각 줄을 파싱
    /// </summary>
    private static void ParseDialogueLines(DialogueData data, string[] cols)
    {
        // 새로운 ID의 데이터가 시작될 때 리스트를 초기화
        if (data.lines == null)
        {
            data.lines = new List<DialogueLine>();
            data.choices = new List<DialogueChoice>();
            data.nextEventType = EventType.None;
            data.nextEventParameter = string.Empty;
        }

        data.lines.Add(new DialogueLine
        {
            speakerId = cols[2].Trim(),
            expression = cols[3].Trim(),
            text = cols[4].Trim().Replace("\"", "")
        });

        // 마지막 줄일 경우 이벤트 및 선택지 파싱
        if (cols.Length > 5 && !string.IsNullOrEmpty(cols[5].Trim()))
        {
            if (Enum.TryParse(cols[5].Trim(), true, out EventType parsedEvent))
            {
                data.nextEventType = parsedEvent;
                data.nextEventParameter = cols.Length > 6 ? cols[6].Trim() : "";
            }
        }
        for (int i = 0; i < 2; i++)
        {
            int textIndex = 7 + i;
            if (cols.Length > textIndex && !string.IsNullOrEmpty(cols[textIndex].Trim()))
            {
                data.choices.Add(new DialogueChoice
                {
                    text = cols[textIndex].Trim(),
                    nextDialogueId = $"{data.id}_choice{i + 1}"
                });
            }
        }
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
    private static Sprite LoadIcon(string name, string category = "")
    {
        if (string.IsNullOrEmpty(name)) return null;
        string iconName = name.PascalToSnake();
        
        string path = string.IsNullOrEmpty(category) 
            ? $"Icons/{iconName.Trim()}" 
            : $"Icons/{category}/{iconName.Trim()}";
            
        Sprite icon = Resources.Load<Sprite>(path);
        if (icon == null)
        {
            Debug.LogWarning($"[LoadIcon] Icon 없어요!!!: Resources/{path}");
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

/// <summary>
/// 모든 데이터 SO가 id를 갖도록 강제하는 인터페이스
/// </summary>
public interface IData
{
    string id { get; set; }
}

#endif