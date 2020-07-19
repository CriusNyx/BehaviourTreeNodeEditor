using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UIStack
{
    private Stack<UIElement> stack = new Stack<UIElement>();

    public void Push(UIElement element)
    {
        stack.Push(element);
        element.SetOwningStack(this);
    }

    public void Pop()
    {
        stack.Pop();
    }

    public bool Pop(UIElement element)
    {
        if(stack.Peek() == element)
        {
            stack.Pop();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SendEvent(object sender, CEvent e)
    {
        var elements = stack.ToArray();
        foreach(var element in elements)
        {
            if(element.OnEvent(sender, e))
            {
                break;
            }
        }
        foreach(var element in elements)
        {
            element.OnEventInactive(sender, e);
        }
    }
}
