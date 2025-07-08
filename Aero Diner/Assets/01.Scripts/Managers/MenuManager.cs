using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// FoodData 관리
/// </summary>
public class MenuManager : Singleton<MenuManager>
{
    [Header("메뉴 데이터")]
    [SerializeField] private List<Menu> allMenus = new(); // 게임에 존재하는 모든 메뉴
    [SerializeField] private List<Menu> playerMenus = new(); // 플레이어가 해금한 메뉴

    [Header("오늘 영업 메뉴")] 
    [SerializeField] private List<Menu> todayMenus = new(); // 오늘 영업할 메뉴
    private List<FoodData> todayRecipes = new();
    
    [Header("디버깅")]
    [SerializeField] private string startMenuId;
    [SerializeField] private bool showDebugInfo;

    private readonly Dictionary<string, int> todayMenuSales = new();
    
    #region public getters

    // FoodData 조회
    public FoodData[] GetTodayMenuData() => todayMenus.Select(m => m.foodData).ToArray(); // 오늘 메뉴의 FoodData만
    public List<FoodData> GetTodayRecipes() => todayRecipes; // 하위 레시피 포함 전체 레시피
   
    // Menu 조회
    public List<Menu> GetAllMenus() => new List<Menu>(playerMenus);
    public List<Menu> GetTodayMenus() => todayMenus; // Menu 리스트 (해금, 선택정보 포함)
    public List<Menu> PlayerMenus => playerMenus; // 플레이어가 해금한 레시피 전부
    
    // 전체 판매량 조회
    public int GetTotalSalesToday() => todayMenus.Sum(menu => GetTodayMenuSales(menu.foodData.id));
    public int GetTotalRevenueToday() => todayMenus.Sum(menu => GetTodayMenuRevenue(menu.foodData.id));
    
    // 메뉴별 판매량 조회 (id로 조회함)
    public int GetTodayMenuSales(string menuId) => todayMenuSales.GetValueOrDefault(menuId, 0);
    
    public int GetTodayMenuRevenue(string menuId)
    {
        int soldCount = GetTodayMenuSales(menuId);
        int price = GetMenuPrice(menuId);
        return soldCount * price;
    }
    
    private int GetMenuPrice(string menuId) => todayMenus.Find(m => m.foodData.id == menuId).Price;
    #endregion
    
    protected override void Awake()
    {
        base.Awake();

        LoadAllMenu();
        InitializePlayerMenus();
    }

    private void InitializeTodayStats()
    {
        todayMenuSales.Clear();

        foreach (var menu in todayMenus)
        {
            todayMenuSales[menu.foodData.id] = 0;
        }
    }
    
    /// <summary>
    /// 존재하는 모든 음식 데이터 로드
    /// </summary>
    public void LoadAllMenu()
    {
        allMenus.Clear();
        Dictionary<string, FoodData> allFoodData = RecipeManager.Instance.FoodDatabase;
        
        // Menu 클래스 세팅
        foreach (var foodData in allFoodData.Values) // .Values 추가!
        {
            if (foodData.foodType == FoodType.Menu)
            {
                var menu = new Menu(foodData);
                allMenus.Add(menu);
            }
        }

        if (showDebugInfo) Debug.Log($"[MenuManager]: 전체 {allFoodData.Count}개 데이터 로드");
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

    #region 판매 통계

    public void OnMenuServed(string menuId)
    {
        if (string.IsNullOrEmpty(menuId)) return;
        
        todayMenuSales[menuId]++;
    }
    
    /// <summary>
    /// 모든 메뉴의 판매 데이터 반환
    /// </summary>
    public List<MenuSalesData> GetAllMenuSalesData()
    {
        List<MenuSalesData> salesData = new();
        
        foreach (var menu in todayMenus)
        {
            salesData.Add(new MenuSalesData
            {
                MenuName = menu.MenuName,
                SoldCount = GetTodayMenuSales(menu.foodData.id),
                TotalRevenue = GetTodayMenuSales(menu.foodData.id) * menu.Price,
            });
        }
        
        return salesData;
    }
    
    #endregion
    
    #region 외부 사용 함수
    
    /// <summary>
    /// 오늘 사용할 메뉴 업데이트
    /// </summary>
    public void UpdateTodayMenus()
    {
        todayMenus = playerMenus.Where(menu => menu.CanServeToday).ToList();
        todayRecipes = RecipeManager.Instance.GetAllRecipe(todayMenus);
        
        InitializeTodayStats();
        
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

}

/// <summary>
/// 통계용 클래스
/// </summary>
[System.Serializable]
public class MenuSalesData
{
    public string MenuName;
    public int SoldCount;
    public int TotalRevenue;
}