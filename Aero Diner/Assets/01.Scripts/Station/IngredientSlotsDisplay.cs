using System.Collections.Generic;
using UnityEngine;

public class IngredientSlotsDisplay : MonoBehaviour
{
    [Header("아이콘으로 사용할 SpriteRenderer 프리팹")]
    [SerializeField] private SpriteRenderer iconPrefab;

    [Header("아이콘 간격")]
    [SerializeField] private float spacing = 0.5f;

    // FoodType 기반 슬롯 관리
    private Dictionary<FoodType, SpriteRenderer> slotIcons = new();
    private List<FoodType> orderedTypes = new();

    /// <summary>
    /// FoodType별 슬롯을 초기화합니다.
    /// </summary>
    public void Initialize(List<SlotDisplayData> slotConfigs)
    {
        ClearSlots();

        for (int i = 0; i < slotConfigs.Count; i++)
        {
            var config = slotConfigs[i];
            var icon = Instantiate(iconPrefab, transform);
            icon.sprite = config.placeholderIcon;
            icon.transform.localPosition = Vector3.right * i * spacing;

            slotIcons.Add(config.foodType, icon);
            orderedTypes.Add(config.foodType);
        }
    }

    /// <summary>
    /// 해당 FoodType의 슬롯을 비활성화합니다.
    /// </summary>
    public void ConsumeSlot(FoodType type)
    {
        if (slotIcons.TryGetValue(type, out var icon))
        {
            icon.gameObject.SetActive(false);
            slotIcons.Remove(type);
            orderedTypes.Remove(type);

            UpdateSlotPositions();
        }
    }

    /// <summary>
    /// 모든 슬롯을 제거합니다.
    /// </summary>
    public void ClearSlots()
    {
        foreach (var icon in slotIcons.Values)
            if (icon) Destroy(icon.gameObject);

        slotIcons.Clear();
        orderedTypes.Clear();
    }

    /// <summary>
    /// 남은 아이콘들을 왼쪽부터 정렬합니다.
    /// </summary>
    public void UpdateSlotPositions()
    {
        for (int i = 0; i < orderedTypes.Count; i++)
        {
            if (slotIcons.TryGetValue(orderedTypes[i], out var icon))
                icon.transform.localPosition = Vector3.right * i * spacing;
        }
    }
}