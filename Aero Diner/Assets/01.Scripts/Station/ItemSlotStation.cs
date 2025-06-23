using UnityEngine;

public abstract class ItemSlotStation : MonoBehaviour, IInteractable
{
    [Header("아이템 슬롯 위치")]
    public Transform slotPoint;

    protected IItem storedItem;
    public IItem StoredItem => storedItem;
    public bool HasItem => storedItem != null;

    public virtual void PlaceItem(IItem item)
    {
        if (HasItem) return;

        storedItem = item;

        if (item is MonoBehaviour mb)
        {
            mb.transform.SetParent(slotPoint);
            mb.transform.localPosition = Vector3.zero;
        }
    }
    public virtual bool CanInteract()
    {
        // 기본은 항상 true
        return true;
    }
    public virtual void RemoveItem()
    {
        storedItem = null;
    }

    public abstract void Interact(PlayerInventory playerInventory);
    public virtual void OnHoverEnter() { }
    public virtual void OnHoverExit() { }
}