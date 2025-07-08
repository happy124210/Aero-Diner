using System;
using System.Collections.Generic;
public enum SFXType
{
    PressAnyKey, ButtonClick,BlankClick, PlayerMove,
    ItemPickup,Itemlaydown,
    Costomerleave, CostomerOrder, CostomerGetMeal,CostomerPayed,
    NPCscript, BuyinShop,
    DoneCooking, ThrowAway,
    PotSFX, OvenSFX,
    CuttingBoardSFX, MortalSFX,
    GrinderSFX, PanSFX,
    ChopKnifeSFX, BlenderSFX,
    AutoGrinderSFX, RollPanSFX

}
public enum UIEventType
{
    //pause, Option
    OpenPause, ClosePause,
    OpenOption, CloseOption,
    CloseSound, CloseVideo,
    CloseControl, ShowSoundTab,
    ShowVideoTab, ShowControlTab,
    //StartScene
    ShowStartWarningPanel, ShowStartMenuWithSave,
    ShowStartMenuNoSave, LoadMainScene,
    QuitGame, ShowPressAnyKey,OnClickNewGame,
    //MainSceneUI
    ShowRoundTimer, HideRoundTimer,
    UpdateEarnings, ShowMenuPanel, UpdateMenuPanel,
    HideMenuPanel, ShowResultPanel,
    HideResultPanel, ShowInventory,
    HideInventory
}

public static class EventBus
{
    public static Action<SFXType> OnSFXRequested;
    public static Action<UIEventType, object> OnUIEvent;
    public static void PlaySFX(SFXType type)
    {
        OnSFXRequested?.Invoke(type);
    }
    public static void Raise(UIEventType eventType, object payload = null)
    {
        OnUIEvent?.Invoke(eventType, payload);
    }


    //게임 종료 시점 혹은 씬 변경 시점에 호출하여 메모리 누수 방지
    public static void ClearAll()
    {
        OnSFXRequested = null;
        OnUIEvent = null;
    }
}