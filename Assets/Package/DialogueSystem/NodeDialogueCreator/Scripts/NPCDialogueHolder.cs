using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{

    public class NPCDialogueHolder : MonoBehaviour
    {
        public bool canTalk = true;
        public ConversationAsset myDialogue;
        [SerializeField] ScriptableEvent onDialogueActivated;

        public void ActivateDialogue()
        {
            if (!myDialogue)
            {
                Debug.LogError($"{gameObject.name} is trying to activate dialoguem but doesn't have a dialogue asset");
                return;
            }

            onDialogueActivated?.RaiseWithData(myDialogue);
        }
    }
}

