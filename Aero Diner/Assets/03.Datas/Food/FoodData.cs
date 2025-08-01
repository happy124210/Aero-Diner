using UnityEngine;

/// <summary>
/// 음식/재료 데이터
/// </summary>
public enum FoodType
{
    Water,
    Topping,
    Sauce,
    Noodle,
    Menu,
    Raw,
    Cheese,
    Chili,
    Capsicum,
    Pepper,
    DryNoodle,
    GratedCheese
}

[CreateAssetMenu(fileName = "New Food Data", menuName = "Game Data/Food Data")]
public class FoodData : ScriptableObject
{
    [Header("음식 정보")]
    public string id; // 고유 아이디, 레시피(ingredients)에 사용
    public string foodName;
    public string displayName; // UI용 한글 이름
    public FoodType foodType;
    public Sprite foodIcon;
    public string description;
    public StationType[] stationType; // 가공될 설비
    public string[] ingredients; // 레시피. 원재료라면 null
    public float cookTime;
    public int foodCost;
    public string recipeDescription;
}