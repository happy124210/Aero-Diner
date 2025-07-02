using DG.Tweening;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private GameObject foodItemPrefab;      // 프리팹 연결
    [SerializeField] private Transform contentTransform;      // ScrollView의 Content
    [SerializeField] private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        GenerateFoodList();
    }

    private void GenerateFoodList()
    {
        // 기존 아이템 제거
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // 메뉴 가져오기
        var foodList = RestaurantManager.Instance.GetAvailableMenus();

        if (foodList == null)
        {
            Debug.LogWarning(" availableMenus 가 null입니다!");
            return;
        }

        Debug.Log($" 메뉴 수: {foodList.Length}");

        foreach (var food in foodList)
        {
            if (food == null)
            {
                Debug.LogWarning(" null인 FoodData 발견");
                continue;
            }


            var go = Instantiate(foodItemPrefab, contentTransform);
            var foodUI = go.GetComponent<MenuPanelContent>();
            foodUI.SetData(food);
        }
    }
    public void OnClickDayStartBtn()
    {
        // 두트윈으로 페이드 아웃 → 종료 후 비활성화
        canvasGroup.DOFade(0f, 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                gameObject.SetActive(false); // 비활성화
                EventBus.Raise(UIEventType.HideMenuPanel);
            });
    }
}
