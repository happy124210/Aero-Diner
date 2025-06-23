using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private IItem heldItem;
    public IItem HeldItem => heldItem;
    public bool IsHoldingItem => heldItem != null;


    [SerializeField] private Transform itemSlotTransform;
    [SerializeField] private SpriteRenderer itemSlotRenderer;
    [SerializeField] private TextMeshProUGUI curruntItemname;
    public void HoldItem(IItem item)
    {
        heldItem = item;
        itemSlotRenderer.sprite = item.GetSprite(); // IItem에 GetSprite() 정의 필요
        curruntItemname.text = item.GetItemName();
        curruntItemname.gameObject.SetActive(true);
        itemSlotTransform.gameObject.SetActive(true);
    }
    public void ClearItem()
    {
        heldItem = null;
        curruntItemname.gameObject.SetActive(false);
        itemSlotTransform.gameObject.SetActive(false);
    }
    public void Use()
    {
        if (heldItem != null)
        {
            heldItem.Use(); // IItem에 정의된 메서드
        }
    }
}
