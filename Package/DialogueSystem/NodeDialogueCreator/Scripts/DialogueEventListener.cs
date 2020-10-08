using UnityEngine;
using UnityEngine.Events;

namespace DuckburgerDev.DialogueNodes
{
    public class DialogueEventListener : MonoBehaviour
    {
        [SerializeField] DialogueNodeEvent eventToListenFor;
        public UnityEvent onRaised;

        private void OnEnable()
        {
            eventToListenFor?.RegisterListener(this);
        }

        private void OnDisable()
        {
            eventToListenFor?.RemoveListener(this);
        }
    }
}


