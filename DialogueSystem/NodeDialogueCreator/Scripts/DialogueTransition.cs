using System;
using UnityEditor;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    [Serializable]
    public class DialogueTransition
    {
        [SerializeReference]
        public DialogueNode StartNode;
        [SerializeReference]
        public DialogueNode EndNode;
        
        public void Initialize(DialogueNode fromNode, DialogueNode toNode)
        {
            Debug.Log($"Assigning new variables to transition");
            StartNode = fromNode;
            EndNode = toNode;
        }
#if UNITY_EDITOR
        public void Draw()
        {
            Vector3 startPos = Vector3.zero;
            if (StartNode != null && !string.IsNullOrEmpty(StartNode.WindowTitle))
            {
                startPos = new Vector3
                (
                    StartNode.WindowRect.x + StartNode.WindowRect.width,
                    StartNode.WindowRect.y + (StartNode.WindowRect.height * 0.5f),
                    0
                );
            }

            Vector3 endPos;
            if (EndNode != null && !string.IsNullOrEmpty(EndNode.WindowTitle))
            {
                endPos = new Vector3
                (
                    EndNode.WindowRect.x,
                    EndNode.WindowRect.y + (EndNode.WindowRect.height * 0.5f),
                    0
                );
            }
            else
            {
                endPos = new Vector3
                (
                    Event.current.mousePosition.x,
                    Event.current.mousePosition.y,
                    0
                );
            }


            Vector3 startTan = startPos + Vector3.right * 50f;
            Vector3 endTan = endPos + Vector3.left * 50f;

            Color shadow = new Color(0, 0, 0, 0.06f);

            // for (int i = 0; i < 3; i++)
            // {
            //     Handles.DrawBezier(startPos, endPos, startTan, endTan, shadow, null, (i + 1) * 0.5f);
            // }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 2f);


            if (Handles.Button((startPos + endPos) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            {
                DialogueEditorWindow.WindowInstance.DeleteTransition(this);
            }

            GUI.changed = true;
        }

#endif
    }
}

