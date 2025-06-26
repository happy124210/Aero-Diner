using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDisplay : MonoBehaviour, IInteractable
{
    public FoodData foodData;
    public Shelf originShelf;
    public void Interact(PlayerInventory playerInventory)
    {
        if (playerInventory == null) return;

        // 인벤토리에 들기 시도
        playerInventory.TryPickup(this);
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
