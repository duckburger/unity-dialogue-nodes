using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
