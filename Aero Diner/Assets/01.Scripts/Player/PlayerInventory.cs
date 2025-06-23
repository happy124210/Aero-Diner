using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private IItem heldItem;
    public IItem HeldItem => heldItem;
    public bool IsHoldingItem => heldItem != null;


    [SerializeField] private Transform itemSlotTransform;
    [SerializeField] private SpriteRenderer itemSlotRenderer;
    [SerializeField] private TextMeshProUGUI currentItemname;
    public void TryPickup(IInteractable target)
    {
        if (heldItem != null) return;

        IItem itemToPickup = null;

        if (target is IItem directItem)
        {
            itemToPickup = directItem;
        }
        else if (target is ItemSlotStation station && station.HasItem)
        {
            itemToPickup = station.StoredItem;

            // 슬롯 비우기
            station.RemoveItem();
        }

        if (itemToPickup != null)
        {
            HoldItem(itemToPickup);
            Debug.Log("아이템을 주웠습니다.");
        }
        else
        {
            Debug.Log("이 오브젝트는 아이템이 아닙니다.");
        }
    }
    public void DropItem(IInteractable target)
    {
        if (heldItem == null) return;

        if (target is ItemSlotStation slot)
        {
            if (slot.HasItem)
            {
                Debug.Log("이 설비에는 이미 아이템이 있습니다.");
                return;
            }

            slot.PlaceItem(heldItem);
            ClearItem();
            Debug.Log("설비 위에 아이템을 놓았습니다.");
        }
        else
        {
            Debug.Log("이 오브젝트는 아이템을 놓을 수 없습니다.");
        }
    }
    public void HoldItem(IItem item)
    {
        heldItem = item;

        var itemBehaviour = item as MonoBehaviour;
        if (itemBehaviour)
        {
            // Raycast 대상에서 제외
            itemBehaviour.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            itemBehaviour.transform.SetParent(itemSlotTransform);
            itemBehaviour.transform.localPosition = Vector3.zero;
        }
    }
    public void ClearItem()
    {
        heldItem = null;
        //currentItemname.gameObject.SetActive(false);
    }
    //public void Use()
    //{
    //    if (heldItem != null)
    //    {
    //        heldItem.Use(); // IItem에 정의된 메서드
    //    }
    //}
    //플레이어에서 이 함수를 사용할 일이 있을까? 싶어서 주석처리.
}
