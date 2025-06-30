public abstract class CustomerState
{
    public abstract void Enter(CustomerController customer);
    public abstract CustomerState Update(CustomerController customer);
    public abstract void Exit(CustomerController customer);
    public abstract string StateName { get; }
}