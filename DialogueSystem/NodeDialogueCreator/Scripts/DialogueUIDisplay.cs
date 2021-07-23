using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DuckburgerDev.DialogueNodes
{
    public class DialogueUIDisplay : MonoBehaviour
    {
        public event Action OnConversationModeActivated;
        public event Action OnConversationModeDeactivated;
        
        [Space(10)]
        [SerializeField]
        private ConversationAsset _currentConversationAsset;
        [Space(10)]
        [Header("UI Components")]
        [SerializeField]
        private RectTransform _mainDialogueBox;
        [SerializeField]
        private TextMeshProUGUI _speakerName;
        [SerializeField]
        private TextMeshProUGUI _dialogueLine;
        [SerializeField]
        private Image _speakerIcon;
        [SerializeField]
        private Transform _repliesParent;
        [SerializeField]
        private CanvasGroup _continueButtonCg;
        [SerializeField]
        private GameObject _skipButton;

        [Space(10)]
        [SerializeField]
        private GameObject _replyPrefab;
        private DialogueNode _currentNode;
        private Vector2 _originalBoxPosition;
        private Vector2 _originalRepliesPosition;
        private Action _onCurrentConvoCompleted = null;
        private CanvasGroup _mainCg;
        private AudioSource _audioSource;
        private bool _onScreen = false;

        private void Awake()
        {
            _originalBoxPosition = _mainDialogueBox.localPosition;
            _originalRepliesPosition = _repliesParent.localPosition;
            AnimateOut();
            _mainCg = GetComponent<CanvasGroup>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void AnimateIn()
        {
            if (!_mainDialogueBox)
            {
                Debug.LogError($"No main dialogue box connected");
                return;
            }

            _onScreen = true;
            if (_mainCg)
            {
                _mainCg.alpha = 1;
                _mainCg.blocksRaycasts = true;
                _mainCg.interactable = true;
            }

            Vector2 anchorPos = _mainDialogueBox.anchoredPosition;
            anchorPos.y = 0f;
            _mainDialogueBox.anchoredPosition = anchorPos;
        }

        public void AnimateOut()
        {
            if (!_mainDialogueBox)
            {
                Debug.LogError($"No main dialogue box connected");
                return;
            }

            _onScreen = false;

            Vector2 anchorPos = _mainDialogueBox.anchoredPosition;
            anchorPos.y -= Screen.height / 2;
            _mainDialogueBox.anchoredPosition = anchorPos;
            if (_mainCg)
            {
                _mainCg.alpha = 0;
                _mainCg.blocksRaycasts = false;
                _mainCg.interactable = false;
            }
        }

        void AnimateRepliesIn()
        {
            if (!_repliesParent)
            {
                Debug.LogError($"No replies parent connected");
                return;
            }

            Vector3 repliesLocalPosition = _repliesParent.localPosition;
            repliesLocalPosition.y = _originalRepliesPosition.y;
            _repliesParent.localPosition = repliesLocalPosition;
        }

        void AnimateRepliesOut()
        {
            if (!_repliesParent)
            {
                Debug.LogError($"No replies parent connected");
                return;
            }

            Vector3 repliesLocalPosition = _repliesParent.localPosition;
            repliesLocalPosition.y -= Screen.height / 2f;
            DestroyAllReplyNodes();
        }

        public void AssignActiveConversation(ConversationAsset asset)
        {
            _currentConversationAsset = asset;
        }

        public void ProcessActiveConversation()
        {
            ProcessActiveConversation(null);
        }

        public void ProcessActiveConversation(Action onConversationCompleted = null)
        {
            if (!_currentConversationAsset)
            {
                Debug.LogError($"No active conversation detected, can't play dialogue");
                return;
            }

            if (_currentConversationAsset.allNPCNodes.Count <= 0)
            {
                Debug.Log($"No nodes in the active conversation, can't play dialogue");
                return;
            }

            OnConversationModeActivated?.Invoke();
            if (_skipButton != null)
            {
                if (_currentConversationAsset.skippable)
                {
                    _skipButton.SetActive(true);
                }
                else
                {
                    _skipButton.SetActive(false);
                }
            }
            
            _onCurrentConvoCompleted = onConversationCompleted;

            if (!_onScreen)
            {
                AnimateIn();
            }
            else
            {
                Close();
                AnimateIn();
            }

            for (int i = 0; i < _currentConversationAsset.allNPCNodes.Count; i++)
            {
                if (_currentConversationAsset.allNPCNodes[i].IncomingTransitions.Count <= 0 && _currentConversationAsset.allNPCNodes[i].OutgoingTransitions.Count > 0)
                {
                    // Found a node with only outbound transitions - this will be our first node
                    _currentNode = _currentConversationAsset.allNPCNodes[i];
                    DisplayLine(_currentNode);
                }
            }
        }

        private void DisplayLine(DialogueNode node)
        {
            StartCoroutine(RollOutLine(node.DialogueLine));
        }

        IEnumerator RollOutLine(string line)
        {
            NPCDialogueNode npcNode = _currentNode as NPCDialogueNode;

            if (npcNode != null)
            {
                if (_audioSource && npcNode.lineSoundEffect)
                {
                    _audioSource.clip = npcNode.lineSoundEffect;
                    _audioSource.Play();
                }
            }

            if (npcNode != null)
            {
                if (npcNode.speaker && npcNode.speaker.icon && !_speakerIcon)
                {
                    Debug.LogError($"Connect speaker icon to the Dialogue UI Display");
                }
                _speakerIcon.sprite = npcNode.speaker.icon;
                _speakerName.text = npcNode.speaker.name;
            }
            _dialogueLine.text = "";
            foreach (char character in line)
            {
                _dialogueLine.text += character;
                yield return null;
            }

            npcNode?.attachedEvent?.Raise();
            
            if (_currentNode.PlayerResponses.Count > 0)
            {
                // Display player responses
                ShowReplies(_currentNode.PlayerResponses);
            }
            else if (_currentNode.NpcResponses.Count > 0)
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
            if (_replyPrefab == null)
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
                        _onCurrentConvoCompleted?.Invoke();
                    });
                    continue;
                }

                NPCDialogueNode connectedNpcNode = playerResponseNode.OutgoingTransitions[0].EndNode as NPCDialogueNode;
                SpawnButton(playerResponseNode.DialogueLine, () =>
                {
                    AnimateRepliesOut();
                    DialogueNode savedNode = connectedNpcNode;
                    _currentNode = savedNode;
                    DisplayLine(_currentNode);
                });
            }

            AnimateRepliesIn();
        }

        void SpawnButton(string buttonTitle, Action buttonAction)
        {
            GameObject spawnedReply = Instantiate(_replyPrefab, Vector3.zero, Quaternion.identity, _repliesParent);
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
                _onCurrentConvoCompleted?.Invoke();
            });

            AnimateRepliesIn();
        }

        void ShowContinueButton(bool enabled)
        {
            if (enabled)
            {
                _continueButtonCg.blocksRaycasts = enabled;
                _continueButtonCg.interactable = enabled;
                _continueButtonCg.alpha = 1f;
            }
            else
            {
                _continueButtonCg.blocksRaycasts = enabled;
                _continueButtonCg.interactable = enabled;
                _continueButtonCg.alpha = 0f;
            }

        }

        private void DestroyAllReplyNodes()
        {
            for (int i = _repliesParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_repliesParent.GetChild(i).gameObject);
            }
        }

        public void Close()
        {
            AnimateOut();
            DestroyAllReplyNodes();
            _onCurrentConvoCompleted?.Invoke();
            OnConversationModeDeactivated?.Invoke();
        }

        public void Continue()
        {
            // Display NPC responses
            _currentNode = _currentNode.NpcResponses[0];
            DisplayLine(_currentNode);
            ShowContinueButton(false);
        }

    }
}

