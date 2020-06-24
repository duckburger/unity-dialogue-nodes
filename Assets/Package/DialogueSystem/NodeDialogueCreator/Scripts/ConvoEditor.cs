using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(ConversationAsset))]
public class ConvoEditor : Editor
{
    ConversationAsset targetObj;

    private void OnEnable() 
    {
        targetObj = target as ConversationAsset;    
    }
    
    public override void OnInspectorGUI()
    {    
        DrawDefaultInspector();
      
        EditorGUI.BeginChangeCheck();

        if (GUILayout.Button("Edit Converstion", GUILayout.Height(50f)))
        {
            if (DialogueEditorWindow.WindowInstance == null)
            {
                ShowMyEditWindow();
            }
            else
            {
                Debug.Log($"Destroying current window instance");
                Destroy(DialogueEditorWindow.WindowInstance);
                ShowMyEditWindow();
            }
        }        

        EditorGUI.EndChangeCheck();
        
        serializedObject.Update();
        EditorUtility.SetDirty(target);
    }


    void ShowMyEditWindow()
    {
        DialogueEditorWindow.ShowWindow(targetObj);
        AssetDatabase.Refresh();
    }
}

#endif
