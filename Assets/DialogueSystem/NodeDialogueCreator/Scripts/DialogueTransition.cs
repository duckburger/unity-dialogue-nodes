using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[Serializable]
public class DialogueTransition: ISerializationCallbackReceiver
{
    public int id;
    [SerializeField] ConversationAsset containingConversation;
    public NPCDialogueNode startNPCNode;
    public NPCDialogueNode endNPCNode;
    public PlayerDialogueNode startPlayerNode;
    public PlayerDialogueNode endPlayerNode;

    public void Initialize(NPCDialogueNode fromNPCNode, PlayerDialogueNode fromPlayerNode, NPCDialogueNode toNPCNode, PlayerDialogueNode toPlayerNode, ConversationAsset convo)
    {
        Debug.Log($"Assigning new variables to transition");
        if (fromNPCNode != null)
        {
            startNPCNode = fromNPCNode;
            startPlayerNode = null;
        }
        else if (fromPlayerNode != null)
        {
            startPlayerNode = fromPlayerNode;
            startNPCNode = null;
        }

        if (toNPCNode != null)
        {
            endNPCNode = toNPCNode;
            endPlayerNode = null;
        }
        else if (toPlayerNode != null)
        {
            endPlayerNode = toPlayerNode;
            endNPCNode = null;
        }

        containingConversation = convo;

        id = UnityEngine.Random.Range(1, Int32.MaxValue);
        while (containingConversation.GetTransitionByID(id) != null)
            id = UnityEngine.Random.Range(0, Int32.MaxValue);
    }

    public void Draw()
    {        
        Vector3 startPos = Vector3.zero;
        if (startNPCNode != null && !string.IsNullOrEmpty(startNPCNode.windowTitle))
        {
            startPos = new Vector3
            (
                startNPCNode.windowRect.x + startNPCNode.windowRect.width,
                startNPCNode.windowRect.y + (startNPCNode.windowRect.height * 0.5f),
                0
            );
        }
        else if (startPlayerNode != null && !string.IsNullOrEmpty(startPlayerNode.windowTitle))
        {
            startPos = new Vector3
            (
                startPlayerNode.windowRect.x + startPlayerNode.windowRect.width,
                startPlayerNode.windowRect.y + (startPlayerNode.windowRect.height * 0.5f),
                0
            );
        }
        
        Vector3 endPos;
        if (endNPCNode != null && !string.IsNullOrEmpty(endNPCNode.windowTitle))
        {
            endPos = new Vector3
            (   
                endNPCNode.windowRect.x,
                endNPCNode.windowRect.y + (endNPCNode.windowRect.height * 0.5f),
                0
            );
        }
        else if (endPlayerNode != null && !string.IsNullOrEmpty(endPlayerNode.windowTitle))
        {
            endPos = new Vector3
            (   
                endPlayerNode.windowRect.x,
                endPlayerNode.windowRect.y + (endPlayerNode.windowRect.height * 0.5f),
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

        Color shadow = new Color(0,0,0, 0.06f);

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

    public void OnBeforeSerialize()
    {
        if (startNPCNode != null && startNPCNode.id > 0)
        {   
            startNPCNode = containingConversation.GetNPCNodyByID(startNPCNode.id);
        }
        if (startPlayerNode != null && startPlayerNode.id > 0)
        {
            startPlayerNode = containingConversation.GetPlayerNodeByID(startPlayerNode.id);
        }
        if (endNPCNode != null && endNPCNode.id > 0)
        {
            endNPCNode = containingConversation.GetNPCNodyByID(endNPCNode.id);
        }
        if (endPlayerNode != null && endPlayerNode.id > 0)
        {
            endPlayerNode = containingConversation.GetPlayerNodeByID(endPlayerNode.id);
        }
    }

    public void OnAfterDeserialize()
    {
        if (startNPCNode != null && startNPCNode.id > 0)
        {   
            startNPCNode = containingConversation.GetNPCNodyByID(startNPCNode.id);
        }
        if (startPlayerNode != null && startPlayerNode.id > 0)
        {
            startPlayerNode = containingConversation.GetPlayerNodeByID(startPlayerNode.id);
        }
        if (endNPCNode != null && endNPCNode.id > 0)
        {
            endNPCNode = containingConversation.GetNPCNodyByID(endNPCNode.id);
        }
        if (endPlayerNode != null && endPlayerNode.id > 0)
        {
            endPlayerNode = containingConversation.GetPlayerNodeByID(endPlayerNode.id);
        }
    }
}
