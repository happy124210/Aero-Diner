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

    public bool PlaceIngredient(ScriptableObject dataRaw)
    {
        // IIngredientData인지만 검사
        if (dataRaw is CookingSOGroup.IIngredientData)
            return true;

        return false;
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
