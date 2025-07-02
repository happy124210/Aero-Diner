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
        InitializePlayerMenus();
    }

    /// <summary>
    /// 존재하는 모든 음식 데이터 로드
    /// </summary>
    private void LoadAllFoodData()
    {
        allFoodData = Resources.LoadAll<FoodData>("Data/Food");

        if (showDebugInfo) Debug.Log($"[MenuManager]: {allFoodData.Length}개 데이터 로드 완료");
    }

    /// <summary>
    /// 플레이어 메뉴 초기화
    /// </summary>
    private void InitializePlayerMenus()
    {
        foreach (var foodData in allFoodData)
        {
            if (foodData.foodType == FoodType.Menu)
            {
                // 기본 해금 메뉴 설정
                bool isStartMenu = foodData.id == startMenuId;
                playerMenus.Add(new Menu(foodData, isStartMenu));
            }
        }

        UpdateTodayMenus();
    }

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
        var menu = playerMenus.FirstOrDefault(m => m.foodData.id == foodId);
        if (menu != null && !menu.isUnlocked)
        {
            menu.isUnlocked = true;
            UpdateTodayMenus();
            if (showDebugInfo)  Debug.Log($"[MenuManager]: {menu.MenuName} 해금 완료 !");
            return true;
        }
        return false;
    }


    public void ToddleMenuSelection(string foodId)
    {
        Menu menu = playerMenus.FirstOrDefault(m => m.foodData.id == foodId);
        if (menu?.isUnlocked == true)
        {
            menu.isSelected = !menu.isSelected;
            UpdateTodayMenus();
        }
    }

    #endregion
    
    #region public getters
    
    public FoodData[] GetTodayMenuData() => todayMenus.Select(m => m.foodData).ToArray();
    public List<Menu> GetUnlockedMenus() => playerMenus.Where(menu => menu.isUnlocked).ToList();
    public List<Menu> GetAllMenus() => new List<Menu>(playerMenus);
    
    #endregion
    
}
