using TMPro;
using UnityEngine;
using DG.Tweening;

public class EarningsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Txt_Earning;
    [SerializeField] private float animateDuration = 0.5f;
    [SerializeField] private Color flashColor = Color.yellow;

    private float currentDisplayAmount = 0f;
    private Color originalColor;

    private void Awake()
    {
        if (!Txt_Earning)
            Txt_Earning = GetComponent<TextMeshProUGUI>();

        originalColor = Txt_Earning.color;
    }

    public void AnimateEarnings(float newAmount)
    {
        DOTween.Kill(Txt_Earning); // 기존 애니메이션 정리

        // 숫자 Tween
        DOTween.To(() => currentDisplayAmount, x =>
        {
            currentDisplayAmount = x;
            Txt_Earning.text = $"₩ {Mathf.RoundToInt(x):N0}";
        },
        newAmount, animateDuration)
        .SetEase(Ease.OutCubic);

        // 색상 깜빡임 + 확대 효과
        var seq = DOTween.Sequence();
        seq.Append(Txt_Earning.DOColor(flashColor, 0.2f));
        seq.Join(Txt_Earning.transform.DOScale(1.2f, 0.2f));
        seq.AppendInterval(0.1f);
        seq.Append(Txt_Earning.DOColor(originalColor, 0.2f));
        seq.Join(Txt_Earning.transform.DOScale(1.0f, 0.2f));
    }
}