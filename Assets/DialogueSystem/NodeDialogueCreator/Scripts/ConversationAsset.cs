using System.Collections.Generic;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "ConversationAsset", menuName = "Dialogue Editor/Conversation Asset")]
    public class ConversationAsset : ScriptableObject
    {
        [SerializeReference]
        public List<NPCDialogueNode> allNPCNodes = new List<NPCDialogueNode>();
        [SerializeReference]
        public List<PlayerDialogueNode> allPlayerNodes = new List<PlayerDialogueNode>();
        [SerializeReference]
        public List<DialogueTransition> allTransitions = new List<DialogueTransition>();
        public bool skippable = false;
    }

}

