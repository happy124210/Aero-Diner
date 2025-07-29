using UnityEngine;
public class Tu1 : MonoBehaviour
{

    public void OnOpenButtonClick()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        UIEventCaller.CallUIEvent("tu2");
        EventBus.PlayBGM(BGMEventType.PlayRecipeChoice);
    }
    public void OnShopButtonClick()
    {
        EventBus.Raise(UIEventType.FadeInStore);
    }
}
