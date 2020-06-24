using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "ConversationAsset", menuName = "Dialogue Editor/Conversation Asset")]
public class ConversationAsset : ScriptableObject
{
    public List<NPCDialogueNode> allNPCNodes = new List<NPCDialogueNode>();
    public List<PlayerDialogueNode> allPlayerNodes = new List<PlayerDialogueNode>();
    public List<DialogueTransition> allTransitions = new List<DialogueTransition>();
    public bool skippable = false;

    public NPCDialogueNode GetNPCNodyByID(int id)
    {
        for (int i = 0; i < allNPCNodes.Count; i++)
        {
            if (allNPCNodes[i].id == id)
            {
                return allNPCNodes[i];
            }
        }
        return null;
    }

    public PlayerDialogueNode GetPlayerNodeByID(int id)
    {
        for (int i = 0; i < allPlayerNodes.Count; i++)
        {
            if (allPlayerNodes[i].id == id)
            {
                return allPlayerNodes[i];
            }
        }
        return null;
    }

    public DialogueTransition GetTransitionByID(int id)
    {
        for (int i = 0; i < allTransitions.Count; i++)
        {
            if (allTransitions[i].id == id)
            {
                return allTransitions[i];
            }
        }
        return null;
    }
    
}
