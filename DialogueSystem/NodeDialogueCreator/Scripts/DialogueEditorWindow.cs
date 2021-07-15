using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DuckburgerDev.DialogueNodes
{
#if UNITY_EDITOR

    public class DialogueEditorWindow : EditorWindow
    {
        public static DialogueEditorWindow WindowInstance;
        [SerializeReference]
        public List<DialogueNode> allNodes = new List<DialogueNode>();
        [SerializeReference]
        public List<DialogueNode> allNPCNodes = new List<DialogueNode>();
        [SerializeReference]
        public List<DialogueNode> allPlayerNodes = new List<DialogueNode>();
        [SerializeField]
        public List<DialogueTransition> allTransitions = new List<DialogueTransition>();
        public ConversationAsset currentAsset;

        private float _mouseWheelZoom = 1;
        private Vector3 mousePos;
        private Vector2 dragDelta;
        private Vector2 gridOffset;
        private bool makingTransition;
        private bool clickedOnNode;
        private DialogueNode selectedNode;
        private DialogueTransition selectedTransition;

        private enum UserInteractions
        {
            addNPCDialogueNode,
            addPlayerResponseNode,
            duplicateNode,
            deleteNode,
            makeTransition
        }
        
        [MenuItem("Dialogue Editor/Editor")]
        public static void ShowWindow()
        {
            Debug.Log($"Called ShowWindow");
            WindowInstance = EditorWindow.GetWindow<DialogueEditorWindow>();
            WindowInstance.minSize = new Vector2(800f, 600f);
        }

        public static void ShowWindow(ConversationAsset assetToEdit)
        {
            Debug.Log($"Called ShowWindowWIthAsset");
            WindowInstance = null;
            WindowInstance = GetWindow<DialogueEditorWindow>();
            WindowInstance.minSize = new Vector2(800f, 600f);

            WindowInstance.allNodes.Clear();
            WindowInstance.allNPCNodes.Clear();
            WindowInstance.allPlayerNodes.Clear();
            WindowInstance.allTransitions.Clear();

            WindowInstance.allNodes.AddRange(assetToEdit.allNPCNodes);
            WindowInstance.allNodes.AddRange(assetToEdit.allPlayerNodes);

            WindowInstance.allNPCNodes = assetToEdit.allNPCNodes;
            WindowInstance.allPlayerNodes = assetToEdit.allPlayerNodes;
            WindowInstance.allTransitions = assetToEdit.allTransitions;

            WindowInstance.currentAsset = assetToEdit;
            WindowInstance.selectedNode = null;
            WindowInstance.selectedTransition = null;
            WindowInstance.makingTransition = false;
            EditorUtility.SetDirty(assetToEdit);
            EditorUtility.SetDirty(WindowInstance);
            AssetDatabase.Refresh();
        }
        
        private void OnDisable()
        {
            if (WindowInstance.currentAsset != null)
            {
                WindowInstance.currentAsset.allNPCNodes = allNPCNodes;
                WindowInstance.currentAsset.allPlayerNodes = allPlayerNodes;
                WindowInstance.currentAsset.allTransitions = allTransitions;
                EditorUtility.SetDirty(WindowInstance.currentAsset);
                AssetDatabase.Refresh();
            }
            WindowInstance.currentAsset = null;
            
            WindowInstance.selectedNode = null;
            WindowInstance.selectedTransition = null;
            WindowInstance.makingTransition = false;
            EditorUtility.SetDirty(WindowInstance);
            WindowInstance = null;
            AssetDatabase.Refresh();
        }
        
        private void OnGUI()
        {
            Event currentEvent = Event.current;
            mousePos = currentEvent.mousePosition;
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            UserInput(currentEvent);

            DrawTransitions();
            DrawNodes(currentEvent);

            if (currentAsset != null)
            {
                EditorUtility.SetDirty(currentAsset);
            }
            
            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            gridOffset += dragDelta * 0.5f;
            Vector3 newOffset = new Vector3(gridOffset.x % gridSpacing, gridOffset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        void DrawNodes(Event currentEvent)
        {
            BeginWindows();

            for (int i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].SetWindowRect(GUI.Window(i, allNodes[i].WindowRect, DrawNodeRect, allNodes[i].WindowTitle));
            }

            EndWindows();
        }

        void DrawTransitions()
        {
            for (int i = 0; i < allTransitions.Count; i++)
            {
                allTransitions[i].Draw();
            }
        }

        void DrawNodeRect(int index)
        {
            allNodes[index].DrawWindow();
            GUI.DragWindow();
            GUI.changed = true;
        }

        void UserInput(Event currentEvent)
        {
            dragDelta = Vector2.zero;

            if (currentEvent.button == 0 && makingTransition)
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    LeftClick(currentEvent);
                    return;
                }
            }

            if (currentEvent.button == 1 && !makingTransition)
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    RightClick(currentEvent);
                    return;
                }
            }

            if (currentEvent.type == EventType.MouseDrag && !makingTransition && !IsOverNode(currentEvent))
            {
                if (currentEvent.button == 2)
                {
                    OnDragCanvas(currentEvent.delta);
                }
            }

            if (currentEvent.type == EventType.ScrollWheel)
            {
                _mouseWheelZoom += currentEvent.delta.y;
            }
        }

        private void RightClick(Event currentEvent)
        {
            selectedNode = null;
            clickedOnNode = false;
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i].WindowRect.Contains(currentEvent.mousePosition))
                {
                    clickedOnNode = true;
                    selectedNode = allNodes[i];
                    break;
                }
                else
                {
                    clickedOnNode = false;
                }
            }

            if (!clickedOnNode)
            {
                AddNewNode(currentEvent);
            }
            else
            {
                ModifySelectedNode(currentEvent);
            }
        }

        private void LeftClick(Event currentEvent)
        {
            selectedNode = null;
            clickedOnNode = false;
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i].WindowRect.Contains(currentEvent.mousePosition))
                {
                    clickedOnNode = true;
                    selectedNode = allNodes[i];
                    break;
                }
                else
                {
                    clickedOnNode = false;
                }
            }

            if (!clickedOnNode || selectedNode.HasOutgoingTransition(selectedTransition))
            {
                // Delete the pending transition
                DeleteTransition(selectedTransition);
            }
            else
            {
                selectedTransition.EndNode = selectedNode;
                selectedNode.AddIncomingTransition(selectedTransition);
            }
            makingTransition = false;
            selectedNode = null;
        }

        void AddNewNode(Event currentEvent)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Add NPC Dialogue Node"), false, ContextualClick, UserInteractions.addNPCDialogueNode);
            menu.AddItem(new GUIContent("Add Player Response Node"), false, ContextualClick, UserInteractions.addPlayerResponseNode);

            menu.ShowAsContext();
            currentEvent.Use();
        }


        void ModifySelectedNode(Event currentEvent)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete Node"), false, ContextualClick, UserInteractions.deleteNode);
            menu.AddItem(new GUIContent("Duplicate Node"), false, ContextualClick, UserInteractions.duplicateNode);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Make Transition"), false, ContextualClick, UserInteractions.makeTransition);
            menu.ShowAsContext();
            currentEvent.Use();
        }

        void ContextualClick(object o)
        {
            UserInteractions interaction = (UserInteractions)o;
            switch (interaction)
            {
                case UserInteractions.addNPCDialogueNode:

                    NPCDialogueNode newNPCNode = new NPCDialogueNode(new Rect(mousePos.x, mousePos.y, 200, 300), "NPC Dialogue Node");
                    allNPCNodes.Add(newNPCNode);
                    allNodes.Add(newNPCNode);
                    break;

                case UserInteractions.addPlayerResponseNode:

                    PlayerDialogueNode newPlayerNode = new PlayerDialogueNode(new Rect(mousePos.x, mousePos.y, 200, 128), "Player Response Node");
                    allPlayerNodes.Add(newPlayerNode);
                    allNodes.Add(newPlayerNode);
                    break;

                case UserInteractions.duplicateNode:

                    Rect newPosRect = new Rect(selectedNode.WindowRect.x + selectedNode.WindowRect.width, selectedNode.WindowRect.y, selectedNode.WindowRect.width, selectedNode.WindowRect.height);
                    if (selectedNode is PlayerDialogueNode)
                    {
                        PlayerDialogueNode duplicatedNode = new PlayerDialogueNode(newPosRect, selectedNode.WindowTitle);
                        duplicatedNode.SetDialogueLine(selectedNode.DialogueLine);
                        allPlayerNodes.Add(duplicatedNode);
                        allNodes.Add(duplicatedNode);
                    }
                    else if (selectedNode is NPCDialogueNode)
                    {
                        NPCDialogueNode dialNode = selectedNode as NPCDialogueNode;
                        NPCDialogueNode duplicatedNode = new NPCDialogueNode(newPosRect, selectedNode.WindowTitle);
                        duplicatedNode.SetDialogueLine(selectedNode.DialogueLine);
                        duplicatedNode.speaker = dialNode.speaker;
                        allNPCNodes.Add(duplicatedNode);
                        allNodes.Add(duplicatedNode);
                    }

                    break;

                case UserInteractions.deleteNode:

                    selectedNode.DeleteAllTransitions();
                    if (selectedNode is NPCDialogueNode)
                    {
                        allNPCNodes.Remove(selectedNode as NPCDialogueNode);
                    }
                    else
                    {
                        allPlayerNodes.Remove(selectedNode as PlayerDialogueNode);
                    }
                    allNodes.Remove(selectedNode);
                    break;

                case UserInteractions.makeTransition:

                    Debug.Log($"Making transition");
                    
                    DialogueTransition newTransition;
                    newTransition = new DialogueTransition();
                    newTransition.Initialize(selectedNode, null);
                    selectedNode.AddOutgoingTransition(newTransition);
                    
                    selectedTransition = newTransition;
                    allTransitions.Add(newTransition);
                    makingTransition = true;
                    break;

                default:
                    break;
            }
        }

        void OnDragCanvas(Vector2 delta)
        {
            this.dragDelta = delta;

            for (int i = allNPCNodes.Count - 1; i >= 0; i--)
            {
                allNPCNodes[i].Drag(this.dragDelta);
            }

            for (int i = allPlayerNodes.Count - 1; i >= 0; i--)
            {
                allPlayerNodes[i].Drag(this.dragDelta);
            }

            GUI.changed = true;
        }

        public void DeleteTransition(DialogueTransition transitionToDelete)
        {
            allTransitions.Remove(transitionToDelete);

            if (transitionToDelete.StartNode != null && !string.IsNullOrEmpty(transitionToDelete.StartNode.WindowTitle))
            {
                transitionToDelete.StartNode.RemoveOutgoingTransition(transitionToDelete);
            }

            if (transitionToDelete.EndNode != null && !string.IsNullOrEmpty(transitionToDelete.EndNode.WindowTitle))
            {
                transitionToDelete.EndNode.RemoveIncomingTransition(transitionToDelete);
            }
        }

        bool IsOverNode(Event currentEvent)
        {
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i].WindowRect.Contains(currentEvent.mousePosition))
                {
                    return true;
                }
            }
            return false;
        }

    }

#endif



}

