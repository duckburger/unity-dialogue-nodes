using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    [Serializable]
    public class NPCDialogueNode : DialogueNode
    {
        public AudioClip lineSoundEffect;
        public DialogueNodeEvent attachedEvent;

        float windowHeight = 0;

        #region Paths

        string pathToConvoAsset;
        string fullPathToAsset;

        #endregion
        
        public DialogueCharacter speaker;

#if UNITY_EDITOR

        public NPCDialogueNode(Rect rect, string title) : base(rect, title)
        {
            
        }

        public override void DrawWindow()
        {
            speaker = (DialogueCharacter)EditorGUILayout.ObjectField(speaker, typeof(DialogueCharacter), false);
            windowHeight = 350f;

            if (speaker == null)
            {
                EditorGUILayout.LabelField("Add a speaker to modify");
            }
            else
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Character Icon");
                speaker.icon = (Sprite)EditorGUILayout.ObjectField(GUIContent.none, speaker.icon, typeof(Sprite), false, GUILayout.ExpandWidth(true));
                GUILayout.Label("Character Name");
                speaker.name = EditorGUILayout.TextField(speaker.name);
                GUILayout.Label("Character Text Color");
                speaker.textColor = EditorGUILayout.ColorField(speaker.textColor);
                GUILayout.EndVertical();
                EditorGUILayout.LabelField("Dialogue Line");
                EditorStyles.textField.wordWrap = true;
                DialogueLine = EditorGUILayout.TextArea(DialogueLine, GUILayout.Height(88f));
                lineSoundEffect = (AudioClip)EditorGUILayout.ObjectField(lineSoundEffect, typeof(AudioClip), false);

                EditorGUI.BeginChangeCheck();

                if (!attachedEvent)
                {
                    // TODO: Figure events out
                    if (GUILayout.Button("+ Event"))
                    {
                        // if (string.IsNullOrEmpty(pathToConvoAsset))
                        // {
                        //     pathToConvoAsset = AssetDatabase.GetAssetPath(containingConversation);
                        // }
                        // DialogueNodeEvent newEvent = ScriptableObject.CreateInstance<DialogueNodeEvent>();
                        // string directoryName = $"{containingConversation.name}_events";
                        // string pathToFolder = Path.GetDirectoryName(pathToConvoAsset);
                        // fullPathToAsset = $"{pathToFolder}\\{containingConversation.name}_Events";
                        // if (!Directory.Exists(fullPathToAsset))
                        // {
                        //     Directory.CreateDirectory(fullPathToAsset);
                        // }
                        // AssetDatabase.CreateAsset(newEvent, $"{fullPathToAsset}\\EventForNode#{containingConversation.allNPCNodes.IndexOf(this)}.asset");
                        // AssetDatabase.SaveAssets();
                        // AssetDatabase.Refresh();
                        // attachedEvent = newEvent;
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    windowHeight += 25f;
                    if (GUILayout.Button("- Event"))
                    {
                        if (attachedEvent)
                        {
                            bool deleted = AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(attachedEvent));
                            Debug.Log($"Event was deleted = {deleted}");
                            if (!deleted)
                                attachedEvent = null;
                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.ObjectField(attachedEvent as UnityEngine.Object, typeof(DialogueNodeEvent), false);
                }

                EditorGUI.EndChangeCheck();

                Rect updatedRect = new Rect(WindowRect);
                updatedRect.height = windowHeight;
                SetWindowRect(updatedRect);
            }
        }
        
#endif
        public void Drag(Vector2 dragDelta)
        {
            Vector2 rectPosition = WindowRect.position;
            rectPosition += dragDelta;
            WindowRect.Set(rectPosition.x, rectPosition.y, WindowRect.width, WindowRect.height);
        }

        #region Interface Requirements
        
        public override void SetWindowRect(Rect rect)
        {
            WindowRect = rect;
        }
        #endregion
    }

}

