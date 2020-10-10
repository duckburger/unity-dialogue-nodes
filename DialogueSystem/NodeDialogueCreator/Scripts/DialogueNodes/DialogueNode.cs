using System;
using System.Collections.Generic;
using DuckburgerDev.DialogueNodes;
using UnityEngine;

[Serializable]
public abstract class DialogueNode 
{
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
    
    public List<DialogueTransition> OutgoingTransitions => _outgoingTransitions;
    public List<DialogueTransition> IncomingTransitions => _incomingTransitions;
    public List<DialogueNode> PlayerResponses => _connectedPlayerResponses;
    public List<DialogueNode> NpcResponses => _connectedNPCLines;
    public string DialogueLine { get => _dialogueLine; protected set => _dialogueLine = value; }
    public string WindowTitle { get => _windowTitle; protected set => _windowTitle = value; }
    public Rect WindowRect { get => _windowRect; protected set => _windowRect = value; }

#if UNITY_EDITOR
    public abstract void DrawWindow();

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
