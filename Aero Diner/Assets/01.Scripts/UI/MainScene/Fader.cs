using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public class Fader : MonoBehaviour
{
    private async void Start()
    {
        await Task.Delay(1000);
        EventBus.RaiseFadeEvent(FadeEventType.FadeIn, new FadeEventPayload(alpha: 0f, duration: 1f));
        GameManager.Instance.ChangePhase(GamePhase.Opening);
        EventBus.PlayBGM(BGMEventType.PlayMainTheme);
    }
}
