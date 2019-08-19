using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Events;
using System.Reflection;
using System.IO;

[System.Serializable]
public class NPCDialogueNode : IDialogueNode, ISerializationCallbackReceiver
{
    [SerializeField] ConversationAsset containingConversation;
    public int id;
    public Rect windowRect;
    public string windowTitle;
    public List<int> outgoingTransitions = new List<int>();
    public List<int> incomingTransitions = new List<int>();
    [TextArea(3, 10)]
    public string dialogueLine;
    public DialogueNodeEvent attachedEvent;

#region Paths

    string pathToConvoAsset;
    string fullPathToAsset;

#endregion

    public NPCDialogueNode(Rect rect, string title, ConversationAsset convo)
    {
        this.windowRect = rect;
        this.windowTitle = title;
        this.containingConversation = convo;
        this.pathToConvoAsset = AssetDatabase.GetAssetPath(containingConversation);
        id = UnityEngine.Random.Range(1, Int32.MaxValue);
        while (containingConversation.GetNPCNodyByID(id) != null)
            id = UnityEngine.Random.Range(1, Int32.MaxValue);        
    }

    public DialogueCharacter speaker;
    public void DrawWindow()
    {
        speaker = (DialogueCharacter) EditorGUILayout.ObjectField(speaker, typeof(DialogueCharacter), false);

        if (speaker == null)
        {
            EditorGUILayout.LabelField("Add a speaker to modify");
        }
        else
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Character Icon");
            speaker.icon = (Sprite) EditorGUILayout.ObjectField(GUIContent.none, speaker.icon, typeof(Sprite), false, GUILayout.ExpandWidth(true));
            GUILayout.Label("Character Name");
            speaker.name = EditorGUILayout.TextField(speaker.name);
            GUILayout.Label("Character Text Color");
            speaker.textColor = EditorGUILayout.ColorField(speaker.textColor);
            GUILayout.EndVertical();            
            EditorGUILayout.LabelField("Dialogue Line");
            EditorStyles.textField.wordWrap = true;
            dialogueLine = EditorGUILayout.TextArea(dialogueLine, GUILayout.Height(88f));   
            
            EditorGUI.BeginChangeCheck();

            if (!attachedEvent)
            {
                if (GUILayout.Button("+ Event"))
                {                    
                    if (string.IsNullOrEmpty(pathToConvoAsset))
                    {
                        pathToConvoAsset = AssetDatabase.GetAssetPath(containingConversation);
                    }
                    DialogueNodeEvent newEvent = ScriptableObject.CreateInstance<DialogueNodeEvent>();
                    string directoryName = $"{containingConversation.name}_events";
                    string pathToFolder = Path.GetDirectoryName(pathToConvoAsset);                    
                    fullPathToAsset = $"{pathToFolder}\\{containingConversation.name}_Events";
                    if ( !Directory.Exists( fullPathToAsset ) )
                    {
                        Directory.CreateDirectory( fullPathToAsset );
                    }
                    AssetDatabase.CreateAsset(newEvent, $"{fullPathToAsset}\\EventForNode#{containingConversation.allNPCNodes.IndexOf(this)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    attachedEvent = newEvent;
                }
            }
            else
            {
                if (GUILayout.Button("- Event"))
                {
                    if (attachedEvent)
                    {
                        bool deleted = AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(attachedEvent));
                        Debug.Log($"Event was deleted = {deleted}");
                        if (!deleted)
                            attachedEvent = null;
                        AssetDatabase.Refresh();
                    }
                }
                EditorGUILayout.ObjectField(attachedEvent as UnityEngine.Object, typeof(DialogueNodeEvent), false);
            }

            EditorGUI.EndChangeCheck();            

            windowRect.height = 350f;
        }                
    }


    public void Drag(Vector2 dragDelta)
    {
        windowRect.position += dragDelta;
    }

    #region Interface Requirements

    public List<int> OutgoingTransitions()
    {
        return outgoingTransitions;
    }

    public List<int> IncomingTransitions()
    {
        return incomingTransitions;
    }

    public Rect WindowRect()
    {
        return windowRect;
    }

    public string WindowTitle()
    {
        return windowTitle;
    }

    public string DialogueLine()
    {
        return dialogueLine;
    }

    public List<int> GetConnectedPlayerResponses()
    {
        List<int> responses = new List<int>();
        for (int i = 0; i < outgoingTransitions.Count; i++)
        {
            if (containingConversation.GetTransitionByID(outgoingTransitions[i]).endPlayerNode.id > 0)
                responses.Add(containingConversation.GetTransitionByID(outgoingTransitions[i]).endPlayerNode.id);            
        }
        return responses;
    }

    public List<int> GetConnectedNPCLines()
    {
        List<int> npcLineNodes = new List<int>();
        for (int i = 0; i < outgoingTransitions.Count; i++)
        {
            if (containingConversation.GetTransitionByID(outgoingTransitions[i]).endNPCNode.id > 0)
                npcLineNodes.Add(containingConversation.GetTransitionByID(outgoingTransitions[i]).endNPCNode.id);        
        }
        return npcLineNodes;
    }

    public void DeleteAllTransitions()
    {
        outgoingTransitions.Clear();
        incomingTransitions.Clear();
    }

    public void SetWindowRect(Rect rect)
    {
        this.windowRect = rect;
    }

    public void OnBeforeSerialize()
    {
        if (containingConversation != null)        
        {
            NPCDialogueNode myNode = containingConversation.GetNPCNodyByID(id);
            if (myNode != null)
            {
                this.windowRect = myNode.windowRect;
                this.windowTitle = myNode.windowTitle;
                this.dialogueLine = myNode.dialogueLine;
            }   
        }            
    }

    public void OnAfterDeserialize()
    {
        if (containingConversation != null)        
        {
            NPCDialogueNode myNode = containingConversation.GetNPCNodyByID(id);
            if (myNode != null)
            {
                this.windowRect = myNode.windowRect;
                this.windowTitle = myNode.windowTitle;
                this.dialogueLine = myNode.dialogueLine;
            }   
        }      
    }

    #endregion
}
