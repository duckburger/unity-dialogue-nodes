using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(menuName = "Dialogue Events/Scriptable Event")]
public class ScriptableEvent : ScriptableObject
{
    List<ScriptableEventListener> listeners = new List<ScriptableEventListener>();

    #region Raising Events

    public void Raise()
    {
        if (listeners.Count <= 0)
        {
            // Debug.Log("Trying to raise an event - " + this.name + " - without any listeners");
            return;
        }
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].Raise();
        }
    }

    public void RaiseWithData(object obj)
    {
        if (listeners.Count <= 0)
        {
            // Debug.Log("Trying to raise an event - " + this.name + " - without any listeners");
            return;
        }
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].RaiseWithData(obj);
        }
    }

    public void Activate()
    {
        if (listeners.Count <= 0)
        {
            // Debug.Log("Trying to raise an event - " + this.name + " - without any listeners");
            return;
        }
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].Activate();
        }
    }

    public void ActivateWithData(object o)
    {
        if (listeners.Count <= 0)
        {
            // Debug.Log("Trying to raise an event - " + this.name + " - without any listeners");
            return;
        }
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].Activate(o);
        }
    }


    public void Deactivate()
    {
        if (listeners.Count <= 0)
        {
            // Debug.Log("Trying to raise an event - " + this.name + " - without any listeners");
            return;
        }
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].Deactivate();
        }
    }

    public void DeactivateWithData(object o)
    {
        if (listeners.Count <= 0)
        {
            // Debug.Log("Trying to raise an event - " + this.name + " - without any listeners");
            return;
        }
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].Deactivate(o);
        }
    }


    #endregion

    #region Registering/DeRegistering Listeners

    public void RegisterListener(ScriptableEventListener newListener)
    {
        if (!listeners.Contains(newListener))
        {
            this.listeners.Add(newListener);
        }
    }

    public void DeRegisterListener(ScriptableEventListener toRemove)
    {
        if (listeners.Contains(toRemove))
        {
            this.listeners.Remove(toRemove);
        }
    }

    #endregion
}
