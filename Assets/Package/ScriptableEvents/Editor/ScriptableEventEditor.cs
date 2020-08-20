using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
    
[CustomEditor(typeof(ScriptableEvent))]
public class ScriptableEventEditor : Editor
{
    [HideInInspector] [SerializeField] ScriptableEventListener[] eventListenersInScene = new ScriptableEventListener[0];
    public ScriptableEvent myEvent;
    bool showingItems = false;


    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        myEvent = target as ScriptableEvent;

        if (GUILayout.Button("Find All Listeners In Scene"))
        {
            SearchForListeners();
        }

        if (eventListenersInScene == null || eventListenersInScene.Length == 0)
        {
            GUILayout.Label("No event listers found in scene!");
        }
        else
        {
            for (int i = 0; i < eventListenersInScene.Length; i++)
            {
                if (eventListenersInScene[i].trackedEvent == myEvent)
                {
                    EditorGUILayout.ObjectField(eventListenersInScene[i], typeof(ScriptableEventListener), false);
                    showingItems = true;
                }
            }
            if (!showingItems)
            {
                GUILayout.Label("No event listers found in scene!");
            }
        }

        EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();
    }


    void SearchForListeners()
    {
        eventListenersInScene = FindObjectsOfType<ScriptableEventListener>();
    }
}


#endif