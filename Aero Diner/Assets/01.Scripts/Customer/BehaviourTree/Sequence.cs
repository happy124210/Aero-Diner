using System.Collections.Generic;

/// <summary>
/// 자식 노드들을 왼쪽에서부터 순차적으로 실행
/// Success일 경우에만 다음 노드로 넘어감
/// </summary>
public class Sequence : BaseNode
{
    public override string NodeName => "Sequence";

    private List<INode> children = new List<INode>();
    private int currentIndex;
    
    public Sequence(CustomerController customer, params INode[] nodes) : base(customer)
    {
        children.AddRange(nodes);
    }

    public override NodeState Execute()
    {
        while (currentIndex < children.Count)
        {
            customer.SetCurrentNodeName($"{NodeName}[{children[currentIndex].NodeName}]");
            var status = children[currentIndex].Execute();
            
            if (status == NodeState.Failure)
            {
                Reset();
                return NodeState.Failure;
            }
            
            if (status == NodeState.Running)
                return NodeState.Running;
                
            // 다음 노드로 이동
            currentIndex++;
        }
        
        Reset();
        return NodeState.Success;
    }
    
    public override void Reset()
    {
        currentIndex = 0;
        foreach (var child in children)
            child.Reset();
    }
}