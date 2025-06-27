using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trashcan : MonoBehaviour, IInteractable
{
    private OutlineShaderController outline;

    private void Awake()
    {
        outline = GetComponent<OutlineShaderController>();
    }


    public void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {

    }

    public bool PlaceIngredient(FoodData data)
    {
        return true; // 어떤 재료든 내려놓기 허용

    }

    public void OnHoverEnter()
    {
        outline?.EnableOutline();
    }
    public void OnHoverExit()
    {
        outline?.DisableOutline();
    }
}
