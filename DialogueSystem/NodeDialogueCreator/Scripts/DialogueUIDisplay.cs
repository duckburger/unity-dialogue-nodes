using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DuckburgerDev.DialogueNodes
{
    public class DialogueUIDisplay : MonoBehaviour
    {
        [SerializeField]
        ScriptableEvent onDialogueStarted;
        [SerializeField]
        ScriptableEvent onDialogueEnded;
        [Space(10)]
        [SerializeField]
        ConversationAsset activeConversation;
        [Space(10)]
        [Header("UI Components")]
        [SerializeField]
        RectTransform mainDialogueBox;
        [SerializeField]
        TextMeshProUGUI speakerName;
        [SerializeField]
        TextMeshProUGUI dialogueLine;
        [SerializeField]
        Image speakerIcon;
        [SerializeField]
        Transform repliesParent;
        [SerializeField]
        CanvasGroup continueButtonCG;
        [SerializeField]
        GameObject skipButton;

        [Space(10)]
        [SerializeField]
        GameObject responsePrefab;
        DialogueNode currentNode;
        Vector2 originalBoxPosition;
        Vector2 originalRepliesPosition;
        Action onCurrentConvoCompleted = null;
        CanvasGroup mainCG;
        AudioSource audioSource;

        bool onScreen = false;


        private void Awake()
        {
            originalBoxPosition = mainDialogueBox.localPosition;
            originalRepliesPosition = repliesParent.localPosition;
            AnimateOut();
            mainCG = GetComponent<CanvasGroup>();
            audioSource = GetComponent<AudioSource>();
        }

        public void AnimateIn()
        {
            if (!mainDialogueBox)
            {
                Debug.LogError($"No main dialogue box connected");
                return;
            }

            onScreen = true;
            if (mainCG)
            {
                mainCG.alpha = 1;
                mainCG.blocksRaycasts = true;
                mainCG.interactable = true;
            }

            Vector2 anchorPos = mainDialogueBox.anchoredPosition;
            anchorPos.y = 0f;
            mainDialogueBox.anchoredPosition = anchorPos;
        }

        public void AnimateOut()
        {
            var tcs = new TaskCompletionSource<bool>();
            if (!mainDialogueBox)
            {
                Debug.LogError($"No main dialogue box connected");
                return;
            }

            onScreen = false;

            Vector2 anchorPos = mainDialogueBox.anchoredPosition;
            anchorPos.y -= Screen.height / 2;
            mainDialogueBox.anchoredPosition = anchorPos;
            if (mainCG)
            {
                mainCG.alpha = 0;
                mainCG.blocksRaycasts = false;
                mainCG.interactable = false;
            }
            tcs.TrySetResult(true);
        }

        void AnimateRepliesIn()
        {
            if (!repliesParent)
            {
                Debug.LogError($"No replies parent connected");
                return;
            }

            Vector3 repliesLocalPosition = repliesParent.localPosition;
            repliesLocalPosition.y = originalRepliesPosition.y;
            repliesParent.localPosition = repliesLocalPosition;
        }

        void AnimateRepliesOut()
        {
            if (!repliesParent)
            {
                Debug.LogError($"No replies parent connected");
                return;
            }

            Vector3 repliesLocalPosition = repliesParent.localPosition;
            repliesLocalPosition.y -= Screen.height / 2f;
            DestroyAllReplyNodes();
        }

        public void AcceptConversationAssetFromEvent(object convoAsset)
        {
            ConversationAsset asset = (ConversationAsset)convoAsset;
            AssignActiveConversation(asset);
            if (!onScreen)
                ProcessActiveConversation();
            else
                Debug.Log("Cannot load a new conversation, while the previous one is still on screen");
        }

        public void AssignActiveConversation(ConversationAsset asset)
        {
            activeConversation = asset;
        }

        public void ProcessActiveConversation()
        {
            ProcessActiveConversation(null);
        }

        public void ProcessActiveConversation(Action onConversationCompleted = null)
        {
            onDialogueStarted?.Raise();
            if (!activeConversation)
            {
                Debug.LogError($"No active conversation detected, can't play dialogue");
                return;
            }

            if (activeConversation.allNPCNodes.Count <= 0)
            {
                Debug.Log($"No nodes in the active conversation, can't play dialogue");
                return;
            }

            if (activeConversation.skippable)
            {
                skipButton.SetActive(true);
            }
            else
            {
                skipButton.SetActive(false);
            }

            onCurrentConvoCompleted = onConversationCompleted;

            if (!onScreen)
            {
                AnimateIn();
            }
            else
            {
                Close();
                AnimateIn();
            }

            for (int i = 0; i < activeConversation.allNPCNodes.Count; i++)
            {
                if (activeConversation.allNPCNodes[i].IncomingTransitions.Count <= 0 && activeConversation.allNPCNodes[i].OutgoingTransitions.Count > 0)
                {
                    // Found a node with only outbound transitions - this will be our first node
                    currentNode = activeConversation.allNPCNodes[i];
                    DisplayLine(currentNode);
                }
            }
        }

        private void DisplayLine(DialogueNode node)
        {
            StartCoroutine(RollOutLine(node.DialogueLine));
        }

        IEnumerator RollOutLine(string line)
        {
            NPCDialogueNode npcNode = currentNode as NPCDialogueNode;

            if (npcNode != null)
            {
                if (audioSource && npcNode.lineSoundEffect)
                {
                    audioSource.clip = npcNode.lineSoundEffect;
                    audioSource.Play();
                }
                npcNode?.attachedEvent?.Raise();
            }

            if (npcNode != null)
            {
                if (npcNode.speaker && npcNode.speaker.icon && !speakerIcon)
                {
                    Debug.LogError($"Connect speaker icon to the Dialogue UI Display");
                }
                speakerIcon.sprite = npcNode.speaker.icon;
                speakerName.text = npcNode.speaker.name;
            }
            dialogueLine.text = "";
            foreach (char character in line)
            {
                dialogueLine.text += character;
                yield return null;
            }

            if (currentNode.PlayerResponses.Count > 0)
            {
                // Display player responses
                ShowReplies(currentNode.PlayerResponses);
            }
            else if (currentNode.NpcResponses.Count > 0)
            {
                ShowContinueButton(true);
            }
            else
            {
                SpawnDoneButton();
            }
        }

        void ShowReplies(List<DialogueNode> availableReplies)
        {
            if (!responsePrefab)
            {
                Debug.LogError($"No response prefab connected");
                return;
            }

            foreach (var playerResponseNode in availableReplies)
            {
                if (playerResponseNode.OutgoingTransitions.Count == 0)
                {
                    SpawnButton(playerResponseNode.DialogueLine, () =>
                    {
                        Close();
                        onCurrentConvoCompleted?.Invoke();
                    });
                    continue;
                }

                NPCDialogueNode connectedNpcNode = playerResponseNode.OutgoingTransitions[0].EndNode as NPCDialogueNode;
                SpawnButton(playerResponseNode.DialogueLine, () =>
                {
                    AnimateRepliesOut();
                    DialogueNode savedNode = connectedNpcNode;
                    currentNode = savedNode;
                    DisplayLine(currentNode);
                });
            }

            AnimateRepliesIn();
        }

        void SpawnButton(string buttonTitle, Action buttonAction)
        {
            GameObject spawnedReply = Instantiate(responsePrefab, Vector3.zero, Quaternion.identity, repliesParent);
            TextMeshProUGUI replyText = spawnedReply.GetComponentInChildren<TextMeshProUGUI>();
            Button replyButton = spawnedReply.GetComponent<Button>();

            replyText.text = buttonTitle;
            replyButton.onClick.AddListener(buttonAction.Invoke);
        }

        void SpawnDoneButton()
        {
            SpawnButton("Done", () =>
            {
                Close();
                onCurrentConvoCompleted?.Invoke();
            });

            AnimateRepliesIn();
        }

        void ShowContinueButton(bool enabled)
        {
            if (enabled)
            {
                continueButtonCG.blocksRaycasts = enabled;
                continueButtonCG.interactable = enabled;
                continueButtonCG.alpha = 1f;
            }
            else
            {
                continueButtonCG.blocksRaycasts = enabled;
                continueButtonCG.interactable = enabled;
                continueButtonCG.alpha = 0f;
            }

        }

        private void DestroyAllReplyNodes()
        {
            for (int i = repliesParent.childCount - 1; i >= 0; i--)
            {
                Destroy(repliesParent.GetChild(i).gameObject);
            }
        }

        public void Close()
        {
            AnimateOut();
            DestroyAllReplyNodes();
            onCurrentConvoCompleted?.Invoke();
            onDialogueEnded?.Raise();
        }

        public void Continue()
        {
            // Display NPC responses
            currentNode = currentNode.NpcResponses[0];
            DisplayLine(currentNode);
            ShowContinueButton(false);
        }

    }
}

