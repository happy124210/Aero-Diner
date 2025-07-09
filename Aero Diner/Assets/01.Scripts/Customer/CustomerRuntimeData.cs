public class CustomerRuntimeData
{
    public CustomerStateName CurrentState { get; set; }
    public float CurrentPatience { get; set; }
    public FoodData CurrentOrder { get; set; }
    public Table AssignedTable { get; set; }
    public float EatingTimer { get; set; }


    public CustomerRuntimeData(float maxPatience, float eatingTimer)
    {
        CurrentPatience = maxPatience;
        CurrentOrder = null;
        AssignedTable = null;
        EatingTimer = eatingTimer;
    }
}
