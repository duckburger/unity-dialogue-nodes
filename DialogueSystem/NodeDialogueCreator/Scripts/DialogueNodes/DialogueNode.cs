using System;
using System.Collections.Generic;
using System.IO;
using DuckburgerDev.DialogueNodes;
using UnityEditor;
using UnityEngine;

[Serializable]
public abstract class DialogueNode 
{
    protected const string ADD_A_SPEAKER_TO_MODIFY = "Add a speaker to modify";
    protected const string CHARACTER_NAME = "Character Name";
    protected const string CHARACTER_TEXT_COLOR = "Character Text Color";
    protected const string DIALOGUE_LINE = "Dialogue Line";
    protected const string ADD_EVENT = "+ Event";
    protected const string REMOVE_EVENT = "- Event";
    
    [SerializeReference]
    private List<DialogueTransition> _outgoingTransitions = new List<DialogueTransition>();
    [SerializeReference]
    private List<DialogueTransition> _incomingTransitions = new List<DialogueTransition>();

    [SerializeReference]
    protected List<DialogueNode> _connectedPlayerResponses = new List<DialogueNode>();
    [SerializeReference]
    protected List<DialogueNode> _connectedNPCLines = new List<DialogueNode>();

    [SerializeField]
    private string _dialogueLine;
    [SerializeField]
    private string _windowTitle; 
    [SerializeField]
    private Rect _windowRect;
    
    public DialogueNodeEvent attachedEvent;
    public List<DialogueTransition> OutgoingTransitions => _outgoingTransitions;
    public List<DialogueTransition> IncomingTransitions => _incomingTransitions;
    public List<DialogueNode> PlayerResponses => _connectedPlayerResponses;
    public List<DialogueNode> NpcResponses => _connectedNPCLines;
    public string DialogueLine { get => _dialogueLine; protected set => _dialogueLine = value; }
    public string WindowTitle { get => _windowTitle; protected set => _windowTitle = value; }
    public Rect WindowRect { get => _windowRect; protected set => _windowRect = value; }
    
    protected float windowHeight = 0;
    protected string pathToConvoAsset;
    protected string fullPathToAsset;
    

#if UNITY_EDITOR
    public virtual void DrawWindow()
    {
        EditorGUI.BeginChangeCheck();

        windowHeight += 35f;
        
        if (!attachedEvent)
        {
            if (GUILayout.Button(ADD_EVENT))
            {
                if (string.IsNullOrEmpty(pathToConvoAsset))
                {
                    pathToConvoAsset = AssetDatabase.GetAssetPath(DialogueEditorWindow.WindowInstance.currentAsset);
                }
                DialogueNodeEvent newEvent = ScriptableObject.CreateInstance<DialogueNodeEvent>();
                string pathToFolder = Path.GetDirectoryName(pathToConvoAsset);
                fullPathToAsset = $"{pathToFolder}\\{DialogueEditorWindow.WindowInstance.currentAsset.name}-Events";
                if (!Directory.Exists(fullPathToAsset))
                {
                    Directory.CreateDirectory(fullPathToAsset);
                }
                AssetDatabase.CreateAsset(newEvent, $"{fullPathToAsset}\\EventForNode#{DialogueEditorWindow.WindowInstance.currentAsset.allNPCNodes.IndexOf(this)}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                attachedEvent = newEvent;
            }
        }
        else
        {
            
            if (GUILayout.Button(REMOVE_EVENT))
            {
                if (attachedEvent)
                {
                    bool deleted = AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(attachedEvent));
                    if (!deleted)
                        attachedEvent = null;
                    AssetDatabase.Refresh();
                }
            }
            EditorGUILayout.ObjectField(attachedEvent as UnityEngine.Object, typeof(DialogueNodeEvent), false);
            windowHeight += 15f;
        }
        EditorGUI.EndChangeCheck();

    }

    public DialogueNode(Rect rect, string title)
    {
        WindowRect = rect;
        WindowTitle = title;
    }
    
#endif

    public virtual void AddIncomingTransition(DialogueTransition transitionToAdd)
    {
        _incomingTransitions.Add(transitionToAdd);
    }
    public virtual void AddOutgoingTransition(DialogueTransition transitionToAdd)
    {
        _outgoingTransitions.Add(transitionToAdd);
    }

    public virtual void RemoveOutgoingTransition(DialogueTransition transitionToRemove)
    {
        _outgoingTransitions.Remove(transitionToRemove);
    }

    public virtual void RemoveIncomingTransition(DialogueTransition transitionToRemove)
    {
        _incomingTransitions.Remove(transitionToRemove);
    }
    public virtual bool HasOutgoingTransition(DialogueTransition transitionToCheck)
    {
        return _outgoingTransitions.Contains(transitionToCheck);
    }

    public virtual void SetWindowRect(Rect rect)
    {
        WindowRect = rect;
    }

    public virtual void SetDialogueLine(string newLine)
    {
        DialogueLine = newLine;
    }

    public virtual void DeleteAllTransitions()
    {
        _incomingTransitions.Clear();
        _outgoingTransitions.Clear();
    }

    public abstract void Drag(Vector2 dragDelta);
}
