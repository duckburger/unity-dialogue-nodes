using System;
using UnityEditor;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    [Serializable]
    public class DialogueTransition
    {
        [SerializeReference]
        public NPCDialogueNode StartNPCNode;
        [SerializeReference]
        public PlayerDialogueNode StartPlayerNode;
        [SerializeReference]
        public PlayerDialogueNode EndPlayerNode;
        [SerializeReference]
        public NPCDialogueNode EndNPCNode;

        
        public void Initialize(NPCDialogueNode fromNPCNode, PlayerDialogueNode fromPlayerNode, NPCDialogueNode toNPCNode, PlayerDialogueNode toPlayerNode, ConversationAsset convo)
        {
            Debug.Log($"Assigning new variables to transition");
            if (fromNPCNode != null)
            {
                StartNPCNode = fromNPCNode;
                StartPlayerNode = null;
            }
            else if (fromPlayerNode != null)
            {
                StartPlayerNode = fromPlayerNode;
                StartNPCNode = null;
            }

            if (toNPCNode != null)
            {
                EndNPCNode = toNPCNode;
                EndPlayerNode = null;
            }
            else if (toPlayerNode != null)
            {
                EndPlayerNode = toPlayerNode;
                EndNPCNode = null;
            }
        }
#if UNITY_EDITOR
        public void Draw()
        {
            Vector3 startPos = Vector3.zero;
            if (StartNPCNode != null && !string.IsNullOrEmpty(StartNPCNode.WindowTitle))
            {
                startPos = new Vector3
                (
                    StartNPCNode.WindowRect.x + StartNPCNode.WindowRect.width,
                    StartNPCNode.WindowRect.y + (StartNPCNode.WindowRect.height * 0.5f),
                    0
                );
            }
            else if (StartPlayerNode != null && !string.IsNullOrEmpty(StartPlayerNode.WindowTitle))
            {
                startPos = new Vector3
                (
                    StartPlayerNode.WindowRect.x + StartPlayerNode.WindowRect.width,
                    StartPlayerNode.WindowRect.y + (StartPlayerNode.WindowRect.height * 0.5f),
                    0
                );
            }

            Vector3 endPos;
            if (EndNPCNode != null && !string.IsNullOrEmpty(EndNPCNode.WindowTitle))
            {
                endPos = new Vector3
                (
                    EndNPCNode.WindowRect.x,
                    EndNPCNode.WindowRect.y + (EndNPCNode.WindowRect.height * 0.5f),
                    0
                );
            }
            else if (EndPlayerNode != null && !string.IsNullOrEmpty(EndPlayerNode.WindowTitle))
            {
                endPos = new Vector3
                (
                    EndPlayerNode.WindowRect.x,
                    EndPlayerNode.WindowRect.y + (EndPlayerNode.WindowRect.height * 0.5f),
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

            for (int i = 0; i < 3; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadow, null, (i + 1) * 0.5f);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 2f);


            if (Handles.Button((startPos + endPos) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
            {
                DialogueEditorWindow.WindowInstance.DeleteTransition(this);
            }

            // if (endPlayerNode == null && endNPCNode == null || string.IsNullOrEmpty(endPlayerNode.windowTitle) && string.IsNullOrEmpty(endNPCNode.windowTitle))
            GUI.changed = true;
        }

#endif
    }
}

