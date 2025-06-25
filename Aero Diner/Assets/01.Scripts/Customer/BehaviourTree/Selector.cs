using System.Collections.Generic;

/// <summary>
/// 여러 노드 중 하나만 선택하서 실행
/// Failure일 경우 다음 노드로 넘어감
/// </summary>
public class Selector : BaseNode
{
    public override string NodeName { get; }
    
    private List<INode> children = new List<INode>();
    private int currentIndex = 0;
    
    public Selector(CustomerController customer, params INode[] nodes) : base(customer)
    {
        NodeName = "Selector";
        children.AddRange(nodes);
    }
    
    public override NodeState Execute()
    {
        while (currentIndex < children.Count)
        {
            customer.SetCurrentNodeName($"{NodeName}[{children[currentIndex].NodeName}]");
            var status = children[currentIndex].Execute();
            
            if (status == NodeState.Success)
            {
                Reset();
                return NodeState.Success;
            }
            
            if (status == NodeState.Running)
                return NodeState.Running;
                
            // 다음 노드로 이동
            currentIndex++;
        }
        
        Reset();
        return NodeState.Failure;
    }
    
    public override void Reset()
    {
        currentIndex = 0;
        foreach (var child in children)
            child.Reset();
    }
}