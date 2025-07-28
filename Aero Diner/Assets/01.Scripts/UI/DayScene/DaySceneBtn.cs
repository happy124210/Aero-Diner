using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaySceneBtn : MonoBehaviour
{
    public void OnOpenButtonClick()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.ShowMenuPanel);
        EventBus.PlayBGM(BGMEventType.PlayRecipeChoice);
    }
    public void OnShopButtonClick()
    {
        EventBus.Raise(UIEventType.FadeInStore);
    }
}
