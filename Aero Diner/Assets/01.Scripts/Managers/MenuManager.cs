using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// FoodData 관리
/// </summary>
public class MenuManager : Singleton<MenuManager>
{
    [Header("오늘 영업 메뉴")] 
    [SerializeField] private List<string> todayMenuIds = new(); // 오늘 영업할 메뉴
    
    [Header("디버깅")]
    [SerializeField] private string startMenuId;
    [SerializeField] private bool showDebugInfo;

    private readonly List<Menu> menuDatabase = new(); // 전체 Menu타입 푸드데이터 담음
    private readonly Dictionary<string, int> todayMenuSales = new();
    
    #region public getters & methods
    
    // Menu 조회는 전부 id로 함 !!!
    // 메뉴 조회하기
    public Menu FindMenuById(string id) => menuDatabase.FirstOrDefault(m => m.foodData.id == id);
    
    // 플레이어 해제 메뉴만 가져오기
    public List<Menu> GetUnlockedMenus() => menuDatabase.Where(menu => menu.isUnlocked).ToList();
    
    // 오늘 영업 Menu/FoodData 가져오기
    public List<Menu> GetTodayMenuData() => todayMenuIds.Select(FindMenuById).Where(data => data != null).ToList();
    public List<FoodData> GetTodayFoodData() => todayMenuIds.Select(id => FindMenuById(id).foodData).Where(data => data != null).ToList();
    
    // 하위 레시피 포함 전체 레시피
    public List<FoodData> GetTodayRecipes()
    {
        var menus = todayMenuIds.Select(FindMenuById).Where(m => m != null).ToList();
        return RecipeManager.Instance.GetAllRecipe(menus);
    }

    // Menu 조회
    public List<Menu> GetAllMenus() => menuDatabase;
    public string[] GetPlayerMenuIds() => menuDatabase.Where(m => m.isUnlocked).Select(m => m.foodData.id).ToArray();

    // 전체 판매량 / 판매액 조회
    public int GetTotalSalesToday() => todayMenuSales.Values.Sum();
    public int GetTotalRevenueToday() => todayMenuIds.Sum(GetTodayMenuRevenue);

    // 메뉴별 판매량 / 판매액 조회
    public int GetTodayMenuSales(string menuId) => todayMenuSales.GetValueOrDefault(menuId, 0);
    public int GetTodayMenuRevenue(string menuId)
    {
        int soldCount = GetTodayMenuSales(menuId);
        int price = GetMenuPrice(menuId);
        return soldCount * price;
    }
    private int GetMenuPrice(string menuId) => FindMenuById(menuId)?.Price ?? 0;
    
    #endregion
    
    protected override void Awake()
    {
        base.Awake();
        
        InitializePlayerMenus();
    }

    private void InitializeTodayStats()
    {
        todayMenuSales.Clear();
        foreach (var menuId in todayMenuIds)
        {
            todayMenuSales[menuId] = 0;
        }
    }

    /// <summary>
    /// 메뉴 데이터베이스 초기화
    /// </summary>
    private void InitializePlayerMenus()
    {
        menuDatabase.Clear();
        
        // RecipeManager에서 모든 FoodData를 가져와 Menu 객체로 변환
        foreach (var foodData in RecipeManager.Instance.FoodDatabase.Values)
        {
            if (foodData.foodType == FoodType.Menu)
            {
                menuDatabase.Add(new Menu(foodData));
            }
        }
        
        // 세이브 로드
        MenuSaveHandler.LoadMenuDatabase();
        
        // 데이터 없으면 시작 메뉴 자동 해금
        if (!string.IsNullOrEmpty(startMenuId))
        {
            UnlockMenu(startMenuId);
            SetMenuSelection(startMenuId, true);
        }
        
        UpdateTodayMenus();
        if (showDebugInfo) Debug.Log($"[MenuManager]: 전체 {menuDatabase.Count}개 메뉴 데이터베이스 생성 완료");
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
        foreach (var menuId in todayMenuIds)
        {
            Menu menu = FindMenuById(menuId);
            if (menu == null) continue;

            salesData.Add(new MenuSalesData
            {
                MenuName = menu.MenuName,
                SoldCount = GetTodayMenuSales(menuId),
                TotalRevenue = GetTodayMenuRevenue(menuId),
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
        todayMenuIds = menuDatabase
            .Where(menu => menu.CanServeToday)
            .Select(menu => menu.foodData.id)
            .ToList();
            
        InitializeTodayStats();

        if (showDebugInfo) Debug.Log($"[MenuManager]: 오늘 메뉴 - {todayMenuIds.Count}개");
    }

    /// <summary>
    /// id 기반 메뉴 해금
    /// </summary>
    /// <param name="foodId"> 해금하려는 메뉴의 foodData.id </param>
    public void UnlockMenu(string foodId)
    {
        Menu menuToUnlock = FindMenuById(foodId);

        if (menuToUnlock == null)
        {
            Debug.LogError($"[MenuManager] ID '{foodId}'인 메뉴 없음 !!!");
            return;
        }
        
        if (menuToUnlock.isUnlocked) return;

        menuToUnlock.isUnlocked = true;
    }

    public void SetMenuSelection(string foodId, bool selection)
    {
        Menu menu = FindMenuById(foodId);
        if (menu != null && menu.isUnlocked)
        {
            menu.isSelected = selection;
            UpdateTodayMenus();
        }
    }

    public FoodData GetRandomMenu()
    {
        if (todayMenuIds.Count == 0) return null;
        
        string randomId = todayMenuIds[Random.Range(0, todayMenuIds.Count)];
        return FindMenuById(randomId)?.foodData;
    }

    #endregion
    
    #region cheater

    public void UnlockAllMenus()
    {
        foreach (var menu in menuDatabase)
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