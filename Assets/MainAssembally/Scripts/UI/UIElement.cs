using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class UIElement : MonoBehaviour, CEventListener
{
    UIStack owningStack;
    public abstract bool OnEvent(object sender, CEvent e);
    public abstract void OnEventInactive(object sender, CEvent e);

    public void SetOwningStack(UIStack stack)
    {
        if (owningStack != null)
        {
            throw new System.InvalidOperationException("This element has already been asigned to a stack");
        }
        this.owningStack = stack;
    }

    public virtual void Pop(bool destroy)
    {
        if (owningStack.Pop(this))
        {
            owningStack = null;
        }
        if (destroy)
        {
            Destroy(gameObject);
        }
    }
}