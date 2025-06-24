using System;
using UnityEngine;
public enum SFXType
{
    BlankClick,
    ButtonClick,
    ItemPickup,
    Itemlaydown,
    Cutting
}


public static class SFXEventBus
{
    public static Action<SFXType> OnSFXRequested;

    public static void PlaySFX(SFXType type)
    {
        OnSFXRequested?.Invoke(type);
    }
}