using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    [Serializable]
    public class NPCDialogueNode : DialogueNode
    {
        private const string ADD_A_SPEAKER_TO_MODIFY = "Add a speaker to modify";
        private const string CHARACTER_ICON = "Character Icon";
        private const string CHARACTER_NAME = "Character Name";
        private const string CHARACTER_TEXT_COLOR = "Character Text Color";
        private const string DIALOGUE_LINE = "Dialogue Line";
        private const string ADD_EVENT = "+ Event";
        private const string REMOVE_EVENT = "- Event";
        
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

        public override void AddIncomingTransition(DialogueTransition transitionToAdd)
        {
            base.AddIncomingTransition(transitionToAdd);
            transitionToAdd.StartNode.NpcResponses.Add(this);
        }

        public override void DrawWindow()
        {
            speaker = (DialogueCharacter)EditorGUILayout.ObjectField(speaker, typeof(DialogueCharacter), false);
            windowHeight = 350f;

            if (speaker == null)
            {
                EditorGUILayout.LabelField(ADD_A_SPEAKER_TO_MODIFY);
            }
            else
            {
                GUILayout.BeginVertical();
                GUILayout.Label(CHARACTER_ICON);
                speaker.icon = (Sprite)EditorGUILayout.ObjectField(GUIContent.none, speaker.icon, typeof(Sprite), false, GUILayout.ExpandWidth(true));
                GUILayout.Label(CHARACTER_NAME);
                speaker.name = EditorGUILayout.TextField(speaker.name);
                GUILayout.Label(CHARACTER_TEXT_COLOR);
                speaker.textColor = EditorGUILayout.ColorField(speaker.textColor);
                GUILayout.EndVertical();
                EditorGUILayout.LabelField(DIALOGUE_LINE);
                EditorStyles.textField.wordWrap = true;
                DialogueLine = EditorGUILayout.TextArea(DialogueLine, GUILayout.Height(88f));
                lineSoundEffect = (AudioClip)EditorGUILayout.ObjectField(lineSoundEffect, typeof(AudioClip), false);

                EditorGUI.BeginChangeCheck();

                if (!attachedEvent)
                {
                    if (GUILayout.Button(ADD_EVENT))
                    {
                        if (string.IsNullOrEmpty(pathToConvoAsset))
                        {
                            pathToConvoAsset = AssetDatabase.GetAssetPath(DialogueEditorWindow.WindowInstance.currentAsset);
                        }
                        DialogueNodeEvent newEvent = ScriptableObject.CreateInstance<DialogueNodeEvent>();
                        string pathToFolder = Path.GetDirectoryName(pathToConvoAsset);
                        fullPathToAsset = $"{pathToFolder}\\{DialogueEditorWindow.WindowInstance.currentAsset.name}-Events";
                        if (!Directory.Exists(fullPathToAsset))
                        {
                            Directory.CreateDirectory(fullPathToAsset);
                        }
                        AssetDatabase.CreateAsset(newEvent, $"{fullPathToAsset}\\EventForNode#{DialogueEditorWindow.WindowInstance.currentAsset.allNPCNodes.IndexOf(this)}.asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        attachedEvent = newEvent;
                    }
                }
                else
                {
                    
                    if (GUILayout.Button(REMOVE_EVENT))
                    {
                        if (attachedEvent)
                        {
                            bool deleted = AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(attachedEvent));
                            if (!deleted)
                                attachedEvent = null;
                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.ObjectField(attachedEvent as UnityEngine.Object, typeof(DialogueNodeEvent), false);
                }

                windowHeight += 25f;
                EditorGUI.EndChangeCheck();

                Rect updatedRect = new Rect(WindowRect);
                updatedRect.height = windowHeight;
                SetWindowRect(updatedRect);
            }
        }
        
#endif
        public override void Drag(Vector2 dragDelta)
        {
            Rect newRect = WindowRect;
            newRect.position += dragDelta;
            WindowRect = newRect;
        }
    }

}

