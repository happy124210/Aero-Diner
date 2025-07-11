using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("게임 진행값")]
    [SerializeField] private int totalEarnings;
    [SerializeField] private int currentDay = 1;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo;
    private static readonly int[] DaysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    
    #region properties
    public int TotalEarnings => totalEarnings;
    #endregion
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        
        LoadEarnings();
        LoadDay();
    }
    
    #region money
    
    public void LoadEarnings()
    {
        var data = SaveLoadManager.LoadGame();

        int money = data?.totalEarnings ?? 0;
        SetMoney(money);
        
        EventBus.Raise(UIEventType.UpdateEarnings, TotalEarnings);
    }

    public void SetMoney(int amount)
    {
        totalEarnings = amount;
    }

    // 결제 처리 메서드
    public void AddMoney(int amount)
    {
        totalEarnings += amount;
        EventBus.Raise(UIEventType.UpdateEarnings, TotalEarnings);
        EventBus.OnSFXRequested(SFXType.CostomerPayed);
    }
    
    #endregion

    #region day
    
    private void LoadDay()
    {
        var data = SaveLoadManager.LoadGame();
        currentDay = Mathf.Max(1, data?.currentDay ?? 1);
    }

    public void GetCurrentDate(out int month, out int day)
    {
        int totalDays = currentDay;
        month = 1;

        foreach (var daysInThisMonth in DaysInMonth)
        {
            if (totalDays > daysInThisMonth)
            {
                totalDays -= daysInThisMonth;
                month++;
            }
            else
            {
                break;
            }
        }

        day = totalDays;
    }

    public void IncreaseDay()
    {
        currentDay++;
        if (showDebugInfo) Debug.Log($"[GameManager] Incrementing day from {currentDay} → {currentDay + 1}");
    }

    public void SaveData()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();
        data.totalEarnings = TotalEarnings;
        data.currentDay = currentDay;
        SaveLoadManager.SaveGame(data);
    }
    
    #endregion
    
    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ContinueGame()
    {
        Time.timeScale = 1;
    }
}