public enum NodeState
{
    Running,
    Success,
    Failure
}

public interface INode
{
    string NodeName { get; }
    
    NodeState Execute();
    void Reset();
}