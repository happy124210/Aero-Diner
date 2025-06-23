using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Game Data/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    public string id;
    public string foodName;
    public Sprite foodIcon;
}
