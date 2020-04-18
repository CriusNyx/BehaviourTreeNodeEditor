using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEditor.NodeEditor
{
    [Serializable]
    public class NodeEditorDropBox : NodeEditorGUIControl
    {
        Rect myRect;

        public NodeEditorDropBox(NodeEditorNode parent) : base(parent)
        {
        }

        public Vector2 centerLastFrame { get; private set; }

        public override void OnGUI(NodeEditorWindow nodeEditor, int id)
        {
            myRect = GUILayoutUtility.GetRect(10, 10);
            GUI.Box(myRect, "");
            if (Event.current.type == EventType.Repaint)
            {
                centerLastFrame = myRect.center + Parent.Position;
            }

            var active = nodeEditor.activeControlSet.Get<NodeEditorGrabBox>();
            if (active != null)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    if (myRect.Contains(Event.current.mousePosition))
                    {
                        active.OnDrop(this);
                    }
                }
            }
        }

        public override void ProcessEvents(NodeEditorWindow nodeEditor)
        {
            var active = nodeEditor.activeControlSet.Get<NodeEditorGrabBox>();
        }
    }
}
