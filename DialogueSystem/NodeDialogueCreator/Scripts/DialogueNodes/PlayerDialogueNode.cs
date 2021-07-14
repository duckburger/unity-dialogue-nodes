using UnityEditor;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    [System.Serializable]
    public class PlayerDialogueNode : DialogueNode
    {
#if UNITY_EDITOR

        public PlayerDialogueNode(Rect rect, string title) : base(rect, title)
        {
            
        }

        public override void AddIncomingTransition(DialogueTransition transitionToAdd)
        {
            base.AddIncomingTransition(transitionToAdd);
            transitionToAdd.StartNode.PlayerResponses.Add(this);
        }

        public override void RemoveIncomingTransition(DialogueTransition transitionToRemove)
        {
            base.RemoveIncomingTransition(transitionToRemove);
            transitionToRemove.StartNode.PlayerResponses.Remove(this);
        }
        
        public override void DrawWindow()
        {
            EditorStyles.textField.wordWrap = true;
            DialogueLine = EditorGUILayout.TextArea(DialogueLine, GUILayout.Height(88f));
        }

#endif

        public override void Drag(Vector2 dragDelta)
        {
            Rect newRect = WindowRect;
            newRect.position += dragDelta;
            WindowRect = newRect;
        }
    }

}
