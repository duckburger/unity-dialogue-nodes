using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class EventWithData : UnityEvent<object> { }
[Serializable]
public class ScriptableEventListener : MonoBehaviour
{
    public ScriptableEvent trackedEvent;
    [Space(10)]
    [Header("Responses")]
    [SerializeField] UnityEvent raiseResponse;
    [SerializeField] EventWithData raisedWithData;
    [SerializeField] UnityEvent activateResponse;
    [SerializeField] EventWithData activateWithDataResponse;
    [SerializeField] UnityEvent deactivateResponse;
    [SerializeField] EventWithData deactivateWithDataResponse;

    #region Registration

    private void OnEnable()
    {
        if (this.trackedEvent != null)
        {
            this.trackedEvent.RegisterListener(this);
        }
        else
        {
            Debug.LogError("No assigned tracked event on the " + this.gameObject.name + "!");
        }
    }

    private void OnDisable()
    {
        if (this.trackedEvent != null)
        {
            this.trackedEvent.DeRegisterListener(this);
        }
        else
        {
            Debug.LogError("No assigned tracked event on the " + this.gameObject.name + "!");
        }
    }

    #endregion

    #region Invoking Events

    public void Raise()
    {
        this.raiseResponse.Invoke();
    }

    public void RaiseWithData(object obj)
    {
        this.raisedWithData.Invoke(obj);
    }

    public void Activate()
    {
        this.activateResponse.Invoke();
    }

    public void Activate(object o)
    {
        this.activateWithDataResponse.Invoke(o);
    }

    public void Deactivate()
    {
        this.deactivateResponse.Invoke();
    }

    public void Deactivate(object o)
    {
        this.deactivateWithDataResponse.Invoke(o);
    }

    #endregion
}
