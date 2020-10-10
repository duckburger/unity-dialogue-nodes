using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace DuckburgerDev.DialogueNodes
{
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
            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Edit Converstion", GUILayout.Height(50f)))
            {
                if (DialogueEditorWindow.WindowInstance != null)
                {
                    Debug.Log($"Destroying current window instance");
                    Destroy(DialogueEditorWindow.WindowInstance);
                }
                ShowMyEditWindow();
            }

            serializedObject.Update();
            EditorUtility.SetDirty(target);
            EditorGUI.EndChangeCheck();
        }
        
        void ShowMyEditWindow()
        {
            DialogueEditorWindow.ShowWindow(targetObj);
            AssetDatabase.Refresh();
        }
    }
}

#endif
