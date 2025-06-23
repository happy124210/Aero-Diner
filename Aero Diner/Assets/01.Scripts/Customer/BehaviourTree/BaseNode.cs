public abstract class BaseNode : INode
{
    protected CustomerController customer;
    
    public abstract string NodeName { get; }
    
    protected BaseNode(CustomerController customer)
    {
        this.customer = customer;
    }
    
    public abstract NodeState Execute();
    public virtual void Reset() { }
}
