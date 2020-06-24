using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class BaseDialogueNode
{
    public Rect windowRect;
    public string windowTitle;
    public List<DialogueTransition> outgoingTransitions = new List<DialogueTransition>();
    public List<DialogueTransition> incomingTransitions = new List<DialogueTransition>();

    [TextArea(3, 10)]
    public string dialogueLine;
    public virtual void DrawWindow() {}

    public virtual void Drag(Vector2 dragDelta) {}

#if UNITY_EDITOR

    public virtual void DeleteAllTransitions()
    {
        for (int i = 0; i < outgoingTransitions.Count; i++)
        {
            DialogueEditorWindow.WindowInstance.DeleteTransition(outgoingTransitions[i]);
        }

        for (int i = 0; i < incomingTransitions.Count; i++)
        {
            DialogueEditorWindow.WindowInstance.DeleteTransition(incomingTransitions[i]);
        }
    }

#endif

}
