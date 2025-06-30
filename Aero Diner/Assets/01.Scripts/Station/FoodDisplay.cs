using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDisplay : MonoBehaviour, IInteractable
{
    public FoodData foodData;
    public Shelf originShelf;
    public AutomaticStation originAutomatic;
    public PassiveStation originPassive;

    // IIngredientData로 접근할 수 있도록 추가
    public CookingSOGroup.IIngredientData data => foodData;


    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (playerInventory == null) return;

        //상호작용 타입을 들기로 고정.
        if (interactionType == InteractionType.Pickup)
        {
            playerInventory.TryPickup(this);
        }
        else
        {
            Debug.Log("FoodDisplay는 상호작용할 기능이 없습니다.");
        }
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
