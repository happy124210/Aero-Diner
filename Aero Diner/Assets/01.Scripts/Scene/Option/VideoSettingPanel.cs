using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VideoSettingPanel : MonoBehaviour
{

    public TMP_Text screenModeText;
    public TMP_Text resolutionText;

    private int screenModeIndex = 0;
    private int resolutionIndex = 0;

    private int originalScreenModeIndex = 0;
    private int originalResolutionIndex = 0;

    private readonly string[] screenModes = { "창모드", "테두리 없는 창모드", "전체 화면" };
    private readonly FullScreenMode[] fullScreenModes =
    {
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };

    private readonly List<Vector2Int> resolutions = new List<Vector2Int>
    {
        new Vector2Int(1024, 768),
        new Vector2Int(1152, 864),
        new Vector2Int(1176, 664),
        new Vector2Int(1280, 720),
        new Vector2Int(1280, 768),
        new Vector2Int(1280, 800),
        new Vector2Int(1280, 960),
        new Vector2Int(1280, 1024),
        new Vector2Int(1360, 768),
        new Vector2Int(1366, 768),
        new Vector2Int(1440, 900),
        new Vector2Int(1440, 1080),
        new Vector2Int(1600, 900),
        new Vector2Int(1600, 1024),
        new Vector2Int(1680, 1050),
        new Vector2Int(1920, 1080)
    };
    
    private void Start()
    {
        // 현재 설정값을 기준으로 original 저장
        originalScreenModeIndex = GetCurrentScreenModeIndex();
        originalResolutionIndex = GetCurrentResolutionIndex();

        screenModeIndex = originalScreenModeIndex;
        resolutionIndex = originalResolutionIndex;

        UpdateUI();
    }
    
    private int GetCurrentScreenModeIndex()
    {
        for (int i = 0; i < fullScreenModes.Length; i++)
        {
            if (Screen.fullScreenMode == fullScreenModes[i])
                return i;
        }
        return 0;
    }

    private int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (Screen.currentResolution.width == resolutions[i].x &&
                Screen.currentResolution.height == resolutions[i].y)
                return i;
        }

        return resolutions.FindIndex(r => r.x == Screen.width && r.y == Screen.height);
    }

    public void OnClickLeft_ScreenMode()
    {
        screenModeIndex = (screenModeIndex - 1 + screenModes.Length) % screenModes.Length;
        UpdateUI();
    }

    public void OnClickRight_ScreenMode()
    {
        screenModeIndex = (screenModeIndex + 1) % screenModes.Length;
        UpdateUI();
    }

    public void OnClickLeft_Resolution()
    {
        resolutionIndex = (resolutionIndex - 1 + resolutions.Count) % resolutions.Count;
        UpdateUI();
    }

    public void OnClickRight_Resolution()
    {
        resolutionIndex = (resolutionIndex + 1) % resolutions.Count;
        UpdateUI();
    }

    public void OnClickSave()
    {
        ApplyPending();

        // Save current as original
        originalScreenModeIndex = screenModeIndex;
        originalResolutionIndex = resolutionIndex;

        Debug.Log("[VideoSettingPanel] 해상도 및 모드 저장 완료");
    }

    public void OnClickReset()
    {
        screenModeIndex = 2; // 전체화면
        resolutionIndex = resolutions.FindIndex(r => r.x == 1920 && r.y == 1080);
        if (resolutionIndex < 0) resolutionIndex = 0;

        OnClickSave();
        UpdateUI();
    }

    /// <summary>
    /// ESC 등으로 변경 사항 무시 시 호출
    /// </summary>
    public void RollbackPending()
    {
        screenModeIndex = originalScreenModeIndex;
        resolutionIndex = originalResolutionIndex;
        UpdateUI();

        Debug.Log("[VideoSettingPanel] 변경 사항 롤백됨");
    }
    public bool HasUnsavedChanges()
    {
        return screenModeIndex != originalScreenModeIndex
            || resolutionIndex != originalResolutionIndex;
    }
    /// <summary>
    /// 실제 해상도 적용
    /// </summary>
    private void ApplyPending()
    {
        Vector2Int res = resolutions[resolutionIndex];
        FullScreenMode mode = fullScreenModes[screenModeIndex];

        Screen.SetResolution(res.x, res.y, mode);
        Debug.Log($"해상도 적용: {res.x} x {res.y}, 모드: {screenModes[screenModeIndex]}");
    }

    private void UpdateUI()
    {
        screenModeText.text = screenModes[screenModeIndex];
        Vector2Int res = resolutions[resolutionIndex];
        resolutionText.text = $"{res.x} * {res.y}";
    }
}