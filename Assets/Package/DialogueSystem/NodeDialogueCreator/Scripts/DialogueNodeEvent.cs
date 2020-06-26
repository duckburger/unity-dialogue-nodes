using System.Collections.Generic;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    public class DialogueNodeEvent : ScriptableObject
    {
        List<DialogueEventListener> listeners = new List<DialogueEventListener>();

        public void Raise()
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].onRaised?.Invoke();
            }
        }

        public void RegisterListener(DialogueEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        public void RemoveListener(DialogueEventListener listener)
        {
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
            }
        }
    }

}

