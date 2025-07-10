using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class EarningsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI earningText;
    [SerializeField] private float animateDuration = 0.5f;
    [SerializeField] private Color flashColor = Color.yellow;

    private int currentDisplayAmount = 0;
    private Color originalColor;

    private void Awake()
    {
        if (!earningText)
            earningText = transform.FindChild<TextMeshProUGUI>("Tmp_Earnings");

        originalColor = earningText.color;
    }

    public void AnimateEarnings(int newAmount)
    {
        DOTween.Kill(earningText); // 기존 애니메이션 정리

        // 숫자 Tween
        DOTween.To(() => currentDisplayAmount, x =>
        {
            currentDisplayAmount = x;
            earningText.text = $"{Mathf.RoundToInt(x):N0} G";
        },
        newAmount, animateDuration)
        .SetEase(Ease.OutCubic);

        // 색상 깜빡임 + 확대 효과
        var seq = DOTween.Sequence();
        seq.Append(earningText.DOColor(flashColor, 0.2f));
        seq.Join(earningText.transform.DOScale(1.2f, 0.2f));
        seq.AppendInterval(0.1f);
        seq.Append(earningText.DOColor(originalColor, 0.2f));
        seq.Join(earningText.transform.DOScale(1.0f, 0.2f));
    }
}