using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    public Item heldItem;

    public bool HasItem => heldItem != null;

    public void Hold(Item item)
    {
        heldItem = item;
        // UI 업데이트 등
    }

    public Item Drop()
    {
        Item temp = heldItem;
        heldItem = null;
        return temp;
    }
}
