using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupTaskTest : UIElement
{
    IEnumerator outputs = (IEnumerator)new string[] { "1", "2", "3" }.GetEnumerator();


    private void Start()
    {
        outputs.MoveNext();
    }

    public override bool OnEvent(object sender, CEvent e)
    {
        if (e is GamepadInputEvent inputEvent)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void OnEventInactive(object sender, CEvent e)
    {

    }
}
