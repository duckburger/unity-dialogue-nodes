using System.Collections.Generic;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "ConversationAsset", menuName = "Dialogue Editor/Conversation Asset")]
    public class ConversationAsset : ScriptableObject
    {
        [SerializeReference]
        public List<DialogueNode> allNPCNodes = new List<DialogueNode>();
        [SerializeReference]
        public List<DialogueNode> allPlayerNodes = new List<DialogueNode>();
        [SerializeField]
        public List<DialogueTransition> allTransitions = new List<DialogueTransition>();
        public bool skippable = false;
    }

}

