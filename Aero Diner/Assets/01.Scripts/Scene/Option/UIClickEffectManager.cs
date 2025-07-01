using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIClickEffectManager : MonoBehaviour
{
    [SerializeField] private Canvas canvas;               // 월드 ↔ UI 좌표 변환용
    [SerializeField] private RectTransform effectPrefab;  // 클릭 효과 프리팹

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            CreateEffect(Input.mousePosition);
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject(); // UI 위 클릭 여부
    }

    private void CreateEffect(Vector2 screenPos)
    {
        RectTransform effect = Instantiate(effectPrefab, canvas.transform);
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, canvas.worldCamera, out localPos);
        effect.anchoredPosition = localPos;

        // 초기 설정
        effect.localScale = Vector3.zero;
        CanvasGroup group = effect.GetComponent<CanvasGroup>();
        group.alpha = 1f;

        // DOTween 연출
        Sequence seq = DOTween.Sequence();
        seq.Append(effect.DOScale(1.5f, 0.5f).SetEase(Ease.OutCubic));
        seq.Join(group.DOFade(0f, 0.5f));
        seq.OnComplete(() => Destroy(effect.gameObject));
    }
}
