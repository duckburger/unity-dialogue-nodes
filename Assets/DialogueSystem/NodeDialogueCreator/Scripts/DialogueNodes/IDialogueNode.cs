using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogueNode 
{
    List<int> OutgoingTransitions();
    List<int> IncomingTransitions();

    List<int> GetConnectedPlayerResponses();
    List<int> GetConnectedNPCLines();

#if UNITY_EDITOR
    void DrawWindow();
#endif

    Rect WindowRect();
    void SetWindowRect(Rect rect);
    string DialogueLine();
    string WindowTitle();
    void DeleteAllTransitions();
    
}
