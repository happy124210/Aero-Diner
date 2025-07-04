using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 서빙되는 Menu, 즉 FoodType == Menu인 FoodData만 관리
/// </summary>
public class MenuManager : Singleton<MenuManager>
{
    [Header("메뉴 데이터")] 
    [SerializeField] private FoodData[] allFoodData;
    [SerializeField] private List<Menu> allMenus = new(); // 게임에 존재하는 모든 메뉴
    [SerializeField] private List<Menu> playerMenus = new(); // 플레이어가 해금한 메뉴

    [Header("오늘 영업 메뉴")] 
    [SerializeField] private List<Menu> todayMenus = new(); // 오늘 영업할 메뉴

    [Header("디버깅")] 
    [SerializeField] private string startMenuId;
    [SerializeField] private bool showDebugInfo;

    protected override void Awake()
    {
        base.Awake();
        LoadAllFoodData();
        LoadAllMenu();
        InitializePlayerMenus();
    }

    public void LoadAllFoodData()
    {
        allFoodData = Resources.LoadAll<FoodData>("Datas/Food");
    }
    
    /// <summary>
    /// 존재하는 모든 음식 데이터 로드
    /// </summary>
    public void LoadAllMenu()
    {
        allMenus.Clear();
        
        // Menu 클래스 세팅
        foreach (var foodData in allFoodData)
        {
            if (foodData.foodType == FoodType.Menu)
            {
                var menu = new Menu(foodData);
                allMenus.Add(menu);
            }
        }

        if (showDebugInfo) Debug.Log($"[MenuManager]: 전체 {allFoodData.Length}개 데이터 로드");
    }

    /// <summary>
    /// 플레이어 메뉴 초기화
    /// </summary>
    private void InitializePlayerMenus()
    {
        playerMenus.Clear();
        
        // 시작 메뉴 자동 해금
        if (!string.IsNullOrEmpty(startMenuId))
        {
            UnlockMenu(startMenuId);
        }

        UpdateTodayMenus();
    }

    #region 레시피 검색

    public List<RecipeMatchResult> FindMatchingRecipes(List<FoodData> candidateRecipes, List<string> ingredientIds)
    {
        if (candidateRecipes == null || ingredientIds == null)
        {
            Debug.LogWarning("[RecipeManager] 입력값이 null입니다.");
            return new List<RecipeMatchResult>();
        }

        var matches = candidateRecipes
            .Where(r => r.ingredients != null)
            .Select(r =>
            {
                var matchedCount = r.ingredients.Count(ingredientIds.Contains);
                var result = new RecipeMatchResult
                {
                    recipe = r,
                    matchedCount = matchedCount,
                    totalRequired = r.ingredients.Length
                };

                Debug.Log($"[RecipeManager] Checking recipe: {r.foodName}");
                Debug.Log($" - Required: {string.Join(", ", r.ingredients)}");
                Debug.Log($" - Given:   {string.Join(", ", ingredientIds)}");
                Debug.Log($" - Match: {matchedCount}/{r.ingredients.Length} | MatchRatio: {result.MatchRatio:F2}");

                return result;
            })
            .Where(r => r.matchedCount > 0)
            .ToList();

        var fullMatches = matches
            .Where(r => r.matchedCount == r.totalRequired && ingredientIds.Count == r.totalRequired)
            .ToList();

        if (fullMatches.Count > 0)
        {
            Debug.Log($"[RecipeManager] 정확 일치 레시피 {fullMatches.Count}개");
            return fullMatches;
        }

        Debug.Log("[RecipeManager] 정확 일치 없음 — 유사도 기반 결과 반환");
        return matches
            .OrderByDescending(r => r.MatchRatio)
            .ThenByDescending(r => r.matchedCount)
            .ToList();
    }
    
    #endregion
    
    
    
    #region 외부 사용 함수
    
    /// <summary>
    /// 오늘 사용할 메뉴 업데이트
    /// </summary>
    public void UpdateTodayMenus()
    {
        todayMenus = playerMenus.Where(menu => menu.CanServeToday).ToList();

        if (showDebugInfo)  Debug.Log($"[MenuManager]: 오늘 메뉴 - {todayMenus.Count}개");
    }

    /// <summary>
    /// id 기반 메뉴 해금
    /// </summary>
    /// <param name="foodId"> 해금하려는 메뉴의 foodData.id </param>
    public bool UnlockMenu(string foodId)
    {
        if (playerMenus.Any(m => m.foodData.id == foodId))
        {
            if (showDebugInfo) Debug.Log($"[MenuManager] ℹID '{foodId}' 메뉴 이미 해금됨.");
            return false;
        }
        
        var menuToUnlock = allMenus.FirstOrDefault(m => m.foodData.id == foodId);
        if (menuToUnlock == null)
        {
            Debug.LogError($"[MenuManager] ID '{foodId}'인 메뉴 없음 !!!");
            return false;
        }
        
        // 해금 상태로 변경, playerMenus에 추가
        menuToUnlock.isUnlocked = true;
        playerMenus.Add(menuToUnlock);
        
        UpdateTodayMenus();
        return true;
    }

    public void SetMenuSelection(string foodId, bool selection)
    {
        Menu menu = playerMenus.FirstOrDefault(m => m.foodData.id == foodId);
        if (menu != null) menu.isSelected = selection;
        
        UpdateTodayMenus();
    }

    public FoodData GetRandomMenu()
    {
        return todayMenus[Random.Range(0, todayMenus.Count)].foodData;
    }

    #endregion
    
    #region cheater

    public void UnlockAllMenus()
    {
        foreach (var menu in allMenus)
        {
            UnlockMenu(menu.foodData.id);
        }  
        
        UpdateTodayMenus();
    }
    
    #endregion
    
    #region public getters

    public List<Menu> GetTodayMenus() => todayMenus; // Menu 리스트 (해금, 선택정보 포함)
    public FoodData[] GetTodayMenuData() => todayMenus.Select(m => m.foodData).ToArray(); // FoodData만
    public List<Menu> PlayerMenus => playerMenus; // 플레이어가 해금한 레시피 전부
    public List<Menu> GetAllMenus() => new List<Menu>(playerMenus);
    
    #endregion
    
}

public class RecipeMatchResult
{
    public FoodData recipe;
    public int matchedCount;
    public int totalRequired;
    public float MatchRatio => totalRequired == 0 ? 0f : (float)matchedCount / totalRequired;
}