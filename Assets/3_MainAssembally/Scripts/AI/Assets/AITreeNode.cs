using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[Serializable]
public class AITreeNode
{
    public MethodBinding methodBinding;
    [SerializeReference]
    public AITreeNode[] children;

    [SerializeField]
    private long guid;
    public long GUID => guid;

    public AITreeNode(MethodBinding methodBinding, long guid, params AITreeNode[] children)
    {
        this.methodBinding = methodBinding;
        this.children = children;
        this.guid = guid;
    }

    public IEnumerable<AIResult> Call(AIExecutionContext context)
    {
        context = new AIExecutionContext(context.gameObject, context.log, children, context.memoryMap);
        context.log.AppenTreeNode(guid);
        var itterator = methodBinding.Bind<AIResult>(context.gameObject, context.memoryMap, context.log, ("context", context));

        foreach (var value in itterator)
        {
            if (value == AIResult.Success || value == AIResult.Failure)
            {
                yield return value;
                break;
            }
            else if (value == AIResult.Running)
            {
                yield return AIResult.Running;
            }
        }
    }

    public override string ToString()
    {
        return ToString("");
    }

    public string ToString(string indentation = "")
    {
        string[] childrenDefs = children?.Select(x => x.ToString(indentation + "   "))?.ToArray();
        string childrenString = childrenDefs == null ? "" : string.Join("", childrenDefs);
        return $"{indentation}{methodBinding.ToString()}\n{childrenString}";
    }
}