using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
    [Serializable]
    public class NPCDialogueNode : DialogueNode
    {
        public AudioClip lineSoundEffect;
        public DialogueCharacter speaker;

#if UNITY_EDITOR

        public NPCDialogueNode(Rect rect, string title) : base(rect, title)
        {
            // Empty   
        }

        public override void AddIncomingTransition(DialogueTransition transitionToAdd)
        {
            base.AddIncomingTransition(transitionToAdd);
            transitionToAdd.StartNode.NpcResponses.Add(this);
        }

        public override void DrawWindow()
        {
            speaker = (DialogueCharacter)EditorGUILayout.ObjectField(speaker, typeof(DialogueCharacter), false);
            windowHeight = 245f;

            if (speaker == null)
            {
                EditorGUILayout.LabelField(ADD_A_SPEAKER_TO_MODIFY);
            }
            else
            {
                GUILayout.BeginVertical();
                GUILayout.Label(CHARACTER_NAME);
                speaker.name = EditorGUILayout.TextField(speaker.name);
                GUILayout.Label(CHARACTER_TEXT_COLOR);
                speaker.textColor = EditorGUILayout.ColorField(speaker.textColor);
                GUILayout.EndVertical();
                EditorGUILayout.LabelField(DIALOGUE_LINE);
                EditorStyles.textField.wordWrap = true;
                DialogueLine = EditorGUILayout.TextArea(DialogueLine, GUILayout.Height(88f));
                lineSoundEffect = (AudioClip)EditorGUILayout.ObjectField(lineSoundEffect, typeof(AudioClip), false);

                base.DrawWindow();

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

