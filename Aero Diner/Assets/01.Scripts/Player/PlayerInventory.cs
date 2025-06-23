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
    public void TryPickup(IInteractable target)
    {
        if (heldItem != null) return;

        if (target is IItem item)
        {
            HoldItem(item); // ← 바로 여기서 이 함수 사용!

            var itemBehaviour = item as MonoBehaviour;
            if (itemBehaviour != null)
                itemBehaviour.transform.SetParent(itemSlotTransform);

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

        if (target != null)
        {
            var targetMono = target as MonoBehaviour;
            var itemBehaviour = heldItem as MonoBehaviour;

            if (targetMono != null && itemBehaviour != null)
            {
                itemBehaviour.transform.SetParent(targetMono.transform);
                itemBehaviour.transform.position = targetMono.transform.position;
            }

            ClearItem();
            Debug.Log("아이템을 상호작용 대상 위에 놓았습니다.");
        }
        else
        {
            Debug.Log("놓을 수 있는 대상이 없습니다.");
        }
    }
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
    //public void Use()
    //{
    //    if (heldItem != null)
    //    {
    //        heldItem.Use(); // IItem에 정의된 메서드
    //    }
    //}
    //플레이어에서 이 함수를 사용할 일이 있을까? 싶어서 주석처리.
}
