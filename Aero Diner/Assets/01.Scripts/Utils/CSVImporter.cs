using System;
using System.Collections.Generic;
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
        data.stationIcon = LoadIcon($"{data.stationName}", "Station");
        data.description = cols[5].Trim();

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
        data.foodIcon = LoadIcon($"{data.foodName}", "Food"); // Resources에서 아이콘 로드
        data.description = cols[4].Trim();
        data.stationType = ParseEnumArray<StationType>(cols[5]); // StationType enum값 parse
        data.ingredients = ParseStringArray(cols[6]);
        data.cookTime = float.Parse(cols[7]);
        data.foodCost = int.Parse(cols[8]);
        
        return data;
    }
    
    #endregion

    #region DialogueData 생성

    [MenuItem("Tools/Import Game Data/Dialogue Data")]
    public static void ImportGroupedDialogueData()
    {
        string path = EditorUtility.OpenFilePanel("Select Dialogue CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일에 데이터가 없습니다.");
            return;
        }

        // csv 파일의 모든 라인을 첫 번째 열(id)을 기준으로 그룹화
        var groupedById = new Dictionary<string, List<string[]>>();
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 1 || string.IsNullOrEmpty(cols[0].Trim())) continue;

            string dialogueId = cols[0].Trim();
            if (!groupedById.ContainsKey(dialogueId))
            {
                groupedById[dialogueId] = new List<string[]>();
            }
            groupedById[dialogueId].Add(cols);
        }

        string targetFolder = "Assets/Resources/Datas/Dialogue/";
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        // 에셋 생성
        foreach (var dialogueId in groupedById.Keys)
        {
            string assetPath = $"{targetFolder}/{dialogueId}.asset";
            if (AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath) == null)
            {
                DialogueData newAsset = ScriptableObject.CreateInstance<DialogueData>();
                AssetDatabase.CreateAsset(newAsset, assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 데이터 채우기
        int successCount = 0;
        foreach (var pair in groupedById)
        {
            string dialogueId = pair.Key;
            List<string[]> rows = pair.Value;

            try
            {
                string assetPath = $"{targetFolder}/{dialogueId}.asset";
                DialogueData data = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);

                // 데이터 초기화
                data.id = dialogueId;
                data.lines = new List<DialogueLine>();
                // data.choices = new List<DialogueChoice>();
                
                rows.Sort((a, b) => int.Parse(a[1]).CompareTo(int.Parse(b[1])));
                
                foreach (var cols in rows)
                {
                    Expression parsedExpression = Expression.Default;
                    if (Enum.TryParse<Expression>(cols[3].Trim(), true, out var tempExpression))
                    {
                        parsedExpression = tempExpression;
                    }
                
                    data.lines.Add(new DialogueLine
                    {
                        speakerId = cols[2].Trim(),
                        text = cols[4].Trim(),
                        expression = parsedExpression
                    });
                }
                
                // // 선택지 파싱
                // for (int i = 0; i < 2; i++)
                // {
                //     int textIndex = 7 + i;
                //     if (lastRow.Length > textIndex && !string.IsNullOrEmpty(lastRow[textIndex].Trim()))
                //     {
                //         data.choices.Add(new DialogueChoice
                //         {
                //             text = lastRow[textIndex].Trim(),
                //             nextDialogueId = $"{dialogueId}_choice{i + 1}"
                //         });
                //     }
                // }

                EditorUtility.SetDirty(data);
                successCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Dialogue ID '{dialogueId}' 처리 중 오류 발생: {e.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"총 {successCount}개 DialogueData CSV 임포트 완료");
    }

    #endregion

    #region SpeakerData 생성

    [MenuItem("Tools/Import Game Data/Speaker Data")]
    public static void ImportSpeakerData()
    {
        string path = EditorUtility.OpenFilePanel("Select Speaker CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일에 데이터 없음");
            return;
        }

        string targetFolder = "Assets/Resources/Datas/Speakers/";
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }
        
        // 에셋 생성
        foreach (var line in lines.Skip(1))
        {
            string[] cols = line.Split(',');
            if (cols.Length < 2 || string.IsNullOrEmpty(cols[0].Trim())) continue;
            string id = cols[0].Trim();
            string assetPath = $"{targetFolder}/{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<SpeakerData>(assetPath) == null)
            {
                var newAsset = ScriptableObject.CreateInstance<SpeakerData>();
                AssetDatabase.CreateAsset(newAsset, assetPath);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 데이터 채우기 및 초상화 연결
        int successCount = 0;
        foreach (var line in lines.Skip(1))
        {
            try
            {
                string[] cols = line.Split(',');
                if (cols.Length < 2 || string.IsNullOrEmpty(cols[0].Trim())) continue;

                string id = cols[0].Trim();
                string assetPath = $"{targetFolder}/{id}.asset";
                SpeakerData data = AssetDatabase.LoadAssetAtPath<SpeakerData>(assetPath);
                
                data.id = id;
                data.speakerName = cols[1].Trim();
                data.portraits = new List<PortraitEntry>(); 
                
                // 초상화 연결
                foreach (Expression expression in Enum.GetValues(typeof(Expression)))
                {
                    string portraitPath = $"Icons/Portrait/{id}_{expression}";
                    Sprite portraitSprite = Resources.Load<Sprite>(portraitPath);

                    if (portraitSprite == null) continue;
                    
                    data.portraits.Add(new PortraitEntry 
                    { 
                        expression = expression, 
                        portrait = portraitSprite 
                    });
                    Debug.Log($"{data.id}의 {expression} 초상화 연결 완료");
                }
                
                EditorUtility.SetDirty(data);
                successCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Speaker ID '{lines[successCount + 1].Split(',')[0]}' 처리 중 오류 발생: {e.Message}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"총 {successCount}명의 SpeakerData CSV 임포트 완료");
    }

    #endregion
    
    #region StoryData 생성

    [MenuItem("Tools/Import Game Data/Story Data")]
    public static void ImportStoryData()
    {
        string path = EditorUtility.OpenFilePanel("Select Story CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일에 데이터 없음");
            return;
        }

        string targetFolder = "Assets/Resources/Datas/Story/";
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }
        
        // 에셋 생성/업데이트
        int successCount = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                string[] cols = lines[i].Split(',');
                if (cols.Length < 2 || string.IsNullOrEmpty(cols[0].Trim())) continue;

                string id = cols[0].Trim();
                string assetPath = $"{targetFolder}/{id}.asset";
                StoryData data = AssetDatabase.LoadAssetAtPath<StoryData>(assetPath);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<StoryData>();
                    AssetDatabase.CreateAsset(data, assetPath);
                }

                // 데이터 채우기
                data.id = id;
                
                if (Enum.TryParse<GamePhase>(cols[1].Trim(), true, out var phase))
                {
                    data.triggerPhase = phase;
                }
                else
                {
                    Debug.LogWarning($"Story ID '{id}'의 triggerPhase '{cols[1].Trim()}'없음");
                    data.triggerPhase = GamePhase.Day; // 기본값 설정
                }
                
                data.conditions = ParseConditions(cols[2]); // 조건 파싱
                data.actions = ParseActions(cols[3]);     // 액션 파싱

                EditorUtility.SetDirty(data);
                successCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Story ID '{lines[i].Split(',')[0]}' 처리 중 오류 발생: {e.Message}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"총 {successCount}개의 StoryData CSV 임포트 완료");
    }

    /// <summary>
    /// 조건 문자열을 파싱하여 StoryCondition 리스트 반환
    /// </summary>
    private static List<StoryCondition> ParseConditions(string conditionsString)
    {
        var result = new List<StoryCondition>();
        if (string.IsNullOrEmpty(conditionsString)) return result;

        string[] conditionParts = conditionsString.Split('|');
        foreach (var part in conditionParts)
        {
            string[] attrs = part.Split(';').Select(s => s.Trim()).ToArray();
            if (attrs.Length < 2) continue;

            if (Enum.TryParse<ConditionType>(attrs[0], true, out var type))
            {
                var condition = new StoryCondition();
                condition.conditionType = type;

                // ConditionType에 따라 파싱
                switch (type)
                {
                    case ConditionType.Day:
                        // 형식: Day;>=;3
                        condition.@operator = attrs[1];
                        condition.rValue = attrs[2];
                        break;
                    
                    case ConditionType.QuestStatus:
                        // 형식: QuestStatus;quest_id;==;Completed
                        condition.lValue = attrs[1];
                        condition.@operator = attrs[2];
                        condition.rValue = attrs[3];
                        break;

                    case ConditionType.DialogueEnded:
                        // 형식: DialogueEnded;Is;dialogue_id
                        condition.@operator = attrs[1];
                        condition.lValue = attrs[2];
                        break;
                }
                result.Add(condition);
            }
        }
        return result;
    }

    /// <summary>
    /// 액션 문자열을 파싱하여 StoryAction 리스트 반환
    /// </summary>
    private static List<StoryAction> ParseActions(string actionsString)
    {
        var result = new List<StoryAction>();
        if (string.IsNullOrEmpty(actionsString)) return result;

        string[] actionParts = actionsString.Split('|');
        foreach (var part in actionParts)
        {
            string[] attrs = part.Split(';').Select(s => s.Trim()).ToArray();
            if (attrs.Length < 1) continue;

            if (Enum.TryParse<StoryType>(attrs[0], true, out var type))
            {
                var action = new StoryAction();
                action.storyType = type;

                // StoryType에 따라 파싱
                switch (type)
                {
                    case StoryType.StartDialogue:
                    case StoryType.StartQuest:
                    case StoryType.UnlockRecipe:
                    case StoryType.UnlockStation:
                        // 형식: ActionType;targetId
                        action.targetId = attrs[1];
                        break;
                    case StoryType.GiveMoney:
                        // 형식: ActionType;value
                        action.targetId = "";
                        action.value = attrs[1];
                        break;
                    case StoryType.LostMoney:
                        // 형식: ActionType;value
                        action.targetId = "";
                        action.value = attrs[1];
                        break;
                }

                result.Add(action);
            }
        }

        return result;
    }

    #endregion

    #region QuestData 생성

    [MenuItem("Tools/Import Game Data/Quest Data")]
    public static void ImportQuestData()
    {
        string path = EditorUtility.OpenFilePanel("Select Quest CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV 파일에 데이터가 없습니다.");
            return;
        }
        
        string targetFolder = "Assets/Resources/Datas/Quest/";
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        Dictionary<string, QuestData> questDataMap = new Dictionary<string, QuestData>();
        
        for (int i = 1; i < lines.Length; i++)
        {
            try
            {
                string[] cols = lines[i].Split(',');
                if (cols.Length < 9) continue; 

                string id = cols[0].Trim();
                QuestData data;

                // 딕셔너리에서 기존 퀘스트 데이터를 찾거나, 없으면 새로 생성
                if (questDataMap.TryGetValue(id, out var value))
                {
                    data = value;
                }
                else
                {
                    string assetPath = $"{targetFolder}/{id}.asset";
                    data = AssetDatabase.LoadAssetAtPath<QuestData>(assetPath);
                    if (data == null)
                    {
                        data = ScriptableObject.CreateInstance<QuestData>();
                        AssetDatabase.CreateAsset(data, assetPath);
                    }
                    
                    data.objectives = new List<QuestObjective>();
                    questDataMap.Add(id, data);
                    
                    data.id = id;
                    data.questName = cols[1].Trim();
                    data.description = cols[2].Trim();
                    data.rewardDescription = cols[3].Trim();
                    data.rewardMoney = int.TryParse(cols[7], out int money) ? money : 0;
                    data.rewardItemIds = string.IsNullOrEmpty(cols[8]) ? Array.Empty<string>() : cols[8].Split('|').Select(s => s.Trim()).ToArray();
                }

                // --- 각 줄의 목표정보를 파싱하여 data.objectives 리스트에 추가 ---
                
                // 목표 타입(ObjectType) 파싱
                if (Enum.TryParse<QuestObjectiveType>(cols[5].Trim(), true, out var type))
                {
                    // 목표(Objective) 컬럼 파싱 ('targetId;requiredIds' 형식)
                    string[] objectiveParts = cols[6].Split(';');
                    string targetId = "";
                    string[] requiredIds = Array.Empty<string>();

                    if (objectiveParts.Length > 0)
                    {
                        targetId = objectiveParts[0].Trim();
                    }
                    if (objectiveParts.Length > 1 && !string.IsNullOrEmpty(objectiveParts[1]))
                    {
                        requiredIds = objectiveParts[1].Split('|').Select(s => s.Trim()).ToArray();
                    }

                    // QuestObjective 인스턴스 생성 및 리스트에 추가
                    var objective = new QuestObjective
                    {
                        description = cols[4].Trim(),
                        objectiveType = type,
                        targetId = targetId,
                        requiredIds = requiredIds
                    };
                    data.objectives.Add(objective);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"CSV 라인 {i + 1} (퀘스트 ID: {lines[i].Split(',')[0]}) 처리 중 오류 발생: {e.Message}");
            }
        }
        
        foreach (var data in questDataMap.Values)
        {
            EditorUtility.SetDirty(data);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"총 {questDataMap.Count}개의 QuestData CSV 임포트/업데이트 완료");
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
        
        string targetFolder = $"Assets/Resources/Datas/{folderName}/";
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
#endif